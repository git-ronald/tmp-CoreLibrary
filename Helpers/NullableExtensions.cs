namespace CoreLibrary.Helpers
{
    public static class NullableExtensions
    {
        public static TValue GetOrThrow<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key) where TKey : notnull
        {
            return dictionary.GetOrThrow(key, $"Failed to read key {key}");
        }
        public static TValue GetOrThrow<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, string exceptionMessage) where TKey : notnull
        {
            return dictionary.GetOrThrow(key, () => new Exception(exceptionMessage));
        }
        public static TValue GetOrThrow<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, Func<Exception> getException) where TKey : notnull
        {
            if (dictionary == null)
            {
                throw new ArgumentNullException(nameof(dictionary));
            }

            if (!dictionary.TryGetValue(key, out TValue? value))
            {
                throw getException();
            }
            if (value == null)
            {
                throw getException();
            }
            return value;
        }

        public static TValue GetOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue) where TKey : notnull
        {
            if (dictionary == null)
            {
                return defaultValue;
            }

            if (!dictionary.TryGetValue(key, out TValue? value))
            {
                return defaultValue;
            }
            return value;
        }

        public static int ParseToIntValue(this string? value)
        {
            if (value == null)
            {
                return default;
            }

            try
            {
                return int.Parse(value);
            }
            catch
            {
                return default;
            }
        }

        public static string ToStringValue(this object value)
        {
            if (value == null)
            {
                return string.Empty;
            }
            return value.ToString() ?? string.Empty;
        }
    }
}
