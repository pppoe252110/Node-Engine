using System.Drawing;

public class IfElseNode : ExecutableNode
{
    private ConnectorValueBool _condition;
    private ConnectorValueVoid _trueTrigger, _falseTrigger;

    [NodeValue("Condition", typeof(bool), KnownColor.Red)]
    public void Condition(ConnectorValueBool condition) => _condition = condition;

    [NodeValue("True", typeof(void), KnownColor.BlueViolet)]
    public void TrueTrigger(ConnectorValueVoid trigger) => _trueTrigger = trigger;

    [NodeValue("False", typeof(void), KnownColor.BlueViolet)]
    public void FalseTrigger(ConnectorValueVoid trigger) => _falseTrigger = trigger;

    public override void Execute()
    {
        if (_condition.GetValue())
        {
            _trueTrigger.SetValueFast(_trueTrigger);
            _trueTrigger.Execute();
        }
        else
        {
            _falseTrigger.SetValueFast(_falseTrigger);
            _falseTrigger.Execute();
        }
    }

    public override void Setup()
    {
        inputFields = new()
        {
            new NodeField<ConnectorValueBool>(true).SetFunc(Condition).ProvideDefaultValue(new ConnectorValueBool(false))
        };
        outputFields = new()
        {
            new NodeField<ConnectorValueVoid>(false).SetFunc(TrueTrigger).ProvideDefaultValue(new ConnectorValueVoid()),
            new NodeField<ConnectorValueVoid>(false).SetFunc(FalseTrigger).ProvideDefaultValue(new ConnectorValueVoid())
        };
    }
}