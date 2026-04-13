/// <summary>
/// Represents a data connection between two node ports.
/// Data flows from SourceNode's output port to TargetNode's input port.
/// </summary>
public class DataConnection
{
    public BaseNode SourceNode;
    public string OutputPortName;
    public BaseNode TargetNode;
    public string InputPortName;
}