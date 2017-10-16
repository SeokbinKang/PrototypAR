using UnityEngine;
using System.Collections;
using OpenCvSharp;
using OpenCvSharp.Blob;
using OpenCvSharp.CPlusPlus;
using System.Collections.Generic;
using System.Linq;
public class ConceptModelManager {

    public List<prototypeDef> mPrototypeList;

   
    private List<CvMat> debugImageList;
    private CvSize debugWindowSize = new CvSize(1200, 800);
    private int pMaxPrototype = 1;
    private Visual2DModelManager pVisual2DMgr = null;
    public FBSModel pFBSModel = null;
    public ConceptModelManager()
    {
        mPrototypeList = new List<prototypeDef>();
      
        debugImageList = new List<CvMat>();
    }
    public void reset()
    {
        mPrototypeList.Clear();
        debugImageList.Clear();
    }
    public void setFBSModel(FBSModel t)
    {
        this.pFBSModel = t;
    }
    public void setVisual2DMgr(Visual2DModelManager t)
    {
        this.pVisual2DMgr = t;
    }
    public void ConceptModelTick()
    {
        bool isDone;
        foreach(var proto in mPrototypeList)
        {
            if (proto.isRefinementFinished()) continue;
            isDone = proto.RefinePrototypeModels();
            if (isDone) EvaluationandVisualize(proto);
        }             
    }
    private void EvaluationandVisualize(prototypeDef proto)
    {
        // evaluate the design
        
        if(proto==null || this.pFBSModel ==null)
        {
            Debug.Log("[Error] evaluation model does not exist");
            return;
        }

        // evaulate the prototype model
        List<FeedbackToken> feedback = pFBSModel.EvaluatePrototype(proto);
        pVisual2DMgr.createVisual2DModel(proto, feedback);
        // generate 2d feedback

        // or genera 2d simulated visualization


        //construct 2d visualization model
    }

    public bool isModelRefined()
    {
        bool isDone = true; 
        foreach (var proto in mPrototypeList)
        {            
            bool fin = proto.isRefinementFinished();
            if (!fin) isDone = false;
        }
        return isDone;
    }
    public void create2DVisualModel()
    {

    }
    public void animateVisual3DModelTick()
    {
        foreach(var t in mPrototypeList)
        {
            //t.animationSynchronizer.Tick();
        }
    }
    public void addPrototype(prototypeDef t)
    {
        //        if (mPrototypeList.Count > 0) mPrototypeList.Clear(); //for debugging, single prototype.
        if (mPrototypeList.Count >= pMaxPrototype) return;
        mPrototypeList.Add(t);

    }
    public void showDebugImage()
    {
        for(int i = 0; i < mPrototypeList.Count; i++)
        {
           
            //draw prototype
    
            drawPrototype(mPrototypeList[i]);
    

            //Summary of prototype
         //   mPrototypeList[i].DebugPrintSummary();

        }
    }
    private void drawPrototype(prototypeDef proto)
    {
        CvMat debugImage = new CvMat(GlobalRepo.GetRegionBox(false).Width, GlobalRepo.GetRegionBox(false).Height, MatrixType.U8C3);
        CvPoint pointCenter = new CvPoint(debugImage.Width / 2, debugImage.Height / 2);
        debugImage.Zero();
        int tmpY = 10;
        if (proto.mModels != null)
        {
            foreach (KeyValuePair<ModelCategory, List<ModelDef>> item in proto.mModels)
            {
                CvColor modelColor = ColorDetector.getColorforModelIndex((int)item.Key);
                foreach (ModelDef m in item.Value)
                {
                    //relative
                    //debugImage.DrawCircle(pointCenter + m.centeroidRelative, 5, modelColor, 2);
                    debugImage.DrawCircle( m.centeroidAbsolute, 5, modelColor, 2);
                }
                if (item.Value.Count > 0)
                {
                    debugImage.PutText(Content.getOrganName(item.Key) + " : " + item.Value.Count + " found", new CvPoint(10, tmpY), new CvFont(FontFace.HersheyTriplex, 1.0f, 1.0f), modelColor);
                }
                tmpY += 30;
            }
            GlobalRepo.showDebugImage("Debug ProtoDef", debugImage);
        }

    }

}

