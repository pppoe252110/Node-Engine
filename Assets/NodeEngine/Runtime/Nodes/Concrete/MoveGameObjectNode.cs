using System.Drawing;
using UnityEngine;

[NodePath("Transform/Move")]
public class MoveGameObjectNode : ExecutableNodeBase
{
    private ConnectorValueObject _target;  // GameObject
    private ConnectorValueObject _position;  // Vector3
    private ConnectorValueFloat _speed;

    [NodeValue("Target", typeof(GameObject), KnownColor.Cyan)]
    public void Target(ConnectorValueObject target) => _target = target;

    [NodeValue("Position", typeof(Vector3), KnownColor.Plum)]
    public void Position(ConnectorValueObject position) => _position = position;

    [NodeValue("Speed", typeof(float), KnownColor.LawnGreen)]
    public void Speed(ConnectorValueFloat speed) => _speed = speed;

    public override void Execute()
    {
        if (_target.GetValue() is GameObject go && _position.GetValue() is Vector3 pos)
            go.transform.position = Vector3.MoveTowards(go.transform.position, pos, _speed.GetValue() * Time.deltaTime);
    }

    public override void Setup()
    {
        base.Setup();

        inputFields = new()
        {
            new NodeField<ConnectorValueObject>(true).SetHandler(Target).SetDefaultValue(new ConnectorValueObject(null)),
            new NodeField<ConnectorValueObject>(true).SetHandler(Position).SetDefaultValue(new ConnectorValueObject(Vector3.zero)),
            new NodeField<ConnectorValueFloat>(true).SetHandler(Speed).SetDefaultValue(new ConnectorValueFloat(1f))
        };
    }
}