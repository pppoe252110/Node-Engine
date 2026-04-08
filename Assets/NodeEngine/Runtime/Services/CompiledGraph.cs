using System;
using System.Collections.Generic;
using UnityEngine;

public class GraphContext
{
    public object[] Memory;
    public Func<GraphContext, int>[] Instructions;

    // Maximum number of flow steps before forced termination (prevents freeze)
    private const int MAX_EXECUTION_STEPS = 10_000;

    public void ExecuteFlow(int startIndex)
    {
        int ip = startIndex;
        int stepCount = 0;

        while (ip >= 0 && ip < Instructions.Length)
        {
            stepCount++;
            if (stepCount > MAX_EXECUTION_STEPS)
            {
                Debug.LogError($"[GraphContext] Execution exceeded {MAX_EXECUTION_STEPS} steps. Possible infinite loop. Halting.");
                break;
            }

            try
            {
                ip = Instructions[ip](this);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GraphContext] Exception at instruction {ip}: {ex.Message}\n{ex.StackTrace}");
                break;
            }
        }
    }
}

public class CompiledGraph
{
    public GraphContext Context;
    public Dictionary<BaseNode, int> NodeToIndex;

    public CompiledGraph(Func<GraphContext, int>[] instructions, int memorySize, Dictionary<BaseNode, int> nodeToIndex)
    {
        Context = new GraphContext
        {
            Memory = new object[memorySize],
            Instructions = instructions
        };
        NodeToIndex = nodeToIndex;
    }

    public void ExecuteNode(BaseNode node)
    {
        if (NodeToIndex.TryGetValue(node, out int idx))
        {
            Context.ExecuteFlow(idx);
        }
    }
}