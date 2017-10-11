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
//using System.Configuration;
//using System.Collections.Specialized;

public class ColorDetector : MonoBehaviour {


    public int scaleFactor = 1;
    public bool ShowDebugImage = true;
    public CvMat rawcolorImage = null;
    public float hueFilterThreshold = 4.0f;
    private CvMat colorImg=null;
    private CvMat debugImg = null;
    private CvMat regionImgHSV = null;
    private CvMat signImg = null;
    private CvMat labelImg = null;

    CvWindow debugWindow;
    CvWindow debugWindow2;
    CvWindow debugWindow3;
    CvWindow debugWindow4;
    
    private List<CvMat> colorblobImage = null;
    private List<CvMat> tmpImage = null;
    // Use this for initialization
    
    public static ColorDetector instance = null;
    private static List<CvPoint> regionPoints = new List<CvPoint>();
    public static colorProfile mCP;
    private static int BlobAccumulationN = -1;
    private static int DebugIteration = 0;

    private int TemporalWindow = 10;
    private int RegionImgRepoIndex = 0;
    private CvMat[] RegionImgRepo = new CvMat[10];
    
    public static void mouseCallback(MouseEvent @mevent, int x, int y, MouseEvent flags)
    {
        
        if (mevent == MouseEvent.LButtonUp)
        {
            //mouse click           
            CvMat regionHSV = GlobalRepo.GetRepo(RepoDataType.dRawRegionHSV);
            if (regionHSV != null )
            {
                CvScalar t = regionHSV.Get2D(y, x);
                //   Debug.Log(t.Val0 + "   " + t.Val1 + "   " + t.Val2 + "   " + t.Val3);
                //  colorModel cm = new colorModel(t.Val2, t.Val1, t.Val0);                                       
                colorModel cm = new colorModel(t.Val0, t.Val1, t.Val2);
                mCP.addSampleColor(cm);        

            }
        }
     
    }
    public static void mouseCallback2(MouseEvent @mevent, int x, int y, MouseEvent flags)
    {

       
        if (mevent == MouseEvent.LButtonUp)
        {
            //mouse click
            CvPoint t = new CvPoint(x, y);
            ImageBlender2D.startCollect2DBlobs(t);           

        }
    }

    void Start () {        
        CvMat src = new CvMat("Lenna.bmp");
        
        mCP = new colorProfile();
        mCP.setParamThreshold(0.04f);
        //    SignDetector.TestTemplateMatching();
        if (this.ShowDebugImage)
        {
        }
        //using (new CvWindow("dst image", dst))
  
        
        //Cv.SetMouseCallback("debug2", mouseCallback2);
        instance = this;
       
    }
	
