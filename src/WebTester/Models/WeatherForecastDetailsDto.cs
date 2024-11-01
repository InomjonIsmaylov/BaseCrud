using System.ComponentModel.DataAnnotations.Schema;
using BaseCrud.Entities;

namespace WebTester.Models;

public class WeatherForecastDetailsDto : IDataTransferObject<WeatherForecast>
{
    public DateOnly Date { get; set; }

    public int TemperatureC { get; set; }

    [NotMapped]
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

    public string? Summary { get; set; }
}