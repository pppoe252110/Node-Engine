using System;
using UnityEngine;

public class QuaternionPersister : IValuePersister
{
    public bool CanPersist(Type t) => t == typeof(Quaternion);

    public string Serialize(object value) => JsonUtility.ToJson((Quaternion)value);

    public object Deserialize(string data, Type _)
    {
        if (string.IsNullOrEmpty(data)) return Quaternion.identity;
        try { return JsonUtility.FromJson<Quaternion>(data); }
        catch { return Quaternion.identity; }
    }
}