	// Update is called once per frame
	void Update () {

        regionImgHSV = GlobalRepo.GetRepo(RepoDataType.dRawRegionHSV);
        if (regionImgHSV != null && mCP.mProfileList != null && mCP.mProfileList.Count >= 5)
        {
            //blob analysis
            if (GlobalRepo.getLearningCount()> 0)
            {
                processColorBlobs();                
                Debug.Log("Color Processing..." + GlobalRepo.getLearningCount() + " frames remain");
                if (GlobalRepo.getLearningCount() == 1)
                {
                    //BlobAccumulationN = 40;
                    GlobalRepo.setLearningCount(-1);
                    DebugIteration++;
                    createPrototypeFromColorBlobs();
                    /*if (DebugIteration > 2) createPrototypeFromColorBlobs();
                    else colorblobImage.ForEach(p => p.Zero());*/
                }
            } 
        }
        
        if (this.ShowDebugImage)
        {
            
            //if (debugWindow3 != null && signImg != null) debugWindow3.ShowImage(signImg);
            if (colorblobImage != null)
            {
                for (int i = 0; i < colorblobImage.Count; i++)
                {
                 //   GlobalRepo.showDebugImage("colorBlob#"+i,colorblobImage[i]);
                }
            }
            if(regionImgHSV !=null) GlobalRepo.showDebugImage("colorBlob",regionImgHSV);
        }
     //   int key = Cv.WaitKey(1);
        {
      //      processKeyInput(key);
        }


    }
    public void reset()
    {
        ResetandGo();
    }
    public static void processKeyInput(int key)
    {
        if(key == '0')
        {
            mCP.updateIndexCursor = 0;
            Debug.Log("Update Color index = 0");
        } else if (key =='1')
        {
            mCP.updateIndexCursor = 1;
            Debug.Log("Update Color index = 1");
        }
        else if (key == '2')
        {
            mCP.updateIndexCursor = 2;
            Debug.Log("Update Color index = 2");
        }
        else if (key == '3')
        {
            mCP.updateIndexCursor = 3;
            Debug.Log("Update Color index = 3");
        }
        else if (key == '4')
        {
            mCP.updateIndexCursor = 4;
            Debug.Log("Update Color index = 4");
        } else if (key =='s')
        {
            Debug.Log("Start Color Processing...");
            GlobalRepo.setLearningCount(10);
        } else if( key == 'c')
        {
            Debug.Log("Image Captured");
            CaptureImage();
        }

    }
    private static void CaptureImage()
    {
        string path = "./log/input/InputImage_";        
        CvMat img = GlobalRepo.GetRepo(RepoDataType.dRawRGBA);
        path = path + System.DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss")+".png";
        Debug.Log("Image Saved to " + path);
        img.SaveImage(path);
    }
    private void ResetandGo()
    {
        if(colorblobImage!=null)
        {
            foreach(var t in colorblobImage)
            {
                t.Zero();
            }
        }
        if (tmpImage != null)
        {
            foreach(var t in tmpImage)
            {
                t.Zero();
            }
        }        
    }
    private void processColorBlobs()
    {
        //init debug windows and images
        if (colorblobImage == null)
        {            
            colorblobImage = new List<CvMat>();
            tmpImage = new List<CvMat>();
            for (int i = 0; i < mCP.paramNColors; i++)
            {
                CvMat t1 = new CvMat(regionImgHSV.Rows, regionImgHSV.Cols, MatrixType.U8C1);
                t1.Zero();
                CvMat t2 = new CvMat(regionImgHSV.Rows, regionImgHSV.Cols, MatrixType.U8C1);
                colorblobImage.Add(t1);
                tmpImage.Add(t2);
            }
       
        }
       // Debug.Log(colorblobImage[0].GetSize());
        //Debug.Log(regionImg.GetSize());
        if(!colorblobImage[0].GetSize().Equals(regionImgHSV.GetSize()))
        {
            colorblobImage.Clear();
            tmpImage.Clear();
            for (int i = 0; i < mCP.paramNColors; i++)
            {
                CvMat t1 = new CvMat(regionImgHSV.Rows, regionImgHSV.Cols, MatrixType.U8C1);
                t1.Zero();
                CvMat t2 = new CvMat(regionImgHSV.Rows, regionImgHSV.Cols, MatrixType.U8C1);
                colorblobImage.Add(t1);
                tmpImage.Add(t2);
            }
         
        }      
        FindColorBlosandAccumulate(regionImgHSV);    
        
    }
    private void createPrototypeFromColorBlobs()
    {
        prototypeDef newPrototype = this.createPrototypeDescription();
        colorblobImage.ForEach(p => p.Zero());
        //behavioral marker
        List<UserDescriptionInfo> markerList=new List<UserDescriptionInfo>();
        List<UserDescriptionInfo> behaviorList = new List<UserDescriptionInfo>();
        List<UserDescriptionInfo> BVList = new List<UserDescriptionInfo>();
        List<UserDescriptionInfo> ConnList = new List<UserDescriptionInfo>();
        CvMat avgRegionImg = new CvMat(regionImgHSV.Rows, regionImgHSV.Cols, MatrixType.U8C4);
        designARManager m = this.GetComponentInParent<designARManager>();
      

            CvMat regionImgClone = GlobalRepo.GetRepo(RepoDataType.dRawRegionGray);
        {
            SignDetector.ConnectivityandBehaviorRecognition(regionImgClone, ref newPrototype, ref markerList, ref behaviorList, ref BVList,ref ConnList);            
            this.GetComponentInParent<BehaviorDetector>().recognizeBehaviorVariables(m.pConceptModelManager.pFBSModel);
            behaviorList = this.GetComponentInParent<BehaviorDetector>().exportBehaviorList();
            //   debugSignMarkers(markerList);
            debugTextLabels(behaviorList);
            //function label
        }  
        foreach (var t in behaviorList)
        {
            newPrototype.addBehavior(t);
        }
        foreach (var t in ConnList)
        {
            newPrototype.addConnectivity(t);
        }
        
      
        
        if (m != null && newPrototype != null)
        {
            m.addPrototype(newPrototype);
        }
    }
    
