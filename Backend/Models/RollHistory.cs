using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OptionsTracker.Models
{
    public class RollHistory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int FromOptionsPositionId { get; set; }
        [ForeignKey("FromOptionsPositionId")]
        public OptionsPosition FromPosition { get; set; } = null!;

        [Required]
        public int ToOptionsPositionId { get; set; }
        [ForeignKey("ToOptionsPositionId")]
        public OptionsPosition ToPosition { get; set; } = null!;

        [Required]
        public DateTime RollDate { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "decimal(18,4)")]
        public decimal NetCredit { get; set; } // Positive = credit, Negative = debit

        [MaxLength(500)]
        public string Notes { get; set; } = string.Empty;

        // Computed properties
        [NotMapped]
        public decimal OldStrike => FromPosition?.StrikePrice ?? 0;

        [NotMapped]
        public decimal NewStrike => ToPosition?.StrikePrice ?? 0;

        [NotMapped]
        public DateTime OldExpiration => FromPosition?.ExpirationDate ?? DateTime.MinValue;

        [NotMapped]
        public DateTime NewExpiration => ToPosition?.ExpirationDate ?? DateTime.MinValue;

        [NotMapped]
        public int DaysExtended => (NewExpiration.Date - OldExpiration.Date).Days;

        [NotMapped]
        public bool IsRollUp => NewStrike > OldStrike;

        [NotMapped]
        public bool IsRollDown => NewStrike < OldStrike;

        [NotMapped]
        public bool IsRollOut => NewExpiration > OldExpiration;
    }
}
