using BaseCrud.Abstractions.Entities;
using BaseCrud.Errors;
using BaseCrud.PrimeNg;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using WebTester.DataBase;
using WebTester.Models;
using WebTester.Services;

namespace WebTester.Controllers;

public class WeatherForecastController(
    ILogger<WeatherForecastController> logger,
    IWeatherService service,
    AppDbContext context
) : BaseController
{
    /// <summary>
    /// Gets all weather forecasts from db
    /// </summary>
    /// <returns></returns>
    [HttpGet(Name = "GetWeatherForecast")]
    [SwaggerResponse(StatusCodes.Status200OK, typeof(WeatherForecast[]))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, typeof(ServiceError[]))]
    public async Task<ActionResult<IAsyncEnumerable<WeatherForecastDetailsDto>?>> Get()
    {
        if (!_init)
            Init().Wait();

        return await FromServiceResult(service.GetFullEntityListAsync(UserProfile));
    }


    /// <summary>
    /// Gets all WeatherForecast entities from db
    /// </summary>
    /// <param name="metaData">meta data for filtering, sorting and pagination</param>
    /// <returns>A query result of WeatherForecastDto</returns>
    [HttpPost("[action]")]
    [SwaggerResponse(StatusCodes.Status200OK, typeof(QueryResult<WeatherForecastDto>))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, typeof(ServiceError[]))]
    public async Task<ActionResult<QueryResult<WeatherForecastDto>?>> GetAll(PrimeTableMetaData metaData)
    {
        if (!_init)
            await Init();
        return await FromServiceResult(service.GetAllAsync(metaData, UserProfile));
    }







    private static bool _init;

    private async Task Init()
    {
        _init = true;

        var seed = new WeatherForecast[]
        {
            new()
            {
                Id = 1, TemperatureC = 15, Summary = "humid air", Date = DateOnly.FromDateTime(DateTime.Now.Date.AddDays(-2))
            },
            new()
            {
                Id = 2, TemperatureC = 30, Summary = "hot weather", Date = DateOnly.FromDateTime(DateTime.Now.Date.AddDays(-1))
            },
            new()
            {
                Id = 3, TemperatureC = -5, Summary = "freezing", Date = DateOnly.FromDateTime(DateTime.Now.Date)
            },
            new()
            {
                Id = 4, TemperatureC = 40, Summary = "rain", Date = DateOnly.FromDateTime(DateTime.Now.Date.AddDays(1))
            }
        };

        context.WeatherForecasts.AddRange(seed);

        await context.SaveChangesAsync();
    }
}