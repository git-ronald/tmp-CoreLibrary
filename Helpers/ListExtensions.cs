namespace CoreLibrary.Helpers
{
    public static class ListExtensions
    {
        /// <summary>
        /// Like ToDictionary, but instead of throwing an exception, a duplicate key is dealt with according to duplicateKeyPolicy.
        /// </summary>
        public static Dictionary<TKey, TValue> ConvertToDictionary<TKey, TValue>(
            this IEnumerable<KeyValuePair<TKey, TValue>> source, DuplicateKeyPolicy duplicateKeyPolicy = DuplicateKeyPolicy.Overwrite)
            where TKey: notnull
        {
            var result = new Dictionary<TKey, TValue>();
            foreach (var keyValue in source)
            {
                if (!result.SetItem(keyValue.Key, keyValue.Value, duplicateKeyPolicy))
                {
                    return new Dictionary<TKey, TValue>();
                }
            }
            return result;
        }

        public static Dictionary<TKey, TValue> ConvertToDictionary<TSource, TKey, TValue>(
            this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TValue> valueSelector, DuplicateKeyPolicy duplicateKeyPolicy = DuplicateKeyPolicy.Overwrite)
            where TKey : notnull
        {
            var result = new Dictionary<TKey, TValue>();
            foreach (var item in source)
            {
                if (!result.SetItem(keySelector(item), valueSelector(item), duplicateKeyPolicy))
                {
                    return new Dictionary<TKey, TValue>();
                }
            }
            return result;
        }

        private static bool SetItem<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue value, DuplicateKeyPolicy duplicateKeyPolicy)
            where TKey : notnull
        {
            if (key == null)
            {
                return duplicateKeyPolicy != DuplicateKeyPolicy.FailEntirely;
            }

            if (!dictionary.ContainsKey(key))
            {
                dictionary.Add(key, value);
                return true;
            }

            switch (duplicateKeyPolicy)
            {
                case DuplicateKeyPolicy.Overwrite:
                    dictionary[key] = value;
                    return true;
                case DuplicateKeyPolicy.Ignore:
                    return true;
                case DuplicateKeyPolicy.DeleteAndSkip:
                    dictionary.Remove(key);
                    return true;
                default: // DuplicateKeyPolicyEnum.FailEntirely:
                    return false;
            }
        }

        /// <summary>
        /// Convert IAsyncEnumerable to List.
        /// </summary>
        public async static Task<List<T>> ConvertToListAsync<T>(this IAsyncEnumerable<T> source)
        {
            var result = new List<T>();

            await foreach (T item in source)
            {
                result.Add(item);
            }

            return result;
        }

        public static async IAsyncEnumerable<T> ConvertToAsyncEnumerable<T>(this List<T> source)
        {
            foreach (T item in source)
            {
                yield return await Task.Run(() => item);
            }
        }

        public async static IAsyncEnumerable<TResult> SelectAsAsyncEnumerable<TSource, TResult>(this IAsyncEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            await foreach (TSource sourceItem in source)
            {
                yield return selector(sourceItem);
            }
        }

        public static bool TryFirst<T>(this IEnumerable<T> source, Func<T, bool> predicate, out T? result)
        {
            if (source.Any(predicate))
            {
                result = source.First(predicate);
                return true;
            }

            result = default;
            return false;
        }
        // TODO: also implement TryLast

        public static void AddItems<T>(this List<T> list, params T[] items) => list.AddRange(items);

        public static TValue? TryGetValue<TKey, TValue>(this Dictionary<TKey, TValue> source, TKey key) where TKey : notnull
        {
            if (source.TryGetValue(key, out TValue? value))
            {
                return value;
            }
            return default;
        }

        /// <summary>
        /// If item doesn't exist, it's added automatically
        /// </summary>
        public static TValue Ensure<TKey, TValue>(this Dictionary<TKey, TValue> source, TKey key) where TKey : notnull where TValue : new()
        {
            if (!source.ContainsKey(key))
            {
                source[key] = new TValue();
            }
            return source[key];
        }
    }
}