public class prototypeDef
{
    public Dictionary<ModelCategory, List<ModelDef>> mModels;        
    public List<BehaviorDef> mBehaviors=null;
    public static int globalInstanceId = 0;
    public Transform pGameObject=null;
    public AnimationSync animationSynchronizer = null;
    public List<UserDescriptionInfo> mConnections = null;
    public bool isEvaluated = false;
    private bool isRefinementDone=false;
    public Visual2DModel p2DVisualModel = null;
    public prototypeDef()
    {
        if (mModels == null)
        {
            mModels = new Dictionary<ModelCategory, List<ModelDef>>();
            foreach (ModelCategory val in System.Enum.GetValues(typeof(ModelCategory)))
            {
                mModels.Add(val, new List<ModelDef>());
            }

        }
    }
    public void DebugPrintSummary()
    {
        Debug.Log("======Prototype Summary======");
        Debug.Log("======Structures======");
        foreach (ModelCategory val in System.Enum.GetValues(typeof(ModelCategory)))
        {
            
            for (int i = 0; i < mModels[val].Count; i++)
            {
                Debug.Log("ID:" + mModels[val][i].instanceID+"\t Type:" + Content.getOrganName(val) + "\t model");
                Debug.Log("ID:" + mModels[val][i].instanceID + "\t behavior:" + mModels[val][i].OLDbehaviors.Count + "\t function:" + mModels[val][i].behaviors.Count);
            }
        }
        Debug.Log("======Behaviors======");
 
        if (mBehaviors != null)
        {
            foreach (var t in mBehaviors)
            {
                Debug.Log("ID:" + t.instanceID + "\t Type:" + t.behaviorType + "\t associated structureID:" + t.baseStrcuture.instanceID);
            }
        }
    }
    public bool isEmpty()
    {
        int modelCnt = 0;
        foreach (ModelCategory val in System.Enum.GetValues(typeof(ModelCategory)))
        {
            modelCnt += mModels[val].Count;        
        }
        if (modelCnt == 0) return true;
        return false;
    }
    public ModelDef getModelDefbyID(int id)
    {
        foreach (var group in mModels)
        {
            for(int i=0;i<group.Value.Count;i++)            
            {
                if (group.Value[i].instanceID == id) return group.Value[i];

            }
        }
        return null;
    }
    public ModelDef getModelDefbyType(ModelCategory type)
    {
        //get the largest model
        ModelDef largestModel = null;
        double size = -1;
        foreach (var group in mModels)
        {
            
            for (int i = 0; i < group.Value.Count; i++)
            {
                if (group.Value[i].modelType == type)
                {
                    if (group.Value[i].AreaSize > size)
                    {
                        size = group.Value[i].AreaSize;

                        largestModel =  group.Value[i];
                    }
                }

            }
        }
        return largestModel;
    }
    public bool RefinePrototypeModels()
    {
        if (isRefinementDone || mModels==null) return true;
        bool isDone = true;
        foreach(var group in mModels)
        {
            foreach(var model in group.Value)
            {
                if(!model.ShapeBuildTick()) isDone=false;
                
            }
        }
        if (isDone) isRefinementDone=true;
        return isDone;
    }
    public bool isRefinementFinished()
    {
        return isRefinementDone;
    }
    
    public void initAnimation()
    {  //DEPRECATED
        float defaultPeriod = 4.0f;
        this.animationSynchronizer = new AnimationSync(2, defaultPeriod);
     /*   foreach (var t in mBehaviors)
        {
            Debug.Log("ID:" + t.instanceID + "\t Type:" + t.behaviorType + "\t associated structureID:" + t.baseStrcuture.instanceID);
            ModelDef baseModel = t.baseStrcuture;
            if (baseModel != null)
            {
                if (t.behaviorType == BehaviorInstanceType.MoveUpDown)
                {
                    Vector3 moveRange = new Vector3(0f, 0.1f, 0f);
                    animationControl3D ac = new animationControl3D(baseModel, t.behaviorType, moveRange, new Vector3(0f, 0f, 0f), defaultPeriod);
                    animationSynchronizer.addAnimationControl(ac);
                }
                if (t.behaviorType == BehaviorInstanceType.Expand)
                {
                    Vector3 moveRange = new Vector3(0.1f, 0.1f, 0.1f);
                    animationControl3D ac = new animationControl3D(baseModel, t.behaviorType, moveRange, new Vector3(0f, 0f, 0f), defaultPeriod);
                    animationSynchronizer.addAnimationControl(ac);
                }
            }
            
        }*/
    }
    public void addConnectivity(UserDescriptionInfo conn)
    {
        if (mConnections == null) mConnections = new List<UserDescriptionInfo>();
        mConnections.Add(conn);
    }
    public int GetNumberofModels()
    {
        int modelCnt = 0;
        foreach (ModelCategory val in System.Enum.GetValues(typeof(ModelCategory)))
        {
            modelCnt += mModels[val].Count;
        }
        
        return modelCnt;
    }
    public void  addModels(List<ModelDef> modelList)
    {
        if (modelList == null || modelList.Count==0) return;
        if (mModels == null) {
            mModels = new Dictionary<ModelCategory, List<ModelDef>>();
            foreach (ModelCategory val in System.Enum.GetValues(typeof(ModelCategory)))
            {
                mModels.Add(val, new List<ModelDef>());
            }

        }
        //construct models
        for(int i = 0; i < modelList.Count; i++)
        {
            mModels[modelList[i].modelType].Add(modelList[i]);
        }
        //construct structures
        
        //construct functions        

    }
   
