using Microsoft.EntityFrameworkCore;
using simpledispatch_unitservice;
using simpledispatch_unitservice.Data;
using simpledispatch_unitservice.Repositories;

var builder = Host.CreateApplicationBuilder(args);

// Configure Entity Framework
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure RabbitMQ settings
builder.Services.Configure<RabbitMQConfiguration>(
    builder.Configuration.GetSection("RabbitMQ"));

// Register repository
builder.Services.AddScoped<IDatabaseRepository, DatabaseRepository>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
