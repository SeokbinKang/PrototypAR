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


public class Simulation_Content4_Camera : MonoBehaviour {
    public GameObject part_lens;
    public GameObject part_shutter;
    public GameObject part_sensor;
    public GameObject part_lightray;

    public GameObject UIControl;
    public GameObject UIComponentsControl;
    
    public GameObject PhotoResource;
    public GameObject PhotographyApp;
    private SimulationParam param=null;
    private bool initialized = false;
    private static Dictionary<string, Asset2DTexture> PhotoDict = null;

    private byte[] PhotoByteBuffer;
    private Texture2D Txt2D;
    // Use this for initialization
    void Start () {
        init();
        
        if (part_lens == null || part_shutter == null || part_sensor == null || part_lightray == null)
        {
            Debug.Log("[ERROR] the camera system's sub-objects are null");
            return;
        }
        
    }
	
	// Update is called once per frame
	void Update () {
        
        SimulateLensLightRay();
        SimulateSensorView();
        SimulateShutter();
    }
    private void init()
    {
        if (!initialized)
        {            
            initialized = true;
        }
        //    PhotoDict = new Dictionary<string, Asset2DTexture>();
        //  loadPhotos();
        UIControl.SetActive(true);
        PhotographyApp.SetActive(false);
        reset();
        //  this.PhotoByteBuffer = null;
        

    }
    private void hideLightRay()
    {
        Simulation.SetSpriteAlphato(part_lightray, 0);
    }
    
