using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using System.Reflection;
using WebAPI.Service.abstracts;
using WebAPI.Service.concretes;
using StackExchange.Redis;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "E-commerce Microservice API",
        Version = "v1",
        Description = "API for e-commerce operations including user dashboard, inventory, orders, and feedback."
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});

// Use Autofac as the ServiceProviderFactory
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

// Register services directly with Autofac here
builder.Host.ConfigureContainer<ContainerBuilder>(builder => 
{
    // Register Redis
    builder.Register( c => 
    {
        var redisConnection = c.Resolve<IConfiguration>().GetConnectionString("Redis");
        return ConnectionMultiplexer.Connect(redisConnection);
    }).As<IConnectionMultiplexer>().SingleInstance();

    // Register CommandService
    builder.Register(c =>
    {
        var configuration = c.Resolve<IConfiguration>();
        var postgresConnection = configuration.GetConnectionString("PostgreSQL");
        var rabbitMQHostName = configuration["RabbitMQ:HostName"];

        return new CommandService(
            postgresConnection,
            new ConnectionFactory { HostName = rabbitMQHostName }
        );
    }).As<ICommandService>().InstancePerLifetimeScope();

    // Register QueryService
    builder.RegisterType<QueryService>().As<IQueryService>().InstancePerLifetimeScope();

    // You can add more registrations here
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "E-commerce Microservice API V1");
    });
}
else
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Global error handling middleware
app.UseExceptionHandler("/Error");

app.Run();