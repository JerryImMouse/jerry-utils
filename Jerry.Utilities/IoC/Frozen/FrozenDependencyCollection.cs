using System.Collections.Frozen;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Jerry.Utilities.IoC.Default;
using Jerry.Utilities.IoC.Interfaces;

namespace Jerry.Utilities.IoC.Frozen;

/// <summary>
/// Faster in search, but MUCH slower when you need to add something.<br/><br/>
/// For example <see cref="DependencyCollection"/> will be initialized in ~4 microseconds instead of ~500.<br /><br/>
/// So I hardly not recommend you to use this if you often need to add dependencies to collection.<br /><br/> Use <see cref="DependencyCollection"/> instead.
/// </summary>
public class FrozenDependencyCollection : IDependencyCollection
{
    private FrozenDictionary<Type, object> _dependencies;

    public static IDependencyCollection InitializeDependencies(Dictionary<Type, object> dict)
    {
        return new FrozenDependencyCollection(dict);
    }

    public bool TryGetDependency(Type t, [NotNullWhen(true)] out object? instance)
    {
        instance = null;
        return _dependencies.TryGetValue(t, out instance);
    }

    public bool TryGetDependency<T>([NotNullWhen(true)] out object? instance)
    {
        return TryGetDependency(typeof(T), out instance);
    }

    public object GetDependency(Type t)
    {
        try
        {
            return _dependencies[t];
        }
        catch (KeyNotFoundException)
        {
            throw new DependencyNotFoundException(t);
        }
    }

    public object GetDependency<T>()
    {
        return GetDependency(typeof(T));
    }
    /// <summary>
    /// Injects dependencies into frozen dependency collection
    /// </summary>
    /// <param name="dict"></param>
    /// <param name="logTime"></param>
    /// <exception cref="DependencyAlreadyExistsException"></exception>
    public void InjectDependencies(Dictionary<Type, object> dict)
    {
        var dependencies = _dependencies.ToDictionary();
        foreach (var kvp in dict)
        {
            try
            {
                dependencies.Add(kvp.Key, kvp.Value);
            }
            catch (ArgumentException)
            {
                throw new DependencyAlreadyExistsException(kvp.Key);
            }
        }
        _dependencies = dependencies.ToFrozenDictionary();
    }
    /// <summary>
    /// Injects dependency into frozen collection<br/>
    /// Use <see cref="InjectDependencies"/> if you need to inject multiple dependencies
    /// </summary>
    /// <param name="t">Type to inject</param>
    /// <param name="instance">Instance of that type</param>
    /// <param name="logTime">Log time of freezing internal dictionary</param>
    /// <exception cref="DependencyAlreadyExistsException">Thrown if same dependency has been already added to collection</exception>
    public void InjectDependency(Type t, object instance)
    {
        var dependencies = _dependencies.ToDictionary();

        try
        {
            dependencies.Add(t, instance);
        }
        catch (ArgumentException)
        {
            throw new DependencyAlreadyExistsException(t);
        }
        _dependencies = dependencies.ToFrozenDictionary();
    }
    /// <summary>
    /// Injects dependency into frozen collection<br/>
    /// Use <see cref="InjectDependencies"/> if you need to inject multiple dependencies
    /// </summary>
    /// <param name="instance">Instance of that type</param>
    /// <param name="logTime">Log time of freezing internal dictionary</param>
    /// <typeparam name="T">Type to inject</typeparam>
    /// <exception cref="DependencyAlreadyExistsException">Thrown if same dependency has been already added to collection</exception>
    public void InjectDependency<T>(object instance)
    {
        InjectDependency(typeof(T), instance);
    }

    public IEnumerable<KeyValuePair<Type, object>> EnumerateDependencies()
    {
        foreach (var dependency in _dependencies)
        {
            yield return dependency;
        }
    }

    private FrozenDependencyCollection(Dictionary<Type, object> dict, bool logTime = false)
    {
        var rt = Stopwatch.StartNew();
        _dependencies = dict.ToFrozenDictionary();
        rt.Stop();
        if (logTime)
            Console.WriteLine($"Froze dictionary in {rt.Elapsed}");
    }

    class DependencyAlreadyExistsException(Type type)
        : Exception($"Dependency with type {type.Name} already exists inside collection");
    class DependencyNotFoundException(Type type)
        : Exception($"Dependency with type {type.Name} not found inside frozen collection");
}