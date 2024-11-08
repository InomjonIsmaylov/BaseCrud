using AutoMapper;
using BaseCrud.EntityFrameworkCore;
using WebTester.DataBase;
using WebTester.Models;

namespace WebTester.Services;

public class WeatherService : BaseCrudService<WeatherForecast, WeatherForecastDto, WeatherForecastDetailsDto>, IWeatherService
{
    public WeatherService(
        AppDbContext dbContext,
        IMapper mapper,
        ILogger<IWeatherService> logger
        ) : base(dbContext, mapper)
    {

    }
}