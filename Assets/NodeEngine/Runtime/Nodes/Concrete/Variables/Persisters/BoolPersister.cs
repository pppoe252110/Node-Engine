using System;
using System.Globalization;

public class BoolPersister : IValuePersister
{
    public bool CanPersist(Type t) => t == typeof(bool);
    public string Serialize(object value) => ((bool)value).ToString(CultureInfo.InvariantCulture);
    public object Deserialize(string data, Type _) => bool.Parse(data);
}