    public void UpdateCameraParams(SimulationParam sp)
    {
        if (sp == null) return;
        param = (SimulationParam) sp.Clone();
       // param.DebugPrint();
    }
    public void reset()
    {
        param = null;
        part_lens.transform.position = new Vector3(10, 2, 1);
        part_shutter.transform.position = new Vector3(10, 2, 1);
        part_sensor.transform.position = new Vector3(10, 2, 1);
        this.UIComponentsControl.GetComponent<Content4_UI_Components>().reset();


    }
    private void SimulateLensLightRay()
    {
        if (part_lens == null || !Simulation.IsActiveinSimulation(this.part_lens)) return;
        UIControl.GetComponent<Content4_UIControl>().SetUIMode(Content4_UIModes.SystemComponents);
        this.UIComponentsControl.GetComponent<Content4_UI_Components>().updateLensUI(part_lens, param.C4_focalLength,param.C4_focusLabelPos);
        //set the X scale of light ray
      
        part_lens.GetComponent<C4_lens>().SetFocalLength(param.C4_focalLength);


        

        //animate light ray


    }
    private void SimulateLensLightRayOBSOLTE()
    {/*
        if (part_lens == null || !Simulation.IsActiveinSimulation(this.part_lens)) return;
        UIControl.GetComponent<Content4_UIControl>().SetUIMode(Content4_UIModes.SystemComponents);
        //set the X scale of light ray
        float baseXscale = 0.07f;
        float minXScale = 0.9f;
        float maxXscale = 2.1f;
        //focal length 10 - 200
        float ScaleOffset = (param.C4_focalLength - 10) / 190;
        float adjustedXScale = baseXscale*(minXScale + ScaleOffset * (maxXscale - minXScale));
        Vector3 sc = part_lightray.transform.localScale;
        sc.x = adjustedXScale;
        sc.y = part_lens.transform.localScale.y;
        sc.z = part_lens.transform.localScale.z;
        part_lightray.transform.localScale = sc;
        Vector3 pos = part_lens.transform.localPosition;
        part_lightray.transform.localPosition = pos;
        Simulation.SetSpriteAlphato(part_lightray, 250);

        this.UIComponentsControl.GetComponent<Content4_UI_Components>().updateLensUI(part_lens, param.C4_focalLength);*/
       
        //animate light ray
        

    }
    private void SimulateShutter()
    {
        if (part_shutter == null || !Simulation.IsActiveinSimulation(this.part_shutter)) return;
        UIControl.GetComponent<Content4_UIControl>().SetUIMode(Content4_UIModes.SystemComponents);
        this.UIComponentsControl.GetComponent<Content4_UI_Components>().updateShutterUI(part_shutter, param.C4_shutterSpeed,param.C4_shutterspeedLabelPos);
    }
    private void loadPhotos()
    {
        PhotoDict.Add("photo1", new Asset2DTexture("Assets/2DAnimation/content_4/photos/ApplicationPhoto1.png"));
    }
    private void SimulateSensorView()
    {
        if (part_sensor == null || !Simulation.IsActiveinSimulation(this.part_sensor)) return;
        if (Txt2D != null) return;
        UIControl.GetComponent<Content4_UIControl>().SetUIMode(Content4_UIModes.SystemComponents);
        this.UIComponentsControl.GetComponent<Content4_UI_Components>().updateSensorUI(part_sensor, param.C4_sensorType,param.C4_sensortypeLabelPos);
        //
        part_sensor.GetComponent<C4_sensor>().SetType(param.C4_sensorType);
        



        /*
        this.UIPhotoView.SetActive(true);
        //load an image to capture
        CvMat srcImg = PhotoDict["photo1"].txtBGRAImg;
        CvMat CropImage;
        //image processing for FOV , cropping to the aspet Ratio
        float croppingAreaPortion = CVProc.linearMap(param.C4_focalLength, 10, 200, 1f, 0.1f);
        float CropSensorW = 0;
        float CropSensorH=0;
        if (croppingAreaPortion < 0.1f) croppingAreaPortion = 0.1f;
        else if (croppingAreaPortion > 1f) croppingAreaPortion = 1f;
        this.UIPhotoView.GetComponent<SensorImageViewer>().GetTextureSize(ref CropSensorW, ref CropSensorH);
        CropImage = CVProc.CropToAspectRatio(srcImg, CropSensorW, CropSensorH, croppingAreaPortion);
        //image processing for exposure. 

        float gainShift = CVProc.linearMap(param.C4_shutterSpeed, 1, 1000, -100f, 100f);
        Debug.Log("[DEBUG][Exposure]" + gainShift);
        CropImage.AddS(new CvScalar(gainShift, gainShift, gainShift), CropImage);
        //image processing for blur
        int blurTimes = CheckFocusPin();
            
        while (blurTimes-- > 0)
        {
            CropImage.Smooth(CropImage, SmoothType.Gaussian,31);
        }
        


        //image processing for sensor type
        if(param.C4_sensorType=="mono")
        {
            CvMat tmpGrayscale = new CvMat(CropImage.Rows, CropImage.Cols, MatrixType.U8C1);
            CropImage.CvtColor(tmpGrayscale, ColorConversion.BgraToGray);
            tmpGrayscale.CvtColor(CropImage, ColorConversion.GrayToBgra);
        }

        //send the processed image to the UISensorView
        if (Txt2D==null)
        {
            getTxt2D(CropImage);
        }
        this.UIPhotoView.GetComponent<SensorImageViewer>().LoadSrcTexture2D(this.Txt2D);
        Debug.Log("new Asset2D");

        //send the sensor cropping shape to the UISensorView

        //enable callback from "capture" button

        this.UIComponentsControl.GetComponent<Content4_UI_Components>().updateSensorUI(part_sensor, param.C4_sensorType);
        */
    }
    private int CheckFocusPin()
    {
        int maxBlurN = 10;
        Vector2 PiVotFocusinLightRay = new Vector2(0.61f, 0.5f);
        Vector2 PivotObjCenter = new Vector2(0.5f, 0.5f);
        if (this.part_sensor == null) return maxBlurN;
        Vector3 focalPointScreen = new Vector3();
        Vector3 sensorPointScreen = new Vector3();

        //check the focal point using the light ray position
        SceneObjectManager.MeasureObjectPointinScreen(part_lightray, PiVotFocusinLightRay, ref focalPointScreen);
        //check the pos of the sensor
        SceneObjectManager.MeasureObjectPointinScreen(part_sensor, PivotObjCenter, ref sensorPointScreen);
        // check the difference
        float xFocusDifference = Math.Abs(focalPointScreen.x - sensorPointScreen.x);

        
        int BlurN = (int) (xFocusDifference / 15f);
        Debug.Log("[DEBUG][Focus] " + xFocusDifference + "\t" + BlurN);

        return BlurN;

    }
    private void getTxt2D(CvMat img)
    {
        this.PhotoByteBuffer = new byte[img.Step * img.Height];        
        Marshal.Copy(img.DataArrayByte.Ptr, this.PhotoByteBuffer, 0, img.Step * img.Height);
        this.Txt2D = new Texture2D(img.Width, img.Height, TextureFormat.RGBA32, false);
        this.Txt2D.LoadRawTextureData(PhotoByteBuffer);
        Txt2D.Apply();
    }
    public void LoadAppSimulationData()
    {
        Simulation_Content4_Photography s = PhotographyApp.GetComponent<Simulation_Content4_Photography>();
        s.loadPrototypeParam();
    }
    
}
