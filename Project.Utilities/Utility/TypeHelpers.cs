using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Jerry.Utilities.Utility;

/// <summary>
/// Collection of methods to extend <see cref="Type"/> class
/// </summary>
public static class TypeHelpers
{
    public static IEnumerable<Type> GetClassHierarchy(this Type t)
    {
        yield return t;

        while (t.BaseType != null)
        {
            t = t.BaseType;
            yield return t;
        }
    }
    /// <summary>
    ///     Returns absolutely all fields, privates, readonlies, and ones from parents.
    /// </summary>
    public static IEnumerable<FieldInfo> GetAllFields(this Type t)
    {
        // We need to fetch the entire class hierarchy and SelectMany(),
        // Because BindingFlags.FlattenHierarchy doesn't read privates,
        // Even when you pass BindingFlags.NonPublic.
        foreach (var p in GetClassHierarchy(t))
        {
            foreach (var field in p.GetFields(
                         BindingFlags.NonPublic |
                         BindingFlags.Instance |
                         BindingFlags.DeclaredOnly |
                         BindingFlags.Public))
            {
                yield return field;
            }
        }
    }
    /// <summary>
    ///     Returns ALL nested types of the specified type, including private types of its parent.
    /// </summary>
    public static IEnumerable<Type> GetAllNestedTypes(this Type t)
    {
        foreach (var p in GetClassHierarchy(t))
        {
            foreach (var field in p.GetNestedTypes(
                         BindingFlags.NonPublic |
                         BindingFlags.Instance |
                         BindingFlags.DeclaredOnly |
                         BindingFlags.Public))
            {
                yield return field;
            }
        }
    }
    public static bool HasCustomAttribute<T>(this MemberInfo memberInfo) where T : Attribute
    {
        return memberInfo.GetCustomAttribute<T>() != null;
    }

    public static bool HasCustomAttribute(this MemberInfo memberInfo, Type t)
    {
        return memberInfo.GetCustomAttribute(t) != null;
    }

    public static bool TryGetCustomAttribute<T>(this MemberInfo memberInfo, [NotNullWhen(true)] out T? attribute)
        where T : Attribute
    {
        return (attribute = memberInfo.GetCustomAttribute<T>()) != null;
    }
    public static bool HasParameterlessConstructor(this Type t)
    {
        var constructors = t.GetConstructors();
        return !constructors.Any(c => c.ContainsGenericParameters);
    }

    public static FieldInfo[] GetFieldsWithType(this Type t, Type otherT)
    {
        return t.GetFields().Where(type => type.FieldType == otherT).ToArray();
    }

    public static FieldInfo[] GetFieldsWithTypeAndAttribute<TAttr>(this Type t, Type otherT) where TAttr : Attribute
    {
        var attribFields = t.GetFieldsWithAttribute<TAttr>();
        return attribFields.Where(type => type.FieldType == otherT).ToArray();
    }

    public static FieldInfo[] GetFieldsWithAttribute<TAttr>(this Type t, BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public) where TAttr : Attribute
    {
        return t.GetFields(flags).Where(HasCustomAttribute<TAttr>).ToArray();
    }
}