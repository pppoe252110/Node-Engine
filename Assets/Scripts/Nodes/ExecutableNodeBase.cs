using System.Drawing;

public abstract class ExecutableNodeBase : NodeBase
{
    protected ConnectorValueVoid _executeInput;
    protected ConnectorValueVoid _executeOutput;
    protected NodeField<ConnectorValueVoid> _executeInputField;
    protected NodeField<ConnectorValueVoid> _executeOutputField;

    [NodeValue("Input", typeof(void), KnownColor.BlueViolet)]
    protected virtual void OnExecuteInput(ConnectorValueVoid execute)
    {
        _executeInput = execute;
        Execute();
        TriggerOutput();
    }

    [NodeValue("Output", typeof(void), KnownColor.BlueViolet)]
    protected virtual void OnExecuteOutput(ConnectorValueVoid execute)
    {
        _executeOutput = execute;
    }

    protected virtual void TriggerOutput()
    {
        if (_executeOutputField != null)
        {
            _executeOutputField.ProceedValue();
        }
    }
    public override void Setup()
    {
        SetupDefaultExecutionFlow();
    }
    public virtual void Execute()
    {
        TriggerOutput();
    }

    // Helper method to setup execution flow
    protected void SetupDefaultExecutionFlow()
    {
        _executeInputField = new NodeField<ConnectorValueVoid>(true)
            .SetFunc(OnExecuteInput)
            .ProvideDefaultValue(new ConnectorValueVoid());

        _executeOutputField = new NodeField<ConnectorValueVoid>(false)
            .SetFunc(OnExecuteOutput)
            .ProvideDefaultValue(new ConnectorValueVoid());

        // Add to fields list - input first, output last (standard convention)
        inputFields.Insert(0, _executeInputField);
        outputFields.Add(_executeOutputField);
    }
}