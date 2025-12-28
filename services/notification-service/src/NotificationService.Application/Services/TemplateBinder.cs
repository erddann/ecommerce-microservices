using System.Text.Json;
using NotificationService.Infrastructure.Domain;

namespace NotificationService.Application.Services;

public class TemplateBinder
{
    public string Render(string templateBody, Dictionary<string, object> data)
    {
        var result = templateBody;
        foreach (var kvp in data)
        {
            result = result.Replace($"{{{kvp.Key}}}", kvp.Value?.ToString() ?? string.Empty);
        }
        return result;
    }
}
