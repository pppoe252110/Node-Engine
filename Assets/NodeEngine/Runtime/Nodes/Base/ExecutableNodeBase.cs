public abstract class ExecutableNodeBase : NodeBase
{
    protected NodeField _executeInputField;
    protected NodeField _executeOutputField;

    [NodeValue("Input", typeof(void))]
    protected virtual void OnExecuteInput(IConnectorValue execute)
    {
        Execute();
        TriggerOutput();
    }

    [NodeValue("Output", typeof(void))]
    protected virtual void OnExecuteOutput(IConnectorValue execute)
    {
        
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
        
    }

    protected void SetupDefaultExecutionFlow()
    {
        
        _executeInputField = new NodeField().SetHandler(OnExecuteInput);
        _executeOutputField = new NodeField().SetHandler(OnExecuteOutput);

        inputFields.Insert(0, _executeInputField);
        outputFields.Add(_executeOutputField);
    }
}
