[NodePath("Variables/Comparison Operation")]
public class ComparisonOperationVariableNode : VariableNode
{
    public override VariableType VariableType => VariableType.ComparisonOperation;

    public ComparisonOperationVariableNode()
    {
        SetValue(ComparisonOperation.Equal);
    }
}