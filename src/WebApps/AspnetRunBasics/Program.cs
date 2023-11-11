using AspnetRunBasics.Services;
using Serilog;
using Serilog.Sinks.Elasticsearch;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Common.Logging;
using Polly.Extensions.Http;
using Polly;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using HealthChecks.UI.Client;

namespace AspnetRunBasics
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            ConfigurationManager configuration = builder.Configuration;
            
            //Registering the Delegate Handler.
            builder.Services.AddTransient<LoggingDelegatingHandler>();

            builder.Services.AddHttpClient<ICatalogService, CatalogService>(c =>
                            c.BaseAddress = new Uri(configuration["ApiSettings:GatewayAddress"]))
                            .AddHttpMessageHandler<LoggingDelegatingHandler>()
                            .AddPolicyHandler(GetRetryPolicy())
                            .AddPolicyHandler(GetCircuitBreakerPolicy());

            builder.Services.AddHttpClient<IBasketService, BasketService>(c =>
                c.BaseAddress = new Uri(configuration["ApiSettings:GatewayAddress"]))
                .AddHttpMessageHandler<LoggingDelegatingHandler>()
                .AddPolicyHandler(GetRetryPolicy())
                .AddPolicyHandler(GetCircuitBreakerPolicy());

            builder.Services.AddHttpClient<IOrderService, OrderService>(c =>
                c.BaseAddress = new Uri(configuration["ApiSettings:GatewayAddress"]))
                .AddHttpMessageHandler<LoggingDelegatingHandler>()
                .AddPolicyHandler(GetRetryPolicy())
                .AddPolicyHandler(GetCircuitBreakerPolicy());

            // Configure logging with Serilog
            builder.Services.AddSeriLogger(builder.Configuration);

            //This code will work only for one project ASPNETBASICS
            //builder.Services.AddSerilog((context, services) => {
            //    services.Enrich.FromLogContext();
            //    services.Enrich.WithMachineName();
            //    services.WriteTo.Console();
            //    services.WriteTo.Elasticsearch(
            //        new ElasticsearchSinkOptions(new Uri(configuration["ElasticConfiguration:Uri"]))
            //        {
            //            IndexFormat = $"applogs-{Assembly.GetExecutingAssembly().GetName().Name.ToLower().Replace(".", "-")}-{"Devlopment"?.ToLower().Replace(".", "-")}-logs-{DateTime.UtcNow:yyyy-MM}",
            //            AutoRegisterTemplate = true,
            //            NumberOfShards = 2,
            //            NumberOfReplicas = 1
            //        });
            //    services.Enrich.WithProperty("Environment", "Development");
            //    services.ReadFrom.Configuration(configuration);
            //});


            //builder.Services.AddSerilog(SeriLogger.ConfigureSerilog);
            // Add services to the container.
            builder.Services.AddRazorPages();

            // Configure Oceletgateway health checks
            builder.Services.AddHealthChecks().AddUrlGroup(new Uri(builder.Configuration["ApiSettings:GatewayAddress"]), "Ocelot API Gw", HealthStatus.Degraded);

            var app = builder.Build();

            

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
            }
            // Failed to determine the https port for redirect.
            //app.UseHttpsRedirection();
            app.UseStaticFiles();
            //app.UseSerilogRequestLogging(SeriLogger.Configure);
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

            app.MapRazorPages();

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
        }
    }
}