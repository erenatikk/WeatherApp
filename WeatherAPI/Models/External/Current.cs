using System.Text.Json.Serialization;
namespace WeatherAPI.Models.External;

public class Current
{
    [JsonPropertyName("temp_c")]
    public double TempC { get; set; }

    [JsonPropertyName("temp_f")]
    public double TempF { get; set; }

    [JsonPropertyName("humidity")]
    public int Humidity { get; set; }

    [JsonPropertyName("wind_kph")]
    public double WindKph { get; set; }

    [JsonPropertyName("condition")]
    public Condition? Condition { get; set; }
    
    [JsonPropertyName("feelslike_c")]
    public double FeelsLikeC { get; set; }

    [JsonPropertyName("is_day")]
    public int IsDay { get; set; }
}