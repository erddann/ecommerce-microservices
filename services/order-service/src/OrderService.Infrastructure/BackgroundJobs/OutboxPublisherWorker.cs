using Microsoft.Extensions.DependencyInjection;
using OrderService.Infrastructure.Messaging;
using OrderService.Infrastructure.Outbox;
using OrderService.Infrastructure.Persistence.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Data;
using OrderService.Infrastructure.Settings;

namespace OrderService.Infrastructure.BackgroundJobs
{
    public class OutboxPublisherWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<OutboxPublisherWorker> _logger;
        private readonly OutboxSettings _outboxSettings;

        public OutboxPublisherWorker(
            IServiceScopeFactory scopeFactory,
            ILogger<OutboxPublisherWorker> logger,
            ISettings<OutboxSettings> outboxSettings)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _outboxSettings = outboxSettings?.Value ?? throw new ArgumentNullException(nameof(outboxSettings));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
                    var bus = scope.ServiceProvider.GetRequiredService<IEventBus>();

                    await using var tx = await context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, stoppingToken);

                    var batchSize = Math.Max(1, _outboxSettings.PublisherBatchSize);

                    // Multi-instance safe: lock a small batch with SKIP LOCKED (locks held until tx commit)
                    var messages = await context.Set<OutboxMessage>()
                        .FromSqlInterpolated($"select * from \"outbox_messages\" where \"processed_on\" is null order by \"occurred_on\" limit {batchSize} for update skip locked") /*SqlInterpolated safe for sql injection*/
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
                        await bus.PublishAsync(message.Type, message.Payload, stoppingToken);
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