    private void debugSignMarkers(List<UserDescriptionInfo> listM)
    {
        if (signImg == null) signImg = new CvMat(1500, 1900, MatrixType.U8C1);
        int markerIndex = 0;
        signImg.SetZero();
        int heightStep = 20;
        int widthStep = 50;
        foreach (UserDescriptionInfo t in listM)
        {
            t.image.Copy(signImg.GetRows(heightStep, heightStep + t.image.Rows).GetCols(widthStep, widthStep + t.image.Cols));
            signImg.PutText(t.instanceID+" : " + t.InfoBehaviorTypeId, new CvPoint(widthStep+t.image.Cols+30, heightStep), new CvFont(FontFace.HersheyTriplex, 1.0f, 1.0f), CvColor.White);
            heightStep += t.image.Rows + 20;
            markerIndex++;
            if(heightStep>900)
            {
                heightStep = 20;
                widthStep += 500;
            }
        }
        debugWindow3.ShowImage(signImg);
    }
    private void debugTextLabels(List<UserDescriptionInfo> listT)
    {
        if (labelImg == null) labelImg = new CvMat(1500, 1900, MatrixType.U8C1);
        int markerIndex = 0;
        labelImg.SetZero();
        int heightStep = 20;
        foreach (UserDescriptionInfo t in listT)
        {
            t.image.Copy(labelImg.GetRows(heightStep, heightStep + t.image.Rows).GetCols(20, 20 + t.image.Cols));
            
                labelImg.PutText("\""+t.InfoTextLabelstring+"\"", new CvPoint(400, heightStep), new CvFont(FontFace.HersheyTriplex, 1.0f, 1.0f), CvColor.White);
                
            
            heightStep += t.image.Rows + 20;
            markerIndex++;
        }

        GlobalRepo.showDebugImage("Behavior Labels", labelImg);
    }

    
    public static CvScalar getColorforModelIndex(int index)
    {
        CvScalar ret = CvColor.White;
        if (mCP == null || index<1 || index> mCP.mProfileList.Count) return ret;
      //  Debug.Log("Get Color for index = " + index);
        ret = mCP.mProfileList[index - 1].BGRA;
        return ret;
    }
    //construct and return a prototypeDef only with structure models
    public prototypeDef createPrototypeDescription()
    {
        if (mCP.paramNColors > mCP.mProfileList.Count) return null;
        prototypeDef pProto = new prototypeDef();
        int validColorsN = 0;
        ModelCategory[] colorObjectMap = Content.loadColorObjectMap(out validColorsN);
        if (this.ShowDebugImage)
        {

            //if (debugWindow3 != null && signImg != null) debugWindow3.ShowImage(signImg);
            if (colorblobImage != null)
            {
                for (int i = 0; i < colorblobImage.Count; i++)
                {
                    GlobalRepo.showDebugImage("colorBlob#" + i, colorblobImage[i]);
                }
                 Cv.WaitKey(100);
            }
        }
        for (int i = 0; i < validColorsN; i++)
        {            
            List<ModelDef> extractedModels = BlobAnalysis.ExtractStrcutureInfoFromImageBlob(colorblobImage[i], regionImgHSV, colorObjectMap[i]);
         
            pProto.addModels(extractedModels);
        }
        //debug

        if(pProto.mModels!=null)  {
            Debug.Log("New Prototype N of Models : " + pProto.mModels.Count);
        }
        PostRefinePrototype(pProto);
        return pProto;
    }
    private void PostRefinePrototype(prototypeDef proto)
    {
        //refine models with classifying models in a same category

        if (proto.mModels == null) return;

        //upper and lower chain. deafult:upper
        if(proto.mModels[ModelCategory.Chain].Count>=1)
        {
            for(int i=0;i< proto.mModels[ModelCategory.Chain].Count;i++)
            {
                ModelCategory det = ModelCategory.UpperChain;
                for (int j = 0; j < proto.mModels[ModelCategory.Chain].Count; j++)
                {
                    if (i == j) continue;
                    if (proto.mModels[ModelCategory.Chain][i].centeroidAbsolute.Y < proto.mModels[ModelCategory.Chain][j].centeroidAbsolute.Y) det = ModelCategory.UpperChain;
                    else det = ModelCategory.LowerChain;
                }
                proto.mModels[ModelCategory.Chain][i].modelType = det;
                proto.addModeltoCategory(proto.mModels[ModelCategory.Chain][i]);
            }
            
        }
        proto.mModels[ModelCategory.Chain].Clear();
    }

