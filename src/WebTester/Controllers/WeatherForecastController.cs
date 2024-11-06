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
    public IEnumerable<WeatherForecastDetailsDto> Get()
    {
        return Enumerable.Range(1, 5).Select(index => new WeatherForecastDetailsDto
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
    }


    /// <summary>
    /// Gets all WeatherForecast entities from db
    /// </summary>
    /// <param name="metaData">meta data for filtering, sorting and pagination</param>
    /// <returns>A query result of WeatherForecastDto</returns>
    [HttpPost("[action]")]
    [ProducesResponseType(200)]
    public async Task<ActionResult<QueryResult<WeatherForecastDto>>> GetAll(PrimeTableMetaData metaData)
    {
        ServiceResult<QueryResult<WeatherForecastDto>> serviceResult = await service.GetAllAsync(metaData, new UserProfile());

        if (serviceResult.TryGetResult(out QueryResult<WeatherForecastDto>? result))
        {
            return Ok(result);
        }

        logger.LogDebug("Not going well");

        return StatusCode(
            serviceResult.StatusCode, serviceResult.Errors.ToString()
        );
    }
}