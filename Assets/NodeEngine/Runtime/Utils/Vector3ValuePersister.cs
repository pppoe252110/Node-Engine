using UnityEngine;

public class Vector3ValuePersister : IValuePersister
{
    public bool CanPersist(VariableType variableType)
    {
        return variableType == VariableType.Vector3;
    }

    public string PersistValue(object value)
    {
        if (value is Vector3 vectorValue)
        {
            return JsonUtility.ToJson(vectorValue);
        }
        return JsonUtility.ToJson(Vector3.zero);
    }

    public object RestoreValue(string serializedValue, VariableType variableType)
    {
        if (variableType != VariableType.Vector3)
            return Vector3.zero;

        try
        {
            return JsonUtility.FromJson<Vector3>(serializedValue);
        }
        catch
        {
            return Vector3.zero;
        }
    }
}