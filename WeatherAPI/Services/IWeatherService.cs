using WeatherAPI.Models.External;

namespace WeatherAPI.Services;

public interface IWeatherService
{
    Task<WeatherResponse> GetWeatherAsync(string city);
}