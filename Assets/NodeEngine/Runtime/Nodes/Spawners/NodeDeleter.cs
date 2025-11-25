using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class NodeDeleter : MonoBehaviour
{
    public void DeleteNode(NodeBase node, List<Connector> inputConnectors, List<Connector> outputConnectors)
    {
        RemoveAllConnections(inputConnectors, outputConnectors);
        Destroy(gameObject);
    }
    private void RemoveAllConnections(List<Connector> inputConnectors, List<Connector> outputConnectors)
    {
        foreach (var connector in inputConnectors.Concat(outputConnectors))
        {
            foreach (var connected in connector.Connections.ToArray())
            {
                LineRenderersController.Remove(connector, connected);
                connected.Connections.Remove(connector);
                connected.UpdateFilled();
            }
            connector.Connections.Clear();
            connector.UpdateFilled();
        }
    }
}