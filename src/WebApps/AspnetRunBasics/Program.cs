using AspnetRunBasics.Services;

namespace AspnetRunBasics
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            ConfigurationManager configuration = builder.Configuration;
            builder.Services.AddHttpClient<ICatalogService, CatalogService>(c =>
                            c.BaseAddress = new Uri(configuration["ApiSettings:GatewayAddress"]));

            builder.Services.AddHttpClient<IBasketService, BasketService>(c =>
                c.BaseAddress = new Uri(configuration["ApiSettings:GatewayAddress"]));

            builder.Services.AddHttpClient<IOrderService, OrderService>(c =>
                c.BaseAddress = new Uri(configuration["ApiSettings:GatewayAddress"]));

            // Add services to the container.
            builder.Services.AddRazorPages();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
            }
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapRazorPages();

            app.Run();
        }
    }
}