    public void addBehavior(UserDescriptionInfo sMarker)
    {
        if (mModels == null) return;
        if (mBehaviors == null) mBehaviors = new List<BehaviorDef>();
        //find the nearest model
        float distanceMin = float.MaxValue;

        ModelDef pClosestModel = null;
        foreach (ModelCategory val in System.Enum.GetValues(typeof(ModelCategory)))
        {
            for (int i = 0; i < mModels[val].Count; i++)
            {
                float dist = (float)sMarker.center.DistanceTo(mModels[val][i].centeroidAbsolute);
                if (dist < distanceMin)
                {
                    distanceMin = dist;
                    pClosestModel = mModels[val][i];
                }

            }
        }
        if (pClosestModel != null)
        {
            BehaviorDef fd = new BehaviorDef(sMarker, pClosestModel);            
            pClosestModel.addBehavior(fd);
            mBehaviors.Add(fd);
        }
    }
    public void addModeltoCategory(ModelDef m)
    {
        if (m == null || this.mModels == null || !this.mModels.ContainsKey(m.modelType)) return;
        this.mModels[m.modelType].Add(m);

    }
    public List<UserDescriptionInfo> GetAllConnectivityofModel(ModelDef m)
    {
        if (m == null || mConnections == null) return null;
        List<UserDescriptionInfo> ret = new List<UserDescriptionInfo>();
        foreach(var conn in mConnections)
        {
            if(conn.StrConnectivity.Key==m.instanceID || conn.StrConnectivity.Value == m.instanceID)
            {
                ret.Add(conn);
            }
        }
        return ret;
    }
}
public class structureDef
{
    public int num;
}
public class OLDbehaviorDef
{
    public int instanceID;
    public BehaviorInstanceType behaviorType;
    public UserDescriptionInfo marker;
    public ModelDef baseStrcuture;    
    public OLDbehaviorDef(UserDescriptionInfo pMarker)
    {
        this.marker = pMarker;
        this.behaviorType = (BehaviorInstanceType)pMarker.InfoBehaviorTypeId;
        //find the nearest structure
        this.baseStrcuture = null;
        instanceID = prototypeDef.globalInstanceId++;
    }
    public OLDbehaviorDef(UserDescriptionInfo pMarker,ModelDef pModel)
    {
        this.marker = pMarker;
        this.behaviorType = (BehaviorInstanceType)pMarker.InfoBehaviorTypeId;
        //find the nearest structure
        this.baseStrcuture = pModel;
        instanceID = prototypeDef.globalInstanceId++;
    }

}
public class BehaviorDef
{
    public int instanceID;
    public BehaviorCategory behaviorType;
    public UserDescriptionInfo marker;
    public ModelDef baseStrcuture;    

    public BehaviorDef(UserDescriptionInfo pMarker)
    {
        int dist;
        if (pMarker.InfoTextLabelstring == "") this.behaviorType = BehaviorCategory.None;
            else this.behaviorType = Content.DetermineBehaviorType(pMarker.InfoTextLabelstring,out dist);
        this.marker = pMarker;
        this.baseStrcuture = null;
        instanceID = prototypeDef.globalInstanceId++;
    }
    public BehaviorDef(UserDescriptionInfo pMarker,ModelDef pModel)
    {
        int dist;
        if (pMarker.InfoTextLabelstring == "") this.behaviorType = BehaviorCategory.None;
        else this.behaviorType = Content.DetermineBehaviorType(pMarker.InfoTextLabelstring,out dist);
        this.marker = pMarker;
        this.baseStrcuture = pModel;
        instanceID = prototypeDef.globalInstanceId++;
    }
   
   


}
public class ModelDef
{
    public int instanceID;
    public CvPoint centeroidRelative;
    public CvPoint centeroidAbsolute;
    public double AreaSize;
    public double AreaSizeNormalized;
    public CvRect bBox;
    public float WHRatio;
    public Point[] contourArray;
    public ModelCategory modelType;
    public float mBaseHueValue;
    public List<OLDbehaviorDef> OLDbehaviors;
    public List<BehaviorDef> behaviors;

    public Transform pGameObject;
    public bool[] mDesignPhase;
    private ObjectShape2DBuilder mShapeBuilder;
    public CvPoint[] TruthShapeContourinRegionCoord;

    public int tmpInt;

    public CvPoint ConnPoint;
    
