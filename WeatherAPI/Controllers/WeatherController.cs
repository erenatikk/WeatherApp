using Microsoft.AspNetCore.Mvc;
using WeatherAPI.Models.External;
using WeatherAPI.Services;

namespace WeatherAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WeatherController : ControllerBase
{
    private readonly IWeatherService _weatherService;

    public WeatherController(IWeatherService weatherService)
    {
        _weatherService = weatherService;
    }

    [HttpGet("{city}")]
    public async Task<IActionResult> Get(string city)
    {
        if (string.IsNullOrWhiteSpace(city))
        {
            return BadRequest("City cannot be empty");
        }

        try
        {
            var result = await _weatherService.GetWeatherAsync(city);
            
            if (result == null)
            {
                return NotFound($"'{city}' not found");
            }
            return Ok(result);

        }
        catch (KeyNotFoundException ex)
        {
            // Şehir bulunamadı hatası
            return NotFound(new { Error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            // API anahtarı hatası
            return Unauthorized(new { Error = ex.Message });
        }
        catch (Exception ex)
        {
            // Genel sunucu hatası
            return StatusCode(500, new { Error = "Server side error. Please try again later.", Desc = ex.Message });
        }

        

    }
}