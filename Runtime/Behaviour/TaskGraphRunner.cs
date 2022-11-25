using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KTaskGraph
{
    public class TaskGraphRunner : MonoBehaviour
    {
        TaskGraph graph;
        public void SetGraph(TaskGraph graph)
        {
            this.graph = graph;
        }

        public void WaitAFrame(System.Action OnComplete)
        {
            StartCoroutine(WaitAFrameCOR(OnComplete));
            IEnumerator WaitAFrameCOR(System.Action OnComplete)
            {
                yield return null;
                OnComplete?.Invoke();
            }
        }

        public void WaitSeconds(float seconds, System.Action OnComplete)
        {
            StartCoroutine(WaitSecCOR(seconds, OnComplete));
            IEnumerator WaitSecCOR(float seconds, System.Action OnComplete)
            {
                yield return new WaitForSeconds(seconds);
                OnComplete?.Invoke();
            }
        }
    }
}