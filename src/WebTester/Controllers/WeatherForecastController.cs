using BaseCrud.Abstractions.Entities;
using BaseCrud.PrimeNg;
using BaseCrud.ServiceResults;
using Microsoft.AspNetCore.Mvc;
using WebTester.Classes;
using WebTester.Models;
using WebTester.Services;

namespace WebTester.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController(
    ILogger<WeatherForecastController> logger,
    IWeatherService service
) : ControllerBase
{
    private static readonly string[] Summaries =
    [
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    ];

    [HttpGet(Name = "GetWeatherForecast")]
    public IEnumerable<WeatherForecast> Get()
    {
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
    }

    [HttpPost("[action]")]
    public async Task<ActionResult<QueryResult<WeatherForecastDto>>> GetAll(PrimeTableMetaData metaData)
    {
        ServiceResult<QueryResult<WeatherForecastDto>> serviceResult = await service.GetAllAsync(metaData, new UserProfile());

        if (serviceResult.TryGetResult(out var result))
        {
            return Ok(result);
        }

        logger.LogDebug("Not going well");

        return StatusCode(
            serviceResult.StatusCode, serviceResult.Errors.ToString()
        );
    }
}