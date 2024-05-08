using System.Reflection;
using Project.Utilities.Utility;

namespace Project.Utilities.Reflection;

public class ReflectionManager
{
    private readonly List<Assembly> _assemblies = new();
    private static ReflectionManager? _instance;

    public static ReflectionManager Instance
    {
        get
        {
            if (_instance != null) 
                return _instance;
            
            _instance = new ReflectionManager();
            _instance.LoadAssemblies(new []{Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly()});
            return _instance;

        }
        private set => _instance = value;
    }
    private readonly List<Type> _getAllTypesCache = [];

    public void LoadAssemblies(Assembly[] assemblies)
    {
        _assemblies.AddRange(assemblies);
        EnsureGetAllTypesCache();
    }

    public void EnsureGetAllTypesCache()
    {
        if (_getAllTypesCache.Count != 0)
            return;

        var typeSets = new List<Type[]>();
        var totalLength = 0;
        foreach (var assembly in _assemblies)
        {
            var types = assembly.GetTypes();
            typeSets.Add(types);
            totalLength += types.Length;
        }

        _getAllTypesCache.Capacity = totalLength;

        foreach (var typeSet in typeSets)
        {
            foreach (var type in typeSet)
            {
                _getAllTypesCache.Add(type);
            }
        }
    }

    public IEnumerable<Type> FindTypesWithInheritor<T>()
    {
        return FindTypesWithInheritor(typeof(T));
    }

    public IEnumerable<Type> FindTypesWithInheritor(Type inheritor)
    {
        return _getAllTypesCache.Where(t => t.IsSubclassOf(inheritor));
    }   
    
    public IEnumerable<Type> FindTypesWithAttribute<T>() where T : Attribute
    {
        return FindTypesWithAttribute(typeof(T));
    }

    public IEnumerable<Type> FindTypesWithAttribute(Type attributeType)
    {
        EnsureGetAllTypesCache();
        return _getAllTypesCache.Where(type => type.HasCustomAttribute(attributeType));
    }

    public IEnumerable<Type> FindAllTypes()
    {
        EnsureGetAllTypesCache();
        return _getAllTypesCache;
    }
}