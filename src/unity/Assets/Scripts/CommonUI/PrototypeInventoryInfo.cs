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
    public GameObject[] GraphicParamObjects;

    // Use this for initialization
    void Start() {
        //if (ApplicationControl.ActiveInstance.getContentType() == DesignContent.CameraSystem)

    }

    // Update is called once per frame
    void Update() {

    }
    public void SetName(string name)
    {
        if (NameObject != null) NameObject.GetComponent<Text>().text = name;
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
    public void SetParamBar_Numerical(int idx, string name, float val, float low, float high, string valueString, Color c)
    {
        if (idx >= GraphicParamObjects.Length) return;

        GraphicParamObjects[idx].GetComponent<HorizontalLevelBar>().SetValueName(name);
        GraphicParamObjects[idx].GetComponent<HorizontalLevelBar>().SetNumericalValue(val, low, high, valueString);
        GraphicParamObjects[idx].GetComponent<HorizontalLevelBar>().SetColor(c);

    }
    public void SetParamBar_Categorical(int idx, string name,string valueString, Color c)
    {
        if (idx >= GraphicParamObjects.Length) return;

        GraphicParamObjects[idx].GetComponent<HorizontalLevelBar>().SetValueName(name);
        GraphicParamObjects[idx].GetComponent<HorizontalLevelBar>().SetCategoricalValue(valueString);            
        GraphicParamObjects[idx].GetComponent<HorizontalLevelBar>().SetColor(c);

    }
}
