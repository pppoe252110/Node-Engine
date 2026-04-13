public static class BaseNodeExtensions
{
    /// <summary>
    /// Gets the Connector UI element for a given port name on this node.
    /// Searches both input and output connectors.
    /// </summary>
    public static Connector GetConnector(this BaseNode node, string portName)
    {
        if (node == null || node.LogicView == null) return null;

        return node.LogicView.InputConnectors.Find(c => c.PortName == portName)
            ?? node.LogicView.OutputConnectors.Find(c => c.PortName == portName);
    }
}