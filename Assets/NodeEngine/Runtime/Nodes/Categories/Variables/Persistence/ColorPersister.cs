using System;
using UnityEngine;

public class ColorPersister : IValuePersister
{
    public bool CanPersist(Type t) => t == typeof(Color);

    public string Serialize(object value) => JsonUtility.ToJson((Color)value);

    public object Deserialize(string data, Type _)
    {
        if (string.IsNullOrEmpty(data)) return Color.white;
        try { return JsonUtility.FromJson<Color>(data); }
        catch { return Color.white; }
    }
}