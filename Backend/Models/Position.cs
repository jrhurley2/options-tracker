using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OptionsTracker.Models
{
    public class Position
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(10)]
        public string Symbol { get; set; } = string.Empty;

        [Required]
        public decimal Quantity { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal AverageCost { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal CurrentPrice { get; set; }

        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        [MaxLength(50)]
        public string Account { get; set; } = string.Empty;

        // Computed properties
        [NotMapped]
        public decimal TotalCost => Quantity * AverageCost;

        [NotMapped]
        public decimal MarketValue => Quantity * CurrentPrice;

        [NotMapped]
        public decimal UnrealizedPnL => MarketValue - TotalCost;

        [NotMapped]
        public decimal UnrealizedPnLPercent => TotalCost != 0 ? (UnrealizedPnL / TotalCost) * 100 : 0;

        // Navigation properties
        public ICollection<OptionsPosition> CoveredCalls { get; set; } = new List<OptionsPosition>();
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}
