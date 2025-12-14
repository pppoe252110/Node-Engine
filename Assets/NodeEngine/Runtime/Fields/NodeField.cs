using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

[Serializable]
public class NodeField : NodeFieldBase
{
    public delegate void ValueHandlerFunc(IConnectorValue value);
    public event ValueHandlerFunc CurrentValueHandler;

    public NodeField SetHandler(ValueHandlerFunc handler)
    {
        CurrentValueHandler = handler;
        return this;
    }

    public override Type GetValueType() => typeof(void);
    public override NodeValueAttribute GetAttribute() => CurrentValueHandler?.GetMethodInfo()?.GetCustomAttribute<NodeValueAttribute>();

    public override void ProceedValue()
    {
        ProceedValue(new HashSet<NodeBase>());
    }

    private void ProceedValue(HashSet<NodeBase> processedNodes)
    {
        if (Connector?.Node == null) return;

        if (processedNodes.Contains(Connector.Node))
            return;

        processedNodes.Add(Connector.Node);

        var value = Connector?.GetConnectorValue() ?? ConnectorValueVoid.Instance;
        CurrentValueHandler?.Invoke(value);

        if (Connector != null)
        {
            foreach (var connectedConnector in Connector.Connections)
            {
                var field = connectedConnector?.Field;
                if (field != null && !connectedConnector.Node.IsProcessing && connectedConnector.Node != Connector.Node)
                {
                    if (field is NodeField executionField)
                    {
                        executionField.ProceedValue(processedNodes);
                    }
                    else
                    {
                        field.ProceedValue();
                    }
                }
            }
        }
    }

    public override void UpdateValueFromSource(IConnectorValue sourceValue) { }

    public override IConnectorValue GetCurrentValue()
    {
        throw new NotImplementedException();
    }
}
