using System.Text.Json;
using BaseCrud.Abstractions.Entities;
using BaseCrud.General.AutoMappers;
using BaseCrud.PrimeNg;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tester;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<AppDbContext>();
builder.Services.AddScoped<AppDbContext>();

builder.Services.AddAutoMapper(c =>
{
    c.AddProfile(new DtoMapperProfile(typeof(Service).Assembly));
});

builder.Services.AddScoped<IService, Service>();

using var host = builder.Build();






const string b = """
                 {
                   "first": 0,
                   "rows": 10,
                   "sortField": "id",
                   "sortOrder": 1,
                   "filters": {},
                   "globalFilter": null
                 }
                 """;

var m = JsonSerializer.Deserialize<DataTableMetaData>(b)!;

await PlayGroundWithDiAsync(host.Services, m);

await host.RunAsync();

return;

static async Task PlayGroundWithDiAsync(IServiceProvider hostProvider, IDataTableMetaData metaData)
{
    using var serviceScope = hostProvider.CreateScope();
    var service = serviceScope.ServiceProvider.GetRequiredService<IService>();

    var a = new ModelDetailsDto
    {
        Age = "1",
        Address = "address",
        Name = "name",
        Email = "email",
        Phone = "phone",
    };

    var b = JsonSerializer.Deserialize<ModelDetailsDto>(JsonSerializer.Serialize(a))!;

    var user = new UserProfile();

    try
    {
        var entity = await service.InsertAsync(b);

        var entity1 = await service.GetByIdAsync(entity.Id, user);

        Console.WriteLine(entity1);

        var all = await service.GetAllAsync(metaData, user);
        
        Console.WriteLine(all);
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
    }
}