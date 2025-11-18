// Values\ConnectorValueVoid.cs
using System;
using UnityEngine;

[Serializable]
public class ConnectorValueVoid : ConnectorValueBase, IFastConnectorValue<object>, IConnectorValueBridge
{
    // Execution context for tracking flow
    private System.Action _onExecute;
    private int _executionId;
    private bool _wasExecuted = false;

    public ConnectorValueVoid()
    {
        _executionId = UnityEngine.Random.Range(1, int.MaxValue);
    }

    // Fast interface implementation
    public void SetValue(object value)
    {
        // For void types, treat any non-null value as execution trigger
        if (value != null && !_wasExecuted)
        {
            Execute();
            _wasExecuted = true;
        }
    }

    public object GetValue() => null;
    public override object GetInnerValue() => null;

    // Bridge interface implementation (self-bridging)
    public IConnectorValue WrappedValue => this;
    public Type ValueType => typeof(void);
    public void SetValueFast(object value)
    {
        if (value != null && !_wasExecuted)
        {
            Execute();
            _wasExecuted = true;
        }
    }
    public object GetValueFast() => null;

    // Execution flow methods
    public void Execute()
    {
        _onExecute?.Invoke();
        // Don't clear the callback - it might be needed for multiple executions
        // but reset the execution flag for potential reuse
        _wasExecuted = false;
    }

    public void SetExecutionCallback(System.Action callback)
    {
        _onExecute = callback;
    }

    public ConnectorValueVoid Then(System.Action nextAction)
    {
        if (_onExecute == null)
        {
            _onExecute = nextAction;
        }
        else
        {
            var previous = _onExecute;
            _onExecute = () => { previous(); nextAction(); };
        }
        return this;
    }

    // Reset for reuse
    public void ResetExecution()
    {
        _wasExecuted = false;
    }

    // Singleton pattern
    public override bool Equals(object obj) => obj is ConnectorValueVoid;
    public override int GetHashCode() => _executionId;
    public override string ToString() => $"Void({_executionId})";

    // Override bridge creation for optimal performance
    protected override IConnectorValueBridge CreateFastBridge()
    {
        return new FastConnectorBridge<object>(this);
    }
}