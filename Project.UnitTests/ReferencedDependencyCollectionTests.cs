using System.Diagnostics;
using Project.Utilities.IoC.General;
using Project.Utilities.IoC.Referenced;

namespace Project.UnitTests;

public class ReferencedDependencyCollectionTests
{
    private ReferencedDependencyCollection _dependencies = default!;
    [SetUp]
    public void Setup()
    {
        var dict = new Dictionary<Type, object>()
        {
            { typeof(Dep1), new Dep1() },
            { typeof(Dep2), new Dep2() }
        };
        _dependencies = (ReferencedDependencyCollection)ReferencedDependencyCollection.InitializeDependencies(dict);
    }


    [Test]
    public void GetDependency()
    {
        var instance = _dependencies.GetDependency<Dep1>();
        Debug.Assert(instance != null);
        Debug.Assert(instance.GetType() == typeof(Dep1));
    }

    [Test]
    public void TryGetDependency()
    {
        var result = _dependencies.TryGetDependency<Dep2>(out var instance);
        var result2 = _dependencies.TryGetDependency<Dep3Unreg>(out var instance2);
        Debug.Assert(result, "result was not true");
        Debug.Assert(!result2, "result2 was true");
        Debug.Assert(instance != null, "instance was null");
        Debug.Assert(instance2 == null, "instance2 was not null");
        Debug.Assert(instance.GetType() == typeof(Dep2), "instance.GetType() == typeof(Dep2)");
    }

    [Test]
    public void InjectDependency()
    {
        var unreg = new Dep3Unreg();
        _dependencies.InjectDependency<Dep3Unreg>(unreg);
        var instance = _dependencies.GetDependency<Dep3Unreg>();
        Debug.Assert(instance != null);
        Debug.Assert(instance.GetType() == typeof(Dep3Unreg));
    }
}

public class Dep1
{
}

public class Dep2
{
}

public class Dep3Unreg
{
}