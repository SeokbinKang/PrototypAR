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

public class Content2UI_Prototypeimages : MonoBehaviour {

    public GameObject[] BikeImagePanes;
    public GameObject[] PanesBgLeft;
    public GameObject[] PanesBgRight;

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
    public void LoadBikeImages(int idx, Texture2D txt )
    {
        if (idx >= BikeImagePanes.Length) return;
        
        RawImage txtdest = BikeImagePanes[idx].GetComponent<RawImage>();
        if (txtdest == null) return;
        txtdest.texture = txt;
    }
    public void AdjustPanesPositions()
    {
        // align images to the virtual bikes

    }

    public void EnablePanes(bool BikeontheLeft)
    {
        if (BikeImagePanes == null || BikeImagePanes.Length <= 0) return;
        foreach (var t in BikeImagePanes)
        {
            if(t.GetComponent<RawImage>().texture!=null) t.transform.parent.gameObject.SetActive(true);
                else t.transform.parent.gameObject.SetActive(false);
        }
        AdjustPanesPositions();
        if (BikeontheLeft)
        {
            foreach (var t in PanesBgLeft)
            {
                t.SetActive(true);
            }
            foreach (var t in PanesBgRight)
            {
                t.SetActive(false);
            }
            this.GetComponent<RectTransform>().localPosition = new Vector3(0, 0,-1);
        } else
        {
            foreach (var t in PanesBgLeft)
            {
                t.SetActive(false);
            }
            foreach (var t in PanesBgRight)
            {
                t.SetActive(true);
            }
            this.GetComponent<RectTransform>().localPosition = new Vector3(-565, 0, -1);
        }
    }

    public void disablePanes()
    {
        if (BikeImagePanes == null || BikeImagePanes.Length <= 0) return;
        foreach (var t in BikeImagePanes)
        {
            t.transform.parent.gameObject.SetActive(false);
        }
    }
}
