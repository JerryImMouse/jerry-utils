using System.Diagnostics;
using Project.Utilities;
using Project.Utilities.Examples.IoCExample;
using Project.Utilities.IoC.Frozen;
using Project.Utilities.IoC.General;

namespace Project.UnitTests;

public class FrozenDependencyCollectionTests
{
    private FrozenDependencyCollection _frozenCollection;

    class DependencyTestClass1
    {
        
    }
    class DependencyTestClass2
    {
        
    }
    [SetUp]
    public void Setup()
    {
        var dict = new Dictionary<Type, object>()
        {
            { typeof(DependencyTestClass1), new DependencyTestClass1() },
        };
        _frozenCollection = (FrozenDependencyCollection)FrozenDependencyCollection.InitializeDependencies(dict);
    }

    [Test]
    public void InjectDependencyType()
    {
        var type = typeof(DependencyTestClass2);
        var dependency = new DependencyTestClass2();
        
        var rt = Stopwatch.StartNew();
        _frozenCollection.InjectDependency(type, dependency);
        rt.Stop();
        Debug.WriteLine($"Froze dictionary in {rt.Elapsed}");
    }

    [Test]
    public void InjectDependency()
    {
        var dep = new DependencyTestClass2();
        var rt = Stopwatch.StartNew();
        _frozenCollection.InjectDependency<DependencyTestClass2>(dep);
        rt.Stop();
        Debug.WriteLine($"Froze dictionary in {rt.Elapsed}");
    }

    [Test]
    public void ResolveDependency()
    {
        var result = _frozenCollection.TryGetDependency<DependencyTestClass1>(out var instance);
        Debug.Assert(result, "Unable to get registered type");
        Debug.Assert(instance != null, "Method said it was resolved, but value was null");
        Debug.Assert(instance.GetType().IsAssignableTo(typeof(DependencyTestClass1)), "Returned instance was not assignable to its type");
    }

    [Test]
    public void GetDependency()
    {
        var instance = _frozenCollection.GetDependency<DependencyTestClass1>();
        Debug.Assert(instance != null, "Instance was null");
    }

    [Test]
    public void EnumerateDependencies()
    {
        var count = _frozenCollection.EnumerateDependencies();
        Debug.Assert(count.Count() == 1, "Returned count was not 1");
    }
}