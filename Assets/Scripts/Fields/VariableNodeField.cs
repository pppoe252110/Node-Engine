using System;
using UnityEngine;

public class VariableNodeField : NodeField<ConnectorValueObject>
{
    private VariableType _variableType;

    public VariableNodeField(VariableType variableType) : base(false)
    {
        _variableType = variableType;
    }

    public override Type GetValueType()
    {
        return _variableType switch
        {
            VariableType.Int => typeof(int),
            VariableType.Single => typeof(float),
            VariableType.String => typeof(string),
            VariableType.Vector3 => typeof(Vector3),
            _ => typeof(object)  // Fallback
        };
    }

    public override NodeValueAttribute GetAttribute()
    {
        // Map VariableType to a KnownColor (matching your existing node colors for consistency)
        System.Drawing.KnownColor color = _variableType switch
        {
            VariableType.Int => System.Drawing.KnownColor.Purple,          // Matches ForLoopNode (int)
            VariableType.Single => System.Drawing.KnownColor.LawnGreen,     // Matches AddNode/TimeNode (float)
            VariableType.String => System.Drawing.KnownColor.PaleVioletRed, // Matches DebugNode/ToStringNode (string)
            VariableType.Vector3 => System.Drawing.KnownColor.Plum,        // Matches MoveGameObjectNode (Vector3)
            _ => System.Drawing.KnownColor.Gray  // Fallback (original gray)
        };

        // Return a new attribute with the dynamic type and color
        return new NodeValueAttribute("Value", GetValueType(), color);
    }
}