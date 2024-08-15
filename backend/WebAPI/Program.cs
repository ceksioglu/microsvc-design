using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using WebAPI.Data;
using WebAPI.Auth;
using StackExchange.Redis;
using RabbitMQ.Client;
using WebAPI.Auth.abstracts;
using WebAPI.Auth.concretes;
using WebAPI.EventHandler;
using WebAPI.Packages.RabbitMQ.abstracts;
using WebAPI.Packages.RabbitMQ.concretes;
using WebAPI.Packages.Redis.abstracts;
using WebAPI.Packages.Redis.concretes;
using WebAPI.SagaOrchestrator;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Configure Entity Framework with PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSQL")));

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
builder.Services.AddSingleton(jwtSettings);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret))
    };
});

// Add authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
    options.AddPolicy("RequireCustomerRole", policy => policy.RequireRole("Customer"));
});

// Register services
builder.Services.AddScoped<IJwtService, JwtService>();

// Configure Redis
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis")));
builder.Services.AddSingleton<IRedisService, RedisService>();

// Configure RabbitMQ
builder.Services.AddSingleton<IConnection>(sp =>
{
    var factory = new ConnectionFactory
    {
        HostName = builder.Configuration["RabbitMQ:HostName"],
        UserName = builder.Configuration["RabbitMQ:UserName"],
        Password = builder.Configuration["RabbitMQ:Password"]
    };
    return factory.CreateConnection();
});
builder.Services.AddSingleton<IRabbitMQService, RabbitMQService>();

// Register OrderSaga and OrderEventHandler
builder.Services.AddSingleton<OrderSaga>();
builder.Services.AddSingleton<OrderEventHandler>();

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "E-Commerce API", Version = "v1" });
    
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Start the Saga Orchestrator and Event Handler
using (var scope = app.Services.CreateScope())
{
    var sagaOrchestrator = scope.ServiceProvider.GetRequiredService<OrderSaga>();
    sagaOrchestrator.StartListening();

    var eventHandler = scope.ServiceProvider.GetRequiredService<OrderEventHandler>();
    eventHandler.StartListening();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Ensure the database is created and apply any pending migrations
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var dbContext = services.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate();
}

app.Run();