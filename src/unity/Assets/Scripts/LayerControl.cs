using UnityEngine;
using System.Collections;
using OpenCvSharp;
using OpenCvSharp.Blob;
using OpenCvSharp.CPlusPlus;
using System.Collections.Generic;

using System.IO;
using System.Text;

using System.Runtime.Serialization.Formatters.Binary;
using System;

public class LayerControl : MonoBehaviour {
    // Use this for initialization
    [Tooltip("GUI-texture used to display the color camera feed on the scene background.")]
    public GameObject Layer0_LiveStream;
    public GameObject Layer1_RealityAR;  

    [Tooltip("GUI-texture used to display the color camera feed on the scene background.")]
    public GameObject backgroundOverlay;

    [Tooltip("Camera that will be used to overlay the 3D-objects over the background.")]
    public Camera foregroundCamera;

    
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        OnOffLayers();

    }
    private void OnOffLayers()
    {
        if (!GlobalRepo.isInit()) return;
        if(GlobalRepo.UserMode==GlobalRepo.UserStep.design) {
            Layer0_LiveStream.SetActive(true);
            Layer1_RealityAR.SetActive(true);
            adjustAlphaTextureColor(Layer0_LiveStream, 255, 0.15f);
            adjustAlphaTextureColor(Layer1_RealityAR, 0, 0.07f);
        }
        if (GlobalRepo.UserMode == GlobalRepo.UserStep.feedback)
        {
            Layer0_LiveStream.SetActive(true);
            Layer1_RealityAR.SetActive(true);
            adjustAlphaTextureColor(Layer1_RealityAR, 255, 0.02f);
        }
        if (GlobalRepo.UserMode == GlobalRepo.UserStep.simulation)
        {
            Layer0_LiveStream.SetActive(true);
            Layer1_RealityAR.SetActive(true);
            adjustAlphaTextureColor(Layer0_LiveStream, 0, 0.15f);
            adjustAlphaTextureColor(Layer1_RealityAR, 255, 0.015f);
            /*Color t = Layer1_RealityAR.GetComponent<GUITexture>().color;
            t.a = 0.5f;
            Layer1_RealityAR.GetComponent<GUITexture>().color = t;*/
        }
    }
    public static void adjustAlphaTextureColor(GameObject obj, float value, float mindiff)
    {
        if (obj == null) return;
        obj.SetActive(true);
        GUITexture sr = obj.GetComponent<GUITexture>();
        if (sr == null) return;
        Color spriteColor = sr.color;
        if (Mathf.Abs(spriteColor.a - value / 255f)>mindiff*1.5f)
        {
            //       Debug.Log("alpha : " + spriteColor.a + "---->" + value / 255f);
            spriteColor.a = spriteColor.a + Math.Max(Mathf.Abs((value / 255f - spriteColor.a)) * mindiff, mindiff)*Mathf.Sign(value / 255f-spriteColor.a);
        }
        else
        {

            spriteColor.a = value / 255f;
        }
        sr.color = spriteColor;
        return;
    }
}
