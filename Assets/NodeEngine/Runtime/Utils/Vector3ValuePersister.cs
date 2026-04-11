using System;
using UnityEngine;

public class Vector3ValuePersister : IValuePersister
{
    public bool CanPersist(Type type) => type == typeof(Vector3);

    public string Serialize(object value)
    {
        if (value is Vector3 v)
            return JsonUtility.ToJson(v);
        return JsonUtility.ToJson(Vector3.zero);
    }

    public object Deserialize(string data, Type targetType)
    {
        if (string.IsNullOrEmpty(data))
            return Vector3.zero;
        try
        {
            return JsonUtility.FromJson<Vector3>(data);
        }
        catch
        {
            return Vector3.zero;
        }
    }
}