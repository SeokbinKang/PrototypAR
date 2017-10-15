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
using System;
using System.Runtime.InteropServices;
public class GlobalRepo
{
    private static CvSize RawSize;
    private static CvRect RegionBoxYFlip;
    private static CvRect RegionBoxRaw;
    private static Dictionary<RepoDataType, CvMat> repoDict=null;
    private static Dictionary<RepoDataType, Byte[]> repoDictByteStream = null;
    private static Dictionary<ModelCategory, Asset2DTexture> asset2DDict = null;
    private static Dictionary<string, Asset2DTexture> ExtraAsset2DlDict = null;
    private static Dictionary<string, CvWindow> CvWindowDict = null;

    private static int TasksDone;
    private static bool beInit=false;
    private static int learningCount = 0;

    private static CvMat lastdRawRegionRGBA=null;

    private static bool setting_suppressCVWins;
    private static bool setting_showdebugimg;
    //initialize with a size
    public enum UserPhase : int { none,design, feedback, simulation }
    //design : live stream. BG overlayed
    //feedback : last image still but update background texture. BG become more transparent
    //simulation : BG still but do not update background texture
    public static UserPhase UserMode = UserPhase.design;

    [DllImport("msvcrt.dll", EntryPoint = "memcpy")]
    public static extern void CopyMemory(IntPtr dest, IntPtr src, uint count);
   

