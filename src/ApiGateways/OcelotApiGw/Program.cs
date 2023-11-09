using Ocelot.Middleware;
using Ocelot.DependencyInjection;
using Ocelot.Values;
using Ocelot.Cache.CacheManager;
using Common.Logging;


namespace OcelotApiGw
{

    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            // Configure logging with Serilog
            //builder.Services.AddSeriLogger(builder.Configuration);
            new WebHostBuilder()
            .UseKestrel()
            .UseContentRoot(Directory.GetCurrentDirectory())
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                config
                    //.SetBasePath(hostingContext.HostingEnvironment.ContentRootPath)
                    //.AddJsonFile("appsettings.json", true, true)
                    .AddJsonFile($"ocelot.{hostingContext.HostingEnvironment.EnvironmentName}.json", true, true);
                    //.AddJsonFile("ocelot.json")
                    //.AddEnvironmentVariables();
            })
            .ConfigureServices(s => {
                s.AddOcelot()
        .AddCacheManager(x => x.WithDictionaryHandle());
                s.AddSeriLogger(builder.Configuration);
            })
            //.ConfigureLogging((hostingContext, loggingbuilder) =>
            //{
            //    //add your logging
            //    loggingbuilder.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
            //    loggingbuilder.AddConsole();
            //    loggingbuilder.AddDebug();
            //})
            .UseIISIntegration()
            .Configure(app =>
            {
                app.UseOcelot().Wait();
            })
            .Build()
            .Run();
        }
    }
   }