//using Basket.API.GrpcServices;
//using Basket.API.Mapper;
using Basket.API.GrpcServices;
using Basket.API.Repositories;
using Discount.Grpc.Protos;
using MassTransit;
using Microsoft.OpenApi.Models;
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

    });

});
//builder.Services.AddMassTransitHostedService();

//builder.Services.
//builder.Services.AddMassTransit(config =>
//{
//    config.UsingRabbitMq((ctx, cfg) =>
//    {
//        cfg.Host(new Uri("amqp://localhost"), h =>
//        {
//            h.Username("guest");
//            h.Password("guest");
//        });
//    });
//});
//builder.Services.AddMassTransitHostedService();

// Add services to the container.
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
