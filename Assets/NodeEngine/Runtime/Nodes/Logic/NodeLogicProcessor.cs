using System.Linq;
using UnityEngine;

public class NodeLogicProcessor : MonoBehaviour
{
    public void Process()
    {
        var node = NodeSpawnerService.Instance.GetAllNodes().FirstOrDefault(s => s.Value.Node is UpdateNode).Value;
        if (node)
        {
            NodeEngine.SetIsExecuting(true);
            node.Process();
            NodeEngine.SetIsExecuting(false);
        }
    }
}
