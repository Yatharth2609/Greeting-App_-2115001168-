﻿using BusinessLayer.Interface;
using BusinessLayer.Service;
using Microsoft.EntityFrameworkCore;
using NLog;
using NLog.Web;
using RepositoryLayer.Interface;
using RepositoryLayer.Service;

//Implementing NLogger
var logger = LogManager.Setup().LoadConfigurationFromFile("nlog.config").GetCurrentClassLogger();

logger.Info("Application is starting...");

var builder = WebApplication.CreateBuilder(args);

//Database Connection
var connectionString = builder.Configuration.GetConnectionString("SqlConnection");

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

var app = builder.Build();

//Custom exception handling middleware
app.UseMiddleware<ExceptionMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

LogManager.Shutdown();
