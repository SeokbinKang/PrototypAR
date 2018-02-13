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
    public GameObject shutterUIText;
    public GameObject shutterUIButton;
    public GameObject shutterPart;
    public GameObject sensorUI;
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
    void reset()
    {
        lensUI.SetActive(false);
        shutterUIText.SetActive(false);
        shutterUIButton.SetActive(false);
        sensorUI.SetActive(false);
        shutterPart = null;
    }
    public void updateLensUI(GameObject go,  float fVal)
    {
      //  Debug.Log("!!!Update Lens UI");
        Vector3 goCenter = new Vector3();
        Vector3 goSize = new Vector3();
        Vector2 objRectPos = new Vector2();
        lensUI.SetActive(true);
        SceneObjectManager.MeasureObjectInfoinScreenCoord(go, ref goCenter, ref goSize);
        //RectTransformUtility.ScreenPointToLocalPointInRectangle(this.GetComponent<RectTransform>(), new Vector2(goCenter.x, goCenter.y), Camera.main, out objRectPos);
        goCenter.y -=(goSize.y+20);
        objRectPos.x = goCenter.x - Screen.width / 2 ;
        objRectPos.y = goCenter.y - Screen.height / 2;
        //Debug.Log("!!!!!!!!!!"+goCenter+"\t"+objRectPos+"\t"+Screen.width);
        objRectPos.x = objRectPos.x * (1200f/Screen.width);
        lensUI.GetComponent<RectTransform>().localPosition = new Vector3(objRectPos.x, objRectPos.y, 0);

        lensUI.GetComponentInChildren<Text>().text = "Focal Length: " + ((int)fVal) + " mm";

    }
    public void updateShutterUI(GameObject go_partShutter, float sVal)
    {
        shutterPart = go_partShutter;
        Vector3 goCenter = new Vector3();
        Vector3 goSize = new Vector3();
        Vector2 objRectPos = new Vector2();
        shutterUIButton.SetActive(true);
        shutterUIText.SetActive(true);
        SceneObjectManager.MeasureObjectInfoinScreenCoord(go_partShutter, ref goCenter, ref goSize);
        //RectTransformUtility.ScreenPointToLocalPointInRectangle(this.GetComponent<RectTransform>(), new Vector2(goCenter.x, goCenter.y), Camera.main, out objRectPos);
        goCenter.y -= (goSize.y + 40);
        //objRectPos.x = goCenter.x - Screen.width / 2;
        objRectPos.x = goCenter.x - Screen.width / 2;
        objRectPos.y = goCenter.y - Screen.height / 2;
        //Debug.Log("!!!!!!!!!!"+goCenter+"\t"+objRectPos);
        objRectPos.x = objRectPos.x * (1200f / Screen.width);
        shutterUIText.GetComponent<RectTransform>().localPosition = new Vector3(objRectPos.x, objRectPos.y, 0);
        shutterUIText.GetComponent<Text>().text = "Shutter Speed: " + ((int)sVal) + " ms";

        shutterUIButton.SetActive(true);

        float scaledShutterSpeed = CVProc.linearMap(sVal, 1, 1000, 10, 1);
        go_partShutter.GetComponent<Animator>().SetFloat("speed", scaledShutterSpeed);
        

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
        //RectTransformUtility.ScreenPointToLocalPointInRectangle(this.GetComponent<RectTransform>(), new Vector2(goCenter.x, goCenter.y), Camera.main, out objRectPos);
        goCenter.y -= (goSize.y + 40);
        objRectPos.x = goCenter.x - Screen.width / 2;
        objRectPos.y = goCenter.y - Screen.height / 2;
        //  Debug.Log("!!!!!!!!!!"+goCenter+"\t"+objRectPos);
        objRectPos.x = objRectPos.x * (1200f / Screen.width);
        sensorUI.GetComponent<RectTransform>().localPosition = new Vector3(objRectPos.x, objRectPos.y, goCenter.z);

        sensorUI.GetComponentInChildren<Text>().text = "Sensor Type: " + sVal;

    }
}
