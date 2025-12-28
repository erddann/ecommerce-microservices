using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StockService.Infrastructure.Configuration;
using StockService.Infrastructure.Entities;
using StockService.Infrastructure.Messaging;
using StockService.Infrastructure.Persistence.Context;
using System.Data;
using System.Text;
using System.Text.Json;

namespace StockService.Infrastructure.BackgroundJobs
{
    public class OutboxPublisherWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly OutboxPublisherSettings _settings;
        private readonly TimeSpan _pollInterval;
        private readonly int _batchSize;
        private readonly ILogger<OutboxPublisherWorker> _logger;

        public OutboxPublisherWorker(
            IServiceScopeFactory scopeFactory,
            ISettings<OutboxPublisherSettings> settings,
            ILogger<OutboxPublisherWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _settings = settings.Value;
            _logger = logger;
            var pollSeconds = _settings.PollIntervalSeconds > 0 ? _settings.PollIntervalSeconds : 5;
            _pollInterval = TimeSpan.FromSeconds(pollSeconds);
            _batchSize = _settings.BatchSize > 0 ? _settings.BatchSize : 50;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<StockDbContext>();
                    var bus = scope.ServiceProvider.GetRequiredService<IEventBus>();

                    await using var tx = await context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, stoppingToken);

                    var batchSize = Math.Max(1, _batchSize);

                    // Multi-instance safe: lock a small batch with SKIP LOCKED (locks held until tx commit)
                    var messages = await context.Set<OutboxMessage>()
                        .FromSqlInterpolated($"select * from \"outbox_messages\" where \"ProcessedOn\" is null order by \"OccurredOn\" limit {batchSize} for update skip locked") /*SqlInterpoated safe for sql injection*/
                        .ToListAsync(stoppingToken);

                    if (messages.Count == 0)
                    {
                        await tx.CommitAsync(stoppingToken);
                        _logger.LogDebug("OutboxPublisher: no pending messages");
                        await Task.Delay(1000, stoppingToken);
                        continue;
                    }

                    foreach (var message in messages)
                    {
                        await bus.PublishAsync(_settings.ExchangeName, message.Type, message.Payload, stoppingToken);
                        message.MarkAsProcessed();
                        _logger.LogInformation("OutboxPublisher: published {Type} ({EventId})", message.Type, message.EventId);
                    }

                    await context.SaveChangesAsync(stoppingToken);
                    await tx.CommitAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    _logger.LogWarning("OutboxPublisherWorker stopping (cancellation requested)");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in OutboxPublisherWorker loop");
                }
            }
        }
    }
}