    public ModelDef()
    {
        pGameObject = null;
        centeroidRelative.X = 0;
        centeroidRelative.Y = 0;
        centeroidAbsolute.X = 0;
        centeroidAbsolute.Y = 0;
        modelType = ModelCategory.None;
        AreaSizeNormalized = 0;
        AreaSize = 0;
        instanceID = prototypeDef.globalInstanceId++;
        this.OLDbehaviors = new List<OLDbehaviorDef>();
        this.behaviors = new List<BehaviorDef>();
        this.mDesignPhase = new bool[(int)ModelPhase.Total_Phase];
        mShapeBuilder = null;

        
    }
    public ObjectShape2DBuilder getShapeBuilder()
    {
        if (mShapeBuilder.isFinished()) return mShapeBuilder;
           else return null;
    }
    public void initShapeBuilder()
    {

        bool convex = false;
        if (this.modelType == ModelCategory.FrontChainring) convex = true;
        mShapeBuilder = new ObjectShape2DBuilder(this.centeroidAbsolute,this.mBaseHueValue,convex);
    }
    public void initShapeBuilder(Point[] fullcontour, CvRect bBox, Point center)
    {
        bool convex = false;
        if (this.modelType == ModelCategory.FrontChainring) convex = true;
        mShapeBuilder = new ObjectShape2DBuilder(fullcontour, center, bBox, convex);

    }
    
    public bool ShapeBuildTick()
    {
        if (mShapeBuilder == null) return false;        
        mShapeBuilder.Build();
        return mShapeBuilder.isFinished();
    }
    public void addOLDBehavior(OLDbehaviorDef t)
    {
        if(this.OLDbehaviors!=null)      this.OLDbehaviors.Add(t);
    }
    public void addBehavior(BehaviorDef t)
    {
        this.behaviors.Add(t);
    }
    public void setMoment(Moments m,CvPoint center, CvMat labelImage = null)
    {
        int regionwidth = center.X * 2;
        int regionheight = center.Y * 2;
        centeroidAbsolute.X = (int)(m.M10 / m.M00);
        centeroidAbsolute.Y = (int)(m.M01 / m.M00);

        //shift to the center of the image

        if(labelImage!=null && centeroidAbsolute.Y>=0 && centeroidAbsolute.Y<labelImage.Height && centeroidAbsolute.X>=0 && centeroidAbsolute.X<labelImage.Width && labelImage.Get2D(centeroidAbsolute.Y,centeroidAbsolute.X).Val0==0)
        {
            //refine centeroid to labeled pixel
            int winSize = 5;
            for(int xi=centeroidAbsolute.X-winSize;xi<centeroidAbsolute.X+winSize;xi++)
                for (int yi = centeroidAbsolute.Y - winSize; yi < centeroidAbsolute.Y + winSize; yi++)
                {
                    if (xi < 0 || xi > labelImage.Width - 1 || yi < 0 || yi > labelImage.Height - 1) continue;
                    if(labelImage.Get2D(yi,xi).Val0>0)
                    {
                        centeroidAbsolute.X = xi;
                        centeroidAbsolute.Y = yi;
                        break;
                    }
                }
        }
        

        centeroidRelative = centeroidAbsolute - center; // relative position in absolute scale


        //relative position in relative scale x=[-1,1], y=[-1,1]
        centeroidRelative.X = centeroidRelative.X * 100 / center.X;
        centeroidRelative.Y = centeroidRelative.Y * 100 / center.Y;

        //Debug.Log("centeroid" + centeroidAbsolute.X + "  " + centeroidAbsolute.Y);


    }
    
    public void setContour(Point[] c)
    {
        contourArray = (Point[])c.Clone();
       // contourArray = BlobAnalysis.reduceContourPoints(c,6);
        AreaSize = Cv2.ContourArea(c);
        OpenCvSharp.CPlusPlus.Rect boundingBox = Cv2.BoundingRect(c);
        bBox = new CvRect(boundingBox.X, boundingBox.Y, boundingBox.Width, boundingBox.Height);
        WHRatio = bBox.Width / bBox.Height;

    }
}

public enum BehaviorInstanceType
{
    None = 0x00,
    MoveUpDown = 0x01,
    Rotate = 0x02,
    Vibrate = 0x03,
    Expand = 0x04,
}
public enum FunctionInstanceType
{
    None = 0x00,
    PassageofAirinBody = 0x01,
    PullingAirintotheBody = 0x02,
    MakingtheLungsExpand = 0x03,

}
public enum ModelPhase
{
    Phase0_Init = 0x00,
    Phase1_SelectionDone = 0x01,
    Phase2_ShapeDone = 0x02,
    Phase3_PosDone = 0x03,
    Phase4_ConnDone = 0x04,
    Phase5_BehavDone = 0x05,
    Total_Phase,    
}