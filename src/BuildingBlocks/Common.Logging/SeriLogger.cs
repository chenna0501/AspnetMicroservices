using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Sinks.Elasticsearch;
using System.Reflection;

namespace Common.Logging
{
    public static class SeriLogger
    {

        public static void AddSeriLogger(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddLogging(builder =>
            {
                var elasticUri = configuration.GetValue<string>("ElasticConfiguration:Uri");

                Log.Logger = new LoggerConfiguration()
                    .Enrich.FromLogContext()
                    .Enrich.WithMachineName()
                    .WriteTo.Debug()
                    .WriteTo.Console()
                    .WriteTo.Elasticsearch(
                        new ElasticsearchSinkOptions(new Uri(elasticUri))
                        {
                            IndexFormat = $"applogs-{configuration.GetValue<string>("ApplicationName")?.ToLower().Replace(".", "-")}-{configuration.GetValue<string>("Environment")?.ToLower().Replace(".", "-")}-{DateTime.UtcNow:yyyy-MM}",
                            AutoRegisterTemplate = true,
                            NumberOfShards = 2,
                            NumberOfReplicas = 1
                        })
                    .Enrich.WithProperty("Environment", configuration.GetValue<string>("Environment"))
                    .Enrich.WithProperty("Application", configuration.GetValue<string>("ApplicationName"))
                    .ReadFrom.Configuration(configuration)
                    .CreateLogger();

                builder.AddSerilog();
            });
        }


        
    }

}