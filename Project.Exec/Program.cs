using System.Reflection;
using Project.Utilities.IoC.Default;
using Project.Utilities.IoC.Frozen;
using Project.Utilities.IoC.General;
using Project.Utilities.IoC.Referenced;


namespace Project.Exec;

public static class Program
{
    public static void Main(string[] args)
    {
        var settings = new IoCManagerBuilder()
            .WithAttribute<RegisterDependencyAttribute>()
            .WithAssemblies([Assembly.GetExecutingAssembly()])
            .WithMode(IoCMode.Attribute)
            .Build();
        IoCManager.InitializeDependencies<FrozenDependencyCollection>(settings);
        var dep = IoCManager.Resolve<Dependency>();
        dep.WriteLine();
        Thread.Sleep(2);
    }
}
[RegisterDependency]
public sealed class Dependency()
{
    public void WriteLine()
    {
        Console.WriteLine("Test");
    }
}