    public static void initRepo()
    {
        if (beInit) return;
        repoDict = new Dictionary<RepoDataType, CvMat>();
        repoDict.Add(RepoDataType.dRawRGBA, null);
        repoDict.Add(RepoDataType.dRawRegionRGBA, null);        
        repoDict.Add(RepoDataType.dRawRegionBGR, null);
        repoDict.Add(RepoDataType.dRawRegionHSV, null);
        repoDict.Add(RepoDataType.dRawRegionLAB, null);
        repoDictByteStream = new Dictionary<RepoDataType, byte[]>();
        repoDictByteStream.Add(RepoDataType.dRealityARRegionRGBAByte, null);
        
        RawSize.Width = 1920;
        RawSize.Height = 1080;
        TasksDone = 0;
        load2DAssetTexture();
        UserMode = UserPhase.design;
        beInit = true;
    }
    public static void reset()
    {
        TasksDone = 0;
    }
    public static bool isInit()
    {
        return beInit;
    }
    public static Byte[] getByteStream(RepoDataType type)
    {
        Byte[] ret = null;
        if (!repoDictByteStream.ContainsKey(type)) return ret;
        return repoDictByteStream[type];
    }
    public static Asset2DTexture GetTexture2D(ModelCategory type)
    {
        if (!asset2DDict.ContainsKey(type)) return null;
        return asset2DDict[type];
    }
    public static Asset2DTexture GetTexture2D(string asset_name)
    {
        if(!ExtraAsset2DlDict.ContainsKey(asset_name)) return null;
        return ExtraAsset2DlDict[asset_name];
    }
    public static CvScalar GetModelLineColor(ModelCategory type)
    {
        if (type == ModelCategory.LungLeft || type == ModelCategory.LungRight) return new CvScalar(114, 143, 17, 255);
        if (type == ModelCategory.RearSprocket ) return new CvScalar(146, 132, 0, 255);

        return new Scalar(0, 0, 0, 255);
    }
    public static void suppressCVWindows(bool t)
    {
        setting_suppressCVWins = t;
    }
    public static void Setting_ShowDebugImgs(bool t)
    {
        setting_showdebugimg = t;
    }
    public static bool Setting_ShowDebugImgs()
    {
        return setting_showdebugimg;
    }
    private static void load2DAssetTexture()
    {
        asset2DDict = new Dictionary<ModelCategory, Asset2DTexture>();


        //asset2DDict.Add(ModelCategory.LungLeft, new Asset2DTexture("Assets/Organ2D/lung_left.png"));
        //asset2DDict.Add(ModelCategory.LungRight, new Asset2DTexture("Assets/Organ2D/lung_right.png"));
        //asset2DDict.Add(ModelCategory.chainring, new Asset2DTexture("Assets/Organ2D/chainring2.png"));

        //content 1
        asset2DDict.Add(ModelCategory.LungLeft, new Asset2DTexture("Assets/2DAnimation/content_1/leftlung/leftlung_v1.png"));
        asset2DDict.Add(ModelCategory.LungRight, new Asset2DTexture("Assets/2DAnimation/content_1/rightlung/rightlung_v1.png"));
        asset2DDict.Add(ModelCategory.Diaphragm, new Asset2DTexture("Assets/2DAnimation/content_1/diaphragm/diaphragm_v0.png"));
        asset2DDict.Add(ModelCategory.Airways, new Asset2DTexture("Assets/2DAnimation/content_1/trachea/trachea_v1.png"));

        //content 2
        asset2DDict.Add(ModelCategory.RearSprocket, new Asset2DTexture("Assets/2DAnimation/content_2/chainring/charing_shapebase.png"));
        asset2DDict.Add(ModelCategory.FrontChainring, new Asset2DTexture("Assets/2DAnimation/content_2/chainring/charing_shapebase.png"));



        //common asset
        ExtraAsset2DlDict = new Dictionary<string, Asset2DTexture>();

        ExtraAsset2DlDict.Add("finger_1", new Asset2DTexture("Assets/2DObjects/finger1.png",40));

        ExtraAsset2DlDict.Add("suggestionBox_1", new Asset2DTexture("Assets/2DObjects/SuggestionBox1.png", 100));
        ExtraAsset2DlDict["suggestionBox_1"].AnchorPointRelative = new CvPoint(100, 100);
        ExtraAsset2DlDict["suggestionBox_1"].presetOverlayRegion = new CvRect(3, 23, 200, 200);

        ExtraAsset2DlDict.Add("pen_1", new Asset2DTexture("Assets/2DObjects/pen1.png", 35));
        ExtraAsset2DlDict["pen_1"].AnchorPointRelative = new CvPoint(4,99);


        ExtraAsset2DlDict.Add("text_missingbehavior1", new Asset2DTexture("Assets/2DObjects/text_missingbehavior.png", 30));
        ExtraAsset2DlDict["text_missingbehavior1"].AnchorPointRelative = new CvPoint(50, 100);

        ExtraAsset2DlDict.Add("icon_incorrect1", new Asset2DTexture("Assets/2DObjects/icon_incorrect1.png", 20));
        ExtraAsset2DlDict["icon_incorrect1"].AnchorPointRelative = new CvPoint(50, 50);

        ExtraAsset2DlDict.Add("bg_content1", new Asset2DTexture("Assets/2DAnimation/content_1/bg/bg1d.png", 80));
        ExtraAsset2DlDict["bg_content1"].AnchorPointRelative = new CvPoint(60, 27);


        //load animation assets
        ExtraAsset2DlDict.Add("icon_scissor", new Asset2DTexture("Assets/2DAnimation/common_feedback/3_shapeSTR/scissor", 30,true, new CvPoint(53, 47)));
        ExtraAsset2DlDict.Add("icon_finger", new Asset2DTexture("Assets/2DAnimation/common_feedback/4_posSTR/finger", 30, true, new CvPoint(37, 74)));
        ExtraAsset2DlDict.Add("icon_pen", new Asset2DTexture("Assets/2DAnimation/common_feedback/5_connSTR/pen", 60, true, new CvPoint(26, 98)));
        ExtraAsset2DlDict.Add("icon_eraser", new Asset2DTexture("Assets/2DAnimation/common_feedback/5_connSTR/eraser", 60, true, new CvPoint(10, 99)));
        
    }
    public static void showDebugImage(string window_name, CvMat t)
    {
        if (t == null || setting_suppressCVWins) return;
        if (CvWindowDict == null) CvWindowDict = new Dictionary<string, CvWindow>();
        if(!CvWindowDict.ContainsKey(window_name))
        {
            CvWindowDict.Add(window_name, new CvWindow(window_name));
        }

        CvWindowDict[window_name].ShowImage(t);
    }
    public static void closeDebugWindow()
    {
        if (CvWindowDict == null) return;
        foreach(var win in CvWindowDict)
        {
            win.Value.Close();
        }
    }
    
