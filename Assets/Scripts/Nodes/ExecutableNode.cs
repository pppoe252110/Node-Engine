using System.Collections.Generic;
using UnityEngine;

public abstract class ExecutableNode : NodeBase
{
    public override void Process(List<Connector> fromConnectors = null)
    {
        base.Process(fromConnectors);
    }

    public abstract void Execute();
}
