public abstract class ExecutableNodeBase : NodeBase
{
    protected ConnectorValueVoid _executeInput;
    protected ConnectorValueVoid _executeOutput;
    protected NodeField<ConnectorValueVoid> _executeInputField;
    protected NodeField<ConnectorValueVoid> _executeOutputField;

    [NodeValue("Input", typeof(void))]
    protected virtual void OnExecuteInput(ConnectorValueVoid execute)
    {
        _executeInput = execute;
        Execute();
        TriggerOutput();
    }

    [NodeValue("Output", typeof(void))]
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

    
    protected void SetupDefaultExecutionFlow()
    {
        _executeInputField = new NodeField<ConnectorValueVoid>(true)
            .SetHandler(OnExecuteInput)
            .SetDefaultValue(new ConnectorValueVoid());

        _executeOutputField = new NodeField<ConnectorValueVoid>(false)
            .SetHandler(OnExecuteOutput)
            .SetDefaultValue(new ConnectorValueVoid());

        
        inputFields.Insert(0, _executeInputField);
        outputFields.Add(_executeOutputField);
    }
}