using Azure.Data.Tables;
using DigitalIdentitySite.Models;

namespace DigitalIdentitySite.Services;

public class UserProfileStore
{
    private readonly TableClient? _tableClient;
    private readonly ILogger<UserProfileStore> _logger;
    private readonly SemaphoreSlim _tableInitLock = new(1, 1);
    private bool _tableEnsured;

    public UserProfileStore(IConfiguration configuration, ILogger<UserProfileStore> logger)
    {
        _logger = logger;

        var connectionString = configuration["TableStorage:ConnectionString"];
        var tableName = configuration["TableStorage:TableName"] ?? "Users";

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            _logger.LogWarning(
                "TableStorage:ConnectionString is not configured; signed-in user profiles will not be persisted.");
            return;
        }

        var serviceClient = new TableServiceClient(connectionString);
        _tableClient = serviceClient.GetTableClient(tableName);
    }

    public async Task UpsertUserAsync(string objectId, string displayName, string email, CancellationToken cancellationToken = default)
    {
        if (_tableClient is null || string.IsNullOrWhiteSpace(objectId))
        {
            return;
        }

        await EnsureTableExistsAsync(cancellationToken);

        var entity = new UserProfileEntity
        {
            RowKey = objectId,
            DisplayName = displayName,
            Email = email,
            LastLoginUtc = DateTimeOffset.UtcNow
        };

        await _tableClient.UpsertEntityAsync(entity, TableUpdateMode.Replace, cancellationToken);
    }

    private async Task EnsureTableExistsAsync(CancellationToken cancellationToken)
    {
        if (_tableEnsured)
        {
            return;
        }

        await _tableInitLock.WaitAsync(cancellationToken);
        try
        {
            if (!_tableEnsured)
            {
                await _tableClient!.CreateIfNotExistsAsync(cancellationToken: cancellationToken);
                _tableEnsured = true;
            }
        }
        finally
        {
            _tableInitLock.Release();
        }
    }
}
