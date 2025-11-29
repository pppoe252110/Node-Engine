using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ConnectorColorDatabase", menuName = "Node Engine/Connector Color Database", order = 3)]
public class ConnectorColorDatabase : ScriptableObject
{
    [System.Serializable]
    public class TypeColorMapping
    {
        public string typeName;
        public Color color;
    }

    public Color FallbackColor
    {
        get
        {
            return fallbackColor;
        }
        set
        {
            fallbackColor = value;
        }
    }

    public List<TypeColorMapping> ColorMappings => colorMappings;

    [SerializeField] private List<TypeColorMapping> colorMappings = new List<TypeColorMapping>();
    [SerializeField] private Color fallbackColor = Color.gray;

    public Color GetColorForType(Type type)
    {
        if (type == null) return fallbackColor;

        string typeName = type.Name;

        foreach (var mapping in colorMappings)
        {
            if (mapping.typeName == typeName)
                return mapping.color;
        }

        return fallbackColor;
    }
}
