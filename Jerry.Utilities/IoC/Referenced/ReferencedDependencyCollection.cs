using System.Collections.Frozen;
using Jerry.Utilities.IoC.Interfaces;

namespace Jerry.Utilities.IoC.Referenced;

public class ReferencedDependencyCollection : IDependencyCollection
{
    private FrozenDictionary<Type, object> _dependencies;
    private object[] _instancesArray = Array.Empty<object>();
    
    public bool TryGetDependency(Type t, out object? instance)
    {
        instance = _instancesArray[DepIdx.ArrayIndex(t)];
        return instance != null;
    }

    public bool TryGetDependency<T>(out object? instance)
    {
        instance = _instancesArray[DepIdx.ArrayIndex<T>()];
        return instance != null;
    }

    public object GetDependency(Type t)
    {
        return _instancesArray[DepIdx.ArrayIndex(t)];
    }

    public object GetDependency<T>()
    {
        return GetDependency(typeof(T));
    }

    public void InjectDependencies(Dictionary<Type, object> dict)
    {
        var ourDict = _dependencies.ToDictionary();
        foreach (var kvp in dict)
        {
            var idx = DepIdx.Index(kvp.Key);
            ourDict.Add(kvp.Key, kvp.Value);
            DepIdx.AssignArray(ref _instancesArray, idx, kvp.Value);
        }
        _dependencies = ourDict.ToFrozenDictionary();
    }

    public void InjectDependency(Type t, object instance)
    {
        var ourDict = _dependencies.ToDictionary();
        var idx = DepIdx.Index(t);
        ourDict.Add(t, instance);
        DepIdx.AssignArray(ref _instancesArray, idx, instance);
        _dependencies = ourDict.ToFrozenDictionary();
    }

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

    public static IDependencyCollection InitializeDependencies(Dictionary<Type, object> dict)
    {
        return new ReferencedDependencyCollection(dict);
    }

    private ReferencedDependencyCollection(Dictionary<Type, object> dict)
    {
        var otherDict = new Dictionary<Type, object>();
        foreach (var item in dict)
        {
            var idx = DepIdx.Index(item.Key);
            otherDict.Add(item.Key, item.Value);
            DepIdx.AssignArray(ref _instancesArray, idx, item.Value);
        }

        _dependencies = otherDict.ToFrozenDictionary();
    }
}