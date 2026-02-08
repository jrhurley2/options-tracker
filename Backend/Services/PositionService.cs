using Microsoft.EntityFrameworkCore;
using OptionsTracker.Data;
using OptionsTracker.Models;
using OptionsTracker.DTOs;

namespace OptionsTracker.Services
{
    public interface IPositionService
    {
        Task<List<PositionDto>> GetAllPositionsAsync(string? account = null);
        Task<PositionDto?> GetPositionByIdAsync(int id);
        Task<PositionDto> CreateOrUpdatePositionAsync(string symbol, decimal quantity, decimal price, string account);
        Task UpdatePositionPriceAsync(int id, decimal currentPrice);
        Task<bool> DeletePositionAsync(int id);
    }

    public class PositionService : IPositionService
    {
        private readonly ApplicationDbContext _context;

        public PositionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<PositionDto>> GetAllPositionsAsync(string? account = null)
        {
            var query = _context.Positions
                .Include(p => p.CoveredCalls)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(account))
            {
                query = query.Where(p => p.Account == account);
            }

            var positions = await query.ToListAsync();

            return positions.Select(p => new PositionDto
            {
                Id = p.Id,
                Symbol = p.Symbol,
                Quantity = p.Quantity,
                AverageCost = p.AverageCost,
                CurrentPrice = p.CurrentPrice,
                TotalCost = p.TotalCost,
                MarketValue = p.MarketValue,
                UnrealizedPnL = p.UnrealizedPnL,
                UnrealizedPnLPercent = p.UnrealizedPnLPercent,
                LastUpdated = p.LastUpdated,
                Account = p.Account,
                CoveredCallsCount = p.CoveredCalls.Count(cc => cc.Status == PositionStatus.Open)
            }).ToList();
        }

        public async Task<PositionDto?> GetPositionByIdAsync(int id)
        {
            var position = await _context.Positions
                .Include(p => p.CoveredCalls)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (position == null)
                return null;

            return new PositionDto
            {
                Id = position.Id,
                Symbol = position.Symbol,
                Quantity = position.Quantity,
                AverageCost = position.AverageCost,
                CurrentPrice = position.CurrentPrice,
                TotalCost = position.TotalCost,
                MarketValue = position.MarketValue,
                UnrealizedPnL = position.UnrealizedPnL,
                UnrealizedPnLPercent = position.UnrealizedPnLPercent,
                LastUpdated = position.LastUpdated,
                Account = position.Account,
                CoveredCallsCount = position.CoveredCalls.Count(cc => cc.Status == PositionStatus.Open)
            };
        }

        public async Task<PositionDto> CreateOrUpdatePositionAsync(string symbol, decimal quantity, decimal price, string account)
        {
            var existingPosition = await _context.Positions
                .FirstOrDefaultAsync(p => p.Symbol == symbol && p.Account == account);

            if (existingPosition != null)
            {
                // Update existing position
                var totalCost = existingPosition.TotalCost + (quantity * price);
                var totalQuantity = existingPosition.Quantity + quantity;

                if (totalQuantity > 0)
                {
                    existingPosition.AverageCost = totalCost / totalQuantity;
                    existingPosition.Quantity = totalQuantity;
                }
                else
                {
                    // Position closed
                    existingPosition.Quantity = 0;
                }

                existingPosition.CurrentPrice = price;
                existingPosition.LastUpdated = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return await GetPositionByIdAsync(existingPosition.Id) ?? throw new Exception("Failed to retrieve updated position");
            }
            else
            {
                // Create new position
                var newPosition = new Position
                {
                    Symbol = symbol,
                    Quantity = quantity,
                    AverageCost = price,
                    CurrentPrice = price,
                    Account = account,
                    LastUpdated = DateTime.UtcNow
                };

                _context.Positions.Add(newPosition);
                await _context.SaveChangesAsync();

                return await GetPositionByIdAsync(newPosition.Id) ?? throw new Exception("Failed to retrieve new position");
            }
        }

        public async Task UpdatePositionPriceAsync(int id, decimal currentPrice)
        {
            var position = await _context.Positions.FindAsync(id);
            if (position != null)
            {
                position.CurrentPrice = currentPrice;
                position.LastUpdated = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> DeletePositionAsync(int id)
        {
            var position = await _context.Positions.FindAsync(id);
            if (position == null)
                return false;

            _context.Positions.Remove(position);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
