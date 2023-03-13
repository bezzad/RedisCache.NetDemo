using Microsoft.AspNetCore.Mvc;
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
            var cacheData = GetKeyValues();
            if (cacheData.Any())
            {
                return cacheData.Values;
            }

            var newData = Enumerable.Range(1, 10).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            }).ToArray();

            Save(newData, 50);
            return newData;
        }

        [HttpGet(nameof(WeatherForecast))]
        public WeatherForecast Get(int id)
        {
            var cacheData = GetKeyValues();
            cacheData.TryGetValue(id, out var filteredData);

            return filteredData;
        }

        [HttpPost("addWeatherForecast")]
        public async Task<WeatherForecast> Post(WeatherForecast value)
        {
            var cacheData = GetKeyValues();
            cacheData[value.Id] = value;
            Save(cacheData.Values);
            return value;
        }

        [HttpPut("updateWeatherForecast")]
        public void Put(WeatherForecast WeatherForecast)
        {
            var cacheData = GetKeyValues();
            cacheData[WeatherForecast.Id] = WeatherForecast;
            Save(cacheData.Values);
        }

        [HttpDelete("deleteWeatherForecast")]
        public void Delete(int id)
        {
            var cacheData = GetKeyValues();
            cacheData.Remove(id);
            Save(cacheData.Values);
        }

        [HttpDelete("ClearAll")]
        public void Delete()
        {
            _cacheService.Clear();
        }

        private void Save(IEnumerable<WeatherForecast> weatherForecasts, double expireAfterMinutes = 50)
        {
            var expirationTime = DateTimeOffset.Now.AddMinutes(expireAfterMinutes);
            _cacheService.SetData(nameof(WeatherForecast), weatherForecasts, expirationTime);
        }

        private Dictionary<int, WeatherForecast> GetKeyValues()
        {
            var data = _cacheService.Get<IEnumerable<WeatherForecast>>(nameof(WeatherForecast));
            return data?.ToDictionary(key => key.Id, val => val) ?? new Dictionary<int, WeatherForecast>();
        }
    }
}