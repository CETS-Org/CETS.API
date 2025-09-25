using Application.Interfaces.Common.Storage;
using DTOs;
using DTOs.Message.Requests;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using Utils.Helpers;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        //private readonly IPublishEndpoint _publishEndpoint;
        private readonly IdGenerator _idGenerator;
        private readonly IFileStorageService _fileStorageService;

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IdGenerator idGenerator, IFileStorageService fileStorageService)
        {
            _logger = logger;
            //_publishEndpoint = publishEndpoint;
            _idGenerator = idGenerator;
            _fileStorageService = fileStorageService;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }

        //[HttpPost("SendMessage")]
        //public async Task<IActionResult> SendEmail([FromBody] CreateMessageRequest message)
        //{
        //    await _publishEndpoint.Publish(message);
        //    return Ok("Message queued!");
        //}

        [HttpGet("GenerateId")]
        public IActionResult GenerateId()
        {
            var id = _idGenerator.GenerateId();
            return Ok(new { Id = id });
        }

        /// <summary>
        /// Test R2 connection
        /// </summary>
        [HttpGet("test-r2")]
        public async Task<ActionResult> TestR2Connection()
        {
            try
            {
                // Generate a simple presigned URL to test connection
                var testUrl = await _fileStorageService.GetTestPresignedUrlAsync();
                return Ok(new { message = "R2 connection successful", testUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "R2 connection test failed");
                return StatusCode(500, $"R2 connection failed: {ex.Message}");
            }
        }

    }
}
