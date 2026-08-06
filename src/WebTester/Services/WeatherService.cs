using BaseCrud.Abstractions.Mapping;
using BaseCrud.EntityFrameworkCore;
using WebTester.DataBase;
using WebTester.Models;

namespace WebTester.Services;

public class WeatherService : BaseCrudService<WeatherForecast, WeatherForecastDto, WeatherForecastDetailsDto>, IWeatherService
{
    public WeatherService(
        AppDbContext dbContext,
        IDtoMappingRegistry mappingRegistry,
        ILogger<IWeatherService> logger
        ) : base(dbContext, mappingRegistry)
    {

    }
}
