using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KTaskGraph
{
    public class ParallelTaskNode : BaseNode
    {
        public List<AtomicTask> Tasks { get { return tasks; } }
        bool waitForAllCompletion = true;
        System.Action OnComplete = null;
        List<AtomicTask> tasks = null;
        private ParallelTaskNode() { }
        internal static ParallelTaskNode Create(string collectionName, List<IEnumerator> tasks,
            System.Action OnComplete = null, bool waitForAllCompletion = true)
        {
            var node = new ParallelTaskNode { };
            node.tasks = new List<AtomicTask>();
            node.OnComplete = OnComplete;
            node.waitForAllCompletion = waitForAllCompletion;
#if UNITY_EDITOR
            node.nodeName = collectionName;
#endif
            if (tasks != null && tasks.Count > 0)
            {
                for (int i = 0; i < tasks.Count; i++)
                {
                    var t = tasks[i];
                    if (t == null) { continue; }
                    var _f = AtomicTask.Create(collectionName + "_" + i, t, null);
                    node.tasks.Add(_f);
                }
            }
            return node;
        }

        TaskNode ProcSingle(string taskName, IEnumerator task, System.Action OnComplete = null)
        {
            isNodeDirty = true;
            if (runner == null)
            {
                throw new System.Exception("Can not update graph with the input task(s) since you have not set any runner for it!");
            }
            var node = TaskNode.Create(taskName, task, OnComplete);
            node.Task.SetRunner(runner);
            node.runner = runner;
            node.graphData = graphData;
            if (nextNodes == null) { nextNodes = new List<BaseNode>(); }
            if (nextNormalNodes == null) { nextNormalNodes = new List<TaskNode>(); }
            nextNodes.Add(node);
            nextNormalNodes.Add(node);
            isNodeDirty = false;
            return node;
        }

        public TaskNode Then(string taskName, IEnumerator task, System.Action OnComplete = null)
        {
            return ProcSingle(taskName, task, OnComplete);
        }

        public ParallelTaskNode StartTask(string taskName, IEnumerator task, System.Action OnComplete = null)
        {
            ProcSingle(taskName, task, OnComplete);
            return this;
        }

        ParallelTaskNode ProcMany(string collectionName, List<IEnumerator> tasks, System.Action OnComplete = null, bool waitForAllCompletion = true)
        {
            isNodeDirty = true;
            if (runner == null)
            {
                throw new System.Exception("Can not update graph with the input task(s) since you have not set any runner for it!");
            }
            var node = ParallelTaskNode.Create(collectionName, tasks, OnComplete, waitForAllCompletion);
            node.runner = runner;
            node.graphData = graphData;
            if (node.Tasks != null && node.Tasks.Count > 0)
            {
                foreach (var t in node.Tasks)
                {
                    if (t == null) { continue; }
                    t.SetRunner(runner);
                }
            }

            if (nextNodes == null) { nextNodes = new List<BaseNode>(); }
            if (nextParallelNodes == null) { nextParallelNodes = new List<ParallelTaskNode>(); }
            nextNodes.Add(node);
            nextParallelNodes.Add(node);
            isNodeDirty = false;
            return node;
        }

        public ParallelTaskNode Then(string collectionName, List<IEnumerator> tasks, System.Action OnComplete = null, bool waitForAllCompletion = true)
        {
            return ProcMany(collectionName, tasks, OnComplete, waitForAllCompletion);
        }

        public ParallelTaskNode StartTask(string collectionName, List<IEnumerator> tasks, System.Action OnComplete = null, bool waitForAllCompletion = true)
        {
            var node = ProcMany(collectionName, tasks, OnComplete, waitForAllCompletion);
            return this;
        }

        internal override void StartExecution()
        {
            runner.StartCoroutine(PExec());
            IEnumerator PExec()
            {
#if UNITY_EDITOR
                isRunning = true;
                completed = false;
#endif
                int validCount = 0;
                int completedCount = 0;
                if (tasks != null && tasks.Count > 0)
                {
                    for (int i = 0; i < tasks.Count; i++)
                    {
                        var t = tasks[i];
                        if (t == null) { continue; }
                        validCount++;
                        t.Exec(() =>
                        {
                            completedCount++;
                        });
                    }

                    if (waitForAllCompletion)
                    {
                        while (validCount != completedCount && validCount > 0 && completedCount > 0) { yield return null; }
                    }

                    if (isNodeDirty)
                    {
                        runner.WaitAFrame(() =>
                        {
                            StartNextNodes();
                        });
                    }
                    else
                    {
                        StartNextNodes();
                    }
#if UNITY_EDITOR
                    isRunning = false;
                    completed = true;
#endif
                    OnComplete?.Invoke();
                    graphData.OnNodeCompletion();
                }
            }

            void StartNextNodes()
            {
                if (nextNodes != null && nextNodes.Count > 0)
                {
                    for (int i = 0; i < nextNodes.Count; i++)
                    {
                        var next = nextNodes[i];
                        if (next == null) { continue; }
                        next.StartExecution();
                    }
                }
            }
        }
    }
}