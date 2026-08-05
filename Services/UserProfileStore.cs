using Azure.Data.Tables;
using DigitalIdentitySite.Models;

namespace DigitalIdentitySite.Services;

public class UserProfileStore
{
    private readonly TableClient? _tableClient;
    private readonly ILogger<UserProfileStore> _logger;

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
        serviceClient.CreateTableIfNotExists(tableName);
        _tableClient = serviceClient.GetTableClient(tableName);
    }

    public async Task UpsertUserAsync(string objectId, string displayName, string email, CancellationToken cancellationToken = default)
    {
        if (_tableClient is null || string.IsNullOrWhiteSpace(objectId))
        {
            return;
        }

        var entity = new UserProfileEntity
        {
            RowKey = objectId,
            DisplayName = displayName,
            Email = email,
            LastLoginUtc = DateTimeOffset.UtcNow
        };

        await _tableClient.UpsertEntityAsync(entity, TableUpdateMode.Replace, cancellationToken);
    }
}
