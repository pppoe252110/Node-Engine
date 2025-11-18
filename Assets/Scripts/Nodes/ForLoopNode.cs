using System.Drawing;
using UnityEngine;

public class ForLoopNode : ExecutableNode
{
    private ConnectorValueVoid _execute;
    private ConnectorValueInt _count;
    private ConnectorValueVoid _body, _end;
    private ConnectorValueInt _index;

    private NodeField<ConnectorValueVoid> _bodyField, _endField;
    private NodeField<ConnectorValueInt> _indexField;  // Store the Index field for propagation

    [NodeValue("Execute", typeof(void), KnownColor.BlueViolet)]
    public void ExecuteInput(ConnectorValueVoid execute) => _execute = execute;

    [NodeValue("Count", typeof(int), KnownColor.Red)]
    public void Count(ConnectorValueInt count) => _count = count;

    [NodeValue("Body", typeof(void), KnownColor.BlueViolet)]
    public void Body(ConnectorValueVoid body) => _body = body;

    [NodeValue("Index", typeof(int), KnownColor.Purple)]
    public void Index(ConnectorValueInt index) => _index = index;

    [NodeValue("End", typeof(void), KnownColor.BlueViolet)]
    public void End(ConnectorValueVoid end) => _end = end;

    public override void Execute()
    {
        System.Diagnostics.Stopwatch sw = new();
        sw.Start();
        // ✅ Get updated input values (loop count)
        Process();

        int loopCount = _count?.GetValue() ?? 0;

        for (int i = 0; i < loopCount; i++)
        {
            // Update index value
            _index?.SetValue(i);

            // ✅ This should now work without circular execution
            _bodyField.ProceedValue();
        }

        _endField.ProceedValue();

        sw.Stop();

        ConsoleUI.Instance.LogMessage(sw.ElapsedMilliseconds + "ms");
        Debug.LogError(sw.ElapsedMilliseconds + "ms");
    }

    public override void Setup()
    {
        // Create default void instances
        var bodyVoid = new ConnectorValueVoid();
        var endVoid = new ConnectorValueVoid();

        _bodyField = new NodeField<ConnectorValueVoid>(false).SetFunc(Body).ProvideDefaultValue(bodyVoid);
        _endField = new NodeField<ConnectorValueVoid>(false).SetFunc(End).ProvideDefaultValue(endVoid);
        _indexField = new NodeField<ConnectorValueInt>(false).SetFunc(Index).ProvideDefaultValue(new ConnectorValueInt(0));  // Store the Index field

        inputFields = new()
        {
            new NodeField<ConnectorValueVoid>(true).SetFunc(ExecuteInput).ProvideDefaultValue(new ConnectorValueVoid()),
            new NodeField<ConnectorValueInt>(true).SetFunc(Count).ProvideDefaultValue(new ConnectorValueInt(0))
        };
        outputFields = new()
        {
            _bodyField,
            _indexField,  // Use the stored field
            _endField
        };

        // Set the initial values
        _body = bodyVoid;
        _end = endVoid;
        _index = new ConnectorValueInt(0);
    }
}
