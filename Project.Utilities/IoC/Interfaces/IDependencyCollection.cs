using System.Diagnostics.CodeAnalysis;

namespace Project.Utilities.IoC.Interfaces;

public interface IDependencyCollection
{
    bool TryGetDependency(Type t, [NotNullWhen(true)] out object? instance);
    bool TryGetDependency<T>([NotNullWhen(true)] out object? instance);
    object GetDependency(Type t);
    object GetDependency<T>();
    void InjectDependencies(Dictionary<Type, object> dict);
    void InjectDependency(Type t, object instance);
    void InjectDependency<T>(object instance);
    IEnumerable<KeyValuePair<Type, object>> EnumerateDependencies();

    static abstract IDependencyCollection InitializeDependencies(Dictionary<Type, object> dict);
}