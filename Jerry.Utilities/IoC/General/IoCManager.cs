using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Jerry.Utilities.IoC.Frozen;
using Jerry.Utilities.IoC.Interfaces;
using Jerry.Utilities.Logging;
using Jerry.Utilities.Logging.LogStructs;
using Jerry.Utilities.Reflection;
using Jerry.Utilities.TypeFactory;
using Jerry.Utilities.Utility;

namespace Jerry.Utilities.IoC.General;

/// <summary>
/// Main IoC class which registers, injects, and resolves dependencies inside <see cref="IDependencyCollection"/>s
/// </summary>
public static class IoCManager
{
    private static IDependencyCollection _dependencyCollection = default!;
    private static readonly DynamicTypeFactory TypeFactory = DynamicTypeFactory.Instance;
    private static readonly ReflectionManager Reflection = ReflectionManager.Instance;
    public static event Action<Type[]>? TypesRegistered;
    public static event Action<Dictionary<Type, List<Type>>>? TypesInjected;
    private static Logger? _logger;
    private static bool _initialized = false;

    public static void Initialize()
    {
        if (_initialized)
            throw new AlreadyInitializedException(nameof(IoCManager));
        _logger = Logger.GetLogger("ioc", HandlerFlags.Console | HandlerFlags.File, LogLevel.Debug);
        _initialized = true;
    }

    public static void SetHandler(Logger logger) => _logger = logger;
    public static void RemoveHandler() => _logger = null;
    
    #region AutoRegistrationModes
    private static List<Type> GetAllMode(Type attrib, Type inheritor, bool registerSelf)
    {
        var inheritors = Reflection.FindTypesWithInheritor(inheritor).ToList();
        var attribTypes = Reflection.FindTypesWithAttribute(attrib);
        inheritors.AddRange(attribTypes);
        if (registerSelf)
            inheritors.Add(inheritor);
        return inheritors;
    }

    private static List<Type> GetAttributeMode(Type attrib)
    {
        return Reflection.FindTypesWithAttribute(attrib).ToList();
    }
    
    private static List<Type> GetInheritMode(Type inheritor, bool registerSelf = true)
    {
        var types = Reflection.FindTypesWithInheritor(inheritor).ToList();
        if (registerSelf)
            types.Add(inheritor);
        return types;
    }
    #endregion

    #region AutoRegistrationLogic

    public static void InitializeDependencies<TCollection>(IoCSettings settings) where TCollection : IDependencyCollection
    {
        if (!_initialized)
            Initialize();
        
        // skip validation, settings already validated everything for us
        var assemblies = settings.Assemblies;
        var mode = settings.Mode;
        var attrib = settings.Attribute;
        var inheritor = settings.Inheritor;
        var registerSelf = settings.Register;
        
        Reflection.LoadAssemblies(assemblies.ToArray());
        
        var types = mode switch
        {
            IoCMode.All => GetAllMode(attrib!, inheritor!, registerSelf),
            IoCMode.Attribute => GetAttributeMode(attrib!),
            IoCMode.Inheritor => GetInheritMode(inheritor!),
            _ => throw new ArgumentException("Unrecognized IoC mode")
        };
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
            
            var instance = TypeFactory.CreateInstanceUnchecked(curType);
            if (instance == null)
                throw new UnableToCreateInstanceException(curType);
            
            dict.Add(curType, instance);
        }

        var rt = Stopwatch.StartNew();
        _dependencyCollection = TCollection.InitializeDependencies(dict);
        rt.Stop();
        _logger?.Debug($"Initialized {dict.Count} dependency(ies) in {rt.Elapsed} with {typeof(TCollection).Name}");
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
        if (!_initialized)
            throw new NotInitializedException(nameof(IoCManager));
        return _dependencyCollection.GetDependency(t);
    }

    public static bool TryResolve(Type t, [NotNullWhen(true)] out object? instance)
    {
        if (!_initialized)
            throw new NotInitializedException(nameof(IoCManager));
        return _dependencyCollection.TryGetDependency(t, out instance);
    }

    public static bool TryResolve<T>([NotNullWhen(true)] out object? instance)
    {
        if (!_initialized)
            throw new NotInitializedException(nameof(IoCManager));
        return _dependencyCollection.TryGetDependency<T>(out instance);
    }

    #endregion

    #region Registration

    public static void RegisterDependency<T>()
    {
        if (!_initialized)
            throw new NotInitializedException(nameof(IoCManager));
        
        var instance = TypeFactory.CreateInstanceUnchecked<T>();
        if (instance == null)
            throw new UnableToCreateInstanceException(typeof(T));
        RegisterDependency<T>(instance);
    }
    public static void RegisterDependency(Type t, object instance)
    {
        if (!_initialized)
            throw new NotInitializedException(nameof(IoCManager));
        
        AssertClass(instance);
        _dependencyCollection.InjectDependency(t, instance);
    }
    public static void RegisterDependency<T>(object instance)
    {
        if (!_initialized)
            throw new NotInitializedException(nameof(IoCManager));
        
        AssertClass(instance);
        _dependencyCollection.InjectDependency<T>(instance);
    }

    public static void RegisterDependencies(Dictionary<Type, object> dict)
    {
        if (!_initialized)
            throw new NotInitializedException(nameof(IoCManager));
        
        foreach (var kvp in dict)
        {
            AssertClass(kvp.Key);
        }
        _dependencyCollection.InjectDependencies(dict);
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
        if (!_initialized)
            throw new NotInitializedException(nameof(IoCManager));
        
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
        if (!_initialized)
            throw new NotInitializedException(nameof(IoCManager));
        
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
        if (!_initialized)
            throw new NotInitializedException(nameof(IoCManager));
        
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
        if (!_initialized)
            throw new NotInitializedException(nameof(IoCManager));
        
        var injected = new Dictionary<Type, List<Type>>();
        foreach (var dependency in _dependencyCollection.EnumerateDependencies())
        {
            var instance = dependency.Value;
            InjectDependencies(instance, out var otherInjected);
            injected.AddRange(otherInjected);
        }
        TypesInjected?.Invoke(injected);
    }

    public static void InjectDependencies(object someInstance)
    {
        var instanceType = someInstance.GetType();
        if (!instanceType.IsClass)
            throw new NotAClassException(instanceType);

        var fields = instanceType.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)
            .Where(f => f.GetCustomAttribute<DependencyAttribute>(false) != null);

        foreach (var field in fields)
        {
            var fieldType = field.FieldType;
            if (!TryResolve(fieldType, out var toInject))
                throw new UnregisteredDependencyException(instanceType, fieldType);
            field.SetValue(someInstance, toInject);
        }
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

    class NotInitializedException(string name) 
        : Exception($"Tried to use uninitialized object: {name}");
    class AlreadyInitializedException(string name) 
        : Exception($"Tried to initialize already initialized object: {name}");

    class UnregisteredDependencyException(Type target, Type asked)
        : Exception($"{target.Name} asked about unregistered dependency with type {asked.Name}");

    #endregion
}