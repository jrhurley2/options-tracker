using Microsoft.AspNetCore.Mvc;
using OptionsTracker.Services;
using OptionsTracker.DTOs;

namespace OptionsTracker.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PositionsController : ControllerBase
    {
        private readonly IPositionService _positionService;

        public PositionsController(IPositionService positionService)
        {
            _positionService = positionService;
        }

        [HttpGet]
        public async Task<ActionResult<List<PositionDto>>> GetPositions([FromQuery] string? account = null)
        {
            var positions = await _positionService.GetAllPositionsAsync(account);
            return Ok(positions);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PositionDto>> GetPosition(int id)
        {
            var position = await _positionService.GetPositionByIdAsync(id);
            if (position == null)
                return NotFound();

            return Ok(position);
        }

        [HttpPost]
        public async Task<ActionResult<PositionDto>> CreatePosition([FromBody] CreatePositionRequest request)
        {
            var position = await _positionService.CreateOrUpdatePositionAsync(
                request.Symbol,
                request.Quantity,
                request.Price,
                request.Account
            );

            return CreatedAtAction(nameof(GetPosition), new { id = position.Id }, position);
        }

        [HttpPut("{id}/price")]
        public async Task<IActionResult> UpdatePrice(int id, [FromBody] UpdatePriceRequest request)
        {
            await _positionService.UpdatePositionPriceAsync(id, request.CurrentPrice);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePosition(int id)
        {
            var result = await _positionService.DeletePositionAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }
    }

    public class CreatePositionRequest
    {
        public string Symbol { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        public string Account { get; set; } = string.Empty;
    }

    public class UpdatePriceRequest
    {
        public decimal CurrentPrice { get; set; }
    }
}
