using Azure;
using Azure.Data.Tables;

namespace DigitalIdentitySite.Models;

public class UserProfileEntity : ITableEntity
{
    public string PartitionKey { get; set; } = "User";
    public string RowKey { get; set; } = string.Empty;
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTimeOffset LastLoginUtc { get; set; }
}
