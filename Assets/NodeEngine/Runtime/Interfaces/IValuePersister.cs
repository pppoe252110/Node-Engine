using System;

public interface IValuePersister
{
    bool CanPersist(Type type);
    string Serialize(object value);
    object Deserialize(string data, Type targetType);
}