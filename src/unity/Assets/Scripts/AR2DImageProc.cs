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
public class AR2DImageProc : MonoBehaviour {


    [Tooltip("GUI-texture used to display the color camera feed on the scene background.")]
    public GUITexture RealityARImage;

    [Tooltip("GUI-texture used to display the color camera feed on the scene background.")]
    public GUITexture backgroundOverlay;

    [Tooltip("Camera that will be used to overlay the 3D-objects over the background.")]
    public Camera foregroundCamera;

    private Texture2D AR2DTexture = null;
    private Texture2D AR2DBGTexture = null;
    // Use this for initialization
   
    
   

    void Start () {
    
      


        //test

      /*  WebCamTexture webcamTexture = new WebCamTexture(1920, 1080, 30);
        backgroundImage.texture = webcamTexture;
        webcamTexture.Play();
        Color32[] data = new Color32[webcamTexture.width * webcamTexture.height];*/
        
    }
	
	// Update is called once per frame
	void Update () {
        if(GlobalRepo.RealityARActivated())  updateRealityAR();
      //  updateBGTexture();
        //Debug.Log("UUUUUUUUUUMODEUUUUUUUUUUUU : " + UserMode);
    }
    private void updateBTextureUnityAPI()
    {
        
    }
    
    
    private void updateRealityAR()
    {
        CvRect regionBox = GlobalRepo.GetRegionBox(false);
        if (regionBox.Width == 0 || GlobalRepo.getByteStream(RepoDataType.dRealityARRegionRGBAByte)==null) return;
        if (AR2DTexture == null || AR2DTexture.width != regionBox.Width || AR2DTexture.height != regionBox.Height && AR2DTexture.width!=regionBox.Width)
        {            
            AR2DTexture = new Texture2D(regionBox.Width, regionBox.Height, TextureFormat.RGBA32, false);
            
        }
        //AR2DTexture.LoadRawTextureData(this.colorImagePostProc);
        ImageBlender2D.RasterizeRealityAR(GlobalRepo.GetRepo(RepoDataType.pRealityARRegionRGBA));
      //  Debug.Log("AR2D TXT Size : " + AR2DTexture.width* AR2DTexture.height*4 + "," + GlobalRepo.getByteStream(RepoDataType.dARTxTRegionByteRGBA).Length);
        AR2DTexture.LoadRawTextureData(GlobalRepo.getByteStream(RepoDataType.dRealityARRegionRGBAByte));
        AR2DTexture.Apply();
        if (RealityARImage && (RealityARImage.texture == null))
        {
            //backgroundImage.texture = manager.GetUsersClrTex();
            RealityARImage.texture = AR2DTexture;
        }


        // get the background rectangle (use the portrait background, if available)
        /*   UnityEngine.Rect backgroundRect = foregroundCamera.pixelRect;
           PortraitBackground portraitBack = PortraitBackground.Instance;

           if (portraitBack && portraitBack.enabled)
           {
               backgroundRect = portraitBack.GetBackgroundRect();
           }*/
    }
}
public class ImageBlender2D{
  
    static CvMat localCanvasImg = null;
    static int localCanvasAge = 0;
    static CvWindow tt=null;

    static CvMat bwRegionImg = null;
    static CvMat HSVRegionImg = null;
    static CvMat LABRegionImg = null;
    static CvMat BGRRegionImg = null;
    static CvMat DebugBlobImge = null;
    static CvMat extractedImg = null;
    public static CvPoint debugPoint;
    public static List<CvPoint> pointPool;
    static float dominantH;
    static float dominantCount;
    static bool dominantHSaturated;
    static int debugCounter;
    static CvRect overLayBox;

    static int GrayHueValue = 205 / 2;
    static int HueClusterRange = 17;
  

