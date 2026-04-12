using System;
using UnityEngine;

public class Vector2Persister : IValuePersister
{
    public bool CanPersist(Type t) => t == typeof(Vector2);

    public string Serialize(object value) => JsonUtility.ToJson((Vector2)value);

    public object Deserialize(string data, Type _)
    {
        if (string.IsNullOrEmpty(data)) return Vector2.zero;
        try { return JsonUtility.FromJson<Vector2>(data); }
        catch { return Vector2.zero; }
    }
}