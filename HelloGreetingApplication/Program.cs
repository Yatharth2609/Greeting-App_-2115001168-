using BusinessLayer.Interface;
using BusinessLayer.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NLog;
using NLog.Web;
using RepositoryLayer.Entity;
using RepositoryLayer.Interface;
using RepositoryLayer.Service;
using System.Text;

//Implementing NLogger
var logger = LogManager.Setup().LoadConfigurationFromFile("nlog.config").GetCurrentClassLogger();

logger.Info("Application is starting...");

var builder = WebApplication.CreateBuilder(args);

//Database Connection
var connectionString = builder.Configuration.GetConnectionString("SqlConnection");

// Register RedisCacheService
builder.Services.AddSingleton<RedisCacheService>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetValue<string>("Redis:ConnectionString");
    return new RedisCacheService(connectionString);
});

// Register RabbitMQService
builder.Services.AddSingleton<RabbitMQService>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var hostName = configuration.GetValue<string>("RabbitMQ:HostName");
    var userName = configuration.GetValue<string>("RabbitMQ:UserName");
    var password = configuration.GetValue<string>("RabbitMQ:Password");
    return new RabbitMQService(hostName, userName, password);
});

if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Database connection string is missing.");
}

builder.Services.AddDbContext<GreetingDBContext>(options =>
    options.UseSqlServer(connectionString));

//Configure NLog
builder.Logging.ClearProviders();
builder.Host.UseNLog();

// Register Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add services to the container.
builder.Services.AddControllers();

//Registering the GreetingService
builder.Services.AddScoped<IGreetingService, GreetingService>();
builder.Services.AddScoped<IGreetingRL, GreetingRL>();

// Registering the User services
builder.Services.AddScoped<IUserBL, UserBL>();
builder.Services.AddScoped<IUserRL, UserRL>();

// Configure JWT Authentication
var key = Encoding.ASCII.GetBytes("YourSecretKeyHere"); // Replace with your secret key
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = "YourIssuer",
        ValidAudience = "YourAudience",
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

var app = builder.Build();

//Custom exception handling middleware
app.UseMiddleware<ExceptionMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

LogManager.Shutdown();
