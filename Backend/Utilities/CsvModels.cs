using System;
using System.Collections.Generic;

namespace OptionsTracker.Utilities
{
    public class CsvImportResult
    {
        public bool Success { get; set; }
        public int TransactionsImported { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public string Message { get; set; } = string.Empty;
    }

    public class ParsedTransaction
    {
        public string Type { get; set; } = string.Empty;
        public string Symbol { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; }
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Amount { get; set; }
        public decimal Fees { get; set; }
        public string Account { get; set; } = string.Empty;
        
        // Options specific
        public bool IsOption { get; set; }
        public string? OptionType { get; set; }
        public decimal? StrikePrice { get; set; }
        public DateTime? ExpirationDate { get; set; }
        
        public string Notes { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public int LineNumber { get; set; }
    }

    public interface ICsvParser
    {
        Task<List<ParsedTransaction>> ParseCsvAsync(Stream fileStream, string accountName);
        string BrokerName { get; }
    }
}
