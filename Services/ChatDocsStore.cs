using System.Text;
using Azure.Storage.Blobs;

namespace DigitalIdentitySite.Services;

public class ChatDocsStore
{
    private static readonly string[] TextExtensions = { ".txt", ".md" };
    private static readonly TimeSpan CacheLifetime = TimeSpan.FromMinutes(10);

    private readonly BlobContainerClient? _containerClient;
    private readonly ILogger<ChatDocsStore> _logger;
    private readonly SemaphoreSlim _refreshLock = new(1, 1);

    private string _cachedContext = string.Empty;
    private DateTimeOffset _cachedAtUtc = DateTimeOffset.MinValue;

    public ChatDocsStore(IConfiguration configuration, ILogger<ChatDocsStore> logger)
    {
        _logger = logger;

        var connectionString = configuration["BlobStorage:ConnectionString"]
            ?? configuration["TableStorage:ConnectionString"];
        var containerName = configuration["BlobStorage:ContainerName"] ?? "ChatBotDocs";

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            _logger.LogWarning(
                "BlobStorage:ConnectionString (and TableStorage:ConnectionString fallback) are not configured; chatbot will run without supplementary documents.");
            return;
        }

        var serviceClient = new BlobServiceClient(connectionString);
        _containerClient = serviceClient.GetBlobContainerClient(containerName);
    }

    public async Task<string> GetContextAsync(CancellationToken cancellationToken = default)
    {
        if (_containerClient is null)
        {
            return string.Empty;
        }

        if (DateTimeOffset.UtcNow - _cachedAtUtc < CacheLifetime)
        {
            return _cachedContext;
        }

        await _refreshLock.WaitAsync(cancellationToken);
        try
        {
            if (DateTimeOffset.UtcNow - _cachedAtUtc < CacheLifetime)
            {
                return _cachedContext;
            }

            _cachedContext = await LoadContextAsync(cancellationToken);
            _cachedAtUtc = DateTimeOffset.UtcNow;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to refresh chatbot documents from blob storage; using last known context.");
        }
        finally
        {
            _refreshLock.Release();
        }

        return _cachedContext;
    }

    private async Task<string> LoadContextAsync(CancellationToken cancellationToken)
    {
        if (!await _containerClient!.ExistsAsync(cancellationToken))
        {
            _logger.LogWarning("Blob container '{Container}' does not exist; chatbot will run without supplementary documents.", _containerClient.Name);
            return string.Empty;
        }

        var builder = new StringBuilder();

        await foreach (var blobItem in _containerClient.GetBlobsAsync(cancellationToken: cancellationToken))
        {
            var extension = Path.GetExtension(blobItem.Name);
            if (!TextExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
            {
                continue;
            }

            var blobClient = _containerClient.GetBlobClient(blobItem.Name);
            var download = await blobClient.DownloadContentAsync(cancellationToken);
            var text = download.Value.Content.ToString();

            builder.AppendLine($"--- Document: {blobItem.Name} ---");
            builder.AppendLine(text.Trim());
            builder.AppendLine();
        }

        return builder.ToString();
    }
}
