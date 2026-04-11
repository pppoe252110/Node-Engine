using System;

public class StringPersister : IValuePersister
{
    public bool CanPersist(Type t) => t == typeof(string);
    public string Serialize(object value) => (string)value;
    public object Deserialize(string data, Type _) => data;
}