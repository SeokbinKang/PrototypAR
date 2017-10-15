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
public class Visual2DModelManager : MonoBehaviour
{

    public Vector2 testFramePosition;
    public bool testFramePos = false;
    public GameObject appcontrol;
    private List<Visual2DModel> mModelList = null;
    // Use this for initialization
    private CvMat objectOverlayImgRGBA = null;
    private CvWindow DebugVisual2DModel = null;
    
    void Start()
    {
        mModelList = new List<Visual2DModel>();
        
    }

    // Update is called once per frame
    void Update()
    {
        tickVisual2DModels();
    }
    public void reset()
    {
        foreach(var t in mModelList)
        {
            t.reset();
        }
        mModelList.Clear();
    }
    public void createVisual2DModel(prototypeDef proto, List<FeedbackToken> feedbacklist)
    {
        Visual2DModel vmodel = new Visual2DModel(proto, feedbacklist);
        mModelList.Add(vmodel);
        proto.p2DVisualModel = vmodel;
    }

    private void tickVisual2DModels()
    {
        if (GlobalRepo.GetRepo(RepoDataType.dRawRegionBGR) == null) return;
        if (this.objectOverlayImgRGBA == null)
        {
            CvRect regionBox = GlobalRepo.GetRegionBox(false);            
            objectOverlayImgRGBA = new CvMat(regionBox.Height, regionBox.Width, MatrixType.U8C4);
            

        }
        if(objectOverlayImgRGBA!=null) objectOverlayImgRGBA.Zero();
        foreach (Visual2DModel vmodel in mModelList)
        { //display object augmentation
            vmodel.tick(objectOverlayImgRGBA, null);
        }

        //display feedback augmentation
      //  DebugVisual2DModel.ShowImage(objectOverlayImgRGBA);
    }
    public void exportObjectVisualization(CvMat underlay)
    {
        if (objectOverlayImgRGBA == null) return;
        if (objectOverlayImgRGBA.GetSize() != underlay.GetSize())
        {
            Debug.Log("[ERROR]Visual2dModelManager: overlay/underlay size mismatch");
            return;
        }

        underlay.Add(objectOverlayImgRGBA, underlay);

        /////test point
        if (testFramePos)
        { //for content calibration.
            Vector3 screenPos = new Vector3();
        SceneObjectManager.MeasureObjectPointinScreen(PreLoadedObjects.Content1_BGPartial, testFramePosition, ref screenPos);
        CvPoint regionPos = SceneObjectManager.ScreentoRegion(screenPos);
        underlay.DrawCircle(regionPos, 6, new CvScalar(255, 0, 0, 255), -1); }


        //  GlobalRepo.showDebugImage("export model vis", underlay);
        //ImageBlender2D.AlphBlendingImgRGBA(underlay, objectOverlayImgRGBA, new CvPoint(0, 0));
    }

}

public class Visual2DModel
{
    private prototypeDef mRefConceptModel = null;
    private List<Visual2DObject> mObjectList = null;

