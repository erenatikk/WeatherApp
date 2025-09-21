namespace WeatherAPI.Models.External;

public class WeatherResponse
{
    public Location? Location { get; set; }
    public Current? Current { get; set; }
}