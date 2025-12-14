using System;

public class TypeValuePersister : IValuePersister
{
    public bool CanPersist(VariableType variableType)
    {
        return variableType == VariableType.Type;
    }

    public string PersistValue(object value)
    {
        if (value is Type typeValue)
        {
            return TypeSerializer.SerializeType(typeValue);
        }
        return string.Empty;
    }

    public object RestoreValue(string serializedValue, VariableType variableType)
    {
        if (variableType != VariableType.Type)
            return GetDefaultValue(variableType);

        return TypeSerializer.DeserializeType(serializedValue);
    }

    private object GetDefaultValue(VariableType type)
    {
        return type == VariableType.Type ? typeof(object) : null;
    }
}
