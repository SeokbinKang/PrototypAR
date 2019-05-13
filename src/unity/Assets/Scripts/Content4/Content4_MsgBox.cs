using UnityEngine;
using System.Collections;

public class Content4_MsgBox : MonoBehaviour {

    public GameObject MsgCameraProblems;
	// Use this for initialization
	void Start () {
        reset();
    }
	void onEnable()
    {
        reset();
    }
	// Update is called once per frame
	void Update () {
	
	}
    public void reset()
    {
        MsgCameraProblems.SetActive(false);
    }
    public void ShowMsgCameraProblems()
    {
        MsgCameraProblems.SetActive(true);
    }
}
