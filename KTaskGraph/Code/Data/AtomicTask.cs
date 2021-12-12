using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KTaskGraph
{
    public class AtomicTask
    {
        IEnumerator task = null;
        System.Action completionCallback = null;
        TaskGraphRunner runner = null;
        string taskName = "";
        bool running = false, completed = false;
        Coroutine internalHandle = null;

#if UNITY_EDITOR
        internal string TaskName { get { return taskName; } }
        internal bool IsRunning { get { return running; } }
        internal bool HasBeenCompleted { get { return completed; } }
#endif

        internal void SetRunner(TaskGraphRunner runner)
        {
            this.runner = runner;
        }

        private AtomicTask() { }
        internal static AtomicTask Create(string taskName, IEnumerator task, System.Action completionCallback)
        {
            if (task == null) { return null; }
            var t = new AtomicTask();
            t.taskName = taskName;
            t.task = task;
            t.running = false;
            t.completed = false;
            t.internalHandle = null;
            t.completionCallback = completionCallback;
            return t;
        }

        internal void Stop()
        {
            if (internalHandle != null)
            {
                runner.StopCoroutine(internalHandle);
            }
        }

        internal void Exec(System.Action OnComplete = null)
        {
            running = true;
            internalHandle = runner.StartCoroutine(ExecCOR(OnComplete));
            IEnumerator ExecCOR(System.Action OnComplete)
            {
                yield return runner.StartCoroutine(task);
                running = false;
                completed = true;
                internalHandle = null;
                completionCallback?.Invoke();
                OnComplete?.Invoke();
            }
        }
    }
}