    public static CvWindow getDebugWindow(string window_name)
    {
        if (window_name =="") return null;
        if (CvWindowDict == null) CvWindowDict = new Dictionary<string, CvWindow>();
        if (!CvWindowDict.ContainsKey(window_name))
        {
            CvWindowDict.Add(window_name, new CvWindow(window_name));
        }

        return CvWindowDict[window_name];
    }
    public static bool isProcessingFinished()
    {
        if (TasksDone>0) return true;
        return false;
    }
    public static void ProcessingDone()
    {
        TasksDone++;
    }
    public static bool isRegionSet()
    {
        if (RegionBoxRaw == null || RegionBoxRaw.Width==0) return false;
        return true;
    }
    public static CvRect GetRegionBox(bool Yflip)
    {
        if (Yflip) return RegionBoxYFlip;
        else return RegionBoxRaw;
    }
    public static void setRegionBox(CvRect regionBoxYFlip_)
    {
        /*RegionBoxYFlip = regionBoxYFlip_;
        RegionBoxRaw = regionBoxYFlip_;
        RegionBoxRaw.X = -1 * (RegionBoxYFlip.X + RegionBoxYFlip.Width - RawSize.Width / 2) + RawSize.Width / 2;*/
        RegionBoxRaw = regionBoxYFlip_;
        RegionBoxYFlip = regionBoxYFlip_;
        RegionBoxYFlip.X = -1 * (RegionBoxYFlip.X + RegionBoxYFlip.Width - RawSize.Width / 2) + RawSize.Width / 2;
    }
    public static CvPoint translateRelativePointToAbsoluteinRegion(CvPoint a)
    {
        CvPoint ret = new CvPoint(a.X * RegionBoxRaw.Width / 100, a.Y * RegionBoxRaw.Height / 100);
        return ret;
    }
    public static CvPoint YFlip2D(CvPoint p)
    {
        return new CvPoint(-1 * (p.X - RawSize.Width / 2) + RawSize.Width / 2, p.Y);
    }
    public static CvPoint YFlip2DinROI(CvPoint p)
    {
        int RoiWith = repoDict[RepoDataType.dRawRegionRGBA].Width;
        return new CvPoint(-1 * (p.X - RoiWith / 2) + RoiWith / 2, p.Y);
    }
    public static void updateRepo(RepoDataType dataName, CvMat img)
    {
        if (isProcessingFinished()) return;
        if (dataName < RepoDataType.borderPointerbetweenData)
        { // shallow copy
            repoDict[dataName] = img;
            if (RepoDataType.dRawRGBA==dataName) RawSize = img.GetSize();
        } else
        { // deep copy
            if(!repoDict.ContainsKey(dataName) || repoDict[dataName]==null || repoDict[dataName].GetSize() != img.GetSize())
            {  //create new memory space and copy 
                if(dataName == RepoDataType.dContentBGRGBA)
                {
                    repoDictByteStream[RepoDataType.dContentBGRGBAByte] = new byte[img.Step * img.Height];
                    repoDict[dataName] = new CvMat(img.Height, img.Width, MatrixType.U8C4, repoDictByteStream[RepoDataType.dContentBGRGBAByte]);
                    Marshal.Copy(img.DataArrayByte.Ptr, repoDictByteStream[RepoDataType.dContentBGRGBAByte], 0, img.Step * img.Height);
                } else repoDict[dataName] = img.Clone();
            } else
            { // coppy the data
                img.Copy(repoDict[dataName]);
            }
        }       
    }
    
    public static void updateRepo(RepoDataType dataName, IntPtr datastrea, int w, int h, int c)
    {
        if (isProcessingFinished()) return;
        if (!repoDict.ContainsKey(dataName) || repoDict[dataName] == null || repoDict[dataName].Step* repoDict[dataName].Height != w*h*c)
        {  //create new memory space and copy 

            if(c==3)  repoDict[dataName] = new CvMat(h, w, MatrixType.U8C3);
            if (c == 4) repoDict[dataName] = new CvMat(h, w, MatrixType.U8C4);
        }
        CopyMemory(repoDict[dataName].Data, datastrea, (uint)(w * h * c));        

    }
    public static void updateRepoRaw(RepoDataType dataName, byte[] datastrea, int w, int h, int c)
    {
        if (isProcessingFinished()) return;
        if (!repoDict.ContainsKey(dataName) || repoDict[dataName] == null || repoDict[dataName].Step * repoDict[dataName].Height != w * h * c)
        {  //create new memory space and copy 

            if (c == 3) repoDict[dataName] = new CvMat(h, w, MatrixType.U8C3);
            if (c == 4) repoDict[dataName] = new CvMat(h, w, MatrixType.U8C4);
        }
        GCHandle pinnedArray = GCHandle.Alloc(datastrea, GCHandleType.Pinned);
        IntPtr pointer = pinnedArray.AddrOfPinnedObject();
        // Do your stuff...
        CopyMemory(repoDict[dataName].Data, pointer, (uint)(w * h * c));
        pinnedArray.Free();
        RawSize = repoDict[dataName].GetSize();
       // repoDict[dataName].Flip(repoDict[dataName], FlipMode.Y);



    }

