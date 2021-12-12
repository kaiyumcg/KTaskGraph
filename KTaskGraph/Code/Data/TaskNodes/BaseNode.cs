using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KTaskGraph
{
    public class BaseNode
    {
        internal BaseNode() { }
        internal List<TaskNode> nextNormalNodes = null;
        internal List<ParallelTaskNode> nextParallelNodes = null;
        internal List<BaseNode> nextNodes = null;
        internal TaskGraphRunner runner = null;
        internal TaskGraph graphData = null;
        private protected bool isNodeDirty = false;

#if UNITY_EDITOR
        private protected bool isRunning = false, completed = false;
        internal string nodeName = "";
        public List<TaskNode> NextNormalNodes { get { return nextNormalNodes; } }
        public List<ParallelTaskNode> NextParallelNodes { get { return nextParallelNodes; } }
        public List<BaseNode> NextNodes { get { return nextNodes; } }
        public string NodeName { get { return nodeName; } }
#endif
        public bool IsRunning { get { return isRunning; } }
        public bool HasBeenCompleted { get { return completed; } }

        internal virtual void StartExecution()
        {

        }
    }
}