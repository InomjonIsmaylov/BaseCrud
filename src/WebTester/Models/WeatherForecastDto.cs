using BaseCrud.Entities;

namespace WebTester.Models;

/// <summary>
/// Weather forecast model
/// </summary>
public class WeatherForecastDto : IDataTransferObject<WeatherForecast>
{
    public DateOnly Date { get; set; }

    /// <summary>
    /// Temperature in Celsius
    /// </summary>
    public int TemperatureC { get; set; }
}