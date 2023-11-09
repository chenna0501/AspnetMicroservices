using AspnetRunBasics.Services;
using Serilog;
using Serilog.Sinks.Elasticsearch;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Common.Logging;

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
                            c.BaseAddress = new Uri(configuration["ApiSettings:GatewayAddress"])).AddHttpMessageHandler<LoggingDelegatingHandler>();

            builder.Services.AddHttpClient<IBasketService, BasketService>(c =>
                c.BaseAddress = new Uri(configuration["ApiSettings:GatewayAddress"])).AddHttpMessageHandler<LoggingDelegatingHandler>();

            builder.Services.AddHttpClient<IOrderService, OrderService>(c =>
                c.BaseAddress = new Uri(configuration["ApiSettings:GatewayAddress"])).AddHttpMessageHandler<LoggingDelegatingHandler>();

            
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

            app.UseAuthorization();

            app.MapRazorPages();

            app.Run();
        }
    }
}