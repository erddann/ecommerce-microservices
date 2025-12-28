using System.Text.Json;
using Microsoft.Extensions.Logging;
using NotificationService.Infrastructure.Abstractions;
using NotificationService.Infrastructure.Domain;
using NotificationService.Infrastructure.Domain.Events;

namespace NotificationService.Application.Services;

public class OrderNotificationService : IOrderNotificationService
{
    private readonly INotificationTemplateRepository _notificationTemplateRepository;
    private readonly INotificationSender _notificationSender;
    private readonly INotificationLogRepository _notificationLogRepository;
    private readonly IOutboxRepository _outboxRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TemplateBinder _templateBinder;
    private readonly NotificationContextBuilder _contextBuilder;
    private readonly ILogger<OrderNotificationService> _logger;

    public OrderNotificationService(
        INotificationTemplateRepository notificationTemplateRepository,
        INotificationSender notificationSender,
        INotificationLogRepository notificationLogRepository,
        IOutboxRepository outboxRepository,
        IUnitOfWork unitOfWork,
        TemplateBinder templateBinder,
        NotificationContextBuilder contextBuilder,
        ILogger<OrderNotificationService> logger)
    {
        _notificationTemplateRepository = notificationTemplateRepository;
        _notificationSender = notificationSender;
        _notificationLogRepository = notificationLogRepository;
        _outboxRepository = outboxRepository;
        _unitOfWork = unitOfWork;
        _templateBinder = templateBinder;
        _contextBuilder = contextBuilder;
        _logger = logger;
    }

    public Task<bool> ProcessOrderConfirmedAsync(EventEnvelope envelope, CancellationToken ct)
        => ProcessAsync<OrderConfirmedEvent>(envelope, ct, _contextBuilder.BuildContextAsync);

    public Task<bool> ProcessOrderCancelledAsync(EventEnvelope envelope, CancellationToken ct)
        => ProcessAsync<OrderCancelledEvent>(envelope, ct, _contextBuilder.BuildContextAsync);

    private async Task<bool> ProcessAsync<TEvent>(EventEnvelope envelope, CancellationToken ct, Func<TEvent, CancellationToken, Task<NotificationContext>> contextFactory)
        where TEvent : class
    {
        try
        {
            _logger.LogInformation("Processing {EventType} event {EventId}", typeof(TEvent).Name, envelope.EventId);

            var domainEvent = DeserializeOrThrow<TEvent>(envelope);
            var context = await contextFactory(domainEvent, ct);
            var template = await GetTemplateOrThrowAsync(context, envelope.EventId, ct);

            var renderedBody = RenderTemplateBody(template, context);
            var renderedNotification = BuildRenderedNotification(template, renderedBody, context);
            var sendResult = await SendNotificationAsync(envelope.EventId, context, renderedNotification, ct);

            var destination = ResolveDestination(sendResult, context);
            await WriteNotificationLogAsync(envelope, template, renderedBody, destination, sendResult, ct);
            await WriteOutboxMessageAsync(envelope, domainEvent, ct);

            await _unitOfWork.SaveChangesAsync(ct);
            _logger.LogInformation("Event {EventId} persisted to outbox", envelope.EventId);
            return sendResult.Success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while processing {EventType} event {EventId}", typeof(TEvent).Name, envelope.EventId);
            throw;
        }
    }

    private TEvent DeserializeOrThrow<TEvent>(EventEnvelope envelope)
    {
        try
        {
            var json = envelope.Data.GetRawText();
            var payload = JsonSerializer.Deserialize<TEvent>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (payload == null)
            {
                throw new InvalidOperationException($"Invalid {typeof(TEvent).Name} data");
            }

            return payload;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deserialize payload into {PayloadType}", typeof(TEvent).Name);
            throw;
        }
    }

    private async Task<NotificationTemplate> GetTemplateOrThrowAsync(NotificationContext context, Guid eventId, CancellationToken ct)
    {
        var template = await _notificationTemplateRepository.GetAsync(context.TemplateCode, context.Channel, "en", ct);
        if (template == null)
        {
            _logger.LogWarning("Template {TemplateCode} not found for event {EventId}", context.TemplateCode, eventId);
            throw new InvalidOperationException($"Template not found for {context.TemplateCode}");
        }

        return template;
    }

    private string RenderTemplateBody(NotificationTemplate template, NotificationContext context)
        => _templateBinder.Render(template.Body, context.Data);

    private static RenderedNotification BuildRenderedNotification(NotificationTemplate template, string renderedBody, NotificationContext context)
        => new(template.Subject, renderedBody, template.Channel, context.RecipientEmail);

    private async Task<NotificationSendResult> SendNotificationAsync(Guid eventId, NotificationContext context, RenderedNotification notification, CancellationToken ct)
    {
        var sendResult = await _notificationSender.SendAsync(notification, ct);
        _logger.LogInformation("Event {EventId} resolved destinations: SendResult={SendDestination}, Context={ContextEmail}", eventId, sendResult.Destination, context.RecipientEmail);

        if (sendResult.Success)
        {
            _logger.LogInformation("Event {EventId} notification sent", eventId);
        }
        else
        {
            _logger.LogWarning("Event {EventId} notification failed: {Status} - {Error}", eventId, sendResult.Status, sendResult.Error);
        }

        return sendResult;
    }

    private static string ResolveDestination(NotificationSendResult sendResult, NotificationContext context)
        => sendResult.Destination ?? context.RecipientEmail ?? string.Empty;

    private async Task WriteNotificationLogAsync(EventEnvelope envelope, NotificationTemplate template, string renderedBody, string destination, NotificationSendResult sendResult, CancellationToken ct)
    {
        var logError = string.IsNullOrWhiteSpace(sendResult.Error) ? null : sendResult.Error;
        var log = NotificationLog.Create(envelope.EventId, envelope.EventType, template.Channel, destination, renderedBody, sendResult.Status, logError);
        await _notificationLogRepository.AddAsync(log, ct);
    }

    private async Task WriteOutboxMessageAsync<TEvent>(EventEnvelope envelope, TEvent domainEvent, CancellationToken ct)
    {
        var outboxMessage = OutboxMessage.FromDomainEvent(envelope.EventType, envelope.EventId, envelope.OccurredOn, domainEvent);
        await _outboxRepository.AddAsync(outboxMessage, ct);
    }
}
