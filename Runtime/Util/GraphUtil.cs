using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KTaskGraph
{
    internal static class GraphUtil
    {
        internal static void GetGraphProgress(TaskGraph graph, ref int totalNum, ref int completedNum, ref bool allDone)
        {
            totalNum = completedNum = 0;
            allDone = false;
            if (graph != null)
            {
                var nodes = graph.RootNodes;
                if (nodes != null && nodes.Count > 0)
                {
                    for (int i = 0; i < nodes.Count; i++)
                    {
                        var n = nodes[i];
                        if (n == null) { continue; }
                        GetTotalCount(n, ref totalNum, ref completedNum);
                    }
                }
            }

            allDone = totalNum > 0 && totalNum == completedNum;

            static void GetTotalCount(BaseNode node, ref int totalNum, ref int completedNum)
            {
                if (node == null) { return; }
                totalNum++;
                if (node.HasBeenCompleted) { completedNum++; }
                if (node.nextNodes != null && node.nextNodes.Count > 0)
                {
                    for (int i = 0; i < node.nextNodes.Count; i++)
                    {
                        var nn = node.nextNodes[i];
                        if (nn == null) { continue; }
                        GetTotalCount(nn, ref totalNum, ref completedNum);
                    }
                }
            }
        }
    }
}