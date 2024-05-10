using System.Diagnostics;
using System.Reflection;
using Project.Utilities.Examples.IoCExample;
using Project.Utilities.IoC.Frozen;
using Project.Utilities.IoC.General;
using Project.Utilities.Reflection;

namespace Project.UnitTests;

public class IoCManagerFrozenTestsAll
{
    [SetUp]
    public void SetUp()
    {
        ReflectionManager.Instance.LoadAssemblies([Assembly.GetExecutingAssembly()]);
        var settings = new IoCManagerBuilder()
            .WithMode(IoCMode.All)
            .WithAttribute<RegisterDependencyAttribute>()
            .WithInheritor<InheritorDependency>()
            .WithRegisterInheritor(true)
            .Build();
        IoCManager.InitializeDependencies<FrozenDependencyCollection>(settings);
    }

    [Test]
    public void Resolve()
    {
        var resolved1 = IoCManager.Resolve<DependencyInnerTestClass>();
        Debug.Assert(resolved1 != null);
    }

    [Test]
    public void TryResolve()
    {
        var resolved1 = IoCManager.TryResolve<DependencyInnerTestClass>(out var instance);
        var resolved2 = IoCManager.TryResolve<DependencyUnregistered>(out var instance2);
        Debug.Assert(resolved1, "Unable to resolve registered class");
        Debug.Assert(!resolved2, "Resolved unregistered type");
        Debug.Assert(instance != null);
        Debug.Assert(instance2 == null);
    }

    [Test]
    public void InjectEachOther()
    {
        IoCManager.InjectDependencies();
        var dependency1 = IoCManager.Resolve<DependencyTestClass1>();
        var dependency2 = IoCManager.Resolve<DependencyTestClass2>();
        Debug.Assert(dependency1._inner2 != null);
        Debug.Assert(dependency2._inner != null);
    }

    [Test]
    public void InjectDirectly()
    {
        var instance = new DependencyTestClass1();
        IoCManager.InjectDependency<DependencyTestClass2>(instance);
        Debug.Assert(instance._inner2 != null);
    }
}
[RegisterDependency]
class DependencyTestClass1
{
    [Dependency] public DependencyTestClass2 _inner2 = default!;
}
class DependencyTestClass2 : InheritorDependency
{
    [Dependency] public readonly DependencyInnerTestClass _inner = default!;
}
[RegisterDependency]
class DependencyInnerTestClass
{
}

class DependencyUnregistered
{
        
}