using System;

public class TypePersister : IValuePersister
{
    public bool CanPersist(Type t) => t == typeof(Type);
    public string Serialize(object value) => ((Type)value).AssemblyQualifiedName;
    public object Deserialize(string data, Type _) => Type.GetType(data) ?? typeof(object);
}