    public static int getLearningCount()
    {
        return learningCount;
    }
    public static int setLearningCount(int k)
    {
        learningCount = k;
        return learningCount;
    }
    public static void tickLearningCount()
    {
        if (learningCount > 0) learningCount--;
    }
    public static CvMat GetRepo(RepoDataType dataName)
    {
        if (repoDict.ContainsKey(dataName))
            return repoDict[dataName];
        return null;
     
    }

    public static void updateInternalRepo(bool NeedImgProcessing)
    {  //this should be called after "pRawRGBA" and "pOverlayRGBA" are updated

        /*
        if (isProcessingFinished())
        {
            if(AR2DImageProc.NeedLiveStream())
            {
                if (!repoDict.ContainsKey(RepoDataType.dRawRGBA) || repoDict[RepoDataType.dRawRGBA] == null || repoDict[RepoDataType.dRawRGBA].GetSize() != repoDict[RepoDataType.dRawBGR].GetSize())
                {
                    repoDict[RepoDataType.dRawRGBA] = new CvMat(repoDict[RepoDataType.dRawBGR].Rows, repoDict[RepoDataType.dRawBGR].Cols, MatrixType.U8C4);
                }
                repoDict[RepoDataType.dRawBGR].CvtColor(repoDict[RepoDataType.dRawRGBA], ColorConversion.BgrToRgba);

                if (!repoDictByteStream.ContainsKey(RepoDataType.dARTxTRegionByteRGBA) || repoDictByteStream[RepoDataType.dARTxTRegionByteRGBA] == null || repoDictByteStream[RepoDataType.dARTxTRegionByteRGBA].Length != repoDict[RepoDataType.dRawRegionRGBA].Step * repoDict[RepoDataType.dRawRegionRGBA].Height)
                {
                    repoDictByteStream[RepoDataType.dARTxTRegionByteRGBA] = new byte[repoDict[RepoDataType.dRawRegionRGBA].Step * repoDict[RepoDataType.dRawRegionRGBA].Height];
                    repoDict[RepoDataType.pARTxTRegionRGBA] = new CvMat(RegionBoxRaw.Height, RegionBoxRaw.Width, MatrixType.U8C4, repoDictByteStream[RepoDataType.dARTxTRegionByteRGBA]);
                    Debug.Log("dARTxTRegionByteRGBA created size:" + repoDict[RepoDataType.dRawRegionRGBA].Step + " x " + repoDict[RepoDataType.dRawRegionRGBA].Height);
                    Debug.Log("Region Box : " + RegionBoxRaw.Width + " , " + RegionBoxRaw.Height);
                }
                Marshal.Copy(repoDict[RepoDataType.dRawRegionRGBA].DataArrayByte.Ptr, repoDictByteStream[RepoDataType.dARTxTRegionByteRGBA], 0, repoDict[RepoDataType.dRawRegionRGBA].Step * repoDict[RepoDataType.dRawRegionRGBA].Height);

                return;
            }
            if (AR2DImageProc.NeedBackupStream())
            {
                Marshal.Copy(repoDict[RepoDataType.dRawRegionRGBA].DataArrayByte.Ptr, repoDictByteStream[RepoDataType.dARTxTRegionByteRGBA], 0, repoDict[RepoDataType.dRawRegionRGBA].Step * repoDict[RepoDataType.dRawRegionRGBA].Height);
            }
            //put bg to background.
        }*/
            
        //set region
        /*
         pARTxTRegionRGBA,
        borderPointerbetweenData,
        dRawRGBA,  OK
        dRawRGB,   OK
        dRawRegionRGBA, OK
        dRawRegionBGR, OK
        dRawRegionHSV, ok
        dRawRegionLAB, ok
        dRawRegionGray,ok
        dARTxTRegionByteRGBA, ok*/
        CvMat tmp;
      
        if (!repoDict.ContainsKey(RepoDataType.dRawRGBA) || repoDict[RepoDataType.dRawRGBA] == null || repoDict[RepoDataType.dRawRGBA].GetSize() != repoDict[RepoDataType.dRawBGR].GetSize())
        {
        
            repoDict[RepoDataType.dRawRGBA] = new CvMat(repoDict[RepoDataType.dRawBGR].Rows, repoDict[RepoDataType.dRawBGR].Cols, MatrixType.U8C4);
        }
        repoDict[RepoDataType.dRawBGR].CvtColor(repoDict[RepoDataType.dRawRGBA], ColorConversion.BgrToRgba);

        if (!repoDict.ContainsKey(RepoDataType.dRawRegionRGBA) || repoDict[RepoDataType.dRawRegionRGBA] == null || repoDict[RepoDataType.dRawRegionRGBA].GetSize() != RegionBoxRaw.Size)
        {
         //   repoDict[RepoDataType.dRawRegionRGBA] = new CvMat(RegionBoxRaw.Height, RegionBoxRaw.Width, MatrixType.U8C4);
            repoDictByteStream[RepoDataType.dRawRegionRGBAByte] = new byte[RegionBoxRaw.Height* RegionBoxRaw.Width*4];
            repoDict[RepoDataType.dRawRegionRGBA] = new CvMat(RegionBoxRaw.Height, RegionBoxRaw.Width, MatrixType.U8C4, repoDictByteStream[RepoDataType.dRawRegionRGBAByte]);
           

        }
        repoDict[RepoDataType.dRawRGBA].GetSubArr(out tmp, RegionBoxRaw);
        tmp.Copy(repoDict[RepoDataType.dRawRegionRGBA]);

        if (NeedImgProcessing)            
            {
            //     if (!repoDict.ContainsKey(RepoDataType.dRawRegionBGRA) || repoDict[RepoDataType.dRawRegionBGRA] == null || repoDict[RepoDataType.dRawRegionRGBA].GetSize() != RegionBoxRaw.Size)
            //     {
            //         Debug.Log("create1");
            //         repoDict[RepoDataType.dRawRegionBGRA] = new CvMat(RegionBoxRaw.Height, RegionBoxRaw.Width, MatrixType.U8C4);
            //     }
            // //    repoDict[RepoDataType.dRawRegionRGBA].CvtColor(repoDict[RepoDataType.dRawRegionBGRA], ColorConversion.RgbaToBgra);


            if (!repoDict.ContainsKey(RepoDataType.dRawRegionBGR) || repoDict[RepoDataType.dRawRegionBGR] == null || repoDict[RepoDataType.dRawRegionBGR].GetSize() != RegionBoxRaw.Size)
            {
           
              repoDict[RepoDataType.dRawRegionBGR] = new CvMat(RegionBoxRaw.Height,RegionBoxRaw.Width, MatrixType.U8C3);
            }
            // repoDict[RepoDataType.dRawRegionRGBA].CvtColor(repoDict[RepoDataType.dRawRegionBGR], ColorConversion.RgbaToBgr);
               repoDict[RepoDataType.dRawBGR].GetSubArr(out tmp, RegionBoxRaw);
                 tmp.Copy(repoDict[RepoDataType.dRawRegionBGR]);

            if (!repoDict.ContainsKey(RepoDataType.dRawRegionHSV) || repoDict[RepoDataType.dRawRegionHSV] == null || repoDict[RepoDataType.dRawRegionHSV].GetSize() != repoDict[RepoDataType.dRawRegionRGBA].GetSize())
            {
              
                repoDict[RepoDataType.dRawRegionHSV] = new CvMat(repoDict[RepoDataType.dRawRegionRGBA].Rows, repoDict[RepoDataType.dRawRegionRGBA].Cols, MatrixType.U8C3);
            }
              repoDict[RepoDataType.dRawRegionBGR].CvtColor(repoDict[RepoDataType.dRawRegionHSV], ColorConversion.BgrToHsv);


            if (!repoDict.ContainsKey(RepoDataType.dRawRegionLAB) || repoDict[RepoDataType.dRawRegionLAB] == null || repoDict[RepoDataType.dRawRegionLAB].GetSize() != repoDict[RepoDataType.dRawRegionRGBA].GetSize())
            {
              
                repoDict[RepoDataType.dRawRegionLAB] = new CvMat(repoDict[RepoDataType.dRawRegionRGBA].Rows, repoDict[RepoDataType.dRawRegionRGBA].Cols, MatrixType.U8C3);
            }
            //   repoDict[RepoDataType.dRawRegionBGR].CvtColor(repoDict[RepoDataType.dRawRegionLAB], ColorConversion.BgrToLab);

            if (!repoDict.ContainsKey(RepoDataType.dRawRegionGray) || repoDict[RepoDataType.dRawRegionGray] == null || repoDict[RepoDataType.dRawRegionGray].GetSize() != repoDict[RepoDataType.dRawRegionRGBA].GetSize())
            {
              
                repoDict[RepoDataType.dRawRegionGray] = new CvMat(repoDict[RepoDataType.dRawRegionRGBA].Rows, repoDict[RepoDataType.dRawRegionRGBA].Cols, MatrixType.U8C1);
            }
            repoDict[RepoDataType.dRawRegionBGR].CvtColor(repoDict[RepoDataType.dRawRegionGray], ColorConversion.BgrToGray);
        }

        if (!repoDictByteStream.ContainsKey(RepoDataType.dRealityARRegionRGBAByte) || repoDictByteStream[RepoDataType.dRealityARRegionRGBAByte] == null || repoDictByteStream[RepoDataType.dRealityARRegionRGBAByte].Length != repoDict[RepoDataType.dRawRegionRGBA].Step * repoDict[RepoDataType.dRawRegionRGBA].Height)
        {
            repoDictByteStream[RepoDataType.dRealityARRegionRGBAByte] = new byte[repoDict[RepoDataType.dRawRegionRGBA].Step * repoDict[RepoDataType.dRawRegionRGBA].Height];
            repoDict[RepoDataType.pRealityARRegionRGBA] = new CvMat(RegionBoxRaw.Height,RegionBoxRaw.Width, MatrixType.U8C4, repoDictByteStream[RepoDataType.dRealityARRegionRGBAByte]);
       //     Debug.Log("dARTxTRegionByteRGBA created size:" + repoDict[RepoDataType.dRawRegionRGBA].Step + " x " + repoDict[RepoDataType.dRawRegionRGBA].Height);
            Debug.Log("Region Box : " + RegionBoxRaw.Width + " , " + RegionBoxRaw.Height);
        }
     //   Marshal.Copy(repoDict[RepoDataType.dRawRegionRGBA].DataArrayByte.Ptr, repoDictByteStream[RepoDataType.dRealityARRegionByteRGBA], 0, repoDict[RepoDataType.dRawRegionRGBA].Step * repoDict[RepoDataType.dRawRegionRGBA].Height);
        if(learningCount <3) lastdRawRegionRGBA = repoDict[RepoDataType.dRawRegionRGBA].Clone();

    }

