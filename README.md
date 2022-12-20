# **KTaskGraph**


## **A hierarchical asynchronous task system on top of Unityengine coroutine feature.**


#### Installation:
* Add an entry in your manifest.json as follows:
```C#
"com.kaiyum.ktaskgraph": "https://github.com/kaiyumcg/KTaskGraph.git"
```

Since unity does not support git dependencies, you need the following entries as well:
```C#
"com.kaiyum.attributeext2": "https://github.com/kaiyumcg/NaughtyAttributes",
"com.kaiyum.unityext": "https://github.com/kaiyumcg/UnityExt.git",
"com.github.siccity.xnode": "https://github.com/siccity/xNode.git",
"com.kaiyum.editorutil": "https://github.com/kaiyumcg/EditorUtil.git"
```
Add them into your manifest.json file in "Packages\" directory of your unity project, if they are already not in manifest.json file.

### **<span style="text-decoration:underline;">Workflow:</span>**


#### Each task is a coroutine. Then consider the following example:


```
Transform checkPoint1, checkPoint2;
    TaskGraph heroGraph;
    void Start()
    {
        heroGraph = TaskGraph.Create("Hero", gameObject);
        heroGraph.CreateRootTask("Reach check point", ReachCheckpoint(transform, checkPoint1)).StartTask("Drink", DrinkBeer()).
            Then("dance", DanceForTwoMin()).Then("sleep For one hour", SleepForOneHour())
            .Then("morning task group", new List<IEnumerator> { ThinkForTomorrowPlan(), Freshup(), WakeOthers() }, () =>
            {
                Debug.Log("morning tasks group completed");
            }).Then("reach hunting ground", ReachCheckpoint(transform, checkPoint2));
            
    }
```


This script is attached to our hero. First he/she would reach the first checkpoint. Then the hero will start Drink and Dance. After the dance, the hero will sleep for one hour. Then he/she will complete a group of morning tasks consisting of “Thinking for tomorrow”, “FreshUp”, “waking others”. When all three completes execution, we print a debug log. Then the hero will reach the hunting ground. 

In KTaskGraph, you will implement the task with coroutine. Then you make your execution tree as above. You can schedule execution as follows:


```
heroGraph.ScheduleIfReq(() =>
        {
            Debug.Log("graph completed execution totally");
        });
```


Right now, the system does not contain any node based editor to debug/analyze your tasks. This is something I am up to doing. 

Roadmap:



* Node based editor to analyze/debug tasks
* Node based editor to create tasks and save them in files
* Experimental async wait implementation
