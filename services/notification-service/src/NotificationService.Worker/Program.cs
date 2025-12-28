using Microsoft.Extensions.Hosting;
using NotificationService.Application;
using NotificationService.Infrastructure;
using NotificationService.Infrastructure.Configuration;
using NotificationService.Worker.BackgroundJobs;
using NotificationService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = Microsoft.Extensions.Hosting.Host.CreateApplicationBuilder(args);

builder.Services.AddApplication();
builder.Services.AddSettings<NotificationSettings>(builder.Configuration, NotificationSettings.SectionName);
builder.Services.AddInfrastructure(builder.Configuration, includeMessagingWorkers: true);
builder.Services.AddHostedService<OrderCancelledWorker>();
builder.Services.AddHostedService<OrderConfirmedWorker>();

var host = builder.Build();

//using (var scope = host.Services.CreateScope())
//{
//    var db = scope.ServiceProvider.GetRequiredService<NotificationService.Infrastructure.Persistence.NotificationDbContext>();
//    if (Environment.GetEnvironmentVariable("AUTO_MIGRATE") == "true")
//    {
//        db.Database.Migrate();
//    }
//}

await host.RunAsync();
