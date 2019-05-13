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
    public GameObject prototypeimage;
    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
    public void UpdatePrototypeImage(Texture2D txt)
    {
        RawImage txtdest = prototypeimage.GetComponent<RawImage>();
        if (txtdest == null) return;
        txtdest.texture = txt;
    }
    public void UpdateFocusVal(float f)
    {
        if(f<0) focusUI.GetComponentInChildren<Text>().text = "?";
        else focusUI.GetComponentInChildren<Text>().text = (int) f + "mm";
    }
    public void UpdateSSpeedVal(float f)
    {
        if(f<0) sspeedUI.GetComponentInChildren<Text>().text = "?";
            else sspeedUI.GetComponentInChildren<Text>().text = (int)f + "ms";
    }

}
