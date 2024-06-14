namespace Jerry.Utilities.IoC.General;

/// <summary>
/// Mark classes with this attribute to automatically register them in IoC on launch
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class RegisterDependencyAttribute : Attribute
{
}