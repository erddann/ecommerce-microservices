using Microsoft.Extensions.Logging;
using NotificationService.Infrastructure.Abstractions;
using NotificationService.Infrastructure.Configuration;
using NotificationService.Infrastructure.Domain;
using Polly;
using Polly.Retry;
using System.Net.Http.Json;

namespace NotificationService.Infrastructure.Services;

public class SmsSender : IChannelSender
{
    private readonly HttpClient _httpClient;
    private readonly NotificationSenderOptions _options;
    private readonly ILogger<SmsSender> _logger;
    private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;

    public NotificationChannel Channel => NotificationChannel.Sms;

    public SmsSender(HttpClient httpClient, NotificationSenderOptions options, ILogger<SmsSender> logger)
    {
        _httpClient = httpClient;
        _options = options;
        _logger = logger;

        _retryPolicy = Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .OrResult(msg => !msg.IsSuccessStatusCode)
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryAttempt, context) =>
                {
                    _logger.LogWarning("SMS send failed, retrying in {Timespan}. Attempt {RetryAttempt}", timespan, retryAttempt);
                });
    }

    public async Task<NotificationSendResult> SendAsync(RenderedNotification notification, CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            to = notification.Destination,
            message = notification.Body
        };

        var response = await _retryPolicy.ExecuteAsync(() => _httpClient.PostAsJsonAsync(_options.Endpoint, payload, cancellationToken));

		if (response.IsSuccessStatusCode)
		{
			return new NotificationSendResult(true, response.StatusCode.ToString(), notification.Destination, null);
		}

		var error = await response.Content.ReadAsStringAsync(cancellationToken);
		_logger.LogError("SMS send failed with status {Status} and error {Error}", response.StatusCode, error);
		return new NotificationSendResult(false, response.StatusCode.ToString(), notification.Destination, error);
    }
}