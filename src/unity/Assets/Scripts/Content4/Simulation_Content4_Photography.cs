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
    public GameObject PrototypesInvertoryUI;
    public GameObject viewFinder;
    public GameObject ReviewPhotoUI;
    private Vector3 CameraInitPos;

    public List<PhotoShot> mPhotoshots;
    // Use this for initialization

    private SimulationParam activeParam;
    private string activePrototypeName;
    void Start () {

        //CameraInitPos = game_camera.transform.localPosition;
        this.gameObject.SetActive(false);
        //Debug.Log("local pos " + CameraInitPos);
        mPhotoshots = new List<PhotoShot>();
        activeParam = null;
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
        viewFinder.GetComponent<UI_ViewFinder>().UpdateFocusVal(fov);
    }
    private void SetShutterSpeed(float ss)
    {
        float lightLevel100 = CVProc.linearMap(ss, 1, 1000, 1, 100);
        game_camera.GetComponent<Simulation_Content4_AppCam>().setLightBrightnessLevel100(lightLevel100);
        viewFinder.GetComponent<UI_ViewFinder>().UpdateSSpeedVal(ss);
    }
    private void SetSensorType(string Sensortype)
    {
        if (Sensortype == "none" || Sensortype == "mono") game_camera.GetComponent<Simulation_Content4_AppCam>().SetGrayscale(true);
        if (Sensortype == "color") game_camera.GetComponent<Simulation_Content4_AppCam>().SetGrayscale(false);
    }
    private void reset()
    {
        //alight camera to the initial position
        game_camera.transform.localPosition = new Vector3(0, 0, -1);
        Rigidbody2D rb = game_camera.GetComponent<Rigidbody2D>();
        if (rb == null) return;
        rb.velocity = new Vector2(0, 0);
        SetSensorType("none");
        SetFov(100);
        SetShutterSpeed(800);

        if(activeParam==null)
        {
            activeParam = new SimulationParam();
            activeParam.C4_focalLength = 100;
            activeParam.C4_shutterSpeed = 800;
            activeParam.C4_sensorType = "mono";
        }
        //reset photography mode

        //reload photo

        //load prototypes and init mincams
    }
    public void loadPrototypeParam()
    {
        if (GlobalRepo.UserMode != GlobalRepo.UserStep.AppContent4) return;
        string prototypeName="";
        SimulationParam sp = PrototypesInvertoryUI.GetComponent<PrototypeInstanceManager>().GetPrototypeProperties_Content4(out prototypeName);
        sp.DebugPrint();
        SetSensorType(sp.C4_sensorType);
        SetFov(sp.C4_focalLength);
        SetShutterSpeed(sp.C4_shutterSpeed);
        activeParam = sp;
        activePrototypeName = prototypeName;


    }
    public void OnTakePicture()
    {
        if (ReviewPhotoUI == null) return;
        //retrieve a texture 2d from the camera instance
        Texture2D photoTexture2D = game_camera.GetComponent<Simulation_Content4_AppCam>().Capture();
        Debug.Log("[DEBUG] Pictuer Taken: "+photoTexture2D.width+" "+photoTexture2D.height);
        //create an instance of photo with related parameters
        PhotoShot p = new PhotoShot(activePrototypeName, activeParam, photoTexture2D);
        //add to the picture result
        ReviewPhotoUI.GetComponent<PhotoReviewUI>().AddcompletePrototypeInstance(p);
        
        
    }
}

public class PhotoShot
{
    public string prototypeName;
    public SimulationParam parameter;
    public Texture2D txt2D;

    public PhotoShot()
    {
        prototypeName = "";
        parameter = null;
        txt2D = null;
    }
    public PhotoShot(string name, SimulationParam sp, Texture2D photo)
    {
        prototypeName = name;
        parameter = sp;
        txt2D = photo;
    }

}