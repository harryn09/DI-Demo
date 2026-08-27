namespace DigitalIdentitySite.Services;

public class ChatDocsRefreshService : BackgroundService
{
    private static readonly TimeSpan RefreshInterval = TimeSpan.FromMinutes(30);

    private readonly ChatDocsStore _docsStore;
    private readonly ILogger<ChatDocsRefreshService> _logger;

    public ChatDocsRefreshService(ChatDocsStore docsStore, ILogger<ChatDocsRefreshService> logger)
    {
        _docsStore = docsStore;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(RefreshInterval);

        do
        {
            try
            {
                await _docsStore.RefreshAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Scheduled chatbot document refresh failed.");
            }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
