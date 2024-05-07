using System.Reflection;
using Jerry.Utilities.IoC.Default;
using Jerry.Utilities.IoC.Frozen;
using Jerry.Utilities.IoC.General;

namespace Jerry.Utilities.Examples.IoCExample;

public class BaseEntryPoint
{
    public static void InitDependencies()
    {
        var assemblies = new[] { Assembly.GetCallingAssembly() };
        IoCManager.RegisterAllDependencies<RegisterDependencyAttribute, FrozenDependencyCollection>(assemblies, typeof(InheritorDependency), false);
        IoCManager.InjectDependencies();
    }
}