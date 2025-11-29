using System.Collections.Generic;
using UnityEngine;

public abstract class VariableNode : NodeBase
{
    public abstract VariableType VariableType { get; }
    public VariableUIElement UIElement { get; set; }
    protected NodeFieldBase _outputField;

    public override void Setup()
    {
        _outputField = CreateTypedOutputField();
        outputFields = new() { _outputField };

        // Don't call UpdateOutputValue here - wait for connections
    }

    public void InitializeOutputValue()
    {
        UpdateOutputValue();
    }

    public override void Process(List<Connector> fromConnectors = null)
    {
        base.Process(fromConnectors);
        Debug.LogError("AAAAAAAAA");
    }

    protected override void Initialized()
    {
        UpdateOutputValue();
    }

    private NodeFieldBase CreateTypedOutputField()
    {
        switch (VariableType)
        {
            case VariableType.Int:
                return new NodeField<ConnectorValueInt>(false)
                    .SetHandler(OutputInt)
                    .SetDefaultValue(new ConnectorValueInt(0));

            case VariableType.Single:
                return new NodeField<ConnectorValueFloat>(false)
                    .SetHandler(OutputFloat)
                    .SetDefaultValue(new ConnectorValueFloat(0f));

            case VariableType.Bool:
                return new NodeField<ConnectorValueBool>(false)
                    .SetHandler(OutputBool)
                    .SetDefaultValue(new ConnectorValueBool(false));

            case VariableType.String:
                return new NodeField<ConnectorValueString>(false)
                    .SetHandler(OutputString)
                    .SetDefaultValue(new ConnectorValueString(""));

            case VariableType.Vector3:
                return new NodeField<ConnectorValueVector3>(false)
                    .SetHandler(OutputVector3)
                    .SetDefaultValue(new ConnectorValueVector3(Vector3.zero));

            default:
                return new NodeField<ConnectorValueObject>(false)
                    .SetHandler(OutputObject)
                    .SetDefaultValue(new ConnectorValueObject(null));
        }
    }

    [NodeValue("Value", typeof(int))]
    public void OutputInt(ConnectorValueInt value)
    {
        Debug.LogError("DADA");
        if (UIElement != null && UIElement.GetValue() is int uiVal)
        {
            value.SetInnerValue(uiVal);
        }
    }

    [NodeValue("Value", typeof(float))]
    public void OutputFloat(ConnectorValueFloat value)
    {
        if (UIElement != null && UIElement.GetValue() is float uiVal)
        {
            value.SetInnerValue(uiVal);
        }
    }

    [NodeValue("Value", typeof(bool))]
    public void OutputBool(ConnectorValueBool value)
    {
        if (UIElement != null && UIElement.GetValue() is bool uiVal)
        {
            value.SetInnerValue(uiVal);
        }
    }

    [NodeValue("Value", typeof(string))]
    public void OutputString(ConnectorValueString value)
    {
        if (UIElement != null && UIElement.GetValue() is string uiVal)
        {
            value.SetInnerValue(uiVal);
        }
    }

    [NodeValue("Value", typeof(Vector3))]
    public void OutputVector3(ConnectorValueVector3 value)
    {
        if (UIElement != null && UIElement.GetValue() is Vector3 uiVal)
        {
            value.SetInnerValue(uiVal);
        }
    }

    [NodeValue("Value", typeof(object))]
    public void OutputObject(ConnectorValueObject value)
    {
        if (UIElement != null)
        {
            value.SetInnerValue(UIElement.GetValue());
        }
    }

    public void UpdateOutputValue()
    {
        _outputField?.ProceedValue();
    }
}