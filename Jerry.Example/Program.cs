using System.Reflection;
using Jerry.Utilities.IoC.Default;
using Jerry.Utilities.IoC.Frozen;
using Jerry.Utilities.IoC.General;
using Jerry.Utilities.IoC.Referenced;


namespace Jerry.Example;

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