    public static void loadBackground(DesignContent contentType)
    {
        if (GlobalRepo.GetRegionBox(false).Width == 0 || GlobalRepo.GetRepo(RepoDataType.dRawRegionRGBA)==null) return;
        DesignContent dc = FBSModel.ContentType;
        if (dc == DesignContent.HumanRespiratorySystem)
        {
            Asset2DTexture bgTXT = GlobalRepo.GetTexture2D("bg_content1");
            if (bgTXT == null) return;
            
            CvMat bgImg = GlobalRepo.GetRepo(RepoDataType.dRawRegionRGBA).EmptyClone();
            CvSize bgSize = bgImg.GetSize();
            CvPoint txtAnchor = bgTXT.getAnchorPointAbsolute();
            CvPoint bgAnchor = new CvPoint(bgSize.Width * bgTXT.AnchorPointRelative.X / 100, bgSize.Height * bgTXT.AnchorPointRelative.Y / 100);
            CvPoint overlayLT = txtAnchor - bgAnchor;
            //Debug.Log("overlay LT:" + DebugTrans.ToString(overlayLT));
            ImageBlender2D.loadTexturewithAlphaSmallUnderlay_ignoreunderlayA(bgImg, bgTXT.txtBGRAImg, overlayLT);
            //CvPoint()
            //GlobalRepo.showDebugImage("BGTXT", bgTXT.txtBGRAImg);
            //GlobalRepo.showDebugImage("BGTXT2", bgImg);
            GlobalRepo.updateRepo(RepoDataType.dContentBGRGBA, bgImg);
            //Debug.Log("BG TEXTURE LOADED");
  
        }
    }



