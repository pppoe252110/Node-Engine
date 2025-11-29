using System;
using UnityEngine;

public static class TypeConverter
{
    public static object Convert(object value, Type targetType)
    {
        if (value == null)
        {
            if (targetType.IsValueType)
            {
                return Activator.CreateInstance(targetType);
            }
            return null;
        }

        if (targetType.IsAssignableFrom(value.GetType()))
        {
            return value;
        }

        try
        {
            
            if (targetType == typeof(string))
            {
                return value.ToString();
            }

            if (targetType == typeof(int))
            {
                if (value is int intValue) return intValue;
                if (int.TryParse(value.ToString(), out int parsedInt)) return parsedInt;
                return 0;
            }

            if (targetType == typeof(float))
            {
                if (value is float floatValue) return floatValue;
                if (float.TryParse(value.ToString(), out float parsedFloat)) return parsedFloat;
                return 0f;
            }

            if (targetType == typeof(bool))
            {
                if (value is bool boolValue) return boolValue;
                if (bool.TryParse(value.ToString(), out bool parsedBool)) return parsedBool;
                return false;
            }

            if (targetType == typeof(Vector3))
            {
                if (value is Vector3 vector3Value) return vector3Value;
                
                string str = value.ToString();
                if (str.StartsWith("(") && str.EndsWith(")"))
                {
                    string[] parts = str.Substring(1, str.Length - 2).Split(',');
                    if (parts.Length == 3 &&
                        float.TryParse(parts[0], out float x) &&
                        float.TryParse(parts[1], out float y) &&
                        float.TryParse(parts[2], out float z))
                    {
                        return new Vector3(x, y, z);
                    }
                }
                return Vector3.zero;
            }

            return System.Convert.ChangeType(value, targetType);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Failed to convert {value} to {targetType}: {e.Message}");

            if (targetType.IsValueType)
            {
                return Activator.CreateInstance(targetType);
            }
            return null;
        }
    }
}
