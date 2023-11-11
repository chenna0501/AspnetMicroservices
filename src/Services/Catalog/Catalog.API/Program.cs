using Catalog.API.Data;
using Catalog.API.Repositories;
using Common.Logging;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);
//ConfigurationManager configuration = builder.Configuration;
// Configure logging with Serilog
builder.Services.AddSeriLogger(builder.Configuration);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddScoped<ICatalogContext,CatalogContext > ();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// Configure MongoDb health checks
builder.Services.AddHealthChecks()
                    .AddMongoDb(builder.Configuration["DatabaseSettings:ConnectionString"], "Catalog MongoDb Health", HealthStatus.Degraded);

var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
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