    public static int getParamInt(string name)
    {
        int GrayHueValue = 205 / 2;
        int HueClusterRange = 17;
        if (name == "GrayHueValue") return GrayHueValue;
        if (name == "HueClusterRange") return HueClusterRange;
        if (name == "ContentBackgroundAlpha") return 10;
        if (name == "BlackThreshold") return 130;
        if (name == "CanvasSaturationValueMax") return 25;
        if (name == "minBloxPixelSize") return 50;
        Debug.Log("[ERROR] GlobalRepo.GetParam");
        return -1;
    }

    public static CvPoint TransformRegionPointtoGlobalPoint(CvPoint p)
    {

        //CvPoint ret = p + RegionBoxRaw.TopLeft;
        CvPoint ret = p;
        return ret;

    }

    public static bool NeedLiveStream()
    {
        if (UserMode == UserPhase.design) return true;
        else return false;
    }
    public static bool NeedBackupStream()
    {
        if (UserMode == UserPhase.feedback) return true;
        else return false;
    }
    public static bool NeedStream()
    {
        if (UserMode != UserPhase.simulation) return true;
        else return false;
    }
    public static void SetUserPhas(UserPhase p)
    {
        UserMode = p;
    }
    public static bool RealityARActivated()
    {
        return UserMode != UserPhase.design;
    }




}


