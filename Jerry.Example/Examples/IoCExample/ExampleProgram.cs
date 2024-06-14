using System.Reflection;
using Jerry.Utilities.Examples.IoCExample;
using Jerry.Utilities.IoC.Frozen;
using Jerry.Utilities.IoC.General;

namespace Jerry.Example.Examples.IoCExample;

// let's pretend it's an entry point for our program
public static class IoCExampleProgram
{
    public static void IoCMain(string[] args)
    {
        // creating a new builder instance to build settings for our IoCManager
        var builder = new IoCManagerBuilder();
        
        // setting mode to all, so we will use both methods of auto registration(inheritor and attribute)
        builder.WithMode(IoCMode.All);

        // assigning an attribute type
        builder.WithAttribute<RegisterDependencyAttribute>();
        
        // assigning inheritor type
        builder.WithInheritor<InheritorDependency>();

        // if we need to register our inheritor class too?
        builder.WithRegisterInheritor(true);
        
        // pass a list of assemblies to scan for types
        builder.WithAssemblies([Assembly.GetExecutingAssembly()]);
        
        // build our settings to pass them into IoCManager
        var settings = builder.Build();
        
        // choose collection type to use and pass settings object to configure IoCManager
        IoCManager.InitializeDependencies<FrozenDependencyCollection>(settings);
        
        // auto-injecting all dependencies into each other
        IoCManager.InjectDependencies();
        
        // you also can chain that methods like this
        settings = builder
            .WithMode(IoCMode.All)
            .WithAttribute<RegisterDependencyAttribute>()
            .WithInheritor<InheritorDependency>()
            .WithRegisterInheritor(false)
            .Build();
        // looks clear, isn't it?
        
        // now you can use IoCManager to manage your dependencies!
        var dep = IoCManager.Resolve<Dependency1>();
        dep.Write(); // Dependencies!!!
        
        
        // little note, don't try to use IoCManager without building it with settings, it won't work, or it will work incorrectly
    }
}

[RegisterDependency]
public sealed class Dependency1
{
    public void Write()
    {
        Console.Write("Dependencies!!!");
    }
}