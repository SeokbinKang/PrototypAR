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
public class SensorImageViewer : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void LoadSrcTexture2D(Texture2D txt)
    {
        RawImage targetImageComp = this.GetComponentInParent<RawImage>();
        if (targetImageComp == null) return;
        targetImageComp.texture = txt;        
    }
    public void GetTextureSize(ref float width, ref float height)
    {
        RectTransform textureRect = this.GetComponentInParent<RectTransform>();
        if (textureRect == null)
        {
            width = 1;
            height = 1;
            return;
        }
        width = textureRect.sizeDelta.x;
        height = textureRect.sizeDelta.y;

    }
}
