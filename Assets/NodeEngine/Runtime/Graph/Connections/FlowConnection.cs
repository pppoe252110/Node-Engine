/// <summary>
/// Represents a flow (execution) connection between two node ports.
/// Execution passes from SourceNode's output flow port to TargetNode's input flow port.
/// </summary>
public class FlowConnection
{
    public BaseNode SourceNode;
    public string SourcePortName;
    public BaseNode TargetNode;
    public string TargetPortName;
}