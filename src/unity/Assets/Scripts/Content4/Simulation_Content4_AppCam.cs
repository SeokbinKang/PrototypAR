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
using UnityStandardAssets.ImageEffects;

public class Simulation_Content4_AppCam : MonoBehaviour {

    public GameObject diffuseLight;

    
	// Use this for initialization
	void Start () {
       
    }
	
	// Update is called once per frame
	void Update () {
	
	}
    public void move(directions d)
    {
        Rigidbody2D rb = this.GetComponentInParent<Rigidbody2D>();
        if (rb == null) return;
        float movingFactor = 0.5f;
        if(d==directions.right)
        {
            Vector2 v = rb.velocity;
            v.x += movingFactor;
            rb.velocity = v;
        } else if (d == directions.left)
        {
            Vector2 v = rb.velocity;
            v.x -= movingFactor;
            rb.velocity = v;
        } else if (d == directions.up)
        {
            Vector2 v = rb.velocity;
            v.y += movingFactor;
            rb.velocity = v;
        } else if (d == directions.down)
        {
            Vector2 v = rb.velocity;
            v.y -= movingFactor;
            rb.velocity = v;
        }
    }
    public void setFovLevel100(float fov100)
    {
        //intput fov level 1-100
        //size range 10-1
        float camSize = CVProc.linearMap(fov100, 1, 100, 10, 2);
        Camera cam = this.GetComponentInParent<Camera>();
        cam.orthographicSize = camSize;
    }
    public void setLightBrightnessLevel100(float light100)
    {
        //intput fov level 1-100
        //light intensity range 0-8;
        
        float intensity = CVProc.linearMap(light100, 1f, 100f, 0f,5f);
        
        Light l = diffuseLight.GetComponent<Light>();
        l.intensity = intensity;
    }
    public void SetGrayscale(bool on)
    {
        this.GetComponent<Grayscale>().enabled = on;
    }
    public Texture2D Capture()
    {
        Simulation_Content4_AppCapture c = this.GetComponent<Simulation_Content4_AppCapture>();
        if (c == null) return null;
        return c.Capture();
    }
    
}
public enum directions
{
    none,
    up,
    down,
    right,
    left
}
