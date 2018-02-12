using UnityEngine;
using System.Collections;
using OpenCvSharp;
using OpenCvSharp.Blob;
using OpenCvSharp.CPlusPlus;
using System.Collections.Generic;

using System.IO;
using UnityEngine.UI;
using System.Text;

using System.Runtime.Serialization.Formatters.Binary;
using System;

public class DebugUI : MonoBehaviour {

    public GameObject text1;
    public GameObject text2;
    public GameObject text3;
    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        GlobalRepo.UserStep s = GlobalRepo.UserMode;
        string t1="";
        if (s == GlobalRepo.UserStep.design) t1 = "DESIGN";
        if (s == GlobalRepo.UserStep.feedback) t1 = "feedback";
        if (s == GlobalRepo.UserStep.simulation) t1 = "simulation";
        text1.GetComponent<Text>().text = t1;

        string t2 = "";
        text3.GetComponent<Text>().text = GlobalRepo.TasksDone.ToString();
    }
    public void DebugUserActivity(bool active)
    {
        string t2 = "";
        if (active) t2 = "ACTIVE";
        else t2 = "INACTIVE";
        text2.GetComponent<Text>().text = t2;
    }
}
