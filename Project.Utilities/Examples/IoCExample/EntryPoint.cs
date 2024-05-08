using System.Reflection;
using Project.Utilities.IoC.Frozen;
using Project.Utilities.IoC.General;

namespace Project.Utilities.Examples.IoCExample;

public class BaseEntryPoint
{
    public static void InitDependencies()
    {
        var assemblies = new[] { Assembly.GetCallingAssembly() };
        IoCManager.RegisterAllDependencies<RegisterDependencyAttribute, FrozenDependencyCollection>(assemblies, typeof(InheritorDependency), false);
        IoCManager.InjectDependencies();
    }
}