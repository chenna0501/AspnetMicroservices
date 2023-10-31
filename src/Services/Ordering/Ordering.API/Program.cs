using System;
using Microsoft.Extensions.Configuration;
using Ordering.Infrastructure;
using Ordering.Application;
using Microsoft.EntityFrameworkCore;
using Ordering.Infrastructure.Persistence;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
ConfigurationManager configuration = builder.Configuration;
//string connString = configuration.GetConnectionString("OrderingConnectionString");
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(configuration);
//string connString = builder.Configuration.GetConnectionString("OrderingConnectionString");

//builder.Services.AddDbContext<OrderContext>(options =>
    //options.UseSqlServer(connString));


// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Ordering.API", Version = "v1" });
});

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OrderContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Ordering.API v1"));
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
