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
    public GameObject MsgUI;
    public GameObject[] PhotoPlaces;
    
    public string[] Photonames;
    public int selectedPhotoIndex;
    private Vector3 CameraInitPos;

    public List<PhotoShot> mPhotoshots;
    // Use this for initialization

    private SimulationParam activeParam;
    private string activePrototypeName;
    
    void Start () {

        //CameraInitPos = game_camera.transform.localPosition;
        //this.gameObject.SetActive(false);
        //Debug.Log("local pos " + CameraInitPos);
        
      //  activeParam = null;
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
    public void UpdatePhotoIndex(int index)
    {
        selectedPhotoIndex = index;
        if (PhotoPlaces == null || PhotoPlaces.Length==0) return;
        for (int i = 0; i < PhotoPlaces.Length; i++)
        {
            if (selectedPhotoIndex != i) PhotoPlaces[i].SetActive(false);
            else PhotoPlaces[i].SetActive(true);

        }
        loadPrototypeParam();
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
        Debug.Log("SSSEETTT FFOFOOOVOVVVV" +fov);
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
        game_camera.GetComponent<Simulation_Content4_AppCam>().SetSensorType(Sensortype);
        if (Sensortype == "none" || Sensortype == "GRAYSCALE" || Sensortype == "Black & White")
        {
            game_camera.GetComponent<Simulation_Content4_AppCam>().SetGrayscale(true);
        }
        if (Sensortype == "Red & Green" || Sensortype == "Full Color") game_camera.GetComponent<Simulation_Content4_AppCam>().SetGrayscale(false);
    }
    private void resetCameraPos()
    {
        game_camera.transform.localPosition = new Vector3(0, 0, -1);
    }
    private void reset()
    {
        //alight camera to the initial position
      
        Rigidbody2D rb = game_camera.GetComponent<Rigidbody2D>();
        if (rb == null) return;
        rb.velocity = new Vector2(0, 0);
        SetSensorType("none");
        SetFov(20);
        SetShutterSpeed(10);
        loadPrototypeParam();
        if (activeParam==null)
        {
            activeParam = new SimulationParam();
            activeParam.C4_focalLength = 20;
            activeParam.C4_shutterSpeed = 10;
            activeParam.C4_sensorType = "mono";
        }
        //reset photography mode

        //reload photo
        UpdatePhotoIndex(selectedPhotoIndex);
        MsgUI.GetComponent<Content4_MsgBox>().reset();

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

        for (int i = 0; i < PhotoPlaces.Length; i++)
        {
            if (PhotoPlaces[i].activeSelf) PhotoPlaces[i].GetComponent<PhotoPool>().SetPhotoType(sp.C4_sensorType);
        }
        SetPrototypeCompleteness(sp.NumberOfFeedback,sp);

        resetCameraPos();

        viewFinder.GetComponent<UI_ViewFinder>().UpdatePrototypeImage(PrototypesInvertoryUI.GetComponent<PrototypeInstanceManager>().GetPrototypePhysicalImage());

    }
    private void SetPrototypeCompleteness(int NofFeedback, SimulationParam sp)
    {
      int MaxFeedback = SystemConfig.ActiveInstnace.PrototypeCompleteNFeedback;
        Debug.Log("Checking completeness of Camera Prototype >>>>"+ NofFeedback + " < " + MaxFeedback);
        /*
        if (NofFeedback > MaxFeedback)
        {
            int intensity = NofFeedback - MaxFeedback;
            game_camera.GetComponent<Simulation_Content4_AppCam>().SetBlur(true, intensity);
        } else
        {
            game_camera.GetComponent<Simulation_Content4_AppCam>().SetBlur(false, 0);
        }*/
        if (sp.C4_missingLens)
        {
            Debug.Log("lens missing");
            game_camera.GetComponent<Simulation_Content4_AppCam>().SetBlur(true, 10);
            MsgUI.GetComponent<Content4_MsgBox>().ShowMsgCameraProblems();
        } else game_camera.GetComponent<Simulation_Content4_AppCam>().SetBlur(false, 10);
        if (sp.C4_missingShutter)
        {
            Debug.Log("shutter missing");
            SetShutterSpeed(1000);
            MsgUI.GetComponent<Content4_MsgBox>().ShowMsgCameraProblems();
        }
        if (sp.C4_missingSensor)
        {
            Debug.Log("sensor missing");
            SetShutterSpeed(1);
            MsgUI.GetComponent<Content4_MsgBox>().ShowMsgCameraProblems();
        }
        if ((!sp.C4_missingLens) && (!sp.C4_missingShutter) && (!sp.C4_missingSensor)) MsgUI.GetComponent<Content4_MsgBox>().reset();
    }
    public void OnTakePicture()
    {
        if (ReviewPhotoUI == null) return;
        //retrieve a texture 2d from the camera instance
        Texture2D photoTexture2D = game_camera.GetComponent<Simulation_Content4_AppCam>().Capture();
       // Debug.Log("[DEBUG] Pictuer Taken: "+photoTexture2D.width+" "+photoTexture2D.height);
        //create an instance of photo with related parameters
        PhotoShot p = new PhotoShot(activePrototypeName, activeParam, photoTexture2D);
        //add to the picture result
        p.place = selectedPhotoIndex;
        ReviewPhotoUI.GetComponent<PhotoReviewUI>().AddcompletePrototypeInstance(p);
        
        
    }
}

public class PhotoShot
{
    public string prototypeName;
    public SimulationParam parameter;
    public Texture2D txt2D;
    public int place;
    public float tmpDist;
    public int tmpIdx;
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