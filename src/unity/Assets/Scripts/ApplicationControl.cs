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

public class ApplicationControl : MonoBehaviour
{
    public enum RunMod : int { Configuration, Release }

    public RunMod ApplicationMode;
    public DesignContent ContentType;
    public bool ShowDebugImages = false;
    public bool SuppressCVWindows = true;
    public Vector2 RegionboxLefttop=new Vector2(0,0);
    public Vector2 RegionboxRightbot = new Vector2(0, 0);    
    public int ViewScale=2;


    public GameObject ARDetector = null;
    private List<CvPoint> regionPoints;
    private CvMat scaledFrame = null;
    // Use this for initialization
    void Start()
    {
        regionPoints = new List<CvPoint>();
        if (ApplicationMode == RunMod.Configuration)
        {
            CreateConfigurations();
        }
        loadPref();
        GlobalRepo.suppressCVWindows(SuppressCVWindows);
        GlobalRepo.Setting_ShowDebugImgs(ShowDebugImages);
    }

    // Update is called once per frame
    void Update()
    {
        if (ApplicationMode == RunMod.Configuration)
        {
            showConfigRegion();
        }

        KeyInput();
   /*     float screenWidth = Screen.height * 4 / 3;
        Screen.SetResolution((int)screenWidth, Screen.height, false);*/
    //    if(q++ % 30 ==0)      testSize();
    }
    static int q = 0;
    private void testSize()
    {
        GameObject go_child = null;
        Vector3 goCenter = new Vector3();
        Vector3 goSize = new Vector3();
        Vector3 goCenter_iter = new Vector3();
        GameObject go_child_iter = null;
        Vector3 goSize_iter = new Vector3();
        Vector3 scaleRatio_iter = new Vector3();

        float minSizeGap = float.MaxValue;
        float sizeGap_iter;

        if (GlobalRepo.Setting_ShowDebugImgs()) Debug.Log("[DEBUG-SIMULATION] looking for most similar preloaded object");

        go_child_iter = GameObject.Find("chainring_6");

            SceneObjectManager.MeasureObjectInfoinScreenCoord(go_child_iter, ref goCenter_iter, ref goSize_iter);
            
             Debug.Log(q+"[DEBUG-SIMULATION] child object size in Screen " + goSize_iter);
            
        }
    void OnDestroy()
    {

    }
    private void Reset()
    {
        this.GetComponentInParent<Simulation>().reset();
        ARDetector.GetComponent<designARManager>().resetScene();
        GlobalRepo.reset();

    }
    private void KeyInput()
    {
        if (Input.GetButtonUp("Fire1"))
        {
         
            GlobalRepo.setLearningCount(6);
        }
        if (Input.GetButtonUp("Cancel"))
        {
            Reset();         
        }
      /*  if (Input.GetButtonUp("Fire2"))
        {
            this.GetComponent<CameraControl>().MainCameraFoV = 60;
            SceneObjectManager.getActiveInstance().adjustAlphaSpriteRenderInstant(PreLoadedObjects.Content1_BGFull, 255);
        }*/
        if (Input.GetButtonUp("Fire3"))
        {
            this.GetComponent<CameraControl>().MainCameraFoV = 120;
            
        }
        if (Input.GetButton("key0") )
        {
            ColorDetector.processKeyInput('0');
        }
        if (Input.GetButton("key1"))
        {
            ColorDetector.processKeyInput('1');
        }
        if (Input.GetButton("key2"))
        {
            ColorDetector.processKeyInput('2');
        }
        if (Input.GetButton("key3"))
        {
            ColorDetector.processKeyInput('3');
        }
        if (Input.GetButton("key4"))
        {
            ColorDetector.processKeyInput('4');
        }

    }
    private void loadPref()
    {
        if (PlayerPrefs.HasKey("RegionboxLefttopX")) RegionboxLefttop.x = PlayerPrefs.GetInt("RegionboxLefttopX");
        if (PlayerPrefs.HasKey("RegionboxLefttopY")) RegionboxLefttop.y = PlayerPrefs.GetInt("RegionboxLefttopY");
        if (PlayerPrefs.HasKey("RegionboxRightbotX")) RegionboxRightbot.x = PlayerPrefs.GetInt("RegionboxRightbotX");
        if (PlayerPrefs.HasKey("RegionboxRightbotY")) RegionboxRightbot.y = PlayerPrefs.GetInt("RegionboxRightbotY");
        regionPoints.Clear();
        regionPoints.Add(new CvPoint((int)RegionboxLefttop.x, (int)RegionboxLefttop.y));
        regionPoints.Add(new CvPoint((int)RegionboxRightbot.x, (int)RegionboxRightbot.y));
        setRegionBox(regionPoints);
    }
    private void CreateConfigurations()
    {
        CvWindow configRegionWin = GlobalRepo.getDebugWindow("config_region");
        Cv.SetMouseCallback("config_region", RegionConfigCallback);
        CvWindow configRegionWin2 = GlobalRepo.getDebugWindow("config_color");
        Cv.SetMouseCallback("config_color", ColorDetector.mouseCallback);
    }
    private void showConfigRegion()
    {
        CvRect rBox= GlobalRepo.GetRegionBox(false);
        CvMat rawFullFrame = GlobalRepo.GetRepo(RepoDataType.dRawBGR);
        if (rawFullFrame == null) return;
        if(rBox!=null) rawFullFrame.DrawRect(rBox, CvColor.Red);
            else rawFullFrame.PutText("REGION BOX UNSET", new CvPoint(400, 400), new CvFont(FontFace.HersheyTriplex, 2.0f, 2.0f), CvColor.Red);
        if (scaledFrame == null) scaledFrame = new CvMat (rawFullFrame.Height / ViewScale,rawFullFrame.Width / ViewScale, MatrixType.U8C3);
        Cv.Resize(rawFullFrame, scaledFrame);        

        GlobalRepo.showDebugImage("config_region", scaledFrame);
        GlobalRepo.showDebugImage("config_color", GlobalRepo.GetRepo(RepoDataType.dRawRegionBGR));
    }
    private void setRegionBox(List<CvPoint> regionPoints)
    {
        CvRect regionBox = new CvRect((regionPoints[0].X * ViewScale), (regionPoints[0].Y * ViewScale), (regionPoints[1].X - regionPoints[0].X) * ViewScale, (regionPoints[1].Y - regionPoints[0].Y) * ViewScale);
        //Y flip
        GlobalRepo.setRegionBox(regionBox);
    }
    public void RegionConfigCallback(MouseEvent @mevent, int x, int y, MouseEvent flags)
    {
        if(mevent == MouseEvent.LButtonUp)
        {
            Debug.Log("Setting ROI... (" + x + ", " + y + ")");            
            //mouse click
            CvPoint t = new CvPoint(x, y);
            regionPoints.Add(t);
            if (regionPoints.Count > 2)
            {
                regionPoints.RemoveAt(0);
                regionPoints.RemoveAt(0);
            }
            if(regionPoints.Count==2)
            {
                RegionboxLefttop.x = regionPoints[0].X;
                RegionboxLefttop.y = regionPoints[0].Y;
                RegionboxRightbot.x = regionPoints[1].Y;
                RegionboxRightbot.y = regionPoints[1].Y;


                PlayerPrefs.SetInt("RegionboxLefttopX", regionPoints[0].X);
                PlayerPrefs.SetInt("RegionboxLefttopY", regionPoints[0].Y);
                PlayerPrefs.SetInt("RegionboxRightbotX", regionPoints[1].X);
                PlayerPrefs.SetInt("RegionboxRightbotY", regionPoints[1].Y);

                setRegionBox(regionPoints);
            }
         
        }

    }
    public DesignContent getContentType()
    {
        return ContentType;
    }
}
