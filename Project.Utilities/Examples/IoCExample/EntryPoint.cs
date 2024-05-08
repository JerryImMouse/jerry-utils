using System.Reflection;
using Project.Utilities.IoC.Frozen;
using Project.Utilities.IoC.General;
using Project.Utilities.IoC.Referenced;

namespace Project.Utilities.Examples.IoCExample;

public class BaseEntryPoint
{
    public static void InitDependencies()
    {
        var assemblies = new[] { Assembly.GetCallingAssembly() };
        IoCManager.RegisterAllDependencies<RegisterDependencyAttribute, ReferencedDependencyCollection>(assemblies, typeof(InheritorDependency), false);
        IoCManager.InjectDependencies();
    }
}