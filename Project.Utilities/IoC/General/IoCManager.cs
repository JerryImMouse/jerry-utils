using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Project.Utilities.IoC.Frozen;
using Project.Utilities.IoC.Interfaces;
using Project.Utilities.Reflection;
using Project.Utilities.TypeFactory;
using Project.Utilities.Utility;

namespace Project.Utilities.IoC.General;

/// <summary>
/// Main IoC class which registers, injects, and resolves dependencies inside <see cref="IDependencyCollection"/>s
/// </summary>
public static class IoCManager
{
    private static IDependencyCollection _dependencyCollection = default!;
    private static readonly DynamicTypeFactory _typeFactory = DynamicTypeFactory.Instance;
    private static readonly ReflectionManager _reflection = ReflectionManager.Instance;
    public static event Action<Type[]>? TypesRegistered;
    public static event Action<Dictionary<Type, List<Type>>>? TypesInjected;
    private static bool _log = true;

    #region AutoRegistration

    public static void RegisterAllDependencies<TAttribute, TCollection>(Assembly[] assemblies, Type inheritor,
        bool registerSelf = true) where TCollection : IDependencyCollection where TAttribute : Attribute
    {
        var inheritors = _reflection.FindTypesWithInheritor(inheritor).ToList();
        var attribTypes = _reflection.FindTypesWithAttribute<TAttribute>();
        inheritors.AddRange(attribTypes);
        if (registerSelf)
            inheritors.Add(inheritor);
        RegisterTypesInternal<TCollection>(inheritors.ToArray());
    }
    
    public static void RegisterDependenciesInherits<TCollection>(Type inheritor, bool registerSelf = true) where TCollection : IDependencyCollection
    {
        var types = _reflection.FindTypesWithInheritor(inheritor).ToList();
        if (registerSelf)
            types.Add(inheritor);
        RegisterTypesInternal<TCollection>(types.ToArray());
    }
    public static void RegisterDependencies<TAttribute, TCollection>(Assembly[] assemblies) where TCollection : IDependencyCollection where TAttribute : Attribute
    {
        var types = _reflection.FindTypesWithAttribute<TAttribute>();
        RegisterTypesInternal<TCollection>(types.ToArray());
    }

    private static void RegisterTypesInternal<TCollection>(Type[] types) where TCollection : IDependencyCollection
    {
        var dict = new Dictionary<Type, object>();
        for (var i = 0; i < types.Length; i++)
        {
            var curType = types[i];
            if (!curType.HasParameterlessConstructor())
                throw new DependencyConstructorException(curType); 
            
            var instance = _typeFactory.CreateInstanceUnchecked(curType);
            if (instance == null)
                throw new UnableToCreateInstanceException(curType);
            
            dict.Add(curType, instance);
        }
        _dependencyCollection = TCollection.InitializeDependencies(dict, _log);
        TypesRegistered?.Invoke(types);
    }

    #endregion

    #region Resolving

    public static T Resolve<T>()
    {
        return (T)Resolve(typeof(T));
    }

    public static object Resolve(Type t)
    {
        return _dependencyCollection.GetDependency(t);
    }

    public static bool TryResolve(Type t, [NotNullWhen(true)] out object? instance)
    {
        return _dependencyCollection.TryGetDependency(t, out instance);
    }

    public static bool TryResolve<T>([NotNullWhen(true)] out object? instance)
    {
        return _dependencyCollection.TryGetDependency<T>(out instance);
    }

    #endregion

    #region Registration

    public static void RegisterDependency<T>()
    {
        var instance = _typeFactory.CreateInstanceUnchecked<T>();
        if (instance == null)
            throw new UnableToCreateInstanceException(typeof(T));
        RegisterDependency<T>(instance);
    }
    public static void RegisterDependency(Type t, object instance)
    {
        AssertClass(instance);
        _dependencyCollection.InjectDependency(t, instance);
    }
    public static void RegisterDependency<T>(object instance)
    {
        AssertClass(instance);
        _dependencyCollection.InjectDependency<T>(instance, _log);
    }

    public static void RegisterDependencies(Dictionary<Type, object> dict)
    {
        foreach (var kvp in dict)
        {
            AssertClass(kvp.Key);
        }
        _dependencyCollection.InjectDependencies(dict, _log);
    }
    /// <summary>
    /// Ensures passing object is a class instance
    /// </summary>
    /// <param name="instance">Object to check</param>
    /// <exception cref="NotAClassException">Self-explanatory</exception>
    private static void AssertClass(object instance)
    {
        var type = instance.GetType();
        if (!type.IsClass)
            throw new NotAClassException(type);
    }

    #endregion

    #region Injecting

    /// <summary>
    /// Injects dependency to target instance
    /// </summary>
    /// <param name="injecting">Instance to inject</param>
    /// <param name="target">Target instance</param>
    /// <exception cref="NoValidFieldsException">Thrown when there are no valid fields to inject to</exception>
    public static void InjectDependency(object injecting, object target)
    {
        var injectingType = injecting.GetType();
        var targetType = target.GetType();
        var fields = targetType.GetFieldsWithTypeAndAttribute<DependencyAttribute>(injectingType);
        if (fields.Length == 0)
            throw new NoValidFieldsException(injectingType, targetType);

        foreach (var field in fields)
        {
            field.SetValue(target, injecting);
        }
    }
    /// <summary>
    /// Injects dependency with type <see cref="T"/> to <see cref="target"/> instance
    /// </summary>
    /// <param name="target">Instance to inject to</param>
    /// <typeparam name="T">Type of dependency to inject to target</typeparam>
    /// <exception cref="NoValidFieldsException">Thrown when there are no valid fields to inject to</exception>
    public static void InjectDependency<T>(object target)
    {
        var injectingInstance = Resolve<T>();
        InjectDependency(injectingInstance!, target);
    }

    /// <summary>
    /// Injects dependencies in all fields marked with <see cref="DependencyAttribute"/> in <see cref="instance"/>
    /// </summary>
    /// <param name="instance">Instance to inject dependencies</param>
    /// <param name="injected">Types that were injected</param>
    public static void InjectDependencies(object instance, out Dictionary<Type, List<Type>> injected)
    {
        var type = instance.GetType();
        var fields = type.GetFieldsWithAttribute<DependencyAttribute>();
        injected = new Dictionary<Type, List<Type>>();
        var list = injected.GetOrNew(type);
        foreach (var field in fields)
        {
            var fieldType = field.FieldType;
            if(!TryResolve(fieldType, out var resolved))
                continue;
            field.SetValue(instance, resolved);
            list.Add(fieldType);
        }
    }
    /// <summary>
    /// Injects dependencies into each other, takes instances from <see cref="FrozenDependencyCollection"/> instance
    /// </summary>
    public static void InjectDependencies()
    {
        var injected = new Dictionary<Type, List<Type>>();
        foreach (var dependency in _dependencyCollection.EnumerateDependencies())
        {
            var instance = dependency.Value;
            InjectDependencies(instance, out var otherInjected);
            injected.AddRange(otherInjected);
        }
        TypesInjected?.Invoke(injected);
    }

    #endregion

    #region Exceptions

    class DependencyConstructorException(Type t)
        : Exception($"Type {t.Name} has no parameterless constructor, so it cannot be registered");

    class UnableToCreateInstanceException(Type t)
        : Exception($"Unable to create instance of type {t.Name}");

    class NotAClassException(Type t) 
        : Exception($"Tried to inject non-class dependency with type {t.Name}");

    class NoValidFieldsException(Type t, Type s)
        : Exception($"Not found fields with type {s.Name} for injecting into {t.Name}");

    #endregion
}