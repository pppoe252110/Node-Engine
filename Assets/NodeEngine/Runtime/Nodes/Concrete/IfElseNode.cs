[NodePath("Control Flow/If Else")]
public class IfElseNode : ExecutableNodeBase
{
    private ConnectorValueBool _condition;
    private ConnectorValueVoid _trueTrigger, _falseTrigger;

    [NodeValue("Condition", typeof(bool))]
    public void Condition(ConnectorValueBool condition) => _condition = condition;

    [NodeValue("True", typeof(void))]
    public void TrueTrigger(ConnectorValueVoid trigger) => _trueTrigger = trigger;

    [NodeValue("False", typeof(void))]
    public void FalseTrigger(ConnectorValueVoid trigger) => _falseTrigger = trigger;

    public override void Execute()
    {
        if (_condition.GetInnerValue())
        {
            
            _trueTrigger.Execute();
        }
        else
        {
            
            _falseTrigger.Execute();
        }
    }

    public override void Setup()
    {
        inputFields = new()
        {
            new NodeFieldTyped<ConnectorValueBool>().SetHandler(Condition).SetDefaultValue(new ConnectorValueBool(false))
        };
        outputFields = new()
        {
            new NodeFieldTyped<ConnectorValueVoid>().SetHandler(TrueTrigger).SetDefaultValue(new ConnectorValueVoid()),
            new NodeFieldTyped<ConnectorValueVoid>().SetHandler(FalseTrigger).SetDefaultValue(new ConnectorValueVoid())
        };
    }
}