    //M feedback List
    //private animationClip mAnimationClip
    //public void overlayObject(CvMat dstROI);
    public Visual2DModel(prototypeDef conceptModel, List<FeedbackToken> feedbacklist)
    {
        mRefConceptModel = conceptModel;
        mObjectList = new List<Visual2DObject>();
        Debug.Log("receving feedback " + feedbacklist.Count);
        if (feedbacklist.Count == 0)
        {
            CreateSimulatedVisualization(conceptModel);
            GlobalRepo.SetUserPhas(GlobalRepo.UserPhase.simulation);
            return;
        }
        //for (int i = 0; i < feedbacklist.Count; i++)
        for (int i = 0; i < 1; i++)
        {
            CreateFeedback(conceptModel, feedbacklist[i]);
            if (feedbacklist[i].type == EvaluationResultCategory.Shape_existence_missing) break;
        }
        GlobalRepo.SetUserPhas(GlobalRepo.UserPhase.feedback);


        //add the prototype to prototypeInstanceManager
        GameObject go_multiview = GameObject.Find("InventoryUI");
        PrototypeInstanceManager t = go_multiview.GetComponent<PrototypeInstanceManager>();
        CvRect regionBox = GlobalRepo.GetRegionBox(false);
        Texture2D temporaryTexture = null;
        if (temporaryTexture == null || temporaryTexture.width != regionBox.Width || temporaryTexture.height != regionBox.Height)
        {
            temporaryTexture = new Texture2D(regionBox.Width, regionBox.Height, TextureFormat.RGBA32, false);
        }
        temporaryTexture.LoadRawTextureData(GlobalRepo.getByteStream(RepoDataType.dRawRegionRGBAByte));
        temporaryTexture.Apply();
        if (temporaryTexture != null)
        {
            DesignContent mDesignContent = ApplicationControl.ActiveInstance.getContentType();
            SimulationParam sp = new SimulationParam();
            Content.ExtractSimulationParameters(conceptModel, mDesignContent, ref sp);
            t.AddIncompletePrototypeInstance(temporaryTexture,feedbacklist,sp);
        }





        /*
        foreach (KeyValuePair<ModelCategory, List<ModelDef>> item in mRefConceptModel.mModels)
        {
            foreach (ModelDef m in item.Value)
            {
                if (m.modelType != ModelCategory.Airways)
                { //TODO: airways's seeding point shouldn't be CM, which could be out of the blob)
                    Visual2DObject vObj = new Visual2DObject(m);
                    mObjectList.Add(vObj);
                }
            }
        }*/
    }
    public void reset()
    {
        SceneObjectManager SOMgr = SceneObjectManager.getActiveInstance();
        if (SOMgr != null) SOMgr.initSceneObject();
        mRefConceptModel = null;
        mObjectList.Clear();

    }
    private void CreateFeedback(prototypeDef proto, FeedbackToken feedback)
    {
        feedback.DebugPrint();
        //##########################################################
        //##########################################################
        if (feedback.type == EvaluationResultCategory.Shape_existence_missing)
        {
            PreLoadedObjects feedbackObj = feedback.getFeedbackObjectID();
            if (feedbackObj == PreLoadedObjects.None) return;
            SceneObjectManager SOMgr = SceneObjectManager.getActiveInstance();
            if (SOMgr != null) SOMgr.activateObject(feedbackObj,true);

        }
        //##########################################################
        //##########################################################
        if (feedback.type == EvaluationResultCategory.Shape_existence_redundant)
        {
            //just display preset
            SceneObjectManager SOMgr = SceneObjectManager.getActiveInstance();
            CvPoint BVPoint = feedback.model.getShapeBuilder().center;
            Direction4Way getTakeawayDirection = CVProc.ClosestBorderofRegionBox(BVPoint,false);
            PreLoadedObjects sceneObjType = PreLoadedObjects.None;
            if (getTakeawayDirection == Direction4Way.Down) sceneObjType = PreLoadedObjects.STR_EXTRA_down;
            if (getTakeawayDirection == Direction4Way.Right) sceneObjType = PreLoadedObjects.STR_EXTRA_right;
            if (getTakeawayDirection == Direction4Way.Up) sceneObjType = PreLoadedObjects.STR_EXTRA_up;
            if (getTakeawayDirection == Direction4Way.Left) sceneObjType = PreLoadedObjects.STR_EXTRA_left;
            if (SOMgr != null) SOMgr.activateObject(sceneObjType, GlobalRepo.TransformRegionPointtoGlobalPoint(BVPoint));

        }
        //##########################################################
        //##########################################################
        if (feedback.type == EvaluationResultCategory.Shape_suggestion)
        {
            {
                ModelDef m = proto.getModelDefbyID(feedback.modelInstanceID);
                if (m == null)
                {
                    Debug.Log("[ERROR] Can't create feedback visulization for object id=" + feedback.modelInstanceID);
                    return;
                }
                Visual2DObject vObj = new Visual2DObject(m, false);
                vObj.addFeedback_ShapeSuggestion(feedback.ShapeSuggestedOutline);
                mObjectList.Add(vObj);

                //display preset
                SceneObjectManager SOMgr = SceneObjectManager.getActiveInstance();
                CvPoint BVPoint = m.getShapeBuilder().center;
                BVPoint.Y = m.getShapeBuilder().bBox.TopLeft.Y;
                Direction4Way getTakeawayDirection = CVProc.ClosestBorderofRegionBox(BVPoint, true);
                PreLoadedObjects sceneObjType = PreLoadedObjects.STR_SHAPE_left;
                if (getTakeawayDirection == Direction4Way.Right)
                {
                    sceneObjType = PreLoadedObjects.STR_SHAPE_right;
                    
                }
                if (getTakeawayDirection == Direction4Way.Left)
                {
                    sceneObjType = PreLoadedObjects.STR_SHAPE_left;
                }
                if (SOMgr != null) SOMgr.activateObject(sceneObjType, GlobalRepo.TransformRegionPointtoGlobalPoint(BVPoint));
            }
        }
        //##########################################################
        //##########################################################
        if (feedback.type == EvaluationResultCategory.Position_direction)
        {
            ModelDef m = proto.getModelDefbyID(feedback.modelInstanceID);
            if (m == null)
            {
                Debug.Log("[ERROR] Can't create feedback visulization for object id=" + feedback.modelInstanceID);
                return;
            }
            Visual2DObject vObj = new Visual2DObject(m, false);
            vObj.addFeedback_PositionSuggestion(feedback.PositionSuggestedDirectiontoMove);
            mObjectList.Add(vObj);

            SceneObjectManager SOMgr = SceneObjectManager.getActiveInstance();
            CvPoint BVPoint = m.getShapeBuilder().bBox.TopLeft;
            BVPoint.X = BVPoint.X + m.getShapeBuilder().bBox.Width / 2;
            if (SOMgr != null) SOMgr.activateObject(PreLoadedObjects.STR_POS_dialog, GlobalRepo.TransformRegionPointtoGlobalPoint(BVPoint));

        }
        //##########################################################
        //##########################################################
        if (feedback.type == EvaluationResultCategory.Connectivity_wrong)
        {
            Visual2DObject vObj = new Visual2DObject();
            CvPoint msgPoint = vObj.addFeedback_WrongConnection(feedback);
            mObjectList.Add(vObj);

            SceneObjectManager SOMgr = SceneObjectManager.getActiveInstance();
            if (SOMgr != null) SOMgr.activateObject(PreLoadedObjects.STR_CONN_incorrect_Dialogue, GlobalRepo.TransformRegionPointtoGlobalPoint(msgPoint));
        }
        //##########################################################
        //##########################################################
        if (feedback.type == EvaluationResultCategory.Connectivity_missing)
        {
            Visual2DObject vObj = new Visual2DObject();
           /*CvPoint msgPoint = vObj.addFeedback_MissingConnection(feedback.connectivity_missingConn);
              mObjectList.Add(vObj);*/
            CvPoint msgPoint = feedback.connectivity_missingConn.Key.getShapeBuilder().center;

            SceneObjectManager SOMgr = SceneObjectManager.getActiveInstance();                        
            if (SOMgr != null) SOMgr.activateObject(PreLoadedObjects.STR_CONN_missing_Dialogue, GlobalRepo.TransformRegionPointtoGlobalPoint(msgPoint));

        }
        //##########################################################
        //##########################################################
        if (feedback.type == EvaluationResultCategory.Behavior_missing)
        {
            /*   Visual2DObject vObj = new Visual2DObject(feedback.model, false);
               vObj.addFeedback_MissingBehavior(feedback);
               mObjectList.Add(vObj);*/

            CvPoint msgPoint = feedback.model.getShapeBuilder().center;   
                   

            SceneObjectManager SOMgr = SceneObjectManager.getActiveInstance();
            if (SOMgr != null) SOMgr.activateObject(PreLoadedObjects.BEH_BL_missing, GlobalRepo.TransformRegionPointtoGlobalPoint(msgPoint));
        }
        //##########################################################
        //##########################################################
        if (feedback.type == EvaluationResultCategory.Behavior_Unnecessary)
        {
            //Visual2DObject vObj = new Visual2DObject();
            //vObj.addFeedback_UnnecessaryBehavior(feedback);
            //mObjectList.Add(vObj);
            CvPoint msgPoint = feedback.behavior.marker.center;
            PreLoadedObjects sceneObjType = PreLoadedObjects.None;
            Direction4Way getTakeawayDirection = CVProc.ClosestBorderofRegionBox(msgPoint, false);
            if (getTakeawayDirection == Direction4Way.Down) sceneObjType = PreLoadedObjects.BEH_BL_unnecessary_down;
            if (getTakeawayDirection == Direction4Way.Right) sceneObjType = PreLoadedObjects.BEH_BL_unnecessary_right;
            if (getTakeawayDirection == Direction4Way.Up) sceneObjType = PreLoadedObjects.BEH_BL_unnecessary_up;
            if (getTakeawayDirection == Direction4Way.Left) sceneObjType = PreLoadedObjects.BEH_BL_unnecessary_left;
            SceneObjectManager SOMgr = SceneObjectManager.getActiveInstance();
            if (SOMgr != null) SOMgr.activateObject(sceneObjType, GlobalRepo.TransformRegionPointtoGlobalPoint(msgPoint));
        }
        //##########################################################
        //##########################################################
        if (feedback.type == EvaluationResultCategory.Behavior_relocate)
        {
            Visual2DObject vObj = new Visual2DObject();
            vObj.addFeedback_RelocateBehavior(feedback);
            mObjectList.Add(vObj);
            CvPoint msgPoint = feedback.behavior.marker.center;
            SceneObjectManager SOMgr = SceneObjectManager.getActiveInstance();
            PreLoadedObjects sceneObjType = PreLoadedObjects.BEH_BL_remap;
            if (SOMgr != null) SOMgr.activateObject(sceneObjType, GlobalRepo.TransformRegionPointtoGlobalPoint(msgPoint));
        }
        //##########################################################
        //##########################################################
        if (feedback.type == EvaluationResultCategory.Behavior_variableUnchecked)
        {
            //just display preset
            if (feedback.behaviorType == BehaviorCategory.CONTRACT)
            {
                SceneObjectManager SOMgr = SceneObjectManager.getActiveInstance();
                CvPoint BVPoint = feedback.behavior.marker.center;
                BVPoint.X += feedback.behavior.marker.boundingBox.Width * 3 / 10;
                if (SOMgr != null) SOMgr.activateObject(PreLoadedObjects.BEH_BV_missing_contract, GlobalRepo.TransformRegionPointtoGlobalPoint(BVPoint));
            }
            if (feedback.behaviorType == BehaviorCategory.PEDAL)
            {
                SceneObjectManager SOMgr = SceneObjectManager.getActiveInstance();
                CvPoint BVPoint = feedback.behavior.marker.center;
                BVPoint.X += feedback.behavior.marker.boundingBox.Width * 3 / 10;
                if (SOMgr != null) SOMgr.activateObject(PreLoadedObjects.BEH_BV_missing_pedal, GlobalRepo.TransformRegionPointtoGlobalPoint(BVPoint));
            }
            if (feedback.behaviorType == BehaviorCategory.REDUCE)
            {
                SceneObjectManager SOMgr = SceneObjectManager.getActiveInstance();
                CvPoint BVPoint = feedback.behavior.marker.center;
                BVPoint.X += feedback.behavior.marker.boundingBox.Width * 3 / 10;
                if (SOMgr != null) SOMgr.activateObject(PreLoadedObjects.BEH_BV_missing_reduce, GlobalRepo.TransformRegionPointtoGlobalPoint(BVPoint));
            }
        }

    }
        
