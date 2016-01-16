using System.Collections.Generic;
using System;

namespace Ssis2008Emitter.Phases.Lowering
{
    public static class FlowSorter
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Advanced API for advanced users")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Advanced API for advanced users")]
        public static List<T> BreadthFirstEnumerated<T>(IEnumerable<T> roots, Dictionary<T, ICollection<T>> successors)
        {
            var sorted = new List<T>();
            var remaining = new Queue<T>(roots);
            while (remaining.Count > 0)
            {
                T currentNode = remaining.Dequeue();
                if (!sorted.Contains(currentNode))
                {
                    sorted.Add(currentNode);
                }

                if (successors.ContainsKey(currentNode))
                {
                    foreach (var successorNode in successors[currentNode])
                    {
                        if (!sorted.Contains(successorNode))
                        {
                            remaining.Enqueue(successorNode);
                        }
                    }
                }
            }

            return sorted;
        }

        public static List<T> TopologicalSort<T>(IEnumerable<T> roots,Dictionary<T, ICollection<T>> successors)
        {
            var sorted = new List<T>();
            var visited = new HashSet<T>();
            var nodeSet = new HashSet<T>();
            
            // Add the entire potential set of objects to the nodeSet
            nodeSet.UnionWith(roots);
            nodeSet.UnionWith(successors.Keys);

            foreach (var node in nodeSet)
            {
                TopoVisit(node, visited, sorted, successors);
            }

            sorted.Reverse();
            return sorted;
        }

        private static void TopoVisit<T>(T node, HashSet<T> visitedSet, ICollection<T> sortedList, Dictionary<T, ICollection<T>> successors)
        {
            if (!visitedSet.Contains(node))
            {
                visitedSet.Add(node);
                if (successors.ContainsKey(node))
                {
                    foreach (var successor in successors[node])
                    {
                        TopoVisit(successor, visitedSet, sortedList, successors);
                    }
                }
                sortedList.Add(node);
            }
        }
    }
}
