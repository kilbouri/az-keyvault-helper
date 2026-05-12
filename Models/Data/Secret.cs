namespace KeyVaultHelper.Models.Data;

public record Secret(
    string Name,
    string? ContentType,
    DateTimeOffset? LastModifiedDate,
    DateTimeOffset? ExpiryDate
);