        private void CreateSimulatedVisualization(prototypeDef proto)
    {
        Debug.Log("OK Let's stat simlulation!");

        //check finalvisualizationType
        Simulation.GenerateSimulation(proto,this);
    }
    public void CreateTexturedLoadedObject(ModelDef model)
    {
        Visual2DObject vObj = new Visual2DObject(model, true);        
        mObjectList.Add(vObj);
    }
    public void tick(CvMat objectOverlayDst, CvMat animationOverlayDst)
    {
        bool debugBuildingCheck = true;
        foreach (Visual2DObject vobj in mObjectList)
        {
            vobj.tick(objectOverlayDst);
            if (!vobj.isBuildingDone()) debugBuildingCheck = false;
        }
        if (debugBuildingCheck) GlobalRepo.ProcessingDone();
    }

}

public class Visual2DObject
{
    private CvMat objectImgRGBA;

    public CvPoint localAnchorPoint; //normalized
    public CvPoint GlobalPosition;  // position of local anchor in the ROI 2D canvas
    private bool BuildingDone;
    private int objectBuildingProcessCount = 0;
    private static int imgMargin = 200; // consider noise part

    ////    private ObjectShape2DBuilder mBuilder = null;
    private CvWindow debugWindow = null;
    public ModelDef pModelDef = null;
    private Animation2DClip anim = null;
    private CvRect bBox;
    private CvPoint centeroid;
    public Visual2DObject()
    { // for feedback visualization entity that does NOT link to actual model
        objectImgRGBA = null;
        pModelDef = null;
        anim = null;
    }
    ~Visual2DObject()
    {
        objectImgRGBA.ReleaseData();
        objectImgRGBA = null;
        pModelDef = null;
        anim = null;
    }
    public Visual2DObject(ModelDef concepModel, bool loadTexture)
    { //initializing model        
        objectImgRGBA = null;
        //caculate seed point in the region
        CvPoint Yflipped = concepModel.centeroidAbsolute;
        //CvPoint seedPoint = GlobalRepo.YFlip2DinROI(Yflipped);
        CvPoint seedPoint = Yflipped;
        localAnchorPoint = new CvPoint(); // 1~100 scale
        GlobalPosition = new CvPoint();
        BuildingDone = false;
        ////      mBuilder = new ObjectShape2DBuilder(seedPoint);
        pModelDef = concepModel;
        bBox = new CvRect(0, 0, 0, 0);
        centeroid = new CvPoint(0, 0);
        objectImgRGBA = BlobAnalysis.ExtractBoundedBlobImage(pModelDef.getShapeBuilder().BuildingImg, ref bBox, ref centeroid);
        //    Debug.Log("Centeroid of visual2D object" + centeroid.X + " " + centeroid.Y);
        this.localAnchorPoint.X = (centeroid.X - bBox.X) * 100 / objectImgRGBA.Width;
        this.localAnchorPoint.Y = (centeroid.Y - bBox.Y) * 100 / objectImgRGBA.Height;
        this.GlobalPosition.X = centeroid.X;
        this.GlobalPosition.Y = centeroid.Y;
        BuildingDone = true;
       // objectImgRGBA.CvtColor(objectImgRGBA, ColorConversion.BgraToRgba);
        anim = null;
        if (loadTexture) loadTexturetoFillObject();
       
    }
    private void loadTexturetoFillObject()
    {
        Asset2DTexture texture;
        texture = GlobalRepo.GetTexture2D(pModelDef.modelType);
        if (texture == null)
        {
            Debug.Log("[ERROR] Failed to load texture of an object");
            return;
        }
        Debug.Log("Texture Loading...");
        ImageBlender2D.loadTexturewithAlphaSmallUnderlay(this.objectImgRGBA, texture.txtBGRAImg, new CvPoint(0, 0));
        createObjectOutline();
    }
    private void createObjectOutline()
    {
        CvMemStorage storage = new CvMemStorage();
        System.Random rnd = new System.Random();
        if (this.pModelDef.modelType==ModelCategory.Airways)
        {
            Point[] objcontourFull = this.pModelDef.getShapeBuilder().Fullcontour;
            Point[] objcontour=objcontourFull;
            if (objcontourFull == null || objcontourFull.Length == 0) return;
         
                 
            int lastPoint = 0;
            CvScalar lineColor = new CvScalar(192, 65, 0, 200);
            CvPoint[][] linePoints = new CvPoint[1][];
            
           // objcontour = Cv2.ApproxPolyDP(objcontourFull, 0.002f * Cv2.ArcLength(objcontourFull, false), true);
           // Debug.Log("Texture loading outline CNT full:"+objcontourFull.Length+"approx:" + objcontour.Length);
            while (lastPoint < objcontour.Length)
            {
                int rndStep = rnd.Next(objcontour.Length/15, objcontour.Length/10);

                ArrayList lineArray2 = new ArrayList();
                for (int i = lastPoint; i < lastPoint + rndStep && i < objcontour.Length; i++)
                {
                    lineArray2.Add(objcontourFull[i]);
                }
                Point[] linesegment = lineArray2.ToArray(typeof(Point)) as Point[];
           //     Debug.Log("Texture loading outline CNT full:" + linesegment.Length );
                linesegment = Cv2.ApproxPolyDP(linesegment, 0.04f * Cv2.ArcLength(linesegment, false), false);
             //   Debug.Log("Texture loading outline CNT approx:" + linesegment.Length);
                ArrayList lineArray = new ArrayList();
                foreach (var p in linesegment)
                {                    
                    lineArray.Add((CvPoint)p-this.bBox.TopLeft);
                }
           
                linePoints[0] = lineArray.ToArray(typeof(CvPoint)) as CvPoint[];
                
                this.objectImgRGBA.DrawPolyLine(linePoints, false, lineColor, rnd.Next(2,5),LineType.AntiAlias);
                lastPoint += (rndStep);
            }
        }
        storage.Dispose();
    }
    public void addFeedback_ShapeSuggestion(Point[] outlinesuggestion)
    {
        AnimationParam animParam = new AnimationParam();
        animParam.SuggestedShapeOutline = outlinesuggestion;
        animParam.frameSpeedReverse = 4;
        anim = new Animation2DClip(Animation2DType.ShapeSuggestion2D, objectImgRGBA, 30, this, animParam);

    }
    public void addFeedback_PositionSuggestion(Vector3 direction)
    {
        AnimationParam animParam = new AnimationParam();
        animParam.direction = direction;
        anim = new Animation2DClip(Animation2DType.PositionSuggestion2D, objectImgRGBA, 120, this, animParam);

    }
   
