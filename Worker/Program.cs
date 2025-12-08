using MassTransit;
using Microsoft.EntityFrameworkCore;
using WebApi.Dal;
using Worker.Jobs;

var builder = Host.CreateApplicationBuilder(args);

// Add DbContext
builder.Services.AddDbContext<OrderProcessingDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add MassTransit with RabbitMQ
builder.Services.AddMassTransit(x =>
{
    // Configure RabbitMQ transport
    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitMqSettings = builder.Configuration.GetSection("RabbitMqSettings");
        cfg.Host(rabbitMqSettings["Host"] ?? "localhost", h =>
        {
            h.Username(rabbitMqSettings["Username"] ?? "guest");
            h.Password(rabbitMqSettings["Password"] ?? "guest");
        });
    });
});

// Add Background Job
builder.Services.AddHostedService<OrderExpirationJob>();

var host = builder.Build();
host.Run();
