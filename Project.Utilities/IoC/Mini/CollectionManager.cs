using System.Runtime.CompilerServices;
using Project.Utilities.IoC.Interfaces;
using Project.Utilities.TypeFactory;

namespace Project.Utilities.IoC.Mini;

public static class CollectionManager
{
    private static DynamicTypeFactory _typeFactory = DynamicTypeFactory.Instance;
    private static Dictionary<string, object> _collections = new();
    // for more fast usage
    private static IDependencyCollection? _curCollection = null;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void InitCollection<TCollection>(string name, Dictionary<Type, object> dependencies) where TCollection : IDependencyCollection
    {
        InitCollectionInternal<TCollection>(name, dependencies);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void InitCollection<TCollection>(string name, IEnumerable<Type> types) where TCollection : IDependencyCollection
    {
        var instances = _typeFactory.CreateMultipleUncheckedToDict(types.ToArray());
        InitCollectionInternal<TCollection>(name, instances);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void InitCollectionInternal<TCollection>(string name, Dictionary<Type, object> dependencies) where TCollection : IDependencyCollection
    {
        var collection = TCollection.InitializeDependencies(dependencies);
        _collections.Add(name, collection);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AddCollection(string name, IDependencyCollection collection)
    {
        _collections.Add(name, collection);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void RemoveCollection(string name)
    {
        _collections.Remove(name);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IDependencyCollection GetCollection(string name)
    {
        return (IDependencyCollection)_collections[name];
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryGetCollection(string name, out IDependencyCollection? dependencyCollection)
    {
        dependencyCollection = null;
        if (!_collections.TryGetValue(name, out var obj))
            return false;
        dependencyCollection = (IDependencyCollection)obj;
        return true;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static object Resolve(string name, Type t)
    {
        var col = GetCollection(name);
        var dep = col.GetDependency(t);
        return dep;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Resolve<T>(string name)
    {
        return (T)Resolve(name, typeof(T));
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Resolve<T>()
    {
        return (T)Resolve(typeof(T));
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static object Resolve(Type t)
    {
        return _curCollection == null ? throw new NoDefaultCollectionException() : _curCollection.GetDependency(t);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetCollection(string name)
    {
        _curCollection = (IDependencyCollection)_collections[name];
    }

    class NoDefaultCollectionException : Exception;
}