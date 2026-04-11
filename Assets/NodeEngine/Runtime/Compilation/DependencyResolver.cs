using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NodeEngine.Compilation
{
    public class DependencyResolver
    {
        public List<int> GetDataDependencies(
            BaseNode target,
            List<DataConnection> dataConnections,
            Dictionary<BaseNode, int> nodeToIndex)
        {
            var deps = new List<int>();
            var visited = new HashSet<BaseNode>();
            var recursionStack = new HashSet<BaseNode>();

            Traverse(target);

            void Traverse(BaseNode n)
            {
                if (n == null) return;
                if (recursionStack.Contains(n))
                {
                    Debug.LogError($"[DependencyResolver] Cycle detected involving {n.GetType().Name}");
                    return;
                }
                if (visited.Contains(n)) return;

                visited.Add(n);
                recursionStack.Add(n);

                var inputs = dataConnections.Where(c => c.TargetNode == n);
                foreach (var input in inputs)
                {
                    var source = input.SourceNode;

                    if (IsFlowNode(source))
                        continue;

                    Traverse(source);
                    if (nodeToIndex.TryGetValue(source, out int srcIdx) && !deps.Contains(srcIdx))
                        deps.Add(srcIdx);
                }

                recursionStack.Remove(n);
            }

            if (nodeToIndex.TryGetValue(target, out int targetIdx))
                deps.Remove(targetIdx);

            return deps;
        }

        public bool IsFlowNode(BaseNode node) =>
            node?.Ports?.Any(p => p.IsFlow) ?? false;
    }
}