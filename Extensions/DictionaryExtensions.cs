namespace KeyVaultHelper.Extensions;

public static class DictionaryExtensions
{
    public static TValue GetOrInsert<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue defaultValue)
    {
        if (dict.TryGetValue(key, out var existingValue))
        {
            return existingValue;
        }

        dict.Add(key, defaultValue);
        return defaultValue;
    }
}
