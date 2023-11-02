using Ocelot.Middleware;
using Ocelot.DependencyInjection;
using Ocelot.Values;
using Ocelot.Cache.CacheManager;

namespace OcelotApiGw
{

    public class Program
    {
        public static void Main(string[] args)
        {
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
            })
            .ConfigureLogging((hostingContext, loggingbuilder) =>
            {
                //add your logging
                loggingbuilder.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                loggingbuilder.AddConsole();
                loggingbuilder.AddDebug();
            })
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