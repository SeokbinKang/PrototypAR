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

public class Simulation_Content4_Photography : MonoBehaviour {
    public GameObject game_camera;
    public GameObject[] camera_instances;
    public GameObject PrototypesInvertoryUI;


    private Vector3 CameraInitPos;
    // Use this for initialization

    void Start () {

        //CameraInitPos = game_camera.transform.localPosition;
        this.gameObject.SetActive(false);
        //Debug.Log("local pos " + CameraInitPos);
	}
    void OnEnable()
    {
        reset();
    }
    
    // Update is called once per frame
    void Update () {
        KeyInput();
    }
    private void KeyInput()
    {
        /*  if (ApplicationMode == RunMod.Release)
              return;*/
              
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            game_camera.GetComponent<Simulation_Content4_AppCam>().move(directions.right);
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            game_camera.GetComponent<Simulation_Content4_AppCam>().move(directions.left);
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            game_camera.GetComponent<Simulation_Content4_AppCam>().move(directions.down);
        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            game_camera.GetComponent<Simulation_Content4_AppCam>().move(directions.up);
        }
    }
    public void Pan(int d)
    {
        if(d==0) game_camera.GetComponent<Simulation_Content4_AppCam>().move(directions.right);
        if (d == 1) game_camera.GetComponent<Simulation_Content4_AppCam>().move(directions.down);
        if (d == 2) game_camera.GetComponent<Simulation_Content4_AppCam>().move(directions.left);
        if (d == 3) game_camera.GetComponent<Simulation_Content4_AppCam>().move(directions.up);

    }
    private void SetFov(float fov)
    {
        float fovLevel100 = CVProc.linearMap(fov, 20, 200, 1, 100);
        game_camera.GetComponent<Simulation_Content4_AppCam>().setFovLevel100(fovLevel100);
    }
    private void SetShutterSpeed(float ss)
    {
        float lightLevel100 = CVProc.linearMap(ss, 1, 1000, 100, 1);
        game_camera.GetComponent<Simulation_Content4_AppCam>().setLightBrightnessLevel100(lightLevel100);
    }
    private void reset()
    {
        //alight camera to the initial position
        game_camera.transform.localPosition = new Vector3(0, 0, -1);
        Rigidbody2D rb = game_camera.GetComponent<Rigidbody2D>();
        if (rb == null) return;
        rb.velocity = new Vector2(0, 0);
        SetFov(100);
        SetShutterSpeed(800);
        //reset photography mode

        //reload photo

        //load prototypes and init mincams
    }
    
}
