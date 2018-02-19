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
public class Content4_UI_Components : MonoBehaviour {

    public GameObject lensUI;
    public GameObject lensBV;
    public GameObject shutterUI;
    public GameObject shutterUIText;
    public GameObject shutterUIButton;
    public GameObject shutterPart;
    public GameObject shutterBV;
    public GameObject sensorUI;
    public GameObject sensorBV;


	// Use this for initialization
	void Start () {
        reset();
    }
	
	// Update is called once per frame
	void Update () {
	
	}
    void onEnable()
    {
        reset();
    }
    public void reset()
    {
        lensUI.SetActive(false);
        lensBV.SetActive(false);
        shutterUIText.SetActive(false);
        shutterUIButton.SetActive(false);
        shutterBV.SetActive(false);
        sensorUI.SetActive(false);
        sensorBV.SetActive(false);
        shutterUI.SetActive(false);

        shutterPart = null;
    }
    public void updateLensUI(GameObject go,  float fVal)
    {
      //  Debug.Log("!!!Update Lens UI");
        Vector3 goCenter = new Vector3();
        Vector3 goSize = new Vector3();
        Vector2 objRectPos = new Vector2();
        
        SceneObjectManager.MeasureObjectInfoinScreenCoord(go, ref goCenter, ref goSize);
        
        goCenter.y -=(goSize.y+20);
        objRectPos.x = goCenter.x - Screen.width / 2 ;
        objRectPos.y = goCenter.y - Screen.height / 2;
        
        objRectPos.x = objRectPos.x * (1200f/Screen.width);
        lensUI.GetComponent<RectTransform>().localPosition = new Vector3(objRectPos.x, objRectPos.y, 0);
       // lensUI.GetComponentInChildren<Text>().text = "A Lense focuses light beams to its focal point";
        lensUI.SetActive(true);


        //if fval is valid->specified ->show BV bar
        lensBV.GetComponent<HorizontalLevelBar>().SetNumericalValue(fVal, 20, 220, fVal + " mm");
        


    }
    public void updateShutterUI(GameObject go_partShutter, float sVal)
    {
        shutterPart = go_partShutter;
        Vector3 goCenter = new Vector3();
        Vector3 goSize = new Vector3();
        Vector2 objRectPos = new Vector2();
        shutterUI.SetActive(true);
        shutterUIButton.SetActive(true);
        //shutterUIText.SetActive(true);        
        SceneObjectManager.MeasureObjectInfoinScreenCoord(go_partShutter, ref goCenter, ref goSize);
        //RectTransformUtility.ScreenPointToLocalPointInRectangle(this.GetComponent<RectTransform>(), new Vector2(goCenter.x, goCenter.y), Camera.main, out objRectPos);
        goCenter.y -= (goSize.y + 40);
        //objRectPos.x = goCenter.x - Screen.width / 2;
        objRectPos.x = goCenter.x - Screen.width / 2;
        objRectPos.y = goCenter.y - Screen.height / 2;
        //Debug.Log("!!!!!!!!!!"+goCenter+"\t"+objRectPos);
        objRectPos.x = objRectPos.x * (1200f / Screen.width);
        shutterUI.GetComponent<RectTransform>().localPosition = new Vector3(objRectPos.x, objRectPos.y, 0);
        //shutterUIText.GetComponent<Text>().text = "Shutter Speed: " + ((int)sVal) + " ms";
        
        


        //animation
        float scaledShutterSpeed = CVProc.linearMap(sVal, 1, 1000, 10, 1);
        go_partShutter.GetComponent<Animator>().SetFloat("speed", scaledShutterSpeed);

        //BV
        shutterBV.GetComponent<HorizontalLevelBar>().SetNumericalValue(sVal, 1, 1000, sVal + " ms");

    }
    public void CallbackShutterOpen()
    {
        if(shutterPart!=null) shutterPart.GetComponent<Animator>().Play("open");
    }
    public void updateSensorUI(GameObject go, string sVal)
    {
        Vector3 goCenter = new Vector3();
        Vector3 goSize = new Vector3();
        Vector2 objRectPos = new Vector2();
        sensorUI.SetActive(true);
        SceneObjectManager.MeasureObjectInfoinScreenCoord(go, ref goCenter, ref goSize);        
        goCenter.y -= (goSize.y + 40);
        objRectPos.x = goCenter.x - Screen.width / 2;
        objRectPos.y = goCenter.y - Screen.height / 2;
        
        objRectPos.x = objRectPos.x * (1200f / Screen.width);
        sensorUI.GetComponent<RectTransform>().localPosition = new Vector3(objRectPos.x, objRectPos.y, goCenter.z);

        //sensorUI.GetComponentInChildren<Text>().text = "Sensor Type: " + sVal;


        //BV
        sensorBV.GetComponent<HorizontalLevelBar>().SetCategoricalValue(sVal);
            
    }
}
