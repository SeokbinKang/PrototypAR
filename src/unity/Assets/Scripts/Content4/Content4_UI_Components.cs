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
    public GameObject shutterUI;
    public GameObject sensorUI;
	// Use this for initialization
	void Start () {
	
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
        shutterUI.SetActive(false);
        sensorUI.SetActive(false);
    }
    public void updateLensUI(GameObject go,  float fVal)
    {
        Vector3 goCenter = new Vector3();
        Vector3 goSize = new Vector3();
        Vector2 objRectPos = new Vector2();
        SceneObjectManager.MeasureObjectInfoinScreenCoord(go, ref goCenter, ref goSize);
        //RectTransformUtility.ScreenPointToLocalPointInRectangle(this.GetComponent<RectTransform>(), new Vector2(goCenter.x, goCenter.y), Camera.main, out objRectPos);
        goCenter.y -=(goSize.y+40);
        objRectPos.x = goCenter.x - Screen.width / 2;
        objRectPos.y = goCenter.y - Screen.height / 2;
        //Debug.Log("!!!!!!!!!!"+goCenter+"\t"+objRectPos);
        lensUI.GetComponent<RectTransform>().localPosition = new Vector3(objRectPos.x, objRectPos.y, 0);

        lensUI.GetComponentInChildren<Text>().text = "Focal Length: " + ((int)fVal) + " mm";

    }
    public void updateShutterUI(GameObject go, float sVal)
    {
        Vector3 goCenter = new Vector3();
        Vector3 goSize = new Vector3();
        Vector2 objRectPos = new Vector2();
        SceneObjectManager.MeasureObjectInfoinScreenCoord(go, ref goCenter, ref goSize);
        //RectTransformUtility.ScreenPointToLocalPointInRectangle(this.GetComponent<RectTransform>(), new Vector2(goCenter.x, goCenter.y), Camera.main, out objRectPos);
        goCenter.y -= (goSize.y + 40);
        objRectPos.x = goCenter.x - Screen.width / 2;
        objRectPos.y = goCenter.y - Screen.height / 2;
        //Debug.Log("!!!!!!!!!!"+goCenter+"\t"+objRectPos);
        shutterUI.GetComponent<RectTransform>().localPosition = new Vector3(objRectPos.x, objRectPos.y, 0);

        shutterUI.GetComponentInChildren<Text>().text = "Shutter Speed: " + ((int)sVal) + " ms";

    }
    public void updateSensorUI(GameObject go, string sVal)
    {
        Vector3 goCenter = new Vector3();
        Vector3 goSize = new Vector3();
        Vector2 objRectPos = new Vector2();
        SceneObjectManager.MeasureObjectInfoinScreenCoord(go, ref goCenter, ref goSize);
        //RectTransformUtility.ScreenPointToLocalPointInRectangle(this.GetComponent<RectTransform>(), new Vector2(goCenter.x, goCenter.y), Camera.main, out objRectPos);
        goCenter.y -= (goSize.y + 40);
        objRectPos.x = goCenter.x - Screen.width / 2;
        objRectPos.y = goCenter.y - Screen.height / 2;
        //Debug.Log("!!!!!!!!!!"+goCenter+"\t"+objRectPos);
        sensorUI.GetComponent<RectTransform>().localPosition = new Vector3(objRectPos.x, objRectPos.y, 0);

        sensorUI.GetComponentInChildren<Text>().text = "Sensor Type: " + sVal;

    }
}
