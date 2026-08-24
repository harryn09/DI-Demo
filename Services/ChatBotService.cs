using System.Text.Json;
using System.Text.Json.Serialization;

namespace DigitalIdentitySite.Services;

public record ChatMessage(string Role, string Content);

public class ChatBotService
{
    private const int MaxHistoryMessages = 12;
    private const int MaxMessageLength = 2000;

    private const string ServiceFacts = """
        You are the help assistant for the Digital Identity Team, Ministry of Education (New Zealand) website.
        This site introduces four Digital Identity Services to schools, kura, tertiary providers, sector IT staff and whanau.
        Answer only using the facts below and any supplementary documents provided. Never invent capabilities, statistics
        or testimonials that are not stated here. If you don't know the answer, say so plainly and point the visitor to
        the relevant service page, the "Get in touch" form on this site, or harry.nguyen@education.govt.nz. Keep answers
        short, plain-language, and welcoming to non-technical readers, with more technical detail only if asked.

        1. Education Sector Logon (ESL): identity management, authentication and authorisation giving secure single-account
           access to education sector applications. Serves early learning providers, schools and kura, tertiary institutions,
           Ministry staff, and attendance service providers. Features: two roles (Delegated Authoriser manages accounts for
           an organisation; User manages their own account), multi-factor authentication (Microsoft/Google Authenticator),
           a self-service account management portal, and evidence-of-identity verification.
           Reference: https://applications.education.govt.nz/education-sector-logon-esl

        2. Learner Identity Broker (LIB): a secure "digital bridge" letting students log in once with their school account
           and connect to the Ministry-approved online education services their school supports, no separate accounts per
           platform. Schools and kura opt in via a 10-15 minute registration by their IT administrator. Benefits: simplified
           single login, improved security through centralised authentication, seamless transfer of access and records when
           a student changes schools. Related terms: learner identity, Education Digital Identity.
           Reference: https://www.education.govt.nz/education-professionals/schools-year-0-13/digital-technology/learner-identity-broker

        3. National Student Index (NSI): the education sector's core learner identity register, a Ministry-maintained
           database assigning every learner a unique, lifelong National Student Number (NSN) used across early childhood,
           school and tertiary education. Used by Tertiary Education Organisations (via their Student Management System or
           the NSI web application) to allocate NSNs, search/create/update student records, merge duplicates, and receive
           change notifications. Enables safe sharing of student information while protecting privacy.
           Reference: https://applications.education.govt.nz/national-student-index-nsi-web-application

        4. Enrol: the national register of student enrolments, maintained by schools and kura (mandatory). Records
           enrolments, transfers between schools, and departures from the education system. Also used by Health NZ Vision
           and Hearing Technicians (restricted access, test results entry) and Attendance Service staff (read-only).
           Training is mandatory before access is granted.
           Reference: https://applications.education.govt.nz/enrol
        """;

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ChatDocsStore _docsStore;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ChatBotService> _logger;

    public ChatBotService(
        IHttpClientFactory httpClientFactory,
        ChatDocsStore docsStore,
        IConfiguration configuration,
        ILogger<ChatBotService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _docsStore = docsStore;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<ChatBotResult> GetReplyAsync(string message, IReadOnlyList<ChatMessage> history, CancellationToken cancellationToken)
    {
        var apiKey = _configuration["Groq:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _logger.LogWarning("Groq:ApiKey is not configured; chatbot cannot answer.");
            return ChatBotResult.Failure("The chat assistant isn't configured yet. Please use the contact form instead.");
        }

        var model = _configuration["Groq:Model"] ?? "openai/gpt-oss-20b";
        var docsContext = await _docsStore.GetContextAsync(cancellationToken);

        var systemPrompt = string.IsNullOrWhiteSpace(docsContext)
            ? ServiceFacts
            : $"{ServiceFacts}\n\nAdditional reference documents:\n{docsContext}";

        var trimmedMessage = message.Trim();
        if (trimmedMessage.Length > MaxMessageLength)
        {
            trimmedMessage = trimmedMessage[..MaxMessageLength];
        }

        var messages = new List<ChatCompletionMessage> { new("system", systemPrompt) };
        messages.AddRange(history
            .TakeLast(MaxHistoryMessages)
            .Where(m => m.Role is "user" or "assistant" && !string.IsNullOrWhiteSpace(m.Content))
            .Select(m => new ChatCompletionMessage(m.Role, m.Content.Length > MaxMessageLength ? m.Content[..MaxMessageLength] : m.Content)));
        messages.Add(new ChatCompletionMessage("user", trimmedMessage));

        var requestBody = new ChatCompletionRequest(model, messages, 1024);

        var client = _httpClientFactory.CreateClient("groq");
        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.groq.com/openai/v1/chat/completions")
        {
            Content = JsonContent.Create(requestBody, options: JsonOptions)
        };
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

        try
        {
            using var response = await client.SendAsync(request, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Groq API returned {StatusCode}: {Body}", response.StatusCode, body);
                return ChatBotResult.Failure("Sorry, the chat assistant is having trouble right now. Please try again shortly.");
            }

            var parsed = JsonSerializer.Deserialize<ChatCompletionResponse>(body, JsonOptions);
            var reply = parsed?.Choices?.FirstOrDefault()?.Message?.Content;

            if (string.IsNullOrWhiteSpace(reply))
            {
                return ChatBotResult.Failure("Sorry, I couldn't come up with an answer to that. Please try rephrasing.");
            }

            return ChatBotResult.Success(reply.Trim());
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Failed to call Groq API.");
            return ChatBotResult.Failure("Sorry, the chat assistant is having trouble right now. Please try again shortly.");
        }
    }

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private record ChatCompletionMessage(string Role, string Content);

    private record ChatCompletionRequest(
        string Model,
        List<ChatCompletionMessage> Messages,
        [property: JsonPropertyName("max_tokens")] int MaxTokens);

    private record ChatCompletionResponse(List<ChatCompletionChoice>? Choices);

    private record ChatCompletionChoice(ChatCompletionMessage? Message);
}

public record ChatBotResult(bool IsSuccess, string Reply)
{
    public static ChatBotResult Success(string reply) => new(true, reply);
    public static ChatBotResult Failure(string reply) => new(false, reply);
}
