using Jerry.Utilities.IoC.General;

namespace Jerry.Utilities.TypeFactory;

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

    class NullTypeRegisterAttemptException(Type t) 
        : Exception($"Asked to register null type {t.Name}");
}