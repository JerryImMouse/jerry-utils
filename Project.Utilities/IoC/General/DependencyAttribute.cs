namespace Project.Utilities.IoC.General;

/// <summary>
/// Mark fields with this attribute to inject dependencies automatically
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class DependencyAttribute : Attribute
{
}