    public CvPoint addFeedback_WrongConnection(FeedbackToken feedback)
    {
        //extract and merge regions
        ModelDef obj1 = feedback.connectivity_missingConn.Key;
        ModelDef obj2 = feedback.connectivity_missingConn.Value;
        CvRect markerRegion = feedback.connectivity_wrongDesc.boundingBox;
        UserDescriptionInfo primaryDesc = feedback.connectivity_wrongDesc;
        //merge region
        double mergeDistance = 50;

        foreach (UserDescriptionInfo otherSigns in primaryDesc.StrConnectivitySignGroup)
        {            
                markerRegion = markerRegion.Union(otherSigns.boundingBox);            
        }
        markerRegion.Inflate(20, 20);
        Point[] overlapPoints;
        CvRect overlapPatch = CVProc.OverlappingRegions(obj1, obj2, out overlapPoints);

        
        AnimationParam animParam = new AnimationParam();
        animParam.userDescGroup = feedback.connectivity_wrongDesc.StrConnectivitySignGroup;
        animParam.borderPoints = overlapPoints;
        anim = new Animation2DClip(Animation2DType.ConnectivityIncorrect2D, objectImgRGBA, 90, this, animParam);

        return markerRegion.TopLeft;
    }
    public CvPoint addFeedback_MissingConnection(KeyValuePair<ModelDef,ModelDef> objects)
    {
        ModelDef obj1 = objects.Key;
        ModelDef obj2 = objects.Value;
        CvPoint ret = new CvPoint();
        if(obj1==null || obj2==null)
        {
            Debug.Log("[ERROR] Creating feedback for missing connection");
            return ret;
        }
        //find overlapping region
        Point[] overlapPoints;        
        CvRect overlapPatch = CVProc.OverlappingRegions(obj1, obj2, out overlapPoints);
        if (overlapPoints == null) return ret;
        Debug.Log("overlap points found N " + overlapPoints.Length);

        //overlapPatch.Inflate(60, 60);
        if (GlobalRepo.Setting_ShowDebugImgs())
        {
            using (CvMat candidateRegion = GlobalRepo.GetRepo(RepoDataType.dRawRegionRGBA).Clone())
            {
                foreach (var pp in overlapPoints)
                    candidateRegion.DrawCircle(pp, 2, new CvScalar(255, 0, 0, 255));
                GlobalRepo.showDebugImage("ConnBoundary", candidateRegion);
            }
        }

        AnimationParam animParam = new AnimationParam();
        animParam.AnchorPoint = overlapPatch.TopLeft;
    //    animParam.ImagePatch = candidateRegion;
        animParam.borderPoints = overlapPoints;
        

        anim = new Animation2DClip(Animation2DType.ConnectivityMissing2D, objectImgRGBA, 90, this, animParam);
        ret = overlapPatch.TopLeft;
        
        return ret;
    }
    public void addFeedback_MissingBehavior(FeedbackToken ft)
    {    

        AnimationParam animParam = new AnimationParam();
        animParam.AnchorPoint = ft.model.centeroidAbsolute;
     
        anim = new Animation2DClip(Animation2DType.BehaviorMissing2D, objectImgRGBA, 30, this, animParam);
    }
    public void addFeedback_UnnecessaryBehavior(FeedbackToken ft)
    {
        AnimationParam animParam = new AnimationParam();
        animParam.AnchorPoint = ft.behavior.marker.center;

        anim = new Animation2DClip(Animation2DType.BehaviorTakeOut2D, objectImgRGBA, 30, this, animParam);
    }
    public void addFeedback_RelocateBehavior(FeedbackToken ft)
    {
        AnimationParam animParam = new AnimationParam();
        animParam.AnchorPoint = ft.behavior.marker.center;
        animParam.destPoint = ft.model.centeroidAbsolute;

        anim = new Animation2DClip(Animation2DType.BehaviorRelocate2D, objectImgRGBA, 40, this, animParam);
    }
  
