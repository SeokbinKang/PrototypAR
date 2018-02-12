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
public class PrototypeInventoryParam : MonoBehaviour {

    public GameObject name;
    public GameObject value;
	// Use this for initialization
	void Start () {
	   
	}
	
	// Update is called once per frame
	void Update () {
	
	}
    public void SetParam(string n, string v)
    {
        name.GetComponent<Text>().text = n;
        value.GetComponent<Text>().text = v;
    }
}
