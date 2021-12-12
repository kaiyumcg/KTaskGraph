using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KTaskGraph
{
    public class TaskGraph
    {
        bool isRootNodeListDirty = false;
        List<BaseNode> rootNodes = null;
        bool isGraphRunning = false;
        TaskGraphRunner runner = null;
        GameObject runnerObject = null;
        string graphName = "";
        public bool IsRunning { get { return isGraphRunning; } }
        internal List<BaseNode> RootNodes { get { return rootNodes; } }

#if UNITY_EDITOR
        public string GraphName { get { return graphName; } }
#endif

        int totalNodeCount = 0, completedCount = 0;
        public int TotalNodeCount { get { return totalNodeCount; } set { totalNodeCount = value; } }
        public int CompletedCount { get { return completedCount; } set { completedCount = value; } }

        private TaskGraph() { }
        public static TaskGraph Create(string graphName, GameObject runner)
        {
            var task = new TaskGraph();
            task.runner = runner.AddComponent<TaskGraphRunner>();
            task.runner.SetGraph(task);
            task.runnerObject = runner;
            task.graphName = graphName;
            return task;
        }

        public TaskNode CreateRootTask(string taskName, IEnumerator task, System.Action OnComplete = null)
        {
            isRootNodeListDirty = true;
            if (runner == null)
            {
                runner = runnerObject.AddComponent<TaskGraphRunner>();
                runner.SetGraph(this);
            }

            if (runner == null)
            {
                throw new System.Exception("Can not create root node since you have not set any runner for it! " +
    "Please call 'SetRunner' on TaskGraph");
            }
            var node = TaskNode.Create(taskName, task, OnComplete);
            node.runner = runner;
            node.graphData = this;
            node.Task.SetRunner(runner);
            if (rootNodes == null) { rootNodes = new List<BaseNode>(); }
            rootNodes.Add(node);
            isRootNodeListDirty = false;
            return node;
        }

        public ParallelTaskNode CreateRootTask(string collectionName, List<IEnumerator> tasks, System.Action OnComplete = null, bool waitForAllCompletion = true)
        {
            isRootNodeListDirty = true;
            if (runner == null)
            {
                runner = runnerObject.AddComponent<TaskGraphRunner>();
                runner.SetGraph(this);
            }

            if (runner == null)
            {
                throw new System.Exception("Can not create parallel root node since you have not set any runner for it!");
            }
            var node = ParallelTaskNode.Create(collectionName, tasks, OnComplete, waitForAllCompletion);
            node.runner = runner;
            node.graphData = this;
            if (node.Tasks != null && node.Tasks.Count > 0)
            {
                foreach (var t in node.Tasks)
                {
                    if (t == null) { continue; }
                    t.SetRunner(runner);
                }
            }

            if (rootNodes == null) { rootNodes = new List<BaseNode>(); }
            rootNodes.Add(node);
            isRootNodeListDirty = false;
            return node;
        }

        internal void OnNodeCompletion()
        {
            var completed = false;
            GraphUtil.GetGraphProgress(this, ref totalNodeCount, ref completedCount, ref completed);
            if (completed)
            {
                isGraphRunning = false;
                Clear();

                OnComplete?.Invoke();
                OnComplete = null;
            }
        }

        public void Clear()
        {
            if (runner != null)
            {
                runner.StopAllCoroutines();
                MonoBehaviour.Destroy(runner);
            }
            rootNodes = new List<BaseNode>();
            isGraphRunning = false;
            runner = null;
            OnComplete = null;
            completedCount = totalNodeCount = 0;
            isRootNodeListDirty = false;
        }

        OnCompleteFunc OnComplete = null;
        public void ScheduleIfReq(OnCompleteFunc OnComplete = null)
        {
            this.OnComplete += OnComplete;

            if (isGraphRunning == false)
            {
                isGraphRunning = true;
                if (rootNodes != null && rootNodes.Count > 0)
                {
                    if (isRootNodeListDirty)
                    {
                        runner.WaitAFrame(() =>
                        {
                            if (isRootNodeListDirty == false)
                            {
                                RunRoots();
                            }
                        });
                    }
                    else
                    {
                        RunRoots();
                    }
                }
                else
                {
                    isGraphRunning = false;
                }
            }

            void RunRoots()
            {
                for (int i = 0; i < rootNodes.Count; i++)
                {
                    var node = rootNodes[i];
                    if (node == null) { continue; }
                    node.StartExecution();
                }
            }
        }
    }
}