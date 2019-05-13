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
public class WorkSpaceUI : MonoBehaviour {

    public int CurrentScaffoldingStep; // -1: no-scaffolding   0: WorkSpaceMasks[0];
    public GameObject[] CameraWorkSpaceOverlays;
    // Use this for initialization

    public static  WorkSpaceUI mInstance = null;
    private List<CvMat> WorkSpaceMasks;
    private List<CvRect> WorkSpaceRects;
    private bool initialized = false;
    

	void Start () {
     
	}

    // Update is called once per frame
    void Update()
    {
        if (!initialized)
        {
            bool ret = Init();
            if (ret) initialized = true;
            mInstance = this;
            return;
        }
        UpdateOverlay();

    }
    public int GetCurrentStep()
    {
        return CurrentScaffoldingStep;
    }
    private void UpdateOverlay()
    {
        for(int i=0;i<CameraWorkSpaceOverlays.Length;i++)
        {
            if(i==CurrentScaffoldingStep) CameraWorkSpaceOverlays[i].SetActive(true);
                else CameraWorkSpaceOverlays[i].SetActive(false);
        }
        
    }
    public void Next()
    {
        CurrentScaffoldingStep++;
        if (CurrentScaffoldingStep >= CameraWorkSpaceOverlays.Length) CurrentScaffoldingStep = -1;
    }
    public bool IsUIObjectwithinWS(GameObject o)
    {
        if (o == null) return false;
        Vector3 objRectPos = o.GetComponent<RectTransform>().localPosition; // in UI coordinate 1200/900
        //   Debug.Log("Feedback region pos: " + screenPosition);
        CvMat mask;
        CvRect rect;
        mask=GetWorkSpaceMask(out rect);
        if (mask == null) return true;
        CvPoint adjustedObjectPos = new CvPoint();
        adjustedObjectPos.X = (int) (objRectPos.x * mask.Width / 1200f );
        adjustedObjectPos.X += mask.Width / 2;
        adjustedObjectPos.Y = (int)(objRectPos.y * mask.Height / 900f);
        adjustedObjectPos.Y += mask.Height / 2;
        adjustedObjectPos.Y = mask.Height - adjustedObjectPos.Y;
        if (adjustedObjectPos.X < 0 || adjustedObjectPos.X >= mask.Width) return false;
        if (adjustedObjectPos.Y < 0 || adjustedObjectPos.Y >= mask.Height) return false;

        Debug.Log("hint region position: " + adjustedObjectPos);
        if (mask.Get2D(adjustedObjectPos.Y, adjustedObjectPos.X).Val0 <= 0) return true;

        return false;
        
    }
    public CvMat GetWorkSpaceMask(out CvRect rect)
    {
        if (CurrentScaffoldingStep < 0 || CameraWorkSpaceOverlays == null)
        {
            rect = new CvRect();
            return null;
        }
        if (CurrentScaffoldingStep >= WorkSpaceMasks.Count)
        {
            rect = WorkSpaceRects[WorkSpaceMasks.Count - 1];
            return WorkSpaceMasks[WorkSpaceMasks.Count - 1];
        }
        rect = WorkSpaceRects[CurrentScaffoldingStep];
        return WorkSpaceMasks[CurrentScaffoldingStep];
    }
    private bool Init()
    {
        CvRect regionBox = GlobalRepo.GetRegionBox(false);
        
            
        if (regionBox.Width <= 0 || regionBox.Height <= 0) return false;
       
        WorkSpaceMasks = new List<CvMat>(); //hard coding
        WorkSpaceRects = new List<CvRect>();
        CvMat FittedMask;
        CvRect FittedMaskRect;
        CvMat RawMask;

        RawMask = loadImage("Assets/2DAnimation/content_4/workspacelimit/WS1_lens_mask.png");
        this.WorkSpaceRect(RawMask, out FittedMask, out FittedMaskRect);
        WorkSpaceMasks.Add(FittedMask);
        WorkSpaceRects.Add(FittedMaskRect);
        RawMask = loadImage("Assets/2DAnimation/content_4/workspacelimit/WS2_shuttermask.png");
        this.WorkSpaceRect(RawMask, out FittedMask, out FittedMaskRect);
        WorkSpaceMasks.Add(FittedMask);
        WorkSpaceRects.Add(FittedMaskRect);
        RawMask = loadImage("Assets/2DAnimation/content_4/workspacelimit/WS3_sensormask.png");
        this.WorkSpaceRect(RawMask, out FittedMask, out FittedMaskRect);
        WorkSpaceMasks.Add(FittedMask);
        WorkSpaceRects.Add(FittedMaskRect);
                
        /*GlobalRepo.showDebugImage("sensor mask", WorkSpaceMasks[0]);
        GlobalRepo.showDebugImage("sensor mask2", WorkSpaceMasks[1]);
        GlobalRepo.showDebugImage("sensor mask3", WorkSpaceMasks[2]);*/
        return true;
    }
    private void WorkSpaceRect(CvMat mask,out CvMat fittedmask, out CvRect fittedmaskrect)
    {
        CvRect regionBox = GlobalRepo.GetRegionBox(false);
        CvMat WSFittoRegion = new CvMat(regionBox.Height, regionBox.Width, MatrixType.U8C1);
        mask.Resize(WSFittoRegion);
        WSFittoRegion.Threshold(WSFittoRegion, 200, 255, ThresholdType.BinaryInv);
     //   GlobalRepo.showDebugImage("raw", mask);
     //   GlobalRepo.showDebugImage("converted", WSFittoRegion);
        CvRect WSRect = BlobAnalysis.ExtractLargestBoundingBox(WSFittoRegion);
        
        fittedmask = WSFittoRegion;
        fittedmaskrect = WSRect;
        return;
    }
    private CvMat loadImage(string filename_)
    {
        CvMat newimg = new CvMat(filename_, LoadMode.Unchanged);
        CvMat newimageAlpha = new CvMat(newimg.Rows, newimg.Cols, MatrixType.U8C1);
        if (newimg.ElemType != MatrixType.U8C4)
        {
            Debug.Log("[DEBUG] failed to load [" + filename_ + "] wrong Mat format");
            return null;
        }
        //newimg = RGBA in general        
        newimg.CvtColor(newimageAlpha, ColorConversion.RgbaToGray);
        return newimageAlpha;


    }
    public void AddDefaultModels(CvMat img)
    {
        if (ApplicationControl.ActiveInstance.ContentType != DesignContent.CameraSystem || img==null) return;
        CvPoint lensCenter = new CvPoint(img.Width * 30 / 100, img.Height * 55 / 100);
        CvPoint shutterCenter = new CvPoint(img.Width * 66 / 100,img.Height*55/100);
        CvPoint sensorCenter = new CvPoint(img.Width * 83 / 100, img.Height*55/100);
        if (this.CurrentScaffoldingStep == 0)
        {
            CvMat shutter = GlobalRepo.GetTexture2D("c4_shutter_overlayModel").txtBGRAImg;
            CvMat sensor = GlobalRepo.GetTexture2D("c4_sensor_overlayModel").txtBGRAImg;

            ImageBlender2D.loadTexturewithAlphaSmallOverlay(img, shutter, shutterCenter - new CvPoint(shutter.Width / 2, shutter.Height / 2));
            ImageBlender2D.loadTexturewithAlphaSmallOverlay(img, sensor, sensorCenter - new CvPoint(sensor.Width / 2, sensor.Height / 2));
          

        }
        else if (this.CurrentScaffoldingStep == 1)
        {
            CvMat lens = GlobalRepo.GetTexture2D("c4_lens_overlayModel").txtBGRAImg;
            CvMat sensor = GlobalRepo.GetTexture2D("c4_sensor_overlayModel").txtBGRAImg;

            ImageBlender2D.loadTexturewithAlphaSmallOverlay(img, lens, lensCenter - new CvPoint(lens.Width / 2, lens.Height / 2));
            ImageBlender2D.loadTexturewithAlphaSmallOverlay(img, sensor, sensorCenter - new CvPoint(sensor.Width / 2, sensor.Height / 2));
        }
        else if (this.CurrentScaffoldingStep == 2)
        {
            CvMat lens = GlobalRepo.GetTexture2D("c4_lens_overlayModel").txtBGRAImg;
            CvMat shutter = GlobalRepo.GetTexture2D("c4_shutter_overlayModel").txtBGRAImg;
            ImageBlender2D.loadTexturewithAlphaSmallOverlay(img, lens, lensCenter - new CvPoint(lens.Width / 2, lens.Height / 2));
            ImageBlender2D.loadTexturewithAlphaSmallOverlay(img, shutter, shutterCenter - new CvPoint(shutter.Width / 2, shutter.Height / 2));
        }
    }
    public void AddDefaultParams(ref SimulationParam sp)
    {
        if (ApplicationControl.ActiveInstance.ContentType != DesignContent.CameraSystem) return;
        if(this.CurrentScaffoldingStep==0)
        {
            sp.C4_shutterSpeed = 300;
            sp.C4_sensorType = "Full Color";
        }
        else if (this.CurrentScaffoldingStep == 1)
        {
            sp.C4_focalLength = 100;
            sp.C4_sensorType = "Full Color";
        }
        else if (this.CurrentScaffoldingStep ==2)
        {
            sp.C4_focalLength = 100;
            sp.C4_shutterSpeed = 300;
            
        }
        
    }
}
