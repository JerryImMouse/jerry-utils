using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Jerry.Utilities.Utility;

/// <summary>
/// Collection of methods to extend <see cref="List{T}"/>, <see cref="Dictionary{TKey,TValue}"/>, <see cref="Array"/> etc.
/// </summary>
public static class CollectionExtensions
{
    public static IList<T> Clone<T>(this IList<T> listToClone) where T : ICloneable
    {
        var clone = new List<T>(listToClone.Count);

        foreach (var value in listToClone)
        {
            clone.Add((T) value.Clone());
        }

        return clone;
    }

    /// <summary>
    ///     Creates a shallow clone of a list.
    ///     Basically a new list with all the same elements.
    /// </summary>
    /// <param name="self">The list to shallow clone.</param>
    /// <typeparam name="T">The type of the list's elements.</typeparam>
    /// <returns>A new list with the same elements as <paramref name="list" />.</returns>
    public static List<T> ShallowClone<T>(this List<T> self)
    {
        var list = new List<T>(self.Count);
        list.AddRange(self);
        return list;
    }
    public static Dictionary<TKey, TValue> ShallowClone<TKey, TValue>(this Dictionary<TKey, TValue> self)
        where TKey : notnull
    {
        var dict = new Dictionary<TKey, TValue>(self.Count);
        foreach (var item in self)
        {
            dict[item.Key] = item.Value;
        }
        return dict;
    }

    public static bool TryGetValue<T>(this IList<T> list, int index, out T value)
    {
        if (list.Count > index)
        {
            value = list[index];
            return true;
        }

        value = default!;
        return false;
    }
    /// <summary>
    ///     Remove an item from the list, replacing it with the one at the very end of the list.
    ///     This means that the order will not be preserved, but it should be an O(1) operation.
    /// </summary>
    /// <param name="index">The index to remove</param>
    /// <returns>The removed element</returns>
    public static T RemoveSwap<T>(this IList<T> list, int index)
    {
        // This method has no implementation details,
        // and changing the result of an operation is a breaking change.
        var old = list[index];
        var replacement = list[list.Count - 1];
        list[index] = replacement;
        // TODO: Any more efficient way to pop the last element off?
        list.RemoveAt(list.Count - 1);
        return old;
    }

    /// <summary>
    ///     Pop an element from the end of a list, removing it from the list and returning it.
    /// </summary>
    /// <param name="list">The list to pop from.</param>
    /// <typeparam name="T">The type of the elements of the list.</typeparam>
    /// <returns>The popped off element.</returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if the list is empty.
    /// </exception>
    public static T Pop<T>(this IList<T> list)
    {
        if (list.Count == 0)
            throw new InvalidOperationException();
        
        var t = list[^1];
        list.RemoveAt(list.Count-1);
        return t;
    }
    public static TValue GetOrNew<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key) where TValue : new()
        where TKey : notnull
    {
        if (!dict.TryGetValue(key, out var value))
        {
            value = new TValue();
            dict.Add(key, value);
        }

        return value;
    }

    public static TValue GetOrNew<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key)
        where TValue : new()
        where TKey : notnull
    {
        ref var entry = ref CollectionsMarshal.GetValueRefOrAddDefault(dict, key, out var exists);
        if (!exists)
            entry = new TValue();

        return entry!;
    }

    public static TValue GetOrNew<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, out bool exists)
        where TValue : new()
        where TKey : notnull
    {
        ref var entry = ref CollectionsMarshal.GetValueRefOrAddDefault(dict, key, out exists);
        if (!exists)
            entry = new TValue();

        return entry!;
    }

    // More efficient than LINQ.
    public static KeyValuePair<TKey, TValue>[] ToArray<TKey, TValue>(this Dictionary<TKey, TValue> dict)
        where TKey : notnull
    {
        var array = new KeyValuePair<TKey, TValue>[dict.Count];

        var i = 0;
        foreach (var kvPair in dict)
        {
            array[i] = kvPair;
            i += 1;
        }

        return array;
    }
    /// <summary>
    /// Tries to get a value from a dictionary and checks if that value is of type T
    /// </summary>
    /// <typeparam name="T">The type that sould be casted to</typeparam>
    /// <returns>Whether the value was present in the dictionary and of the required type</returns>
    public static bool TryCastValue<T, TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, [NotNullWhen(true)] out T? value) where TKey : notnull
    {
        if (dict.TryGetValue(key, out var untypedValue) && untypedValue is T typedValue)
        {
            value = typedValue;
            return true;
        }

        value = default;
        return false;
    }
    public static void AddRange<TKey, TValue>(this Dictionary<TKey, TValue> dict, Dictionary<TKey, TValue> otherDictionary) where TKey : notnull
    {
        foreach (var kvp in otherDictionary)
        {
            dict.Add(kvp.Key, kvp.Value);
        }
    }
    
}