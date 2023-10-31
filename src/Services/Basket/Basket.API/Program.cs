//using Basket.API.GrpcServices;
//using Basket.API.Mapper;
using Basket.API.GrpcServices;
using Basket.API.Repositories;
using Discount.Grpc.Protos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
ConfigurationManager configuration = builder.Configuration;

// Add services to the container.

builder.Services.AddStackExchangeRedisCache( options=>
    {
        options.Configuration = configuration["CacheSettings:ConnectionString"]; //"localhost:6379";
    });
builder.Services.AddScoped<IBasketRepository, BasketRepository>();
// Grpc Configuration
builder.Services.AddGrpcClient<DiscountProtoService.DiscountProtoServiceClient>
                        (o => o.Address = new Uri(configuration["GrpcSettings:DiscountUrl"]));
builder.Services.AddScoped<DiscountGrpcService>();
//builder.Services.AddRouting();
//builder.Services.AddScoped<BasketProfile>();
//builder.Services.AddScoped<DiscountGrpcService>();
//builder.Services.AddHealthChecks().AddRedis("localhost:6379", "Redis Health", HealthStatus.Degraded);
//builder.Services.AddHealthChecks().AddCheck<Red>();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Basket.API", Version = "v1" });
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Basket.API v1"));
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
