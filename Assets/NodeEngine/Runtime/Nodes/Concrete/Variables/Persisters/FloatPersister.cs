using System;
using System.Globalization;

public class FloatPersister : IValuePersister
{
    public bool CanPersist(Type t) => t == typeof(float);
    public string Serialize(object value) => ((float)value).ToString(CultureInfo.InvariantCulture);
    public object Deserialize(string data, Type _) => float.Parse(data, CultureInfo.InvariantCulture);
}