using System;

public class EntityRefPersister : IValuePersister
{
    public bool CanPersist(Type type) => type == typeof(EntityRef);

    public string Serialize(object value)
    {
        if (value is EntityRef entityRef)
            return entityRef.Id ?? string.Empty;
        return string.Empty;
    }

    public object Deserialize(string data, Type targetType)
    {
        return new EntityRef(data);
    }
}