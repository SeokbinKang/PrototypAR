using UnityEngine;
using System.Collections;
using OpenCvSharp;
using OpenCvSharp.Blob;
using OpenCvSharp.CPlusPlus;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System;
using System.Runtime.InteropServices;
using Uk.Org.Adcock.Parallel;
using UnityEngine.UI;

public class UI_ViewFinder : MonoBehaviour {
    public GameObject focusUI;
    public GameObject sspeedUI;
    public GameObject shutterUI;
    public GameObject albumUI;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
    public void UpdateFocusVal(float f)
    {
        focusUI.GetComponentInChildren<Text>().text = f + "mm";
    }
    public void UpdateSSpeedVal(float f)
    {
        sspeedUI.GetComponentInChildren<Text>().text = f + "ms";
    }

}
