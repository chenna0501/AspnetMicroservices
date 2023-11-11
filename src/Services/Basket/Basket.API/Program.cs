//using Basket.API.GrpcServices;
//using Basket.API.Mapper;
using Basket.API.GrpcServices;
using Basket.API.Repositories;
using Discount.Grpc.Protos;
using MassTransit;
using Microsoft.OpenApi.Models;
using Common.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using HealthChecks.UI.Client;
//using MassTransit.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
ConfigurationManager configuration = builder.Configuration;
// MassTransit-RabbitMQ Configuration



// Redis Configuration
builder.Services.AddStackExchangeRedisCache( options=>
    {
        options.Configuration = configuration["CacheSettings:ConnectionString"]; //"localhost:6379";
    });
// General Configuration
builder.Services.AddScoped<IBasketRepository, BasketRepository>();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
// Grpc Configuration
builder.Services.AddGrpcClient<DiscountProtoService.DiscountProtoServiceClient>
                        (o => o.Address = new Uri(configuration["GrpcSettings:DiscountUrl"]));
builder.Services.AddScoped<DiscountGrpcService>();

// MassTransit-RabbitMQ Configuration
builder.Services.AddMassTransit(config =>
{
    config.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(configuration["EventBusSettings:HostAddress"]);
        //cfg.UseHealthCheck(ctx);
       

    });
    config.AddHealthChecks();

});

// Configure logging with Serilog
builder.Services.AddSeriLogger(builder.Configuration);
// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Basket.API", Version = "v1" });
});

// Configure Redis health checks
builder.Services.AddHealthChecks()
                    .AddRedis(builder.Configuration["CacheSettings:ConnectionString"], "Redis Health", HealthStatus.Degraded);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Basket.API v1"));
}
// Use routing
app.UseRouting();
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Configure health check endpoint for app & MongoDb
app.UseEndpoints(endpoints =>
{
    endpoints.MapHealthChecks("/hc", new HealthCheckOptions
    {
        Predicate = _ => true, // Include all checks
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });
});


app.Run();
