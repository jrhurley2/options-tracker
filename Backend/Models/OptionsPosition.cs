using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OptionsTracker.Models
{
    public enum OptionType
    {
        Call,
        Put
    }

    public enum OptionStrategy
    {
        Long,           // Bought option
        Short,          // Sold option
        CoveredCall,    // Short call with underlying stock
        CashSecuredPut, // Short put with cash reserve
        Spread          // Multi-leg strategy
    }

    public enum PositionStatus
    {
        Open,
        Closed,
        Expired,
        Assigned,
        Exercised,
        Rolled
    }

    public class OptionsPosition
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(10)]
        public string UnderlyingSymbol { get; set; } = string.Empty;

        [Required]
        public OptionType OptionType { get; set; }

        [Required]
        public OptionStrategy Strategy { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,4)")]
        public decimal StrikePrice { get; set; }

        [Required]
        public DateTime ExpirationDate { get; set; }

        [Required]
        public int Contracts { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal PremiumPerContract { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal CurrentPrice { get; set; }

        public DateTime OpenDate { get; set; } = DateTime.UtcNow;

        public DateTime? CloseDate { get; set; }

        [Required]
        public PositionStatus Status { get; set; } = PositionStatus.Open;

        [MaxLength(50)]
        public string Account { get; set; } = string.Empty;

        // For covered calls - link to underlying position
        public int? UnderlyingPositionId { get; set; }
        [ForeignKey("UnderlyingPositionId")]
        public Position? UnderlyingPosition { get; set; }

        // For tracking rolls
        public int? RolledFromId { get; set; }
        [ForeignKey("RolledFromId")]
        public OptionsPosition? RolledFrom { get; set; }

        public int? RolledToId { get; set; }
        [ForeignKey("RolledToId")]
        public OptionsPosition? RolledTo { get; set; }

        [MaxLength(500)]
        public string Notes { get; set; } = string.Empty;

        // Computed properties
        [NotMapped]
        public decimal TotalPremiumCollected => Contracts * PremiumPerContract * 100; // Options are per 100 shares

        [NotMapped]
        public decimal CurrentValue => Contracts * CurrentPrice * 100;

        [NotMapped]
        public decimal UnrealizedPnL
        {
            get
            {
                // For short positions (CC, CSP), we collected premium, so positive is good
                if (Strategy == OptionStrategy.CoveredCall || Strategy == OptionStrategy.CashSecuredPut || Strategy == OptionStrategy.Short)
                {
                    return TotalPremiumCollected - CurrentValue;
                }
                // For long positions, we paid premium
                else
                {
                    return CurrentValue - TotalPremiumCollected;
                }
            }
        }

        [NotMapped]
        public int DaysToExpiration => (ExpirationDate.Date - DateTime.UtcNow.Date).Days;

        [NotMapped]
        public decimal RequiredCapital
        {
            get
            {
                if (Strategy == OptionStrategy.CashSecuredPut)
                {
                    return Contracts * StrikePrice * 100;
                }
                return 0;
            }
        }

        // Navigation properties
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}
