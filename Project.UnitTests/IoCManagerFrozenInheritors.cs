using System.Diagnostics;
using System.Reflection;
using Project.Utilities.Examples.IoCExample;
using Project.Utilities.IoC.Frozen;
using Project.Utilities.IoC.General;
using Project.Utilities.Reflection;

namespace Project.UnitTests;

//TODO: Probably more tests here
public class IoCManagerFrozenInheritors
{
    [SetUp]
    public void SetUp()
    {
        ReflectionManager.Instance.LoadAssemblies([Assembly.GetExecutingAssembly()]);
        IoCManager.RegisterDependenciesInherits<FrozenDependencyCollection>(typeof(InheritorDependency), true);
    }
    
    [Test]
    public void Resolve()
    {
        var resolved1 = IoCManager.TryResolve<DependencyInnerTestClass>(out var instance);
        var resolved2 = IoCManager.Resolve<DependencyTestClass2>();
        Debug.Assert(instance == null && !resolved1);
        Debug.Assert(resolved2 != null);
    }

    [Test]
    public void TryResolve()
    {
        var resolved1 = IoCManager.TryResolve<DependencyInnerTestClass>(out var instance);
        var resolved2 = IoCManager.TryResolve<DependencyUnregistered>(out var instance2);
        var resolved3 = IoCManager.TryResolve<DependencyTestClass2>(out var instance3);
        Debug.Assert(!resolved1, "Resolved unregistered type");
        Debug.Assert(!resolved2, "Resolved unregistered type");
        Debug.Assert(resolved3, "Unable to resolve registered class");
        Debug.Assert(instance == null);
        Debug.Assert(instance2 == null);
        Debug.Assert(instance3 != null);
    }
}