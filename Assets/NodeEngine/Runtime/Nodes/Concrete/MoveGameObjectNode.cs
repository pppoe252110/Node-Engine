using UnityEngine;

[NodePath("Transform/Move")]
public class MoveGameObjectNode : ExecutableNodeBase
{
    private ConnectorValueObject _target;  
    private ConnectorValueObject _position;  
    private ConnectorValueFloat _speed;

    [NodeValue("Target", typeof(GameObject))]
    public void Target(ConnectorValueObject target) => _target = target;

    [NodeValue("Position", typeof(Vector3))]
    public void Position(ConnectorValueObject position) => _position = position;

    [NodeValue("Speed", typeof(float))]
    public void Speed(ConnectorValueFloat speed) => _speed = speed;

    public override void Execute()
    {
        if (_target.GetValue() is GameObject go && _position.GetValue() is Vector3 pos)
            go.transform.position = Vector3.MoveTowards(go.transform.position, pos, _speed.GetInnerValue() * Time.deltaTime);

        base.Execute();
    }

    public override void Setup()
    {
        inputFields = new()
        {
            new NodeFieldTyped<ConnectorValueObject>().SetHandler(Target).SetDefaultValue(new ConnectorValueObject(null)),
            new NodeFieldTyped<ConnectorValueObject>().SetHandler(Position).SetDefaultValue(new ConnectorValueObject(Vector3.zero)),
            new NodeFieldTyped<ConnectorValueFloat>().SetHandler(Speed).SetDefaultValue(new ConnectorValueFloat(1f))
        };

        base.Setup();
    }
}
