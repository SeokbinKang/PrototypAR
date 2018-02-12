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
public class PrototypeInventoryInfo : MonoBehaviour {
    public GameObject NameObject;
    public GameObject[] ParamObjects;

    public int NofParams;
	// Use this for initialization
	void Start () {
        //if (ApplicationControl.ActiveInstance.getContentType() == DesignContent.CameraSystem)
        {
            NofParams = 3;
        }
        NofParams = 3;
    }
	
	// Update is called once per frame
	void Update () {
	
	}
    public void SetName(string name)
    {
        NameObject.GetComponent<Text>().text = name;
    }
    public string GetName()
    {
        if (NameObject == null) return "";
        return NameObject.GetComponent<Text>().text;
    }
    public void SetParam(int idx, string name, string value)
    {
        if (idx >= ParamObjects.Length) return;
        ParamObjects[idx].GetComponent<PrototypeInventoryParam>().SetParam(name, value);
    }
}
