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
            var cacheData = _cacheService.GetData<IEnumerable<WeatherForecast>>(nameof(WeatherForecast));
            if (cacheData != null)
            {
                return cacheData;
            }

            cacheData = Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            }).ToArray();

            Save(cacheData, 50);
            return cacheData;
        }

        [HttpGet(nameof(WeatherForecast))]
        public WeatherForecast Get(string id)
        {
            WeatherForecast filteredData;
            var cacheData = _cacheService.GetData<IEnumerable<WeatherForecast>>(nameof(WeatherForecast));
            if (cacheData != null)
            {
                filteredData = cacheData.Where(x => x.Id == id).FirstOrDefault();
                return filteredData;
            }

            cacheData = Get();
            return cacheData.Where(w => w.Id == id).FirstOrDefault();
        }

        [HttpPost("addWeatherForecast")]
        public async Task<WeatherForecast> Post(WeatherForecast value)
        {
            var cacheData = Get().ToList();
            cacheData.Add(value);
            Save(cacheData);
            return value;
        }

        [HttpPut("updateWeatherForecast")]
        public void Put(WeatherForecast WeatherForecast)
        {
            var cacheData = Get().ToDictionary(key => key.Id, val => val);
            cacheData[WeatherForecast.Id] = WeatherForecast;
            Save(cacheData.Values);
        }

        [HttpDelete("deleteWeatherForecast")]
        public void Delete(string id)
        {
            var cacheData = Get().ToDictionary(key => key.Id, val => val);
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
    }
}