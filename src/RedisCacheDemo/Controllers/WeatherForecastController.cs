using Microsoft.AspNetCore.Mvc;
using RedisCacheDemo;
using RedisCacheDemo.Cache;

namespace RedisCacheDemo.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool",
            "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly ICacheService _cacheService;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, ICacheService cacheService)
        {
            _logger = logger;
            _cacheService = cacheService;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            var cacheData = _cacheService.GetData<IEnumerable<WeatherForecast>>(nameof(WeatherForecast));
            if (cacheData != null)
            {
                return cacheData;
            }

            var expirationTime = DateTimeOffset.Now.AddMinutes(5.0);
            cacheData = Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            }).ToArray();

            _cacheService.SetData(nameof(WeatherForecast), cacheData, expirationTime);
            return cacheData;
        }
    }
}