    public bool isBuildingDone()
    {
        return BuildingDone;
    }
    public void tick(CvMat objectOverlayDst)
    {
        if (objectOverlayDst != null)
        {
            if (anim != null) exportAnimationOverlay(objectOverlayDst);
            else exportObjectOverlay(objectOverlayDst);
        }
    }
    private void load2DTexturetoObjectNoDeformation(CvMat objImg)
    {
        Asset2DTexture texture;
        if (this.pModelDef.modelType == ModelCategory.Lung)
        {
            if (GlobalPosition.X < pModelDef.getShapeBuilder().BuildingImg.Width / 2)
                texture = GlobalRepo.GetTexture2D(ModelCategory.LungLeft);
            else
                texture = GlobalRepo.GetTexture2D(ModelCategory.LungRight);
        }
        else
            return;
        //get the contour
        if (texture == null || objImg == null) return;
        float objAspectRatio = (float)objImg.Width / (float)objImg.Height;
        float textureAspectRatio = (float)texture.txtBGRAImg.Width / (float)texture.txtBGRAImg.Height;
        //adjust texture img so as to fit with the target object image
        float adjTxtWidth, adjTxtHeight;
        if (objAspectRatio > textureAspectRatio)
        {
            adjTxtWidth = objImg.Width;
            adjTxtHeight = adjTxtWidth / textureAspectRatio;
        }
        else
        {
            adjTxtHeight = objImg.Height;
            adjTxtWidth = adjTxtHeight * textureAspectRatio;
        }
        CvMat adjTextureImg = new CvMat((int)adjTxtHeight, (int)adjTxtWidth, MatrixType.U8C4);
        adjTextureImg.Zero();
        texture.txtBGRAImg.Resize(adjTextureImg);
        float adjRatio = adjTxtWidth / texture.txtBGRAImg.Width;
        CvPoint adjCM = new CvPoint((int)(((float)texture.CM.X) * adjRatio), (int)(((float)texture.CM.Y) * adjRatio));
        CvRect idealTxtRegion = new CvRect(adjCM.X - this.localAnchorPoint.X, adjCM.Y - this.localAnchorPoint.Y, objImg.Width, objImg.Height);
        CvRect actualTxtRegion = new CvRect(Math.Max(idealTxtRegion.X, 0), Math.Max(idealTxtRegion.Y, 0), Math.Min(objImg.Width, idealTxtRegion.Width - Math.Max(idealTxtRegion.X, 0)), Math.Min(objImg.Height, idealTxtRegion.Height - Math.Max(idealTxtRegion.Y, 0)));
        CvMat croppedTxtRegion;
        croppedTxtRegion = adjTextureImg.GetSubRect(out croppedTxtRegion, actualTxtRegion);
        // ImageBlender2D.loadTexturewithAlpha(objImg, croppedTxtRegion, new CvPoint(localAnchorPoint.X - (adjCM.X - actualTxtRegion.X), localAnchorPoint.Y - (adjCM.Y - actualTxtRegion.Y)));
        ImageBlender2D.loadTexturewithAlphaSmallOverlay(objImg, adjTextureImg, new CvPoint(localAnchorPoint.X * objImg.Width / 100 - adjCM.X, localAnchorPoint.Y * objImg.Height / 100 - adjCM.Y));
        //   adjTextureImg.DrawCircle(adjCM, 3, CvColor.Red, 3);
        //    objImg.DrawCircle(localAnchorPoint, 3, CvColor.Red, 3);
        //   GlobalRepo.showDebugImage("adjustedTxT", adjTextureImg);
        //   GlobalRepo.showDebugImage("loadedTexture", objImg);

        //copy the texture

    }

