using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;

namespace OptionsTracker.Utilities
{
    /// <summary>
    /// Parses Schwab CSV export files
    /// Format typically includes: Date, Action, Symbol, Description, Quantity, Price, Fees & Comm, Amount
    /// Schwab options format: "SYMBOL MMDDYY C/P STRIKE" e.g., "AAPL 011725C150"
    /// </summary>
    public class SchwabCsvParser : ICsvParser
    {
        public string BrokerName => "Schwab";

        public async Task<List<ParsedTransaction>> ParseCsvAsync(Stream fileStream, string accountName)
        {
            var transactions = new List<ParsedTransaction>();
            
            using var reader = new StreamReader(fileStream);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null,
                HeaderValidated = null,
                TrimOptions = TrimOptions.Trim
            });

            // Schwab CSV often has header rows with account info - skip to the actual data
            string line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                if (line.Contains("Date") && line.Contains("Action"))
                {
                    // Found the header row
                    break;
                }
            }

            // Reset to read from header
            fileStream.Position = 0;
            using var reader2 = new StreamReader(fileStream);
            
            // Skip to transactions section
            while ((line = await reader2.ReadLineAsync()) != null)
            {
                if (line.Contains("\"Date\"") || line.Contains("Date,"))
                    break;
            }

            using var csv2 = new CsvReader(reader2, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null,
                HeaderValidated = null,
                TrimOptions = TrimOptions.Trim
            });

            await csv2.ReadAsync();
            csv2.ReadHeader();
            int lineNumber = 1;

            while (await csv2.ReadAsync())
            {
                lineNumber++;
                try
                {
                    var date = csv2.GetField<string>("Date") ?? "";
                    var action = csv2.GetField<string>("Action") ?? "";
                    var symbol = csv2.GetField<string>("Symbol") ?? "";
                    var description = csv2.GetField<string>("Description") ?? "";
                    var quantityStr = csv2.GetField<string>("Quantity") ?? "0";
                    var priceStr = csv2.GetField<string>("Price") ?? "0";
                    var feesStr = csv2.GetField<string>("Fees & Comm") ?? csv2.GetField<string>("Fees") ?? "0";
                    var amountStr = csv2.GetField<string>("Amount") ?? "0";

                    // Skip empty rows or summary rows
                    if (string.IsNullOrWhiteSpace(date) || date.Contains("Total") || string.IsNullOrWhiteSpace(action))
                        continue;

                    var transaction = new ParsedTransaction
                    {
                        Symbol = CleanSymbol(symbol),
                        TransactionDate = ParseDate(date),
                        Quantity = Math.Abs(ParseDecimal(quantityStr)), // Schwab uses negative for sells
                        Price = ParseDecimal(priceStr),
                        Amount = ParseDecimal(amountStr),
                        Fees = ParseDecimal(feesStr),
                        Account = accountName,
                        Source = BrokerName,
                        LineNumber = lineNumber
                    };

                    // Determine transaction type
                    transaction.Type = MapSchwabAction(action, description);

                    // Check if this is an options transaction
                    if (IsOptionSymbol(symbol) || description.Contains("CALL", StringComparison.OrdinalIgnoreCase) ||
                        description.Contains("PUT", StringComparison.OrdinalIgnoreCase))
                    {
                        transaction.IsOption = true;
                        ParseSchwabOptionSymbol(symbol, description, transaction);
                    }

                    transactions.Add(transaction);
                }
                catch (Exception ex)
                {
                    transactions.Add(new ParsedTransaction
                    {
                        LineNumber = lineNumber,
                        Notes = $"Error parsing line: {ex.Message}",
                        Source = BrokerName
                    });
                }
            }

            return transactions;
        }

        private string MapSchwabAction(string action, string description)
        {
            var actionUpper = action.ToUpper();
            var isOption = IsOptionDescription(description);

            return actionUpper switch
            {
                var a when a.Contains("BUY") && !isOption => "BuyStock",
                var a when a.Contains("SELL") && !isOption => "SellStock",
                var a when a.Contains("BUY TO OPEN") => "BuyToOpen",
                var a when a.Contains("SELL TO OPEN") => "SellToOpen",
                var a when a.Contains("BUY TO CLOSE") => "BuyToClose",
                var a when a.Contains("SELL TO CLOSE") => "SellToClose",
                var a when a.Contains("ASSIGNED") => "OptionAssigned",
                var a when a.Contains("EXERCISED") => "OptionExercised",
                var a when a.Contains("EXPIRED") => "OptionExpired",
                var a when a.Contains("DIVIDEND") => "Dividend",
                var a when a.Contains("BUY") && isOption => "BuyToOpen",
                var a when a.Contains("SELL") && isOption => "SellToOpen",
                _ => action
            };
        }

        private bool IsOptionSymbol(string symbol)
        {
            // Schwab format: SYMBOL MMDDYY C/P STRIKE or SYMBOLMMDDYYC/PSTRIKE
            return Regex.IsMatch(symbol, @"[A-Z]+\s*\d{6}[CP]\d+") || 
                   symbol.Contains(" C ") || symbol.Contains(" P ");
        }

        private bool IsOptionDescription(string description)
        {
            return description.Contains("CALL", StringComparison.OrdinalIgnoreCase) ||
                   description.Contains("PUT", StringComparison.OrdinalIgnoreCase) ||
                   description.Contains("OPTION", StringComparison.OrdinalIgnoreCase);
        }

        private void ParseSchwabOptionSymbol(string symbol, string description, ParsedTransaction transaction)
        {
            // Schwab format examples:
            // "AAPL 011725C150" = AAPL Jan 17 2025 $150 Call
            // "TSLA 120824P200" = TSLA Dec 08 2024 $200 Put
            
            var cleanSymbol = symbol.Replace(" ", "");
            
            // Pattern: SYMBOL MMDDYY C/P STRIKE
            var match = Regex.Match(cleanSymbol, @"([A-Z]+)(\d{2})(\d{2})(\d{2})([CP])([\d.]+)");
            
            if (match.Success)
            {
                // Extract underlying symbol
                transaction.Symbol = match.Groups[1].Value;
                
                // Extract date components
                var month = match.Groups[2].Value;
                var day = match.Groups[3].Value;
                var year = "20" + match.Groups[4].Value;
                
                // Extract option type
                transaction.OptionType = match.Groups[5].Value == "C" ? "Call" : "Put";
                
                // Extract strike price
                transaction.StrikePrice = ParseDecimal(match.Groups[6].Value);
                
                // Parse expiration date
                if (DateTime.TryParseExact($"{month}/{day}/{year}", "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var expDate))
                {
                    transaction.ExpirationDate = expDate;
                }
            }
            else
            {
                // Try parsing from description
                // Example: "AAPL CALL $150 EXP 01/17/25"
                transaction.Symbol = ExtractSymbolFromDescription(description);
                
                if (description.Contains("CALL", StringComparison.OrdinalIgnoreCase))
                    transaction.OptionType = "Call";
                else if (description.Contains("PUT", StringComparison.OrdinalIgnoreCase))
                    transaction.OptionType = "Put";

                var strikeMatch = Regex.Match(description, @"\$?([\d,]+\.?\d*)");
                if (strikeMatch.Success)
                {
                    transaction.StrikePrice = ParseDecimal(strikeMatch.Groups[1].Value);
                }

                var dateMatch = Regex.Match(description, @"(\d{1,2})/(\d{1,2})/(\d{2,4})");
                if (dateMatch.Success)
                {
                    var dateStr = dateMatch.Value;
                    if (DateTime.TryParse(dateStr, out var expDate))
                    {
                        transaction.ExpirationDate = expDate;
                    }
                }
            }

            transaction.Notes = $"Option: {description}";
        }

        private string ExtractSymbolFromDescription(string description)
        {
            var words = description.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (words.Length > 0)
            {
                return words[0].ToUpper();
            }
            return "";
        }

        private string CleanSymbol(string symbol)
        {
            // For options, extract just the underlying symbol
            var match = Regex.Match(symbol, @"^([A-Z]+)");
            if (match.Success)
                return match.Groups[1].Value;
            
            return Regex.Replace(symbol, @"[^A-Za-z0-9]", "").Trim().ToUpper();
        }

        private DateTime ParseDate(string dateStr)
        {
            if (string.IsNullOrWhiteSpace(dateStr))
                return DateTime.UtcNow;

            string[] formats = {
                "MM/dd/yyyy",
                "M/d/yyyy",
                "yyyy-MM-dd",
                "MM-dd-yyyy",
                "M/d/yy",
                "MM/dd/yy"
            };

            if (DateTime.TryParseExact(dateStr, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                return date;

            if (DateTime.TryParse(dateStr, out date))
                return date;

            return DateTime.UtcNow;
        }

        private decimal ParseDecimal(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return 0;

            value = value.Replace("$", "").Replace(",", "").Trim();
            
            if (value.StartsWith("(") && value.EndsWith(")"))
            {
                value = "-" + value.Trim('(', ')');
            }

            if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
                return result;

            return 0;
        }
    }
}
