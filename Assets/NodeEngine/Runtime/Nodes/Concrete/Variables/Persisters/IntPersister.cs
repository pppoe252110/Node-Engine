using System;

public class IntPersister : IValuePersister
{
    public bool CanPersist(Type t) => t == typeof(int);
    public string Serialize(object value) => value.ToString();
    public object Deserialize(string data, Type _) => int.Parse(data);
}