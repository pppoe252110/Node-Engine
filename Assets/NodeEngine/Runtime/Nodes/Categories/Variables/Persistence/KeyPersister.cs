using System;
using UnityEngine.InputSystem;

public class KeyPersister : IValuePersister
{
    public bool CanPersist(Type type) => type == typeof(Key);

    public string Serialize(object value)
    {
        if (value is Key key)
        {
            return key.ToString();
        }
        return Key.None.ToString();
    }

    public object Deserialize(string data, Type targetType)
    {
        if (string.IsNullOrEmpty(data))
            return Key.None;

        try
        {
            return Enum.Parse(typeof(Key), data);
        }
        catch
        {
            return Key.None;
        }
    }
}