    private void FindColorBlosandAccumulate(CvMat srcImg)
    {
        PointerAccessor1D_Int32 p = srcImg.DataArrayInt32;
        int length = srcImg.Height * srcImg.Width;        
        CvScalar in_, out_;        
        float hue_temp;
        float sat_val;
        float canvasMaxSaturation = GlobalRepo.getParamInt("CanvasSaturationValueMax");
        int validColorN = 0;
        ModelCategory[] colorObjectMap = Content.loadColorObjectMap(out validColorN);
        
        for(int i=0;i<colorObjectMap.Length;i++)
            if(colorObjectMap[i]==ModelCategory.None)
            {
                validColorN = i;
                break;
            }
        for (int i = 0; i < length; i++)
        {
            in_ = srcImg.Get1D(i);
            //hue_temp = colorModel.getHue1(in_.Val2, in_.Val1, in_.Val0);
            
            for(int j = 0; j < validColorN; j++)
            {
                //if (in_.Val1 < canvasMaxSaturation ||                    Mathf.Abs((float)in_.Val0 - mCP.mProfileList[j].HueClass1) > mCP.mProfileList[j].classficationThreshold)
                if (in_.Val1 < canvasMaxSaturation ||
                    Mathf.Abs((float)in_.Val0 - mCP.mProfileList[j].HueClass1) > hueFilterThreshold)
                {
                    tmpImage[j].Set1D(i, 0);
                }
                else tmpImage[j].Set1D(i, 255);          
            }
            
        }
        for (int j = 0; j < validColorN; j++)
        {
            //    debugImagelist[j].Smooth(debugImagelist[j],SmoothType.Gaussian);
    //        Cv.Erode(tmpImage[j], tmpImage[j]);
     //       Cv.Dilate(tmpImage[j], tmpImage[j]);
            colorblobImage[j].Add(tmpImage[j], colorblobImage[j]);

        }

    }
    
    public void UpdateColorImage(byte[] rgbaImage,byte[] overlayedRGBAimage,int width, int height)
    { // called by kinect.
        int[] from_to = { 0, 2, 1, 1, 2, 0, 3, 3 };
        
        if (colorImg==null)
        {
            colorImg = new CvMat(height / scaleFactor, width / scaleFactor, MatrixType.U8C4);
            rawcolorImage = new CvMat(height, width, MatrixType.U8C4);
            //  debugImg = new CvMat(height / scaleFactor, width / scaleFactor, MatrixType.U8C4);            
        }
        
        CvMat rawcolorImageRGBA = new CvMat(height, width, MatrixType.U8C4, rgbaImage);
        //Cv.Flip(rawcolorImage2, rawcolorImage, FlipMode.Y); //flip mode for text recognition
        rawcolorImageRGBA.Copy(rawcolorImage);
        Cv.Resize(rawcolorImage, colorImg);
      //  GlobalRepo.updateRepo(RepoDataType.pRawRGBA, rawcolorImageRGBA);
     //   GlobalRepo.updateRepo(RepoDataType.pOverlayRGBA, new CvMat(height, width, MatrixType.U8C4, overlayedRGBAimage));
        if (regionPoints.Count==2)
        {
            //     regionImg = Cv.GetSubRect(colorImg, out regionImg, new CvRect(regionPoints[0].X, regionPoints[0].Y, regionPoints[1].X - regionPoints[0].X, regionPoints[1].Y - regionPoints[0].Y));
          //  regionImg = Cv.GetSubRect(rawcolorImage, out regionImg, new CvRect((regionPoints[0].X*scaleFactor), (regionPoints[0].Y * scaleFactor), (regionPoints[1].X - regionPoints[0].X) * scaleFactor, (regionPoints[1].Y - regionPoints[0].Y) * scaleFactor));
          
            CvRect regionBox = new CvRect((regionPoints[0].X * scaleFactor), (regionPoints[0].Y * scaleFactor), (regionPoints[1].X - regionPoints[0].X) * scaleFactor, (regionPoints[1].Y - regionPoints[0].Y) * scaleFactor);
            //Y flip
            GlobalRepo.setRegionBox(regionBox);
            GlobalRepo.updateInternalRepo(true);
            
            //debugWindow2.ShowImage(regionImg);
            GlobalRepo.tickLearningCount();
            
        }
        
        
        

       // Cv.MixChannels(colorImg, colorImg, from_to);
        //Cv.ConvertImage(colorImg, colorImg, ConvertImageFlag.SwapRB);
                
    }
    void OnDestroy()
    {
        mCP.exportProfiletoFile();        
        CvWindow.DestroyAllWindows();        
    }
}

