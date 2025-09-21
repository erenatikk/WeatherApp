using System.Text.Json;
using WeatherAPI.Models.External;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;

namespace WeatherAPI.Services;

public class WeatherService : IWeatherService
{
    private readonly HttpClient _client;
    private readonly string _baseUrl;
    private readonly string _apiKey;
    private readonly IMemoryCache _cache;
    private readonly ILogger<WeatherService> _logger;

    public WeatherService(HttpClient client, IConfiguration config, ILogger<WeatherService> logger,  IMemoryCache cache)
    {
        _client = client;
        _apiKey = config["WeatherApi:ApiKey"] ?? throw new Exception("API Key bulunamadı!");
        _baseUrl = config["WeatherApi:BaseUrl"] ?? "http://api.weatherapi.com/v1";
        _logger = logger;
        _cache = cache;
    }

    public async Task<WeatherResponse> GetWeatherAsync(string city)
    {
        string cacheKey = $"weather_{city.ToLowerInvariant()}";
        if (_cache.TryGetValue(cacheKey, out WeatherResponse cachedWeather))
        {
            _logger.LogInformation("Returned data from cache.");
            return cachedWeather;
        }
        
        _logger.LogInformation("Getting weather for city {city}", city);
        string url = $"{_baseUrl}/current.json?key={_apiKey}&q={city}";

        try
        {
            var response = await _client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
                    response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    _logger.LogWarning("Invalid API Key. City:{city}", city);
                    throw new InvalidOperationException("Invalid API Key");
                }

                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest ||
                    response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("City not found. City:{city}", city);
                    throw new KeyNotFoundException($"'{city}' City Not Found!");
                }

                _logger.LogWarning("HTTP error {statusCode} for city {city}", response.StatusCode, city);
                throw new HttpRequestException($"HTTP error: {response.StatusCode}");
            }

            var content = await response.Content.ReadFromJsonAsync<WeatherResponse>();

            if (content != null)
            {
                var cacheEntryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(5));
                _cache.Set(cacheKey, content, cacheEntryOptions);
            }
            
            return content;
        }
        catch (JsonException jsonException)
        {
            _logger.LogError(jsonException, "Weather data parsing error for city {city}", city);
            throw new Exception("Weather data is not eligible. Check the API response.", jsonException);
        }
        catch (HttpRequestException httpRequestException)
        {
            _logger.LogError(httpRequestException, "HTTP request error for city {city}", city);
            throw new Exception("Cannot reach the weather service. Please try again later.", httpRequestException);
        }
        catch (KeyNotFoundException)
        {
            throw;
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while getting weather for city {city}", city);
            throw;
        }
    }
}