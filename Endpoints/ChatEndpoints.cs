using DigitalIdentitySite.Services;

namespace DigitalIdentitySite.Endpoints;

public static class ChatEndpoints
{
    private const int MaxMessageLength = 2000;

    public static void MapChatEndpoints(this WebApplication app)
    {
        app.MapPost("/api/chat", async (ChatRequest? request, ChatBotService chatBotService, CancellationToken cancellationToken) =>
        {
            var message = request?.Message?.Trim();
            if (string.IsNullOrEmpty(message))
            {
                return Results.BadRequest(new { error = "Message is required." });
            }

            if (message.Length > MaxMessageLength)
            {
                return Results.BadRequest(new { error = $"Message must be {MaxMessageLength} characters or fewer." });
            }

            var history = (request?.History ?? new List<ChatHistoryItem>())
                .Select(h => new ChatMessage(h.Role ?? string.Empty, h.Content ?? string.Empty))
                .ToList();

            var result = await chatBotService.GetReplyAsync(message, history, cancellationToken);

            return Results.Json(new { reply = result.Reply });
        }).AllowAnonymous();
    }

    public record ChatHistoryItem(string? Role, string? Content);

    public record ChatRequest(string? Message, List<ChatHistoryItem>? History);
}
