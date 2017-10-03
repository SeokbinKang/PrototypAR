using UnityEngine;
using System.Collections;
using IPPluginWrapper;
public class NewBehaviourScript : MonoBehaviour {

    IPPluginWrapperClass t=new IPPluginWrapperClass();
    // Use this for initialization
    void Start () {
        t.testWrapper();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
