using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using RabbitMQ.Client;
using WebAPI.Data;
using WebAPI.Service.abstracts;
using WebAPI.Service.concretes;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSQL")));

// Configure Redis
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis")));

// Configure RabbitMQ
builder.Services.AddSingleton<ConnectionFactory>(sp => new ConnectionFactory
{
    HostName = builder.Configuration["RabbitMQ:HostName"],
    UserName = builder.Configuration["RabbitMQ:UserName"],
    Password = builder.Configuration["RabbitMQ:Password"]
});

// Register QueryService
builder.Services.AddScoped<IQueryService, QueryService>();

// Register CommandService
builder.Services.AddScoped<ICommandService, CommandService>();

// Register RabbitMQToRedisService
builder.Services.AddSingleton<IRabbitMQToRedisService, RabbitMQToRedisService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Start RabbitMQToRedisService
var rabbitMQToRedisService = app.Services.GetRequiredService<IRabbitMQToRedisService>();
rabbitMQToRedisService.StartListening();

app.Run();