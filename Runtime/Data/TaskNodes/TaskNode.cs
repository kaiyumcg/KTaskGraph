using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KTaskGraph
{
    public class TaskNode : BaseNode
    {
        internal AtomicTask Task { get { return task; } }
        AtomicTask task = null;
        private TaskNode() { }
        internal static TaskNode Create(string taskName, IEnumerator task, System.Action OnComplete = null)
        {
            var node = new TaskNode { };
#if UNITY_EDITOR
            node.nodeName = taskName;
#endif
            AtomicTask _task = AtomicTask.Create(taskName, task, OnComplete);
            node.task = _task;
            return node;
        }

        TaskNode ProcSingle(string taskName, IEnumerator task, System.Action OnComplete = null)
        {
            isNodeDirty = true;
            if (runner == null)
            {
                throw new System.Exception("Can not update graph with the input task(s) since you have not set any runner for it! " +
    "Please call 'SetRunner' on TaskGraph");
            }
            var node = TaskNode.Create(taskName, task, OnComplete);
            node.runner = runner;
            node.graphData = graphData;
            node.Task.SetRunner(runner);
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

        public TaskNode StartTask(string taskName, IEnumerator task, System.Action OnComplete = null)
        {
            ProcSingle(taskName, task, OnComplete);
            return this;
        }

        ParallelTaskNode ProcMany(string collectionName, List<IEnumerator> tasks, System.Action OnComplete = null, bool waitForAllCompletion = true)
        {
            isNodeDirty = true;
            if (runner == null)
            {
                throw new System.Exception("Can not update graph with the input task(s) since you have not set any runner for it! " +
    "Please call 'SetRunner' on TaskGraph");
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

        public TaskNode StartTask(string collectionName, List<IEnumerator> tasks, System.Action OnComplete = null, bool waitForAllCompletion = true)
        {
            var node = ProcMany(collectionName, tasks, OnComplete, waitForAllCompletion);
            return this;
        }

        internal override void StartExecution()
        {
#if UNITY_EDITOR
            isRunning = true;
            completed = false;
#endif
            task.Exec(() =>
            {
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
                graphData.OnNodeCompletion();
            });

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