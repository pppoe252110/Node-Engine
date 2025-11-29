using Radishmouse;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ConnectionManager : MonoBehaviour
{
    [System.Serializable]
    public class ConnectionData
    {
        public int fromNodeId;
        public int toNodeId;
        public string fromConnectorName;
        public string toConnectorName;
        public string fromConnectorType;
        public string toConnectorType;

        public ConnectionData(int fromNode, int toNode, string fromName, string toName, string fromType, string toType)
        {
            fromNodeId = fromNode;
            toNodeId = toNode;
            fromConnectorName = fromName;
            toConnectorName = toName;
            fromConnectorType = fromType;
            toConnectorType = toType;
        }
    }

    private List<ConnectionData> _connections = new List<ConnectionData>();

    public static ConnectionManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public bool CreateConnection(NodeLogic fromNode, NodeLogic toNode, Type outputType, Type inputType)
    {
        if (fromNode == null || toNode == null) return false;
        if (fromNode.Node == null || toNode.Node == null) return false;

        var fromConnector = FindOutputConnectorByType(fromNode, outputType);
        var toConnector = FindInputConnectorByType(toNode, inputType);

        if (fromConnector == null || toConnector == null) return false;

        return CreateConnectionWithConnectors(fromConnector, toConnector);
    }

    private Connector FindOutputConnectorByType(NodeLogic node, Type type)
    {
        if (node?.Node?.outputConnectors == null) return null;

        var connector = node.Node.outputConnectors.FirstOrDefault(c => c.ValueType == type);
        if (connector != null) return connector;

        connector = node.Node.outputConnectors.FirstOrDefault(c => c.ValueType.Name == type.Name);
        if (connector != null) return connector;

        if (type == typeof(void))
        {
            connector = node.Node.outputConnectors.FirstOrDefault(c =>
                c.ValueType == typeof(void) ||
                typeof(IExecutableConnector).IsAssignableFrom(c.ValueType));
        }

        return connector;
    }

    private Connector FindInputConnectorByType(NodeLogic node, Type type)
    {
        if (node?.Node?.inputConnectors == null) return null;

        var connector = node.Node.inputConnectors.FirstOrDefault(c => c.ValueType == type);
        if (connector != null) return connector;

        connector = node.Node.inputConnectors.FirstOrDefault(c => c.ValueType.Name == type.Name);
        if (connector != null) return connector;

        if (type == typeof(object))
        {
            connector = node.Node.inputConnectors.FirstOrDefault();
        }

        if (type == typeof(void))
        {
            connector = node.Node.inputConnectors.FirstOrDefault(c =>
                c.ValueType == typeof(void) ||
                typeof(IExecutableConnector).IsAssignableFrom(c.ValueType));
        }

        return connector;
    }

    public bool CreateConnection(NodeLogic fromNode, NodeLogic toNode, string outputConnectorName, string inputConnectorName)
    {
        if (fromNode == null || toNode == null) return false;

        var fromConnector = fromNode.Node.outputConnectors.FirstOrDefault(c =>
            c.Field?.GetAttribute()?.attributeName == outputConnectorName);
        var toConnector = toNode.Node.inputConnectors.FirstOrDefault(c =>
            c.Field?.GetAttribute()?.attributeName == inputConnectorName);

        if (fromConnector == null || toConnector == null) return false;

        return CreateConnectionWithConnectors(fromConnector, toConnector);
    }

    public bool CreateConnectionWithConnectors(Connector fromConnector, Connector toConnector)
    {
        if (fromConnector == null || toConnector == null) return false;

        var fromAttr = fromConnector.Field?.GetAttribute();
        var toAttr = toConnector.Field?.GetAttribute();

        if (fromAttr == null || toAttr == null ||
            string.IsNullOrEmpty(fromAttr.attributeName) || fromAttr.attributeName == "Unknown" ||
            string.IsNullOrEmpty(toAttr.attributeName) || toAttr.attributeName == "Unknown")
        {
            Debug.LogWarning($"Rejected invalid connection: {fromConnector.Node.GetType().Name}.{fromAttr?.attributeName ?? "Unknown"} -> {toConnector.Node.GetType().Name}.{toAttr?.attributeName ?? "Unknown"}");
            return false;
        }

        if (fromConnector.Node == toConnector.Node || WouldCreateInvalidLoop(fromConnector, toConnector))
        {
            Debug.LogWarning($"Rejected loop/self-connection: {fromConnector.Node.GetType().Name} -> {toConnector.Node.GetType().Name}");
            return false;
        }

        if (LineRenderersController.Instance == null) return false;

        var lineRendererPrefab = LineRenderersController.Instance.LineRendererPrefab;
        if (lineRendererPrefab == null)
        {
            CreateConnectionWithoutVisual(fromConnector, toConnector);
            return true;
        }

        var lineRenderer = Instantiate(lineRendererPrefab);
        if (lineRenderer == null)
        {
            CreateConnectionWithoutVisual(fromConnector, toConnector);
            return true;
        }

        LineRenderersController.Add(fromConnector, toConnector, lineRenderer);

        fromConnector.AddConnection(toConnector);
        fromConnector.UpdateFilled();
        toConnector.AddConnection(fromConnector);
        toConnector.UpdateFilled();

        var fromNodeId = fromConnector.Node.Guid;
        var toNodeId = toConnector.Node.Guid;
        var fromAttrFinal = fromConnector.Field?.GetAttribute();
        var toAttrFinal = toConnector.Field?.GetAttribute();

        var connection = new ConnectionData(
            fromNodeId, toNodeId,
            fromAttrFinal?.attributeName ?? "Unknown",
            toAttrFinal?.attributeName ?? "Unknown",
            fromConnector.ValueType.Name,
            toConnector.ValueType.Name
        );

        _connections.Add(connection);

        return true;
    }

    private bool WouldCreateInvalidLoop(Connector outputConnector, Connector inputConnector)
    {
        if (outputConnector.Node == inputConnector.Node)
        {
            Debug.LogWarning("Rejected self-connection: Node cannot connect to itself");
            return true;
        }

        return false;
    }

    private void CreateConnectionWithoutVisual(Connector fromConnector, Connector toConnector)
    {
        fromConnector.AddConnection(toConnector);
        fromConnector.UpdateFilled();
        toConnector.AddConnection(fromConnector);
        toConnector.UpdateFilled();

        var fromNodeId = fromConnector.Node.Guid;
        var toNodeId = toConnector.Node.Guid;
        var fromAttr = fromConnector.Field?.GetAttribute();
        var toAttr = toConnector.Field?.GetAttribute();

        var connection = new ConnectionData(
            fromNodeId, toNodeId,
            fromAttr?.attributeName ?? "Unknown",
            toAttr?.attributeName ?? "Unknown",
            fromConnector.ValueType.Name,
            toConnector.ValueType.Name
        );

        _connections.Add(connection);
    }

    public bool CreateConnection(int fromNodeId, int toNodeId, string outputType, string inputType)
    {
        var fromNode = NodeSpawnerService.Instance.GetNodeById(fromNodeId);
        var toNode = NodeSpawnerService.Instance.GetNodeById(toNodeId);

        if (fromNode == null || toNode == null) return false;

        var outputTypeObj = Type.GetType(outputType) ?? GetTypeFromName(outputType);
        var inputTypeObj = Type.GetType(inputType) ?? GetTypeFromName(inputType);

        return CreateConnection(fromNode, toNode, outputTypeObj, inputTypeObj);
    }

    private Type GetTypeFromName(string typeName)
    {
        return typeName switch
        {
            "Int32" => typeof(int),
            "Single" => typeof(float),
            "Boolean" => typeof(bool),
            "String" => typeof(string),
            "Void" => typeof(void),
            "Vector3" => typeof(Vector3),
            "GameObject" => typeof(GameObject),
            "Object" => typeof(object),
            _ => typeof(object)
        };
    }

    public List<ConnectionData> GetAllConnections()
    {
        return new List<ConnectionData>(_connections);
    }

    public void ClearAllConnections()
    {
        _connections.Clear();
        LineRenderersController.ClearAllConnections();
    }
}
