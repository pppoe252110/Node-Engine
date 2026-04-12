using System;

public class EnumPersister : IValuePersister
{
    public bool CanPersist(Type type) => type.IsEnum;

    public string Serialize(object value)
    {
        if (value == null) return string.Empty;
        Type enumType = value.GetType();
        return $"{enumType.AssemblyQualifiedName}|{value}";
    }

    public object Deserialize(string data, Type targetType)
    {
        if (string.IsNullOrEmpty(data)) return GetDefaultEnumValue(targetType);

        // 格式："AssemblyQualifiedName|EnumValueName"
        int separatorIndex = data.IndexOf('|');
        if (separatorIndex < 0) return GetDefaultEnumValue(targetType);

        string typeName = data.Substring(0, separatorIndex);
        string enumValueName = data.Substring(separatorIndex + 1);

        Type enumType = Type.GetType(typeName) ?? targetType;
        if (!enumType.IsEnum) return GetDefaultEnumValue(targetType);

        try
        {
            return Enum.Parse(enumType, enumValueName);
        }
        catch
        {
            return GetDefaultEnumValue(targetType);
        }
    }

    private object GetDefaultEnumValue(Type enumType)
    {
        Array values = Enum.GetValues(enumType);
        return values.Length > 0 ? values.GetValue(0) : 0;
    }
}