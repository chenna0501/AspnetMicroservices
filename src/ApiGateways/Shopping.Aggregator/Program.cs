using Common.Logging;
using Shopping.Aggregator.Services;

var builder = WebApplication.CreateBuilder(args);

//Registering the Delegate Handler.
builder.Services.AddTransient<LoggingDelegatingHandler>();

//Add http client services at ConfigureServices(IServiceCollection services)
builder.Services.AddHttpClient<ICatalogService, CatalogService>(c =>
{
    c.BaseAddress = new Uri(builder.Configuration["ApiSettings:CatalogUrl"]);
}).AddHttpMessageHandler<LoggingDelegatingHandler>();
builder.Services.AddHttpClient<IBasketService, BasketService>(c =>
{
    c.BaseAddress = new Uri(builder.Configuration["ApiSettings:BasketUrl"]);
}).AddHttpMessageHandler<LoggingDelegatingHandler>();
builder.Services.AddHttpClient<IOrderService, OrderService>(c =>
{
    c.BaseAddress = new Uri(builder.Configuration["ApiSettings:OrderingUrl"]);
}).AddHttpMessageHandler<LoggingDelegatingHandler>();

// Configure logging with Serilog
builder.Services.AddSeriLogger(builder.Configuration);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
