using Microsoft.EntityFrameworkCore;
using OptionsTracker.Models;

namespace OptionsTracker.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Position> Positions { get; set; }
        public DbSet<OptionsPosition> OptionsPositions { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<RollHistory> RollHistories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Position configuration
            modelBuilder.Entity<Position>()
                .HasIndex(p => p.Symbol);

            modelBuilder.Entity<Position>()
                .HasIndex(p => p.Account);

            // OptionsPosition configuration
            modelBuilder.Entity<OptionsPosition>()
                .HasIndex(o => o.UnderlyingSymbol);

            modelBuilder.Entity<OptionsPosition>()
                .HasIndex(o => o.ExpirationDate);

            modelBuilder.Entity<OptionsPosition>()
                .HasIndex(o => o.Status);

            // Self-referencing relationship for rolls
            modelBuilder.Entity<OptionsPosition>()
                .HasOne(o => o.RolledFrom)
                .WithOne(o => o.RolledTo)
                .HasForeignKey<OptionsPosition>(o => o.RolledFromId)
                .OnDelete(DeleteBehavior.Restrict);

            // Transaction configuration
            modelBuilder.Entity<Transaction>()
                .HasIndex(t => t.TransactionDate);

            modelBuilder.Entity<Transaction>()
                .HasIndex(t => t.Symbol);

            modelBuilder.Entity<Transaction>()
                .HasIndex(t => t.Account);

            // RollHistory configuration
            modelBuilder.Entity<RollHistory>()
                .HasOne(r => r.FromPosition)
                .WithMany()
                .HasForeignKey(r => r.FromOptionsPositionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RollHistory>()
                .HasOne(r => r.ToPosition)
                .WithMany()
                .HasForeignKey(r => r.ToOptionsPositionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
