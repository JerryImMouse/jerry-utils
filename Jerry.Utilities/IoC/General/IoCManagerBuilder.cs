using System.Reflection;

namespace Jerry.Utilities.IoC.General;

public class IoCManagerBuilder
{
    private bool _registerInheritor = false;
    private IoCMode _mode = IoCMode.All;
    private Type? _attribute = null;
    private Type? _inheritor = null;
    private List<Assembly> _assemblies = new();
    
    #region Builder

    public IoCManagerBuilder WithRegisterInheritor(bool registerInheritor)
    {
        _registerInheritor = registerInheritor;
        return this;
    }
    public IoCManagerBuilder WithMode(IoCMode mode)
    {
        _mode = mode;
        return this;
    }
    public IoCManagerBuilder WithAttribute(Type attribute)
    {
        if (!attribute.IsAssignableTo(typeof(Attribute)))
            throw new Exception("Provided non attribute type to attribute");
        
        _attribute = attribute;
        return this;
    }

    public IoCManagerBuilder WithAttribute<TAttribute>() where TAttribute : Attribute
    {
        WithAttribute(typeof(TAttribute));
        return this;
    }

    public IoCManagerBuilder WithInheritor<TInheritor>()
    {
        WithAttribute(typeof(TInheritor));
        return this;
    }

    public IoCManagerBuilder WithAssemblies(List<Assembly> assemblies)
    {
        _assemblies = assemblies;
        return this;
    }

    public IoCManagerBuilder WithInheritor(Type type)
    {
        _inheritor = type;
        return this;
    }

    public IoCSettings Build()
    {
        return IoCSettings.Build(_mode, _attribute, _inheritor, _registerInheritor, _assemblies);
    }
    #endregion
}

public class IoCSettings
{
    public readonly IoCMode Mode;
    public readonly Type? Attribute;
    public readonly Type? Inheritor;
    public readonly bool Register;
    public readonly List<Assembly> Assemblies;

    private IoCSettings(IoCMode mode, Type? attribute, Type? inheritor, bool register, List<Assembly> assemblies)
    {
        if (attribute == null && inheritor == null)
            throw new Exception("Null values provided for reading");
        
        Mode = mode;
        Attribute = attribute;
        Inheritor = inheritor;
        Register = register;
        Assemblies = assemblies.Count == 0 ? [Assembly.GetExecutingAssembly()] : assemblies;
    }

    public static IoCSettings Build(IoCMode mode, Type? attribute, Type? inheritor, bool registerInheritor, List<Assembly> assemblies)
    {
        return mode switch
        {
            IoCMode.Attribute when attribute == null || !attribute.IsAssignableTo(typeof(Attribute)) =>
                throw new Exception("Set mode to Attribute but attribute type was incorrect or null"),
            IoCMode.Inheritor when inheritor == null => throw new Exception(
                "Set mode to Inheritor but inheritor type was not provided"),
            IoCMode.All when (inheritor == null && attribute == null) => throw new Exception(
                "Set mode to All but inheritor or attribute was not provided"),
            IoCMode.Attribute => new IoCSettings(IoCMode.Attribute, attribute, null, false, assemblies),
            IoCMode.All => new IoCSettings(IoCMode.All, attribute, inheritor, registerInheritor, assemblies),
            IoCMode.Inheritor => new IoCSettings(IoCMode.Inheritor, null, inheritor, registerInheritor, assemblies),
            _ => throw new ArgumentException("Wrong mode provided")
        };
    }
}