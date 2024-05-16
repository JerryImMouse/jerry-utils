using Project.Utilities.IoC.General;

namespace Project.Utilities.TypeFactory;

/// <summary>
/// Instantiates new instances of types, also can register them in IoCManager automatically
/// </summary>
public class DynamicTypeFactory
{
    private static DynamicTypeFactory? _instance;

    private DynamicTypeFactory()
    {
    }
    
    public static DynamicTypeFactory Instance
    {
        get { return _instance ??= new DynamicTypeFactory(); }
        private set => _instance = value;
    }
    
    public T? CreateInstanceUnchecked<T>(bool register = false)
    {
        return (T?)CreateInstanceUnchecked(typeof(T), register);
    }

    public object? CreateInstanceUnchecked(Type t, bool register = false)
    {
        var instance = Activator.CreateInstance(t);
        if (!register) 
            return instance;
        
        if (instance == null)
            throw new NullTypeRegisterAttemptException(t);
        
        IoCManager.RegisterDependency(t, instance!);
        return instance;
    }

    public object[] CreateMultipleUnchecked(Type[] types)
    {
        var objects = new object[types.Length];
        for (var i = 0; i < types.Length; i++)
        {
            var inst = CreateInstanceUnchecked(types[i]);
            objects[i] = inst ?? throw new NullTypeProvided(types[i]);
        }
        return objects;
    }

    public Dictionary<Type, object> CreateMultipleUncheckedToDict(Type[] types)
    {
        var dict = new Dictionary<Type, object>();
        var inst = CreateMultipleUnchecked(types);
        for (var i = 0; i < types.Length; i++)
        {
            dict.Add(types[i], inst[i]);
        }
        return dict;
    }

    class NullTypeRegisterAttemptException(Type t) 
        : Exception($"Asked to register null type {t.Name}");
    class NullTypeProvided(Type t) 
        : Exception($"Asked to create null type {t.Name}");
}