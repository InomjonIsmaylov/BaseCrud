using BaseCrud.Entities;

namespace WebTester.Models;

public class WeatherForecastDto : IDataTransferObject<WeatherForecast>
{
    public DateOnly Date { get; set; }

    public int TemperatureC { get; set; }
}