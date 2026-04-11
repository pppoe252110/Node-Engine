using System;
using UnityEngine;

public class Vector3Persister : IValuePersister
{
    public bool CanPersist(Type t) => t == typeof(Vector3);
    public string Serialize(object value) => JsonUtility.ToJson((Vector3)value);
    public object Deserialize(string data, Type _) => JsonUtility.FromJson<Vector3>(data);
}