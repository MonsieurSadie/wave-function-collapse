using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoroutinesExample : MonoBehaviour
{  
  IEnumerator Start()
  {
    StartCoroutine(Algorithm());

    yield return null;
  }

  IEnumerator Algorithm ()
  {
    Debug.Log("Starting algorithm...");
    int data0 = 0;
    // do something with data0
    data0 = 2;
    // wait N seconds
    yield return new WaitForSeconds(data0);

    Debug.Log("starting embeded coroutine AnotherFunction");
    yield return AnotherFunction();

    Debug.Log("algorithm finished");
  }

  IEnumerator AnotherFunction()
  {
    Debug.Log("Another function has done its things");
    yield return null; // end the function
  }
}
