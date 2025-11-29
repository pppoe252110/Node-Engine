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
            new NodeField<ConnectorValueBool>(true).SetHandler(Condition).SetDefaultValue(new ConnectorValueBool(false))
        };
        outputFields = new()
        {
            new NodeField<ConnectorValueVoid>(false).SetHandler(TrueTrigger).SetDefaultValue(new ConnectorValueVoid()),
            new NodeField<ConnectorValueVoid>(false).SetHandler(FalseTrigger).SetDefaultValue(new ConnectorValueVoid())
        };
    }
}