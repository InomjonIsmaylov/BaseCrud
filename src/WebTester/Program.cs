using System.Reflection;
using BaseCrud;
using BaseCrud.Abstractions;
using WebTester.Classes;
using WebTester.DataBase;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonOptions(o =>
{
    o.JsonSerializerOptions.Converters.Add(new ObjectToInferredTypesConverter());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppDbContext>();
builder.Logging.SetMinimumLevel(LogLevel.Debug).AddConsole();

builder.Services.AddBaseCrudService(new BaseCrudServiceOptions
{
    Assemblies = [Assembly.GetExecutingAssembly()]
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
