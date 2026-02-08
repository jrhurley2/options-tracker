using Microsoft.AspNetCore.Mvc;
using OptionsTracker.Services;
using OptionsTracker.Utilities;

namespace OptionsTracker.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImportController : ControllerBase
    {
        private readonly ICsvImportService _csvImportService;

        public ImportController(ICsvImportService csvImportService)
        {
            _csvImportService = csvImportService;
        }

        [HttpPost("csv")]
        public async Task<ActionResult<CsvImportResult>> ImportCsv(
            [FromForm] IFormFile file,
            [FromForm] string broker,
            [FromForm] string account)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded");

            if (string.IsNullOrWhiteSpace(broker))
                return BadRequest("Broker is required");

            if (string.IsNullOrWhiteSpace(account))
                return BadRequest("Account name is required");

            try
            {
                using var stream = file.OpenReadStream();
                var result = await _csvImportService.ImportCsvAsync(stream, broker, account);

                if (!result.Success)
                    return BadRequest(result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new CsvImportResult
                {
                    Success = false,
                    Errors = new List<string> { $"Import failed: {ex.Message}" }
                });
            }
        }

        [HttpGet("brokers")]
        public ActionResult<List<string>> GetSupportedBrokers()
        {
            return Ok(new List<string> { "Fidelity", "Schwab" });
        }
    }
}