public class colorModel : System.IComparable
{
    public CvScalar BGRA;
    public float HueClass1=-1;
    public float profileThreshold=-1;
    public float learningAlpha = 0.1f;
    public int hit = 0;
    public float classficationThreshold = -1;
    private static double constant1 = Mathf.Sqrt(3.0f);
    
    public static float getHue1(double R, double G, double B)
    {
        return (float) R;
        /*float y = (float)(R - G) * (float)constant1;
        float x = (float)(R + G - 2 * B);
        return Mathf.Atan2(y, x);*/
    }
    public colorModel(double R, double G, double B)
    {
        //base
        BGRA.Val0 = B;
        BGRA.Val1 = G;
        BGRA.Val2 = R;
        BGRA.Val3 = 255;

        //hue descriptor #1 (Van De Weijer, ECCV 2006)
        /* float y = (float) (R - G) * (float)constant1;
         float x = (float) ( R+ G - 2 * B);

         HueClass1 = Mathf.Atan2(y,x);*/

        HueClass1 = (float) R;

    }
    public bool isDifferent(colorModel refCM)
    {
        if (profileThreshold < 0) return true;
        //hue1
        if (Mathf.Abs(refCM.HueClass1 - this.HueClass1) < profileThreshold)
        {
            Debug.Log("ColorProfile Exists: hue distance = "+(refCM.HueClass1 - this.HueClass1));
            return false;
        }
        Debug.Log("ColorProfile Differs: hue distance = " + (refCM.HueClass1 - this.HueClass1));
        return true;
    }
    public bool isDifferentLearning(colorModel refCM)
    {
        if (profileThreshold < 0) return true;
        //
        float diff = refCM.HueClass1 - this.HueClass1;
        if (Mathf.Abs(refCM.HueClass1 - this.HueClass1) < profileThreshold)
        { 
            this.HueClass1 += this.learningAlpha * diff;
            this.hit++;
                return false;
        }

        return true;
    }
    public bool LearningColor(colorModel refCM)
    {
        if (profileThreshold < 0) return false;
        //
        float diff = refCM.HueClass1 - this.HueClass1;
        
            this.HueClass1 += this.learningAlpha * diff;
            this.hit++;

        this.BGRA.Val0 += (refCM.BGRA.Val0 - this.BGRA.Val0) * this.learningAlpha;
        this.BGRA.Val1 += (refCM.BGRA.Val1 - this.BGRA.Val1) * this.learningAlpha;
        this.BGRA.Val2 += (refCM.BGRA.Val2 - this.BGRA.Val2) * this.learningAlpha;
        this.BGRA.Val3 += (refCM.BGRA.Val3 - this.BGRA.Val3) * this.learningAlpha;
        return true;
    }


    public serialColorModel exportModel()
    {
        serialColorModel t = new serialColorModel();
        t.BGRA_Val0 = this.BGRA.Val0;
        t.BGRA_Val1 = this.BGRA.Val1;
        t.BGRA_Val2 = this.BGRA.Val2;
        t.BGRA_Val3 = this.BGRA.Val3;
        t.HueClass1 = this.HueClass1;
        t.learningAlpha = this.learningAlpha;
        t.ProfileThreshold = this.profileThreshold;
        t.hit = this.hit;
        t.classficationThreshold = this.classficationThreshold;
        return t;
    }

