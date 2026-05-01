using System.Collections.Generic;
using Avalonia.Data;

namespace KeyVaultHelper.Models.Data;

public record KeyVault(ResourceGroup ResourceGroup, string Name, List<KeyVault.Secret> Secrets)
{
    public record Secret(string Name, Optional<string> Value);
}
