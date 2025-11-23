using UnityEngine;
public class TestBridges : MonoBehaviour
{
    void Start()
    {
        var value = new ConnectorValueInt(42);
        var bridge = ConnectorBridgeFactory.CreateBridge(value);
        Debug.Log($"Bridge created: {bridge != null}");
    }
}
