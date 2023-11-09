//using Discount.API.Extensions;
using Discount.API.Repositories;
using Microsoft.OpenApi.Models;
using Common.Logging;

var builder = WebApplication.CreateBuilder(args);
//ConfigurationManager configuration = builder.Configuration;

//builder.Services.AddScoped<HostExtensions>();
// Add services to the container.
builder.Services.AddScoped<IDiscountRepository, DiscountRepository>();
//builder.Services.AddHealthChecks().AddNpgSql(configuration["DatabaseSettings:ConnectionString"]);

// Configure logging with Serilog
builder.Services.AddSeriLogger(builder.Configuration);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Discount.API", Version = "v1" });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Discount.API v1"));
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
