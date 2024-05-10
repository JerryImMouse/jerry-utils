using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Project.Utilities.IoC.Interfaces;
using Project.Utilities.Utility;

namespace Project.Utilities.IoC.Default;

/// <summary>
/// Simple container that uses usual <see cref="Dictionary{TKey,TValue}"/>
/// </summary>
public class DependencyCollection : IDependencyCollection
{
    private Dictionary<Type, object> _dependencies;

    private DependencyCollection(Dictionary<Type, object> dict)
    {
        _dependencies = dict;
    }
    public static IDependencyCollection InitializeDependencies(Dictionary<Type, object> dict)
    {
        return new DependencyCollection(dict);
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

    public void InjectDependencies(Dictionary<Type, object> dict)
    {
        _dependencies.AddRange(dict);
    }

    public void InjectDependency(Type t, object instance)
    {
        _dependencies.Add(t, instance);
    }

    public void InjectDependency<T>(object instance)
    {
        _dependencies.Add(typeof(T), instance);
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