    public static void RasterizeRealityAR(CvMat ARTxtRegionImg)
    {
        if (GlobalRepo.GetRegionBox(false).Width == 0) return;


        // ConvergeToWhite(ProccolorImage2, overlayRegion);

        ARTxtRegionImg.Zero();
        Visual2DModelManager v2d = GameObject.FindObjectOfType<Visual2DModelManager>();
        if (v2d != null)
        {
            v2d.exportObjectVisualization(ARTxtRegionImg);

        }        
        
        
        /*
        Vector3 centerp = GameObject.Find("sptest").GetComponent<SpriteRenderer>().bounds.center;
        //world 3D coordinate of LT
        
        Vector3 screenp = Camera.main.WorldToScreenPoint(centerp);
        Debug.Log("centerp " + centerp);
        Debug.Log("screenp " + screenp);
        ARTxtRegionImg.DrawCircle(SceneObjectManager.ScreentoRegion(screenp), 3, CvColor.Black, -1);
        centerp = GameObject.Find("sptest").GetComponent<SpriteRenderer>().bounds.min;
        screenp = Camera.main.WorldToScreenPoint(centerp);
        ARTxtRegionImg.DrawCircle(SceneObjectManager.ScreentoRegion(screenp), 3, CvColor.Black, -1);
        */
    }

