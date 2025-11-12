using System.Drawing;
using UnityEngine;

public class MoveGameObjectNode : ExecutableNode
{
    private ConnectorValueObject _target;  // GameObject
    private ConnectorValueObject _position;  // Vector3
    private ConnectorValueSingle _speed;

    [NodeValue("Target", typeof(GameObject), KnownColor.Cyan)]
    public void Target(ConnectorValueObject target) => _target = target;

    [NodeValue("Position", typeof(Vector3), KnownColor.Plum)]
    public void Position(ConnectorValueObject position) => _position = position;

    [NodeValue("Speed", typeof(float), KnownColor.LawnGreen)]
    public void Speed(ConnectorValueSingle speed) => _speed = speed;

    public override void Execute()
    {
        if (_target.GetValue() is GameObject go && _position.GetValue() is Vector3 pos)
            go.transform.position = Vector3.MoveTowards(go.transform.position, pos, _speed.GetValue() * Time.deltaTime);
    }

    public override void Setup()
    {
        inputFields = new()
        {
            new NodeField<ConnectorValueObject>(true).SetFunc(Target).ProvideDefaultValue(new ConnectorValueObject(null)),
            new NodeField<ConnectorValueObject>(true).SetFunc(Position).ProvideDefaultValue(new ConnectorValueObject(Vector3.zero)),
            new NodeField<ConnectorValueSingle>(true).SetFunc(Speed).ProvideDefaultValue(new ConnectorValueSingle(1f))
        };
    }
}