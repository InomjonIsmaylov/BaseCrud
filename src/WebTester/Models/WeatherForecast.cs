using System.ComponentModel.DataAnnotations.Schema;
using BaseCrud.Abstractions.Entities;
using BaseCrud.Expressions;
using BaseCrud.Expressions.Filter;

namespace WebTester.Models;

public class WeatherForecastConfiguration : IFilterExpression<WeatherForecast>
{
    public Func<FilterExpressions<WeatherForecast>, FilterExpressions<WeatherForecast>> FilterExpressions
        => builder => builder
            .ForProperty(
                w => w.Summary,
                summaryExpressions => summaryExpressions
                    .HasFilter((w, filterValue) => w.Summary == filterValue, ExpressionConstraintsEnum.Equals)
                    .HasFilter((w, filterValue) => w.Summary!.Contains(filterValue!),
                        ExpressionConstraintsEnum.Contains)
            )
            .AddRule("is_monday", w => w.Date.DayOfWeek == DayOfWeek.Monday)
            .AddRule<DayOfWeek>("is_day", (w, filterValue) => w.Date.DayOfWeek == filterValue);
}

public class WeatherForecast : EntityBase
{
    public DateOnly Date { get; set; }

    public int TemperatureC { get; set; }

    [NotMapped]
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

    public string? Summary { get; set; }
}