    public static void ApplyBlending_OBSOLTE(byte[] rawsourceRGBA, byte[] ProcImgRGBA,int width, int height)
    {
        if (GlobalRepo.GetRegionBox(false).Width == 0) return;

        CvMat RawcolorImage2 = new CvMat(height, width, MatrixType.U8C4, rawsourceRGBA);
        CvMat ARTextureImg = new CvMat(height, width, MatrixType.U8C4, ProcImgRGBA);
       
       // ConvergeToWhite(ProccolorImage2, overlayRegion);
        if(GlobalRepo.isRegionSet())
        {
            CvMat ARTectureRegionImg;
            ARTectureRegionImg = ARTextureImg.GetSubRect(out ARTectureRegionImg, GlobalRepo.GetRegionBox(false));

            Visual2DModelManager v2d = GameObject.FindObjectOfType<Visual2DModelManager>();
            if(v2d!=null)
            {
                v2d.exportObjectVisualization(ARTectureRegionImg);
             
            }

        }
       /* Collect2DBlobs(RawcolorImage2, overlayRegion);
        if (dominantHSaturated && pointPool.Count == 0)
        {
            //extract image
            extractedImg = extractImage(DebugBlobImge);
            tt.ShowImage(extractedImg);
            testOverlayAnimation(ProccolorImage2,extractedImg);
            return;
        }*/
        
        //Cv.DrawRect(rawcolorImage2, overlayRegion, CvColor.Beige,10);
    }
    static int animclock = 0;
    private static void testOverlayAnimation(CvMat canvas, CvMat obj)
    {
       
        
    }
    public static  void startCollect2DBlobs(CvPoint pickPoint)
    {
        if (pointPool == null) pointPool = new List<CvPoint>();
        pointPool.Add(pickPoint);
        DebugBlobImge.Zero();
        dominantH = 0;
        dominantCount = 0;
        dominantHSaturated = false;
    }
    private static CvMat extractImage(CvMat blobOnlyImg)
    {
        CvRect bBox=new CvRect();
        CvPoint c = new CvPoint();
        CvMat ret =  BlobAnalysis.ExtractBoundedBlobImage(blobOnlyImg,ref bBox,ref c);
        overLayBox = bBox;
        return ret;

    }
    private static void Collect2DBlobs(CvMat img, CvRect region)
    {
        CvMat regionImg;
        img.GetSubArr(out regionImg, region);
        CvPoint probePoint;
        if (localCanvasAge < 400 || pointPool==null)
            return;
        if (pointPool.Count == 0)
            return;
        int winSize = 1;
        CvMat tempHSVImg;
        int thresh = 5;
        int debugT = 0;
        regionImg.CvtColor(HSVRegionImg, ColorConversion.RgbaToBgr);
        HSVRegionImg.CvtColor(HSVRegionImg, ColorConversion.BgrToHsv);
        int poolSize = Math.Min(pointPool.Count,200);
        
        if(dominantCount>200 && !dominantHSaturated)
        {   //color saturated
            dominantH = dominantH / dominantCount;
            dominantHSaturated = true;
            dominantCount = 0;
        }
        for (int i = 0; i < poolSize; i++)
        {
            CvScalar hsvColor = HSVRegionImg.Get2D( pointPool[i].Y, pointPool[i].X);
            if (!dominantHSaturated)
            {
                dominantH += ((float)hsvColor.Val0);
                dominantCount++;
            }
            
            CvPoint centerP = new CvPoint(pointPool[i].X, pointPool[i].Y);
            if (centerP.X >= HSVRegionImg.Width - winSize || centerP.X <= winSize || centerP.Y >= HSVRegionImg.Height - winSize || centerP.Y <= winSize) continue;
            //HSVRegionImg.GetSubArr(out tempHSVImg, new CvRect(pointPool[i].X - winSize / 2, pointPool[i].Y - winSize / 2, winSize, winSize));            
            
            for (int j = winSize * -1; j <= winSize; j++)
            {
                for (int k = winSize * -1; k <= winSize; k++)
                {
                    if (j == 0 && k == 0) continue;

                    if (DebugBlobImge.Get2D(pointPool[i].Y + j, pointPool[i].X + k).Val0 > 0) continue;
                    CvScalar sample = HSVRegionImg.Get2D(pointPool[i].Y + j, pointPool[i].X + k);
                    //Debug.Log("Refcolor" + hsvColor.Val0 + "\t nearColor" +sample.Val0);

                    if (!dominantHSaturated)
                    {
                        if (Math.Abs(hsvColor.Val0 - sample.Val0) < thresh)
                        {
                            DebugBlobImge.Set2D(pointPool[i].Y + j, pointPool[i].X + k, regionImg.Get2D(pointPool[i].Y + j, pointPool[i].X + k));
                            pointPool.Add(new CvPoint(pointPool[i].X + k, pointPool[i].Y + j));
                            debugT++;
                        }
                    } else
                    {
                       // Debug.Log("Refcolor" + dominantH + "\t nearColor" + sample.Val0);
                        if (Math.Abs(dominantH - sample.Val0) < thresh)
                        {
                            DebugBlobImge.Set2D(pointPool[i].Y + j, pointPool[i].X + k, regionImg.Get2D(pointPool[i].Y + j, pointPool[i].X + k));
                            pointPool.Add(new CvPoint(pointPool[i].X + k, pointPool[i].Y + j));
                            debugT++;
                        }
                    }

                }


            }
        }
        
        while (poolSize > 0)
        {
            poolSize--;
            pointPool.RemoveAt(0);
        }


        
        

        



    }
    public static void loadTexturewithAlphaSmallUnderlay_ignoreunderlayA(CvMat underlay, CvMat overlay, CvPoint leftTopinOverlay)
    {
        if (underlay == null || overlay == null) return;
        CvPoint overlayPos = leftTopinOverlay;


        for (int under_y = 0; under_y < underlay.Height; under_y++)
        {
            overlayPos.X = leftTopinOverlay.X;
            overlayPos.Y = leftTopinOverlay.Y + under_y;
            if (overlayPos.Y < 0 || overlayPos.Y > (overlay.Height - 1)) continue;
            for (int under_x = 0; under_x < underlay.Width; under_x++)
            {
                if (overlayPos.X < 0 || overlayPos.X > (overlay.Width - 1))
                {
                    overlayPos.X++;
                    continue;
                }

                CvScalar underlayPixel = underlay.Get2D(under_y, under_x);
                CvScalar overlayPixel = overlay.Get2D(overlayPos.Y, overlayPos.X);

                if (overlayPixel.Val3 > 0)
                {
                    //underlay.Set2D(under_y, under_x, overlayPixel);
                    underlay.Set2D(under_y, under_x, overlayPixel);

                }
                overlayPos.X++;

            }

        }

        /*   Debug.Log("[DEUBG-LOADTXT] underlay:" + underlay.Width + "," + underlay.Height);
           Debug.Log("[DEUBG-LOADTXT] overlay:" + overlay.Width + "," + overlay.Height);
           Debug.Log("[DEUBG-LOADTXT] overlaid pixels :"+t);
           Debug.Log("[DEUBG-LOADTXT] underlay pixels :" + undert);
           GlobalRepo.showDebugImage("underlayafter", underlay);
           GlobalRepo.showDebugImage("overlayafter", overlay);*/

    }
    public static void loadTexturewithAlphaSmallUnderlay(CvMat underlay, CvMat overlay, CvPoint leftTopinOverlay)
    {
        if (underlay == null || overlay == null) return;
        CvPoint overlayPos = leftTopinOverlay;
     
     
        for (int under_y = 0; under_y < underlay.Height; under_y++)
        {
            overlayPos.X = leftTopinOverlay.X;
            overlayPos.Y = leftTopinOverlay.Y + under_y;
            if (overlayPos.Y < 0 || overlayPos.Y > (overlay.Height - 1)) continue;
                for (int under_x = 0; under_x < underlay.Width; under_x++)
            {
                if (overlayPos.X < 0 || overlayPos.X > (overlay.Width - 1))
                {
                    overlayPos.X++;
                    continue;
                }

                CvScalar underlayPixel = underlay.Get2D(under_y, under_x);
                CvScalar overlayPixel = overlay.Get2D(overlayPos.Y, overlayPos.X);
         
                if (underlayPixel.Val3 > 0 && overlayPixel.Val3 > 0)
                {
                    //underlay.Set2D(under_y, under_x, overlayPixel);
                    underlay.Set2D(under_y, under_x, overlayPixel);
               
                }
                overlayPos.X++;

            }

        }

     /*   Debug.Log("[DEUBG-LOADTXT] underlay:" + underlay.Width + "," + underlay.Height);
        Debug.Log("[DEUBG-LOADTXT] overlay:" + overlay.Width + "," + overlay.Height);
        Debug.Log("[DEUBG-LOADTXT] overlaid pixels :"+t);
        Debug.Log("[DEUBG-LOADTXT] underlay pixels :" + undert);
        GlobalRepo.showDebugImage("underlayafter", underlay);
        GlobalRepo.showDebugImage("overlayafter", overlay);*/

    }
    public static void loadTexturewithAlphaSmallOverlay(CvMat underlay, CvMat overlay, CvPoint leftTopinUnderlay)
    {
        if (underlay == null || overlay == null) return;
        CvPoint underlayPos = leftTopinUnderlay;
        for (int over_y = 0; over_y < overlay.Height; over_y++)
        {
            underlayPos.X = leftTopinUnderlay.X;
            underlayPos.Y = leftTopinUnderlay.Y + over_y;
            for (int over_x = 0; over_x < overlay.Width; over_x++)
            {
                if (underlayPos.X < 0 || underlayPos.X > (underlay.Width - 1) || underlayPos.Y < 0 || underlayPos.Y > (underlay.Height - 1))
                {
                    underlayPos.X++;
                    continue;
                }
                    
                    CvScalar underlayPixel = underlay.Get2D(underlayPos.Y, underlayPos.X);
                CvScalar overlayPixel = overlay.Get2D(over_y, over_x);
                if (underlayPixel.Val3 > 0 && overlayPixel.Val3 > 0)
                {
                    underlay.Set2D(underlayPos.Y, underlayPos.X, overlayPixel);
                }
                underlayPos.X++;

            }
        }
    }
    public static void overlayImgRGBA(CvMat underlay, CvMat overlay, CvPoint leftTopinUnderlay)
    {

        if (underlay == null || overlay == null || underlay.Width < overlay.Width || underlay.Height < overlay.Height) return;
        
        
        CvPoint underlayPos = leftTopinUnderlay;
        for (int over_y = 0; over_y < overlay.Height; over_y++)
        {
            underlayPos.X = leftTopinUnderlay.X;
            underlayPos.Y = leftTopinUnderlay.Y + over_y;
            for (int over_x = 0; over_x < overlay.Width; over_x++)
            {
                if (underlayPos.X < 0 || underlayPos.X > (underlay.Width - 1) || underlayPos.Y < 0 || underlayPos.Y > (underlay.Height - 1))
                {
                    underlayPos.X++;
                    continue;
                }
                CvScalar overlayPixel = overlay.Get2D(over_y, over_x);

                if (overlayPixel.Val3 > 0)
                {
                    underlay.Set2D(underlayPos.Y, underlayPos.X, overlayPixel);
                }
                underlayPos.X++;
            }

        }
    }
    public static void AlphBlendingImgRGBA(CvMat underlay, CvMat overlay, CvPoint leftTopinUnderlay)
    {

        if (underlay == null || overlay == null || underlay.Width < overlay.Width || underlay.Height < overlay.Height) return;


        CvPoint underlayPos = leftTopinUnderlay;
        CvScalar overlayPixel, underlayPixel;
        Double scaledAlpha;
        for (int over_y = 0; over_y < overlay.Height; over_y++)
        {
            underlayPos.X = leftTopinUnderlay.X;
            underlayPos.Y = leftTopinUnderlay.Y + over_y;
            for (int over_x = 0; over_x < overlay.Width; over_x++)
            {
                if (underlayPos.X < 0 || underlayPos.X > (underlay.Width - 1) || underlayPos.Y < 0 || underlayPos.Y > (underlay.Height - 1))
                {
                    underlayPos.X++;
                    continue;
                }
                overlayPixel = overlay.Get2D(over_y, over_x);
                //   
                // underlayPixel = underlayPixel * (1 - scaledAlpha) + overlayPixel * scaledAlpha;               


                if (overlayPixel.Val3 > 0)
                {
                    underlayPixel = underlay.Get2D(underlayPos.Y, underlayPos.X);
                    scaledAlpha = overlayPixel.Val3 / 255;
                    overlayPixel.Val0 = underlayPixel.Val0 * (1 - scaledAlpha) + overlayPixel.Val0 * scaledAlpha;
                    overlayPixel.Val1 = underlayPixel.Val1 * (1 - scaledAlpha) + overlayPixel.Val1 * scaledAlpha;
                    overlayPixel.Val2 = underlayPixel.Val2 * (1 - scaledAlpha) + overlayPixel.Val2 * scaledAlpha;
                    overlayPixel.Val3 = 255;
                    underlay.Set2D(underlayPos.Y, underlayPos.X, overlayPixel);
                }
                underlayPos.X++;
            }

        }
    }
    public static void AlphBlendingImgRGBA(CvMat underlay, CvMat overlay, CvPoint leftTopinUnderlay,double fixedAlpha)
    {

        if (underlay == null || overlay == null || underlay.Width < overlay.Width || underlay.Height < overlay.Height)
        {
            Debug.Log("[ERROR] alphablending");
            return;
        }


        CvPoint underlayPos = leftTopinUnderlay;
        CvScalar overlayPixel, underlayPixel;
        Double scaledAlpha = fixedAlpha/(double)255;
        int dbgCount = 0;
        for (int over_y = 0; over_y < overlay.Height; over_y++)
        {
            underlayPos.X = leftTopinUnderlay.X;
            underlayPos.Y = leftTopinUnderlay.Y + over_y;
            for (int over_x = 0; over_x < overlay.Width; over_x++)
            {
                if (underlayPos.X < 0 || underlayPos.X > (underlay.Width - 1) || underlayPos.Y < 0 || underlayPos.Y > (underlay.Height - 1))
                {
                    underlayPos.X++;
                    continue;
                }
                overlayPixel = overlay.Get2D(over_y, over_x);
                //   
                // underlayPixel = underlayPixel * (1 - scaledAlpha) + overlayPixel * scaledAlpha;               


                if (overlayPixel.Val3 > 0)
                {
                    underlayPixel = underlay.Get2D(underlayPos.Y, underlayPos.X);                   
                    overlayPixel.Val0 = underlayPixel.Val0 * (1 - scaledAlpha) + overlayPixel.Val0 * scaledAlpha;
                    overlayPixel.Val1 = underlayPixel.Val1 * (1 - scaledAlpha) + overlayPixel.Val1 * scaledAlpha;
                    overlayPixel.Val2 = underlayPixel.Val2 * (1 - scaledAlpha) + overlayPixel.Val2 * scaledAlpha;
                    overlayPixel.Val3 = underlayPixel.Val3;
                    //overlayPixel.Val3 = 255;
                    underlay.Set2D(underlayPos.Y, underlayPos.X, overlayPixel);
                    dbgCount++;
                }
                underlayPos.X++;
            }

        }
        Debug.Log("[DEBUG] Alpha Blending Count = " + dbgCount);

    }
    public static void overlayImgFrameAlphaImgRGBA(CvMat underlay, CvMat overlay, CvPoint leftTopinUnderlay, double overlayAlpha)
    {

        if (underlay == null || overlay == null || underlay.Width < overlay.Width || underlay.Height < overlay.Height) return;


        CvPoint underlayPos = leftTopinUnderlay;
        CvScalar overlayPixel,underlayPixel;
        Double scaledAlpha = overlayAlpha / 255;
        for (int over_y = 0; over_y < overlay.Height; over_y++)
        {
            underlayPos.X = leftTopinUnderlay.X;
            underlayPos.Y = leftTopinUnderlay.Y + over_y;
            for (int over_x = 0; over_x < overlay.Width; over_x++)
            {
                if (underlayPos.X < 0 || underlayPos.X > (underlay.Width - 1) || underlayPos.Y < 0 || underlayPos.Y > (underlay.Height - 1))
                {
                    underlayPos.X++;
                    continue;
                }
                overlayPixel = overlay.Get2D(over_y, over_x);
             //   underlayPixel = underlay.Get2D(underlayPos.Y, underlayPos.X);
                // underlayPixel = underlayPixel * (1 - scaledAlpha) + overlayPixel * scaledAlpha;               
                
                
                if (overlayPixel.Val3 > 0)
                {
                    overlayPixel.Val3 = overlayPixel.Val3/ 255.0f * overlayAlpha;
                    underlay.Set2D(underlayPos.Y, underlayPos.X, overlayPixel);
                }
                underlayPos.X++;
            }

        }
    }
    private static void ConvergeToWhite(CvMat img, CvRect region)
    {
        CvMat regionImg;        
        CvScalar targetPoint;
        CvScalar randomRef;
        CvScalar stackRef;
        int rndIndex=-1;
        System.Random rnd = new System.Random();
        img.GetSubArr(out regionImg, region);
        //Cv.DrawRect(img, region, CvColor.Red);
        
        if (bwRegionImg==null) bwRegionImg = new CvMat(regionImg.Rows, regionImg.Cols, MatrixType.U8C1);
        if(HSVRegionImg == null) HSVRegionImg = new CvMat(regionImg.Rows, regionImg.Cols, MatrixType.U8C3);
        if (DebugBlobImge == null) DebugBlobImge = new CvMat(regionImg.Rows, regionImg.Cols, MatrixType.U8C4);
        if (localCanvasAge > 100)
        {
            buildLocalCanvas(regionImg);
            localCanvasImg.Copy(regionImg);
            return;
        }
        //regionImg.CvtColor(bwRegionImg, ColorConversion.RgbaToGray);
        regionImg.CvtColor(HSVRegionImg, ColorConversion.RgbaToBgr);
        HSVRegionImg.CvtColor(HSVRegionImg, ColorConversion.BgrToHsv);
        // CvMat objectMask = new CvMat(regionImg.Rows, regionImg.Cols, MatrixType.U8C1);
        //objectMask.Zero();
        CvScalar t = new CvScalar(212, 225, 214);
        

        for (int i = 0; i < regionImg.Width * regionImg.Height; i++)
        {
            
            targetPoint = HSVRegionImg.Get1D(i);
            if(targetPoint.Val1>20)
            {
                int q = 10;
                do
                {
                    if (rndIndex >= 0)
                    {
                        rndIndex++;
                        if (rndIndex >= regionImg.Width * regionImg.Height) rndIndex--;
                        randomRef = HSVRegionImg.Get1D(rndIndex);
                    }
                    else
                    {
                        rndIndex = rnd.Next(Math.Max(0, i - regionImg.Step * 30), Math.Min(regionImg.Width * regionImg.Height - 1, i + regionImg.Step * 30));
                        randomRef = HSVRegionImg.Get1D(rndIndex);
                    }
               //     q--;
                } while (randomRef.Val1>20 && q>10);
                
                regionImg.Set1D(i, regionImg.Get1D(rndIndex));
             //   objectMask.Set1D(i, CvColor.White);
               // regionImg.Set1D(i, t);
            }   else
            {
                rndIndex = -1;
            }
        }
        buildLocalCanvas(regionImg);
     //   tt.ShowImage(objectMask);
        

    }
    private static void buildLocalCanvas(CvMat curimg)
    {
        if (LABRegionImg == null) LABRegionImg = new CvMat(curimg.Rows, curimg.Cols, MatrixType.U8C3);
        if (BGRRegionImg == null) BGRRegionImg = new CvMat(curimg.Rows, curimg.Cols, MatrixType.U8C3);
        if (localCanvasImg == null) localCanvasImg = curimg.Clone();
        int rndIndex = -1;
        System.Random rnd = new System.Random();
        CvScalar randomRef;
        int LabThresh_L = 86 * 255 / 100;
        int LabThresh_L2 = 92 * 255 / 100;
       
        CvMat objectMask = new CvMat(curimg.Rows, curimg.Cols, MatrixType.U8C1);
        objectMask.Zero();
        if (localCanvasAge>=400)
        {
            //do nothing
        }
        else if (localCanvasAge >= 200)
        {
            bwRegionImg.Zero();
            localCanvasImg.CvtColor(BGRRegionImg, ColorConversion.RgbaToBgr);
            BGRRegionImg.CvtColor(LABRegionImg, ColorConversion.BgrToHsv);
            //localCanvasImg.CvtColor(LABRegionImg, ColorConversion.RgbToLab);

            //evolve by referring itself
            int evolveIterationSize = 0;
            CvScalar targetPoint;
            for (int i = 0; i < LABRegionImg.Width * LABRegionImg.Height && evolveIterationSize < 500; i++)
            {
                targetPoint = LABRegionImg.Get1D(i);
                

                if (Math.Abs(targetPoint.Val0 - GrayHueValue)> HueClusterRange) //need to change this to adapt
                {
                    //     Debug.Log("!!!!");
                    do
                    {
                        if (rndIndex >= 0)
                        {
                            rndIndex++;
                            if (rndIndex >= LABRegionImg.Width * LABRegionImg.Height) rndIndex--;
                            randomRef = LABRegionImg.Get1D(rndIndex);
                        }
                        else
                        {
                            rndIndex = rnd.Next(Math.Max(0, i - LABRegionImg.Step * 30), Math.Min(LABRegionImg.Width * LABRegionImg.Height - 1, i + LABRegionImg.Step * 30));
                            randomRef = LABRegionImg.Get1D(rndIndex);
                        }
                    } while (Math.Abs(randomRef.Val0 - GrayHueValue) > HueClusterRange);
                    objectMask.Set1D(i, CvColor.White);
                    localCanvasImg.Set1D(i, localCanvasImg.Get1D(rndIndex));
              

                    evolveIterationSize++;
                }
                else
                {
                    rndIndex = -1;
                }


            }
            // tt.ShowImage(bwRegionImg);
            Debug.Log("FIXED H" + evolveIterationSize);
            if (evolveIterationSize == 0)
            {
                localCanvasAge = 1000;
            }
            localCanvasImg.Smooth(localCanvasImg, SmoothType.Gaussian);
        }
        else if (localCanvasAge > 100)
        {
            bwRegionImg.Zero();
            localCanvasImg.CvtColor(BGRRegionImg, ColorConversion.RgbaToBgr);
            BGRRegionImg.CvtColor(LABRegionImg, ColorConversion.BgrToLab);
            //localCanvasImg.CvtColor(LABRegionImg, ColorConversion.RgbToLab);

            //evolve by referring itself
            int evolveIterationSize = 0;
            CvScalar targetPoint;
            for (int i = 0; i < LABRegionImg.Width * LABRegionImg.Height && evolveIterationSize<500; i++)
            {
                targetPoint = LABRegionImg.Get1D(i);
                CvScalar ttt = localCanvasImg.Get1D(i);
               
                if (targetPoint.Val0 < LabThresh_L) //need to change this to adapt
                {
               //     Debug.Log("!!!!");
                    do
                    {
                        if (rndIndex >= 0)
                        {
                            rndIndex++;
                            if (rndIndex >= LABRegionImg.Width * LABRegionImg.Height) rndIndex--;
                            randomRef = LABRegionImg.Get1D(rndIndex);
                        }
                        else
                        {
                            rndIndex = rnd.Next(Math.Max(0, i - LABRegionImg.Step * 30), Math.Min(LABRegionImg.Width * LABRegionImg.Height - 1, i + LABRegionImg.Step * 30));
                            randomRef = LABRegionImg.Get1D(rndIndex);
                        }
                    } while (randomRef.Val0 < LabThresh_L);

                    localCanvasImg.Set1D(i, localCanvasImg.Get1D(rndIndex));
               
                    
                    evolveIterationSize++;
                }                 else
                {
                    rndIndex = -1;
                }
               

            }
        
            Debug.Log("FIXED L" + evolveIterationSize);
            if(evolveIterationSize==0)
            {
                localCanvasAge = 200;
            }
            localCanvasImg.Smooth(localCanvasImg, SmoothType.Gaussian);
        }
        else
        { //stack up image
            localCanvasImg.AddWeighted(0.9f, curimg, 0.1f, 0, localCanvasImg);
        }        
        localCanvasAge++;
    }
    
}