    private void exportObjectOverlay(CvMat dst)
    {
        if (objectImgRGBA == null) return;
        CvPoint leftTopPosinCanvas = new CvPoint(GlobalPosition.X - localAnchorPoint.X * objectImgRGBA.Width / 100, GlobalPosition.Y - localAnchorPoint.Y * objectImgRGBA.Height / 100);
        //        ImageBlender2D.overlayImgRGBA(dst, this.objectImgRGBA, leftTopPosinCanvas);
        ImageBlender2D.overlayImgFrameAlphaImgRGBA(dst, this.objectImgRGBA, leftTopPosinCanvas, 255);
        Debug.Log("exporting object overlay...");
     //   GlobalRepo.showDebugImage("export object", dst);

    }
    private void exportAnimationOverlay(CvMat dst)
    {
        /*   CvMat animFrame = this.anim.getCurrentFrame();
           if (animFrame == null) return;

           CvPoint leftTopPosinCanvas = new CvPoint(GlobalPosition.X - localAnchorPoint.X * animFrame.Width / 100, GlobalPosition.Y - localAnchorPoint.Y * animFrame.Height / 100);*/
        AnimationFrameData fd = this.anim.getCurrentFrameData();
        if (fd == null) return;
        CvPoint leftTopPosinCanvas = new CvPoint(fd.GlobalAnchor.X - fd.LocalAnchor.X * fd.frameImage.Width / 100, fd.GlobalAnchor.Y - fd.LocalAnchor.Y * fd.frameImage.Height / 100);
        // ImageBlender2D.overlayImgRGBA(dst, fd.frameImage, leftTopPosinCanvas);
        ImageBlender2D.overlayImgFrameAlphaImgRGBA(dst, fd.frameImage, leftTopPosinCanvas, fd.frameAlpha);

    }


    private void debugObjectImage()
    {
        if (this.debugWindow == null)
        {
            System.Random rnd = new System.Random();
            debugWindow = new CvWindow("2DVisualObject" + rnd.Next(0, 999));
        }
        if (objectImgRGBA != null) debugWindow.ShowImage(objectImgRGBA);
    }

  
}
