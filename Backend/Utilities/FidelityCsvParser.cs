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
    /// Parses Fidelity CSV export files
    /// Format typically includes: Run Date, Account, Action, Symbol, Security Description, 
    /// Security Type, Quantity, Price, Commission, Fees, Accrued Interest, Amount, Settlement Date
    /// </summary>
    public class FidelityCsvParser : ICsvParser
    {
        public string BrokerName => "Fidelity";

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

            await csv.ReadAsync();
            csv.ReadHeader();
            int lineNumber = 1;

            while (await csv.ReadAsync())
            {
                lineNumber++;
                try
                {
                    var action = csv.GetField<string>("Action") ?? "";
                    var symbol = csv.GetField<string>("Symbol") ?? "";
                    var description = csv.GetField<string>("Security Description") ?? "";
                    var securityType = csv.GetField<string>("Security Type") ?? "";
                    var quantityStr = csv.GetField<string>("Quantity") ?? "0";
                    var priceStr = csv.GetField<string>("Price") ?? "0";
                    var amountStr = csv.GetField<string>("Amount") ?? "0";
                    var feesStr = csv.GetField<string>("Fees") ?? "0";
                    var commissionStr = csv.GetField<string>("Commission") ?? "0";
                    var settleDateStr = csv.GetField<string>("Settlement Date") ?? "";

                    // Skip empty rows
                    if (string.IsNullOrWhiteSpace(symbol) && string.IsNullOrWhiteSpace(action))
                        continue;

                    var transaction = new ParsedTransaction
                    {
                        Symbol = CleanSymbol(symbol),
                        TransactionDate = ParseDate(settleDateStr),
                        Quantity = ParseDecimal(quantityStr),
                        Price = ParseDecimal(priceStr),
                        Amount = ParseDecimal(amountStr),
                        Fees = ParseDecimal(feesStr) + ParseDecimal(commissionStr),
                        Account = accountName,
                        Source = BrokerName,
                        LineNumber = lineNumber
                    };

                    // Determine transaction type
                    transaction.Type = MapFidelityAction(action, securityType);

                    // Check if this is an options transaction
                    if (securityType.Contains("OPTION", StringComparison.OrdinalIgnoreCase) ||
                        description.Contains("CALL", StringComparison.OrdinalIgnoreCase) ||
                        description.Contains("PUT", StringComparison.OrdinalIgnoreCase))
                    {
                        transaction.IsOption = true;
                        ParseOptionDetails(description, transaction);
                    }

                    transactions.Add(transaction);
                }
                catch (Exception ex)
                {
                    // Log error but continue processing
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

        private string MapFidelityAction(string action, string securityType)
        {
            var actionUpper = action.ToUpper();
            var isOption = securityType.Contains("OPTION", StringComparison.OrdinalIgnoreCase);

            return actionUpper switch
            {
                var a when a.Contains("YOU BOUGHT") && !isOption => "BuyStock",
                var a when a.Contains("YOU SOLD") && !isOption => "SellStock",
                var a when a.Contains("YOU BOUGHT") && isOption => "BuyToOpen",
                var a when a.Contains("YOU SOLD") && isOption => "SellToOpen",
                var a when a.Contains("OPENING TRANSACTION") => "SellToOpen",
                var a when a.Contains("CLOSING TRANSACTION") => "BuyToClose",
                var a when a.Contains("ASSIGNMENT") => "OptionAssigned",
                var a when a.Contains("EXERCISE") => "OptionExercised",
                var a when a.Contains("EXPIRED") => "OptionExpired",
                var a when a.Contains("DIVIDEND") => "Dividend",
                _ => action
            };
        }

        private void ParseOptionDetails(string description, ParsedTransaction transaction)
        {
            // Example: "AAPL Jan 17 2025 $150 CALL"
            // Example: "CALL (AAPL) APPLE INC JAN 17 25 $150"
            
            // Determine option type
            if (description.Contains("CALL", StringComparison.OrdinalIgnoreCase))
                transaction.OptionType = "Call";
            else if (description.Contains("PUT", StringComparison.OrdinalIgnoreCase))
                transaction.OptionType = "Put";

            // Extract strike price
            var strikeMatch = Regex.Match(description, @"\$?([\d,]+\.?\d*)");
            if (strikeMatch.Success)
            {
                transaction.StrikePrice = ParseDecimal(strikeMatch.Groups[1].Value);
            }

            // Extract expiration date - try multiple patterns
            // Pattern 1: "Jan 17 2025" or "JAN 17 25"
            var dateMatch = Regex.Match(description, @"([A-Z]{3})\s+(\d{1,2})\s+(\d{2,4})", RegexOptions.IgnoreCase);
            if (dateMatch.Success)
            {
                var month = dateMatch.Groups[1].Value;
                var day = dateMatch.Groups[2].Value;
                var year = dateMatch.Groups[3].Value;
                
                // Handle 2-digit year
                if (year.Length == 2)
                    year = "20" + year;

                var dateStr = $"{month} {day} {year}";
                if (DateTime.TryParseExact(dateStr, "MMM d yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var expDate))
                {
                    transaction.ExpirationDate = expDate;
                }
            }

            transaction.Notes = $"Option: {description}";
        }

        private string CleanSymbol(string symbol)
        {
            // Remove any non-alphanumeric characters except spaces
            return Regex.Replace(symbol, @"[^A-Za-z0-9\s]", "").Trim().ToUpper();
        }

        private DateTime ParseDate(string dateStr)
        {
            if (string.IsNullOrWhiteSpace(dateStr))
                return DateTime.UtcNow;

            // Try common date formats
            string[] formats = {
                "MM/dd/yyyy",
                "M/d/yyyy",
                "yyyy-MM-dd",
                "MM-dd-yyyy"
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

            // Remove currency symbols, commas, parentheses (negative)
            value = value.Replace("$", "").Replace(",", "").Trim();
            
            // Handle parentheses as negative
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
