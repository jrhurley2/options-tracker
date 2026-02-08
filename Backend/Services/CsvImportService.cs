using Microsoft.EntityFrameworkCore;
using OptionsTracker.Data;
using OptionsTracker.Models;
using OptionsTracker.Utilities;

namespace OptionsTracker.Services
{
    public interface ICsvImportService
    {
        Task<CsvImportResult> ImportCsvAsync(Stream fileStream, string broker, string accountName);
    }

    public class CsvImportService : ICsvImportService
    {
        private readonly ApplicationDbContext _context;
        private readonly IPositionService _positionService;
        private readonly Dictionary<string, ICsvParser> _parsers;

        public CsvImportService(ApplicationDbContext context, IPositionService positionService)
        {
            _context = context;
            _positionService = positionService;
            
            _parsers = new Dictionary<string, ICsvParser>(StringComparer.OrdinalIgnoreCase)
            {
                { "fidelity", new FidelityCsvParser() },
                { "schwab", new SchwabCsvParser() }
            };
        }

        public async Task<CsvImportResult> ImportCsvAsync(Stream fileStream, string broker, string accountName)
        {
            var result = new CsvImportResult();

            if (!_parsers.TryGetValue(broker, out var parser))
            {
                result.Success = false;
                result.Errors.Add($"Unsupported broker: {broker}. Supported brokers are: {string.Join(", ", _parsers.Keys)}");
                return result;
            }

            try
            {
                var parsedTransactions = await parser.ParseCsvAsync(fileStream, accountName);

                foreach (var parsed in parsedTransactions)
                {
                    try
                    {
                        await ProcessTransactionAsync(parsed);
                        result.TransactionsImported++;
                    }
                    catch (Exception ex)
                    {
                        result.Warnings.Add($"Line {parsed.LineNumber}: {ex.Message}");
                    }
                }

                result.Success = true;
                result.Message = $"Successfully imported {result.TransactionsImported} transactions from {parser.BrokerName}";
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Errors.Add($"Failed to parse CSV: {ex.Message}");
            }

            return result;
        }

        private async Task ProcessTransactionAsync(ParsedTransaction parsed)
        {
            // Check for duplicate
            var existingTransaction = await _context.Transactions
                .FirstOrDefaultAsync(t => 
                    t.Symbol == parsed.Symbol &&
                    t.TransactionDate == parsed.TransactionDate &&
                    t.Quantity == parsed.Quantity &&
                    t.Price == parsed.Price &&
                    t.Source == parsed.Source);

            if (existingTransaction != null)
            {
                // Skip duplicate
                return;
            }

            var transaction = new Transaction
            {
                Type = ParseTransactionType(parsed.Type),
                Symbol = parsed.Symbol,
                TransactionDate = parsed.TransactionDate,
                Quantity = parsed.Quantity,
                Price = parsed.Price,
                Amount = parsed.Amount,
                Fees = parsed.Fees,
                Account = parsed.Account,
                Notes = parsed.Notes,
                Source = parsed.Source,
                CreatedAt = DateTime.UtcNow
            };

            if (parsed.IsOption)
            {
                transaction.OptionType = ParseOptionType(parsed.OptionType);
                transaction.StrikePrice = parsed.StrikePrice;
                transaction.ExpirationDate = parsed.ExpirationDate;
            }

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            // Update positions based on transaction type
            await UpdatePositionsFromTransaction(transaction);
        }

        private async Task UpdatePositionsFromTransaction(Transaction transaction)
        {
            switch (transaction.Type)
            {
                case TransactionType.BuyStock:
                    await _positionService.CreateOrUpdatePositionAsync(
                        transaction.Symbol,
                        transaction.Quantity,
                        transaction.Price,
                        transaction.Account
                    );
                    break;

                case TransactionType.SellStock:
                    await _positionService.CreateOrUpdatePositionAsync(
                        transaction.Symbol,
                        -transaction.Quantity,
                        transaction.Price,
                        transaction.Account
                    );
                    break;

                case TransactionType.SellToOpen:
                case TransactionType.BuyToOpen:
                    // Create options position if it doesn't exist
                    if (transaction.OptionType.HasValue && transaction.StrikePrice.HasValue && transaction.ExpirationDate.HasValue)
                    {
                        var existingOption = await _context.OptionsPositions
                            .FirstOrDefaultAsync(o =>
                                o.UnderlyingSymbol == transaction.Symbol &&
                                o.StrikePrice == transaction.StrikePrice.Value &&
                                o.ExpirationDate == transaction.ExpirationDate.Value &&
                                o.Status == PositionStatus.Open);

                        if (existingOption == null)
                        {
                            var optionPosition = new OptionsPosition
                            {
                                UnderlyingSymbol = transaction.Symbol,
                                OptionType = transaction.OptionType.Value,
                                Strategy = transaction.Type == TransactionType.SellToOpen ? OptionStrategy.Short : OptionStrategy.Long,
                                StrikePrice = transaction.StrikePrice.Value,
                                ExpirationDate = transaction.ExpirationDate.Value,
                                Contracts = (int)transaction.Quantity,
                                PremiumPerContract = transaction.Price,
                                CurrentPrice = transaction.Price,
                                OpenDate = transaction.TransactionDate,
                                Status = PositionStatus.Open,
                                Account = transaction.Account
                            };

                            _context.OptionsPositions.Add(optionPosition);
                            await _context.SaveChangesAsync();

                            transaction.OptionsPositionId = optionPosition.Id;
                            await _context.SaveChangesAsync();
                        }
                    }
                    break;

                case TransactionType.BuyToClose:
                case TransactionType.SellToClose:
                    // Close options position
                    if (transaction.OptionType.HasValue && transaction.StrikePrice.HasValue && transaction.ExpirationDate.HasValue)
                    {
                        var optionToClose = await _context.OptionsPositions
                            .FirstOrDefaultAsync(o =>
                                o.UnderlyingSymbol == transaction.Symbol &&
                                o.StrikePrice == transaction.StrikePrice.Value &&
                                o.ExpirationDate == transaction.ExpirationDate.Value &&
                                o.Status == PositionStatus.Open);

                        if (optionToClose != null)
                        {
                            optionToClose.Status = PositionStatus.Closed;
                            optionToClose.CloseDate = transaction.TransactionDate;
                            optionToClose.CurrentPrice = transaction.Price;
                            await _context.SaveChangesAsync();

                            transaction.OptionsPositionId = optionToClose.Id;
                            await _context.SaveChangesAsync();
                        }
                    }
                    break;
            }
        }

        private TransactionType ParseTransactionType(string type)
        {
            return type.ToUpper() switch
            {
                "BUYSTOCK" => TransactionType.BuyStock,
                "SELLSTOCK" => TransactionType.SellStock,
                "BUYTOOPEN" => TransactionType.BuyToOpen,
                "SELLTOOPEN" => TransactionType.SellToOpen,
                "BUYTOCLOSE" => TransactionType.BuyToClose,
                "SELLTOCLOSE" => TransactionType.SellToClose,
                "OPTIONASSIGNED" => TransactionType.OptionAssigned,
                "OPTIONEXERCISED" => TransactionType.OptionExercised,
                "OPTIONEXPIRED" => TransactionType.OptionExpired,
                "DIVIDEND" => TransactionType.Dividend,
                _ => TransactionType.BuyStock
            };
        }

        private OptionType? ParseOptionType(string? optionType)
        {
            if (string.IsNullOrWhiteSpace(optionType))
                return null;

            return optionType.ToUpper() switch
            {
                "CALL" => OptionType.Call,
                "PUT" => OptionType.Put,
                _ => null
            };
        }
    }
}
