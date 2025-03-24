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
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Your API",
        Version = "v1"
    });

    // Adding JWT Authentication to Swagger
    var securityScheme = new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter Token in the text input below.",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    };

    var securityRequirement = new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    };

    options.AddSecurityDefinition("Bearer", securityScheme);
    options.AddSecurityRequirement(securityRequirement);
});

// Add services to the container.
builder.Services.AddControllers();

//Registering the GreetingService
builder.Services.AddScoped<IGreetingService, GreetingService>();
builder.Services.AddScoped<IGreetingRL, GreetingRL>();

// Registering the User services
builder.Services.AddScoped<IUserBL, UserBL>();
builder.Services.AddScoped<IUserRL, UserRL>();

var configuration = builder.Configuration;
// Configure JWT Authentication
var key = Encoding.ASCII.GetBytes(configuration["Jwt:Key"]);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(configuration["Jwt:Key"])),

            ValidateIssuer = true,
            ValidIssuer = configuration["Jwt:Issuer"], 

            ValidateAudience = true,
            ValidAudience = configuration["Jwt:Audience"], 

            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero 
        };
    });


// Applying CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
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
app.UseCors("AllowAll");
app.Run();

LogManager.Shutdown();