public enum RepoDataType
{
    pRealityARRegionRGBA,
    borderPointerbetweenData,    
    dRawRGBA,
    dRawBGR,
    dRawRegionRGBA,
    dRawRegionBGRA,
    dRawRegionBGR,
    dRawRegionHSV, 
    dRawRegionLAB, 
    dRawRegionGray,
    dRawRegionRGBAByte,
    dRealityARRegionRGBAByte,
    dContentBGRGBA,
    dContentBGRGBAByte,
}

public class Asset2DTexture
{
    public string filename;
    public CvMat txtBGRAImg=null;
    public CvPoint CM;
    public CvRect presetOverlayRegion;
    public CvPoint AnchorPointRelative;
    public CvMat[] txtBGRAImgSequence=null;
    public Asset2DTexture(string filename_)
    {
        
        CvMat newimg = new CvMat(filename_,LoadMode.Unchanged);
        if(newimg.ElemType!=MatrixType.U8C4)
        {
            Debug.Log("[DEBUG] failed to load [" + filename_ + "] wrong Mat format");
            return;
        }
        //newimg = RGBA in general
        for(int i = 0; i < newimg.Width * newimg.Height; i++)
        {
            CvScalar pixel = newimg.Get1D(i);

            if (pixel.Val3 == 0) {
                pixel.Val0 = 0;
                pixel.Val1 = 0;
                pixel.Val2 = 0;
                newimg.Set1D(i, pixel);
            }
        }
        newimg.CvtColor(newimg, ColorConversion.RgbaToBgra);
        txtBGRAImg = newimg;
        CM = BlobAnalysis.ExtractBlobCenterfromBGRAImage(newimg);
        filename = filename_;
    }
    public CvPoint getAnchorPointAbsolute()
    {
        CvPoint ret = new CvPoint(0,0);
        if (this.txtBGRAImg != null)
        {
            ret.X = txtBGRAImg.Width * this.AnchorPointRelative.X / 100;
            ret.Y = txtBGRAImg.Height * this.AnchorPointRelative.Y / 100;
        } else if (this.txtBGRAImgSequence != null)
        {
            ret.X = txtBGRAImgSequence[0].Width * this.AnchorPointRelative.X / 100;
            ret.Y = txtBGRAImgSequence[0].Height * this.AnchorPointRelative.Y / 100;
        }

        return ret;
    }
    public Asset2DTexture(string filename_,int scale)
    {

        CvMat newimg = new CvMat(filename_, LoadMode.Unchanged);
        CvMat scaledImg = new CvMat(newimg.Height*scale/100, newimg.Width* scale/100,MatrixType.U8C4);
        if (newimg.ElemType != MatrixType.U8C4)
        {
            Debug.Log("[DEBUG] failed to load [" + filename_ + "] wrong Mat format");
            return;
        }
        newimg.Resize(scaledImg);
        //newimg = RGBA in general
        for (int i = 0; i < scaledImg.Width * scaledImg.Height; i++)
        {
            CvScalar pixel = scaledImg.Get1D(i);

            if (pixel.Val3 == 0)
            {
                pixel.Val0 = 0;
                pixel.Val1 = 0;
                pixel.Val2 = 0;
                scaledImg.Set1D(i, pixel);
            }
        }
        scaledImg.CvtColor(scaledImg, ColorConversion.RgbaToBgra);
        txtBGRAImg = scaledImg;
        CM = BlobAnalysis.ExtractBlobCenterfromBGRAImage(scaledImg);
        filename = filename_;
    }
    public Asset2DTexture(string pathname_, int scale , bool isAnimation, CvPoint anchorRelative)
    {
        if (!isAnimation) return;
        string[] filesPNG = Directory.GetFiles(pathname_, "*.png", SearchOption.AllDirectories);
        if(filesPNG==null || filesPNG.Length==0)
        {
            Debug.Log("[ERROR] loading texture at " + pathname_);
            return;
        }
        Array.Sort(filesPNG, StringComparer.InvariantCulture);
        txtBGRAImgSequence = new CvMat[filesPNG.Length];
        for (int i = 0; i < filesPNG.Length; i++)
        {
            txtBGRAImgSequence[i] = new CvMat(filesPNG[i], LoadMode.Unchanged);
            if (txtBGRAImgSequence[i] != null)
            {
             //   Debug.Log("Anim Textured loaded from " + filesPNG[i]);
                CvMat scaledImg = new CvMat(txtBGRAImgSequence[i].Height * scale / 100, txtBGRAImgSequence[i].Width * scale / 100, MatrixType.U8C4);
                txtBGRAImgSequence[i].Resize(scaledImg);
                for (int k= 0; k< scaledImg.Width * scaledImg.Height; k++)
                {
                    CvScalar pixel = scaledImg.Get1D(k);

                    if (pixel.Val3 == 0)
                    {
                        pixel.Val0 = 0;
                        pixel.Val1 = 0;
                        pixel.Val2 = 0;
                        scaledImg.Set1D(k, pixel);
                    }
                }
                scaledImg.CvtColor(scaledImg, ColorConversion.BgraToRgba);
                txtBGRAImgSequence[i] = scaledImg;
            }
            else Debug.Log("[ERROR] Anim Textured loaded from " + filesPNG[i]);
        }
        AnchorPointRelative = anchorRelative;
        
    }
    public CvMat getAnimationFrame(int frame, double rotation)
    {
        CvMat ret = null;
        if (frame < 0) return ret;
        if(frame>= txtBGRAImgSequence.Length)
        {
            Debug.Log("[Error] retrieving anim frame");
            return ret;
        }
        if (rotation != 0)
        {
            CvMat rotMat = Cv.GetRotationMatrix2D(new CvPoint(txtBGRAImgSequence[frame].Width * AnchorPointRelative.X / 100, txtBGRAImgSequence[frame].Height * AnchorPointRelative.Y / 100), rotation * 180.0f / Mathf.PI, 1);
            ret = txtBGRAImgSequence[frame].EmptyClone();
            Cv.WarpAffine(txtBGRAImgSequence[frame], ret, rotMat);
            //Cv.Transform(txtBGRAImgSequence[frame], ret, rotMat);

        }
        else ret = txtBGRAImgSequence[frame];
        

        return ret;
    }
    public int getAnimationLength()
    {
        if (txtBGRAImgSequence != null) return txtBGRAImgSequence.Length;
        return 1;
    }
}
 