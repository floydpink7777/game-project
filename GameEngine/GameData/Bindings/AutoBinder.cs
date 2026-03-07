using System;
using System.Reflection;

public static class AutoBinder
{
    public static void Bind(object instance, string prefix, VariableStore vars)
    {
        var type = instance.GetType();
        var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var prop in props)
        {
            // プリミティブ型だけ対象
            if (!IsSupportedType(prop.PropertyType))
                continue;

            string key = $"{prefix}.{prop.Name.ToLower()}";

            vars.Bind(
                key,
                getter: () => prop.GetValue(instance),
                setter: v => prop.SetValue(instance, Convert.ChangeType(v, prop.PropertyType))
            );
        }
    }

    private static bool IsSupportedType(Type t)
    {
        return t == typeof(int)
            || t == typeof(bool)
            || t == typeof(string)
            || t == typeof(float)
            || t == typeof(double);
    }
}