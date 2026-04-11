using System;

public class TypeValuePersister : IValuePersister
{
    public bool CanPersist(Type type) => type == typeof(Type);

    public string Serialize(object value)
    {
        if (value is Type t)
            return t.AssemblyQualifiedName ?? string.Empty;
        return string.Empty;
    }

    public object Deserialize(string data, Type targetType)
    {
        if (string.IsNullOrEmpty(data))
            return typeof(object);
        return Type.GetType(data) ?? typeof(object);
    }
}