using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OptionsTracker.Models
{
    public enum TransactionType
    {
        // Stock transactions
        BuyStock,
        SellStock,
        
        // Options transactions
        BuyToOpen,
        SellToOpen,
        BuyToClose,
        SellToClose,
        
        // Assignment/Exercise
        OptionAssigned,
        OptionExercised,
        OptionExpired,
        
        // Dividend
        Dividend,
        
        // Other
        Fee,
        Commission
    }

    public class Transaction
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public TransactionType Type { get; set; }

        [Required]
        [MaxLength(10)]
        public string Symbol { get; set; } = string.Empty;

        [Required]
        public DateTime TransactionDate { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,4)")]
        public decimal Quantity { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal Price { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal Amount { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal Fees { get; set; } = 0;

        [MaxLength(50)]
        public string Account { get; set; } = string.Empty;

        // For options
        public OptionType? OptionType { get; set; }
        
        [Column(TypeName = "decimal(18,4)")]
        public decimal? StrikePrice { get; set; }
        
        public DateTime? ExpirationDate { get; set; }

        // Foreign keys
        public int? PositionId { get; set; }
        [ForeignKey("PositionId")]
        public Position? Position { get; set; }

        public int? OptionsPositionId { get; set; }
        [ForeignKey("OptionsPositionId")]
        public OptionsPosition? OptionsPosition { get; set; }

        [MaxLength(500)]
        public string Notes { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Source { get; set; } = "Manual"; // "Manual", "Fidelity CSV", "Schwab CSV", "Schwab API"

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
