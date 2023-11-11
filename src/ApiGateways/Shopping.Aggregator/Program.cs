using Common.Logging;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Polly;
using Polly.Extensions.Http;
using Serilog;
using Shopping.Aggregator.Services;

var builder = WebApplication.CreateBuilder(args);

//Registering the Delegate Handler.
builder.Services.AddTransient<LoggingDelegatingHandler>();

//Add http client services at ConfigureServices(IServiceCollection services)
builder.Services.AddHttpClient<ICatalogService, CatalogService>(c =>
{
    c.BaseAddress = new Uri(builder.Configuration["ApiSettings:CatalogUrl"]);
}).AddHttpMessageHandler<LoggingDelegatingHandler>()
.AddPolicyHandler(GetRetryPolicy())
.AddPolicyHandler(GetCircuitBreakerPolicy());
builder.Services.AddHttpClient<IBasketService, BasketService>(c =>
{
    c.BaseAddress = new Uri(builder.Configuration["ApiSettings:BasketUrl"]);
}).AddHttpMessageHandler<LoggingDelegatingHandler>()
.AddPolicyHandler(GetRetryPolicy())
.AddPolicyHandler(GetCircuitBreakerPolicy());
builder.Services.AddHttpClient<IOrderService, OrderService>(c =>
{
    c.BaseAddress = new Uri(builder.Configuration["ApiSettings:OrderingUrl"]);
}).AddHttpMessageHandler<LoggingDelegatingHandler>()
.AddPolicyHandler(GetRetryPolicy())
.AddPolicyHandler(GetCircuitBreakerPolicy());

// Configure logging with Serilog
builder.Services.AddSeriLogger(builder.Configuration);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHealthChecks()
    .AddUrlGroup(new Uri($"{builder.Configuration["ApiSettings:CatalogUrl"]}/swagger/index.html"), "Catalog.API", HealthStatus.Degraded)
    .AddUrlGroup(new Uri($"{builder.Configuration["ApiSettings:BasketUrl"]}/swagger/index.html"), "Basket.API", HealthStatus.Degraded)
    .AddUrlGroup(new Uri($"{builder.Configuration["ApiSettings:OrderingUrl"]}/swagger/index.html"), "Ordering.API", HealthStatus.Degraded);


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
// Use routing
app.UseRouting();

static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    // In this case will wait for
    //  2 ^ 1 = 2 seconds then
    //  2 ^ 2 = 4 seconds then
    //  2 ^ 3 = 8 seconds then
    //  2 ^ 4 = 16 seconds then
    //  2 ^ 5 = 32 seconds

    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryAsync(
            retryCount: 5,
            sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            onRetry: (exception, retryCount, context) =>
            {
                Log.Error($"Retry {retryCount} of {context.PolicyKey} at {context.OperationKey}, due to: {exception}.");
            });
}
static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .CircuitBreakerAsync(
            handledEventsAllowedBeforeBreaking: 5,
            durationOfBreak: TimeSpan.FromSeconds(30)
        );
}
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
