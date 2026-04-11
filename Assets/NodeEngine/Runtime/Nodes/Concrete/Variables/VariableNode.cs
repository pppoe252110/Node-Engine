using NodeEngine.Compilation;
using System;
using UniMediator.Runtime;
using VContainer;

public abstract class VariableNode<T> : BaseNode, IVariableNode
{
    [NodePort("Value", false)] public T value;

    private T _cachedValue;
    public event Action<T> OnValueChanged;
    public event Action<object> OnUntypedValueChanged;

    private IMediator _mediator;

    public VariableNode()
    {
        var port = Ports.Find(p => p.Name == "Value");
        if (port != null)
            port.ValueType = typeof(T);
    }

    [Inject]
    public void ConstructVariable(IMediator mediator) 
    {
        _mediator = mediator;
    }

    public override Func<GraphContext, ExecutionResult> Compile(NodeCompilationContext context)
    {
        int outId = context.GetOutputId("Value");
        T val = _cachedValue;
        return ctx =>
        {
            ctx.Write(outId, val);
            return ExecutionResult.Continue(-1);
        };
    }

    public T GetValue() => _cachedValue;
    public void SetValue(T newValue)
    {
        _cachedValue = newValue;
        OnValueChanged?.Invoke(newValue);
        OnUntypedValueChanged?.Invoke(newValue);

        _mediator?.Publish(new MarkGraphDirtyNotification());
    }

    object IVariableNode.GetUntypedValue() => GetValue();
    void IVariableNode.SetUntypedValue(object value) => SetValue((T)value);
    Type IVariableNode.ValueType => typeof(T);
}