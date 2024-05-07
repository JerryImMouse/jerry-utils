using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Jerry.Utilities.IoC.Interfaces;
using Jerry.Utilities.Utility;

namespace Jerry.Utilities.IoC.Default;

/// <summary>
/// Simple container that uses usual <see cref="Dictionary{TKey,TValue}"/>
/// </summary>
public class DependencyCollection : IDependencyCollection
{
    private Dictionary<Type, object> _dependencies;

    private DependencyCollection(Dictionary<Type, object> dict, bool logTime = false)
    {
        var rt = new Stopwatch();
        rt.Start();
        _dependencies = dict;
        rt.Stop();
        if (logTime)
            Console.WriteLine($"Initialized dep dict in {rt.Elapsed}");
    }
    public static IDependencyCollection InitializeDependencies(Dictionary<Type, object> dict, bool logTime = false)
    {
        return new DependencyCollection(dict, logTime);
    }
    
    public bool TryGetDependency(Type t, [NotNullWhen(true)] out object? instance)
    {
        return _dependencies.TryGetValue(t, out instance);
    }

    public bool TryGetDependency<T>([NotNullWhen(true)]out object? instance)
    {
        return TryGetDependency(typeof(T), out instance);
    }

    public object GetDependency(Type t)
    {
        try
        {
            return _dependencies[t];
        }
        catch (ArgumentException)
        {
            throw new DependencyNotFoundException(t);
        }
    }

    public object GetDependency<T>()
    {
        return GetDependency(typeof(T));
    }

    public void InjectDependencies(Dictionary<Type, object> dict, bool logTime = false)
    {
        var rt = Stopwatch.StartNew();
        _dependencies.AddRange(dict);
        rt.Stop();
        if (logTime)
            Console.WriteLine($"Added dependencies in {rt.Elapsed}");
    }

    public void InjectDependency(Type t, object instance, bool logTime = false)
    {
        var rt = Stopwatch.StartNew();
        _dependencies.Add(t, instance);
        rt.Stop();
        if (logTime)
            Console.WriteLine($"Added dependencies in {rt.Elapsed}");
    }

    public void InjectDependency<T>(object instance, bool logTime = false)
    {
        var rt = Stopwatch.StartNew();
        _dependencies.Add(typeof(T), instance);
        rt.Stop();
        if (logTime)
            Console.WriteLine($"Added dependencies in {rt.Elapsed}");
    }

    public IEnumerable<KeyValuePair<Type, object>> EnumerateDependencies()
    {
        foreach (var dep in _dependencies)
        {
            yield return dep;
        }
    }

    class DependencyAlreadyExistsException(Type type)
        : Exception($"Dependency with type {type.Name} already exists inside collection");
    class DependencyNotFoundException(Type type)
        : Exception($"Dependency with type {type.Name} not found inside frozen collection");
}