    public int CompareTo(object obj)
    {
        colorModel objt = obj as colorModel;
        return objt.hit.CompareTo(hit);
    }

    public colorModel(serialColorModel t)
    {
         this.BGRA.Val0= t.BGRA_Val0;
         this.BGRA.Val1 = t.BGRA_Val1;
        this.BGRA.Val2= t.BGRA_Val2;
        this.BGRA.Val3 = t.BGRA_Val3;
        this.HueClass1 = t.HueClass1 ;
        this.learningAlpha = t.learningAlpha ;
        this.profileThreshold = t.ProfileThreshold;
        this.hit = t.hit;
        this.classficationThreshold = t.classficationThreshold;
        if (this.classficationThreshold <= 0) this.classficationThreshold = 0.06f;
    }
}
public class colorProfile
{
    public List<colorModel> mProfileList;
    private float defaultprofileThreshold = 0.1f;
    private float defaultclassificationThreshold = 0.06f;
    private string path = "colorprofile.txt";
    private ConfigObject config;
    public int paramNColors=5;
    public int updateIndexCursor = -1;
    public colorProfile()
    {
    

        mProfileList = new List<colorModel>();
        readProfileFromFile();
        
    }
   
    public void setParamThreshold(float pThresh)
    {
        defaultprofileThreshold = pThresh;
    }
    public void addSampleColor(colorModel cm)
    {
        bool isExisting = false;
      
        if(this.updateIndexCursor>=0)
        {
            mProfileList[updateIndexCursor].LearningColor(cm);
            Debug.Log("Color index " + updateIndexCursor + " learned to " + mProfileList[updateIndexCursor].HueClass1);
            isExisting = true;
        }
        if(!isExisting)
        {
            
            cm.profileThreshold = defaultprofileThreshold;
            cm.classficationThreshold = defaultclassificationThreshold;
            mProfileList.Add(cm);

        }
        Debug.Log("ColorProfile : Total profiles = " + mProfileList.Count);
    }
    
   
    private int readProfileFromFile()
    {
        mProfileList.Clear();
        if (!File.Exists(path))
        {
            for (int i = 0; i < this.paramNColors; i++)
            {

                colorModel cm = new colorModel(0, 0, 0);
                cm.profileThreshold = defaultprofileThreshold;
                cm.classficationThreshold = defaultclassificationThreshold;
                mProfileList.Add(cm);
            }
            return 1;
        }

        using (Stream file = File.Open(path, FileMode.Open))
        {
            BinaryFormatter bf = new BinaryFormatter();

            object obj = bf.Deserialize(file);

            List<object> objects = obj as List<object>;
            //you may want to run some checks (objects is not null and contains 2 elements for example
            for(int i = 0; i < objects.Count; i++)
            {
                var t = objects[i] as serialColorModel;
                mProfileList.Add(new colorModel(t as serialColorModel));
                mProfileList[i].classficationThreshold = 2.00f;
            }
          //  mProfileList.Sort();

           //mProfileList[3].classficationThreshold = 2.00f;

            //use nodes and dictionary
        }

     //   Debug.Log("Color Profile Loaded N=" + mProfileList.Count);
    //    printProfile();
        return 1;
    }
    private void printProfile()
    {
        for (int i = 0; i < mProfileList.Count; i++)
        {
            Debug.Log(i + ") hueclass1 =  " + mProfileList[i].HueClass1+" , hit = " + mProfileList[i].hit);

        }
    }
    public int exportProfiletoFile() {


        List<object> objects = new List<object>();
        for (int i = 0; i < mProfileList.Count; i++)
        {
            if (i >= this.paramNColors) break;
            objects.Add(mProfileList[i].exportModel());
            
        }
        if (File.Exists(path))
        {
            File.Delete(path);
        }
        using (Stream file = File.Open(path, FileMode.Create))
        {
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(file, objects);
        }
        printProfile();
      

        return 1;
    }
        private static void Add
        (FileStream fs, string value)
    {
        byte[] info = new UTF8Encoding(true).GetBytes(value);
        fs.Write(info, 0, info.Length);
    }
}
