using System;
using UnityEngine;

public abstract class JsonPersisterBase<T> : IValuePersister where T : struct
{
    protected abstract T DefaultValue { get; }

    public virtual bool CanPersist(Type t) => t == typeof(T);

    public virtual string Serialize(object value)
    {
        if (value is T typedValue)
            return JsonUtility.ToJson(typedValue);
        return JsonUtility.ToJson(DefaultValue);
    }

    public virtual object Deserialize(string data, Type targetType)
    {
        if (string.IsNullOrEmpty(data))
            return DefaultValue;

        try
        {
            return JsonUtility.FromJson<T>(data);
        }
        catch
        {
            return DefaultValue;
        }
    }
}