public abstract class ExecutableNodeBase : NodeBase
{
    // FIX: Use the non-generic NodeField for execution flow
    protected NodeField _executeInputField;
    protected NodeField _executeOutputField;

    // FIX: The handler for the non-generic NodeField takes IConnectorValue
    [NodeValue("Input", typeof(void))]
    protected virtual void OnExecuteInput(IConnectorValue execute)
    {
        Execute();
        TriggerOutput();
    }

    // FIX: The handler for the non-generic NodeField takes IConnectorValue
    [NodeValue("Output", typeof(void))]
    protected virtual void OnExecuteOutput(IConnectorValue execute)
    {
        // This handler doesn't need to do anything, the call to ProceedValue() is what matters.
    }

    protected virtual void TriggerOutput()
    {
        if (_executeOutputField != null)
        {
            // This will trigger the execution flow to the next node
            _executeOutputField.ProceedValue();
        }
    }

    public override void Setup()
    {
        SetupDefaultExecutionFlow();
    }

    public virtual void Execute()
    {
        // This method is meant to be overridden by concrete nodes
    }

    protected void SetupDefaultExecutionFlow()
    {
        // FIX: Use the non-generic NodeField and its corresponding handler
        _executeInputField = new NodeField().SetHandler(OnExecuteInput);
        _executeOutputField = new NodeField().SetHandler(OnExecuteOutput);

        inputFields.Insert(0, _executeInputField);
        outputFields.Add(_executeOutputField);
    }
}