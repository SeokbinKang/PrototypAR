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
public class FBSModel {



    public string underlyingImageResName;

    int EntityCounter = 0;
    //structure layer
    List<StructureEntity> mStructureList;
    List<BehaviorEntity> mBehaviorList;
    int[,] connectivityMetric = null;
    public static DesignContent ContentType;
    private ShapeEvalType[] ShapeEvaluationMapTable;
    private StrEntityEvalType[] StrEntityEvalMapTalbe;
    public static FBSModel activeFBSInstance = null;


    public List<FeedbackToken> EvaluatePrototype(prototypeDef userPrototype)
    {
        //This on returns most impending feedback. So the evaluation process halts when an issue is deteced
        List<FeedbackToken> ret = new List<FeedbackToken>();
        List<FeedbackToken> feedbacklist;
        //phase 3 check existence
        if (SystemConfig.ActiveInstnace.Get(EvaluationConfigItem._Structure_Missing_objects)
            || SystemConfig.ActiveInstnace.Get(EvaluationConfigItem._Structure_Extra_objects))
        {
            feedbacklist = EvaluateStrExistence(userPrototype);

            ret.AddRange(feedbacklist);  //DEBUG
        }
        if (SystemConfig.ActiveInstnace.Get(EvaluationConfigItem._Structure_shape))
        {
            feedbacklist = EvaluateStrShape(userPrototype);
            ret.AddRange(feedbacklist);
        }
        if (SystemConfig.ActiveInstnace.Get(EvaluationConfigItem._Structure_position))
        {
            feedbacklist = EvaluateStrPosition(userPrototype);
            ret.AddRange(feedbacklist);
        }
        if (SystemConfig.ActiveInstnace.Get(EvaluationConfigItem._Structure_connectivity))
        {
            feedbacklist = EvaluateStrConnectivity(userPrototype);
            ret.AddRange(feedbacklist);
        }
        if (SystemConfig.ActiveInstnace.Get(EvaluationConfigItem._Behavior_Missing_labels)
            || SystemConfig.ActiveInstnace.Get(EvaluationConfigItem._Behavior_Move_labels)
            || SystemConfig.ActiveInstnace.Get(EvaluationConfigItem._Behavior_Extra_labels))
        {
            feedbacklist = EvaluateBehavior(userPrototype);

            ret.AddRange(feedbacklist);  //DEBUG
        }
        if (SystemConfig.ActiveInstnace.Get(EvaluationConfigItem._Behavior_Unmarked_labels))
        {
            feedbacklist = EvaluateBehaviorVariables(userPrototype);

            ret.AddRange(feedbacklist);  //DEBUG
        }


        //phase 1 check shape


        //phase 2 check position



        //phase 4 check connectivity

        //phase 5 check behavior mapping

        //phase 6 check behavior variable
        //userPrototype.mModels[ModelCategory.Lung]
        //if(feedbacklist!=null) ret.AddRange(feedbacklist);
        //   debugFeedback(feedbacklist);

        debugFeedback(ret);
        return ret;
    }
    public FBSModel()
    {
        EntityCounter = 0;
        initFBSModel();
        ContentType = DesignContent.NONE;
        activeFBSInstance = this;
    }
    public FBSModel(DesignContent t)
    {
        ContentType = t;
        mStructureList = new List<StructureEntity>();
        mBehaviorList = new List<BehaviorEntity>();
        initFBSModel();
        activeFBSInstance = this;
    }
  
    private void initFBSModel()
    {
        ShapeEvaluationMapTable = new ShapeEvalType[(int)ModelCategory.TotalNumberOfModels+1];
        ShapeEvaluationMapTable[(int)ModelCategory.LungLeft] = ShapeEvalType.ActualShape;
        ShapeEvaluationMapTable[(int)ModelCategory.LungRight] = ShapeEvalType.ActualShape;
        ShapeEvaluationMapTable[(int)ModelCategory.Lung] = ShapeEvalType.ActualShape;
        ShapeEvaluationMapTable[(int)ModelCategory.Airways] = ShapeEvalType.DoNot;
        ShapeEvaluationMapTable[(int)ModelCategory.Diaphragm] = ShapeEvalType.AspectRatio;


        ShapeEvaluationMapTable[(int)ModelCategory.FrontChainring] = ShapeEvalType.ActualShape;
        ShapeEvaluationMapTable[(int)ModelCategory.RearSprocket] = ShapeEvalType.ActualShape;
        ShapeEvaluationMapTable[(int)ModelCategory.Chain] = ShapeEvalType.DoNot;
        ShapeEvaluationMapTable[(int)ModelCategory.PedalCrank] = ShapeEvalType.DoNot;
        
        ShapeEvaluationMapTable[(int)ModelCategory.Fish] = ShapeEvalType.DoNot;
        ShapeEvaluationMapTable[(int)ModelCategory.Plant] = ShapeEvalType.DoNot;
        ShapeEvaluationMapTable[(int)ModelCategory.Bacteria] = ShapeEvalType.DoNot;
        ShapeEvaluationMapTable[(int)ModelCategory.AirPump] = ShapeEvalType.DoNot;

        ShapeEvaluationMapTable[(int)ModelCategory.C4_lens] = ShapeEvalType.ActualShape;
        ShapeEvaluationMapTable[(int)ModelCategory.C4_shutter] = ShapeEvalType.ActualShape;
        ShapeEvaluationMapTable[(int)ModelCategory.C4_sensor] = ShapeEvalType.DoNot;        

        //set color pallete accordingly
        StrEntityEvalMapTalbe = new StrEntityEvalType[(int)ModelCategory.TotalNumberOfModels + 1];
        StrEntityEvalMapTalbe[(int)ModelCategory.LungLeft] = StrEntityEvalType.OneonOne;
        StrEntityEvalMapTalbe[(int)ModelCategory.LungRight] = StrEntityEvalType.OneonOne;
        StrEntityEvalMapTalbe[(int)ModelCategory.Lung] = StrEntityEvalType.OneonOne;
        StrEntityEvalMapTalbe[(int)ModelCategory.Airways] = StrEntityEvalType.OneonOne;
        StrEntityEvalMapTalbe[(int)ModelCategory.Diaphragm] = StrEntityEvalType.OneonOne;


        StrEntityEvalMapTalbe[(int)ModelCategory.FrontChainring] = StrEntityEvalType.OneonOne;
        StrEntityEvalMapTalbe[(int)ModelCategory.RearSprocket] = StrEntityEvalType.OneonOne;
        StrEntityEvalMapTalbe[(int)ModelCategory.Chain] = StrEntityEvalType.OneonOne;
        StrEntityEvalMapTalbe[(int)ModelCategory.LowerChain] = StrEntityEvalType.OneonOne;
        StrEntityEvalMapTalbe[(int)ModelCategory.UpperChain] = StrEntityEvalType.OneonOne;
        StrEntityEvalMapTalbe[(int)ModelCategory.PedalCrank] = StrEntityEvalType.OneonOne;

        StrEntityEvalMapTalbe[(int)ModelCategory.Fish] = StrEntityEvalType.atLeastOne;
        StrEntityEvalMapTalbe[(int)ModelCategory.Plant] = StrEntityEvalType.atLeastOne;
        StrEntityEvalMapTalbe[(int)ModelCategory.Bacteria] = StrEntityEvalType.atLeastOne;
        StrEntityEvalMapTalbe[(int)ModelCategory.AirPump] = StrEntityEvalType.atLeastOne;

        StrEntityEvalMapTalbe[(int)ModelCategory.C4_lens] = StrEntityEvalType.OneonOne;
        StrEntityEvalMapTalbe[(int)ModelCategory.C4_sensor] = StrEntityEvalType.OneonOne;
        StrEntityEvalMapTalbe[(int)ModelCategory.C4_shutter] = StrEntityEvalType.OneonOne;

    }
    public int AddStructureEntity(StructureEntity se)
    {
        se.v4_StrcutureEntityIndex = EntityCounter;        
        EntityCounter++;
        mStructureList.Add(se);
        return EntityCounter;
    }
    public int AddBehaviorEntity(BehaviorEntity be)
    {
        mBehaviorList.Add(be);
        return mBehaviorList.Count();

    }
    public int AddConnectivity(StructureEntity a, StructureEntity b)
    {
        if(EntityCounter<=0 || a.v4_StrcutureEntityIndex >= EntityCounter || b.v4_StrcutureEntityIndex >= EntityCounter)
        {
            Debug.Log("[ERROR] FBS Model - Addconnectivity");
            return 0;
        }
        if (connectivityMetric == null) connectivityMetric = new int[EntityCounter,EntityCounter];
        connectivityMetric[a.v4_StrcutureEntityIndex, b.v4_StrcutureEntityIndex]++;  //symmetric connectivity.
        connectivityMetric[b.v4_StrcutureEntityIndex, a.v4_StrcutureEntityIndex]++;
        return 1;
    }

    private void debugFeedback(List<FeedbackToken> feedbacklist)
    {
        Debug.Log("Total of Feedback Generated : " + feedbacklist.Count);
        foreach(FeedbackToken token in feedbacklist)
        {
            token.DebugPrint();
        }
    }
    public List<FeedbackToken> EvaluateBehaviorVariables(prototypeDef userPrototype)
    {
        List<FeedbackToken> ret = new List<FeedbackToken>();
        Debug.Log("Evaluating BVs....1");
        if (userPrototype.mBehaviors == null) return ret;
        Debug.Log("Evaluating BVs....2");
        foreach(var bdesign in userPrototype.mBehaviors)
        {
            foreach(var bmodel in this.mBehaviorList)
            {
                if (bdesign.behaviorType != bmodel.behaviorType) continue;
                if (bmodel.VariableEntity == null) continue;  //does not have BV to describe
                //BV has to exist

                if(bmodel.VariableEntity.VariableType==BehaviorVariableType.Numeric)
                {
                    Debug.Log("[DEBUG] BV pair found numberic value=" + bdesign.marker.InfoNumericalBVPercent);
                    if(bdesign.marker.InfoNumericalBVPercent<0)
                    {
                        //generate feedback
                        FeedbackToken token = new FeedbackToken(EvaluationResultCategory.Behavior_variableUnchecked,bdesign);
                        ret.Add(token);
                        break;
                    }
                }
                if (bmodel.VariableEntity.VariableType == BehaviorVariableType.Categorical)
                {
                    Debug.Log("[DEBUG] BV pair found categorical value=" );
                    if (bdesign.marker.InfoCategoricalBVValue=="")
                    {
                        //generate feedback
                        FeedbackToken token = new FeedbackToken(EvaluationResultCategory.Behavior_variableUnchecked, bdesign);
                        ret.Add(token);
                        break;
                    }
                }

            }
        }
        return ret;
    }
    public List<FeedbackToken> EvaluateBehavior(prototypeDef userPrototype)
    {
        List<FeedbackToken> ret = new List<FeedbackToken>();
        if (userPrototype.mModels == null) return ret;

        foreach (var designItem in userPrototype.mModels)
        {
            if (designItem.Value == null) continue;            
            foreach (var modelInstance in designItem.Value)
            {
                foreach (var str in this.mStructureList)
                {
                    if(str.v1_ModelType==modelInstance.modelType)
                    {
                        Debug.Log("[DEBUG] examining userStrObject id =" + modelInstance.instanceID+" type :"+Content.getOrganName(modelInstance.modelType) + " FBS str's behavior ");
                        if(str.v5_BehaviorEntity!=null && (modelInstance.behaviors==null || modelInstance.behaviors.Count==0))
                        {
                            //missing behavior
                            FeedbackToken token = new FeedbackToken(EvaluationResultCategory.Behavior_missing, modelInstance, str.v5_BehaviorEntity.behaviorType, null); 
                            ret.Add(token);
                            break;
                        }  else
                        {
                            //check if correct                            
                            if(str.v5_BehaviorEntity.behaviorType!= modelInstance.behaviors[0].behaviorType)
                            {
                                //incorrect behavior
                                Debug.Log("[DEBUG] Incorrect Behavior Label: "+Content.getBaviorLabelText(str.v5_BehaviorEntity.behaviorType));
                                //check if there's another str that has wrong behavior other than this behavior
                                ModelDef candidateModel = findUnmappedStrModelforBehavior(userPrototype, modelInstance.behaviors[0].behaviorType);
                                if(candidateModel==null)
                                { //unnecessary behavior

                                    if (SystemConfig.ActiveInstnace.Get(EvaluationConfigItem._Behavior_Extra_labels))
                                    {
                                        FeedbackToken token = new FeedbackToken(EvaluationResultCategory.Behavior_Unnecessary, null, modelInstance.behaviors[0].behaviorType, modelInstance.behaviors[0]);
                                        ret.Add(token);
                                    }
                                    break;
                                }
                                else
                                { //remap required
                                    if (SystemConfig.ActiveInstnace.Get(EvaluationConfigItem._Behavior_Move_labels))
                                    {
                                        FeedbackToken token = new FeedbackToken(EvaluationResultCategory.Behavior_relocate, candidateModel, modelInstance.behaviors[0].behaviorType, modelInstance.behaviors[0]);
                                        ret.Add(token);
                                    }
                                    break;
                                }



                            }
                        }
                        
                    }
                }
                
            }


        }
        //evaluate behavior existence

        //evaluate behavior-structure mapping

        //evaluate behavior variable setting

        return ret;

    }
    private ModelDef findUnmappedStrModelforBehavior(prototypeDef userPrototype, BehaviorCategory bc)
    {
        ModelDef ret = null;
        foreach (var behav in this.mBehaviorList)
        {
            if (behav.behaviorType != bc) continue;
            ModelCategory rightModelType = behav.AssociatedModel.v1_ModelType;
            if (!userPrototype.mModels.ContainsKey(rightModelType)) continue;
            if (userPrototype.mModels[rightModelType] == null) continue;
            foreach(var candidateModel in userPrototype.mModels[rightModelType])
            {
                if (candidateModel.behaviors == null || candidateModel.behaviors.Count==0) {
                    ret = candidateModel;
                    break;
                }
                if(candidateModel.behaviors[0].behaviorType!=bc)
                {
                    ret = candidateModel;
                    break;
                }
            }

        }
        return ret;
    }    
   
    public List<FeedbackToken> EvaluateStrConnectivity(prototypeDef userPrototype)
    {
        List<FeedbackToken> ret = new List<FeedbackToken>();
        int[,] evaluationMat = connectivityMetric.Clone() as int[,];
        if (userPrototype.mConnections != null)
        {
            foreach (var conn in userPrototype.mConnections)
            {
                int objectID1 = conn.StrConnectivity.Key;
                int objectID2 = conn.StrConnectivity.Value;
                ModelDef object1 = userPrototype.getModelDefbyID(objectID1);
                ModelDef object2 = userPrototype.getModelDefbyID(objectID2);
                int idx1 = this.GetStructureIndex(object1);
                int idx2 = this.GetStructureIndex(object2);
                if (idx1 == -1 || idx2 == -1)
                {
                    Debug.Log("[ERROR] couldn't find structureindex for connected objects");
                    continue;
                }
                if (evaluationMat[idx1, idx2] > 0)
                {
                    evaluationMat[idx1, idx2] = 0;
                    evaluationMat[idx2, idx1] = 0;
                    continue;
                }
                if (connectivityMetric[idx1, idx2] == 0)
                {
                    FeedbackToken token = new FeedbackToken(EvaluationResultCategory.Connectivity_wrong, conn, new KeyValuePair<ModelDef, ModelDef>(object1, object2));
                    ret.Add(token);
                }
            }
        }
        for (int x = 0; x < evaluationMat.GetLength(0); x += 1)
        {
            for (int y = x+1; y < evaluationMat.GetLength(1); y += 1)
            {
                if(evaluationMat[x,y]>0)
                {
                    //TODO : what if model for a type doesn't exist...can't pass just NULL model
                    KeyValuePair<ModelDef, ModelDef> kp = new KeyValuePair<ModelDef, ModelDef>(userPrototype.getModelDefbyType(mStructureList[x].v1_ModelType), userPrototype.getModelDefbyType(mStructureList[y].v1_ModelType));
                        if (kp.Key != null && kp.Value != null) {
                        FeedbackToken token = new FeedbackToken(EvaluationResultCategory.Connectivity_missing, null, kp);
                        ret.Add(token);
                    }
                   
                }
            }
        }

        return ret;
    }
    public List<FeedbackToken> EvaluateStrPosition(prototypeDef userPrototype)
    {
        List<FeedbackToken> ret = new List<FeedbackToken>(); ;
        if (userPrototype.mModels == null) return ret;
        foreach (var designItem in userPrototype.mModels)
        {        
                foreach (var model in designItem.Value)
                {
                    //find most close and same-type object in FBS model
                    Vector3 suggestedDirection = getPositionSuggestion(model);
                    if (suggestedDirection.magnitude == 0) continue;
                Debug.Log(suggestedDirection);
                    //compute direction

                    //create feedback token.                    
                    FeedbackToken token = new FeedbackToken(EvaluationResultCategory.Position_direction, model.instanceID,suggestedDirection);
                    ret.Add(token);
                }
       
        }

        //    Debug.Log("Centeroid of visual2D object" + centeroid.X + " " + centeroid.Y);


        return ret;
    }
    public List<FeedbackToken> EvaluateStrShape(prototypeDef userPrototype)
    {
        List<FeedbackToken> ret = new List<FeedbackToken>(); ;
        if (userPrototype.mModels == null) return ret;
        int dbg = 0;
        foreach (var designItem in userPrototype.mModels)
        {
            if (ShapeEvaluationMapTable[(int)designItem.Key] == ShapeEvalType.ActualShape)
            {
                for(int i=0;i< designItem.Value.Count; i++) 
                {
                     ModelDef model = designItem.Value[i];
                    CvRect bBox = new CvRect(0, 0, 0, 0);
                    CvPoint centeroid = new CvPoint(0, 0);
                    CvMat objectImgRGBA = BlobAnalysis.ExtractBoundedBlobImage(model.getShapeBuilder().BuildingImg, ref bBox, ref centeroid);
                    Asset2DTexture virtualObj = GlobalRepo.GetTexture2D(model.modelType);
                    bool forceConvextoUserObject = Content.ForceConvexShape(model.modelType);                    
                    Point[] suggestedShapeOutline = IP_ObjectShape.IPMatchObjectShapes(objectImgRGBA, virtualObj.txtBGRAImg,forceConvextoUserObject);
                    if (suggestedShapeOutline == null) continue; //the shape is good!
                    FeedbackToken token = new FeedbackToken(EvaluationResultCategory.Shape_suggestion, model.instanceID, suggestedShapeOutline);
                    ret.Add(token);

                    //update truthshape for final simulation
                    model.TruthShapeContourinRegionCoord = new CvPoint[suggestedShapeOutline.Length];
                    int idx = 0;
                    foreach(var p in suggestedShapeOutline)
                    {
                        model.TruthShapeContourinRegionCoord[idx] = p;
                        model.TruthShapeContourinRegionCoord[idx] = model.TruthShapeContourinRegionCoord[idx] + bBox.TopLeft;
                    }
                    
                }
            }
        }
          
        //    Debug.Log("Centeroid of visual2D object" + centeroid.X + " " + centeroid.Y);
      

        return ret;
    }


    public VirtualPosType getModelsVirtualPosType(ModelDef model)
    {
        VirtualPosType ret = VirtualPosType.none;
        double minDistance = float.MaxValue;
        double dist;
        StructureEntity closestStructure = null;
        foreach (var modelItem in this.mStructureList)
        {
            if (model.modelType != modelItem.v1_ModelType) continue;
            CvPoint diff = modelItem.v3_position.getRegionPosition() - model.centeroidAbsolute;
            dist = Math.Sqrt(diff.X * diff.X + diff.Y * diff.Y);
            if (dist < minDistance)
            {
                closestStructure = modelItem;
                minDistance = dist;
            }
        }
        if (closestStructure == null) return ret;
        return closestStructure.v6_VirtualPositionType;
    }
    public Vector3 getClosestModelsTruthScreenPos(ModelDef model)
    {
        Vector3 ret = new Vector3();        
        double minDistance = float.MaxValue;
        double dist;
        StructureEntity closestStructure = null;
        foreach (var modelItem in this.mStructureList)
        {
            if (model.modelType != modelItem.v1_ModelType) continue;            
            CvPoint diff = modelItem.v3_position.getRegionPosition() - model.centeroidAbsolute;
            dist = Math.Sqrt(diff.X * diff.X + diff.Y * diff.Y);
            if (dist < minDistance)
            {
                closestStructure = modelItem;
                minDistance = dist;
            }
        }
        if (closestStructure == null) return ret;
        ret = closestStructure.v3_position.getScreenPosition();

        return ret;

    }
    public StructurePosVariable getClosestTruthModelPos(ModelDef model)
    {
        StructurePosVariable ret = null;
        double minDistance = float.MaxValue;
        double dist;
        StructureEntity closestStructure = null;
        foreach (var modelItem in this.mStructureList)
        {
            if (model.modelType != modelItem.v1_ModelType) continue;
            if (modelItem.v3_position.v3_posType == PosEvalType.None) continue;
            CvPoint diff = modelItem.v3_position.getRegionPosition() - model.centeroidAbsolute;
            dist = Math.Sqrt(diff.X * diff.X + diff.Y * diff.Y);
            if (dist < minDistance)
            {
                closestStructure = modelItem;
                minDistance = dist;
            }
        }
        if (closestStructure == null) return null;
        return closestStructure.v3_position;
        
    }
    private Vector3 getPositionSuggestion(ModelDef model)
    {
        Vector3 direction = new Vector3(0,0,0);
        double minDistance = 9999999999;
        double dist = 99999; ;
        StructureEntity closestStructure = null;
        
        foreach (var modelItem in this.mStructureList)
        {
            if (model.modelType != modelItem.v1_ModelType) continue;
            if (modelItem.v3_position.v3_posType == PosEvalType.None) continue;
            dist = minDistance;

            //    Debug.Log("BG center screen coord : " + screenPos + "Screen:" + Screen.width + "/" + Screen.height+"\t region"+ regionPos+"\tregionBox"+GlobalRepo.GetRegionBox(false).Size);

            //CvPoint diff = modelItem.v3_position.getAbsolutePosition() - model.centeroidAbsolute;
            CvPoint diff = modelItem.v3_position.getRegionPosition() - model.centeroidAbsolute;
            Point truthPos = modelItem.v3_position.getRegionPosition();
            if (modelItem.v3_position.v3_posType == PosEvalType.ContourProximitytoPoint)
            {
                //caculate closest contour point to the position.
                
                dist= Cv2.PointPolygonTest(model.getShapeBuilder().ReducedContour, truthPos, true);
                dist = dist * -1;
                direction.x = diff.X;
                direction.y = diff.Y;
                direction.z = 0;
            }
            else if (modelItem.v3_position.v3_posType == PosEvalType.CentralProximitytoPoint)
            {

                dist = Math.Sqrt(diff.X * diff.X + diff.Y * diff.Y);
                direction.x = diff.X;
                direction.y = diff.Y;
                direction.z = 0;


            } else if (modelItem.v3_position.v3_posType == PosEvalType.HorizontalProximitytoPoint)
            {

                dist = Math.Abs(diff.X);
                direction.x = diff.X;
                direction.y = 0;
                direction.z = 0;

            } else if (modelItem.v3_position.v3_posType == PosEvalType.VerticalProximitytoPoint)
            {
                dist = Math.Abs(diff.Y);
                direction.y = diff.Y;
                direction.x = 0;
                direction.z = 0;

            }
            
            if (dist < minDistance)
            {
                closestStructure = modelItem;
                minDistance = dist;
            }
        }
        if (closestStructure == null) return new Vector3(0, 0, 0);
        if (dist < closestStructure.v3_position.v3_param_refPosition2DRange) return new Vector3(0,0,0);
        double magnitude = Math.Sqrt(direction.x * direction.x + direction.y * direction.y);
        double normalizedMagnitude = 5;
        direction.x = direction.x * (float)(normalizedMagnitude / magnitude);
        direction.y = direction.y * (float)(normalizedMagnitude / magnitude);
        
        return direction;

    }
    public List<FeedbackToken> EvaluateStrExistence(prototypeDef userPrototype)
    {
        List<FeedbackToken> ret = new List<FeedbackToken>(); ;

        int[] objCounter = new int[(int)ModelCategory.TotalNumberOfModels+1];

        //evaluate missing object
        if (userPrototype.mModels == null) return ret;
        foreach (var modelItem in this.mStructureList)
        {
            objCounter[(int)modelItem.v1_ModelType]++;
        }
        foreach(var designItem in userPrototype.mModels)
        {
            if (designItem.Value == null) continue;
            objCounter[(int)designItem.Key] -= designItem.Value.Count;
            if (objCounter[(int)designItem.Key] < 0 && StrEntityEvalMapTalbe[(int)designItem.Key]==StrEntityEvalType.OneonOne)
            {
                int numberofValidObjects = designItem.Value.Count + objCounter[(int)designItem.Key];
                //sort objects by distance to the desired points
                ModelDef[] modelarr = designItem.Value.ToArray();
                for (int i = 0; i < modelarr.Length; i++)
                {
                    int distance=0;
                    int NCandidateStr=0;
                    foreach(var truthStr in this.mStructureList)
                    {
                        if (truthStr.v1_ModelType == designItem.Key)
                        {
                            NCandidateStr++;
                            distance += truthStr.v3_position.getDistanceto(modelarr[i].getShapeBuilder().center);                                
                        }
                    }
                    modelarr[i].tmpInt = distance / NCandidateStr;
                }

                modelarr.OrderByDescending(x => x.tmpInt);
                int NofRedundantObjs = (objCounter[(int)designItem.Key] * -1);
                if (SystemConfig.ActiveInstnace.Get(EvaluationConfigItem._Structure_Extra_objects))
                {
                    for (int i = 0; i < NofRedundantObjs; i++)
                    {
                        FeedbackToken token = new FeedbackToken(EvaluationResultCategory.Shape_existence_redundant, modelarr[i].modelType, modelarr[i].instanceID, modelarr[i]);
                        ret.Add(token);
                    }
                }               
            }

        }
        //evaluate excessive object
        if (SystemConfig.ActiveInstnace.Get(EvaluationConfigItem._Structure_Missing_objects))
        {
            for (int i = 0; i < (int)ModelCategory.TotalNumberOfModels; i++)
            {
                if (objCounter[i] == 0) continue;
                if (objCounter[i] > 0)
                {
                    //missing object
                    FeedbackToken token = new FeedbackToken(EvaluationResultCategory.Shape_existence_missing, (ModelCategory)i, -1, null);
                    ret.Add(token);
                }

            }
        }

        return ret;

    }
    private int GetStructureIndex(ModelDef m)
    {
        int ret = -1;

        foreach (var modelItem in this.mStructureList)
        {

            if (m.modelType == modelItem.v1_ModelType) ret = modelItem.v4_StrcutureEntityIndex;
        }


        return ret;
    }
    public BehaviorVariableEntity GetBehaviorEntity(BehaviorCategory bc)
    {
        BehaviorVariableEntity ret = null;
        if (this.mBehaviorList == null) return ret;
        foreach(var be in this.mBehaviorList)
        {
            if(be.behaviorType==bc)
            {
                ret = be.VariableEntity;
            }
        }
        return ret;
    }
}
public class BehaviorEntity
{
    public BehaviorCategory behaviorType;
    public StructureEntity AssociatedModel = null;    
    public BehaviorVariableEntity VariableEntity = null;
    public BehaviorEntity(BehaviorCategory behavior, StructureEntity AssoModel, BehaviorVariableEntity bv)
    {
        this.behaviorType = behavior;
        this.AssociatedModel = AssoModel;
        if (AssoModel != null) AssoModel.v5_BehaviorEntity = this;
        this.VariableEntity = bv;        
    }
    public bool hasBV()
    {
        return this.VariableEntity == null;
    }
}
public class BehaviorVariableEntity
{
    public BehaviorVariableType VariableType;
    public KeyValuePair<float, float> numericalValueRange;
    public List<string> CategoricalValueList;
    public BehaviorVariableEntity(BehaviorVariableType t, KeyValuePair<float, float> range)
    {
        this.VariableType = t;
        this.numericalValueRange = range;        
    }
    public BehaviorVariableEntity(BehaviorVariableType t, List<string> categorylist)
    {
        this.VariableType = t;
        this.CategoricalValueList = categorylist;
    }

}
public class StructureEntity
{
    //structure varialble
    public ModelCategory v1_ModelType=ModelCategory.None;
    public Asset2DTexture v2_refernceModel2DImage=null;
    public StructurePosVariable v3_position = null;
    public int v4_StrcutureEntityIndex;
    public BehaviorEntity v5_BehaviorEntity = null;
    public VirtualPosType v6_VirtualPositionType;
    public StructureEntity(ModelCategory mCat, StructurePosVariable pos)
    {
        this.v1_ModelType = mCat;
        this.v2_refernceModel2DImage = GlobalRepo.GetTexture2D(ModelCategory.LungLeft);
        if(this.v2_refernceModel2DImage==null)
        {
            Debug.Log("[ERROR] Failed to load designContent, reference 2DModel");
            return;
        }
        this.v3_position = pos;

    }

}
public class StructurePosVariable
{
    public PosEvalType v3_posType;
    public CvPoint v3_refPosition2D_Relative;
    private CvPoint v3_refPosition2D_AbsoluteinRegion;

    public Vector2 v3_refPosition2D_BGFrame;
    private PreLoadedObjects v3_BGFrameObj=PreLoadedObjects.None;
    public float v3_param_refPosition2DRange = 50;
   

    public StructurePosVariable()
    {
        v3_refPosition2D_AbsoluteinRegion = new CvPoint(-1,-1);

    }
    public StructurePosVariable(PosEvalType v3a_type, PreLoadedObjects frameObj,Vector2 pivotinBGFrame, float v3c_range)
    {
        this.v3_posType = v3a_type;
        this.v3_refPosition2D_BGFrame = pivotinBGFrame;
        this.v3_param_refPosition2DRange = v3c_range;
        v3_BGFrameObj = frameObj;
        v3_refPosition2D_AbsoluteinRegion = new CvPoint(-1, -1);
    }
  
    public CvPoint getRegionPosition()
    {
        CvPoint ret = new CvPoint();
        if (v3_BGFrameObj == PreLoadedObjects.None) return ret;
        Vector3 screenPos = new Vector3();
        SceneObjectManager.MeasureObjectPointinScreen(v3_BGFrameObj, v3_refPosition2D_BGFrame, ref screenPos);
        ret = SceneObjectManager.ScreentoRegion(screenPos);
        return ret;
    }
    public Vector3 getScreenPosition()
    {
        Vector3 ret = new Vector3();
        if (v3_BGFrameObj == PreLoadedObjects.None) return ret;
        Vector3 screenPos = new Vector3();
        SceneObjectManager.MeasureObjectPointinScreen(v3_BGFrameObj, v3_refPosition2D_BGFrame, ref screenPos);
        ret = screenPos;
        return ret;
    }
    public int getDistanceto(CvPoint posinRegionBox)
    {
        int ret = -1;
        if (posinRegionBox == null) return ret;
        CvPoint truthPos = getRegionPosition();        
        ret = (int)truthPos.DistanceTo(posinRegionBox);
        return ret;
    }
    public CvPoint getAbsolutePosition()
    {
        if (v3_refPosition2D_AbsoluteinRegion.X == -1) v3_refPosition2D_AbsoluteinRegion = GlobalRepo.translateRelativePointToAbsoluteinRegion(this.v3_refPosition2D_Relative);
        return v3_refPosition2D_AbsoluteinRegion;
    }

    public StructurePosVariable(PosEvalType v3a_type, CvPoint v3b_refpos, int v3c_range)
    {
        this.v3_posType = v3a_type;
        this.v3_refPosition2D_Relative = v3b_refpos;
        this.v3_param_refPosition2DRange = v3c_range;
        v3_refPosition2D_AbsoluteinRegion = new CvPoint(-1, -1);
    }
    public int getDistancetoOBSOLTE(CvPoint posinRegionBox)
    {
        int ret = -1;
        if (posinRegionBox == null) return ret;
        if (v3_refPosition2D_AbsoluteinRegion.X == -1) v3_refPosition2D_AbsoluteinRegion = GlobalRepo.translateRelativePointToAbsoluteinRegion(this.v3_refPosition2D_Relative);
        ret = (int) posinRegionBox.DistanceTo(v3_refPosition2D_AbsoluteinRegion);
        return ret;
    }
  

}

public enum VirtualPosType
{
    none,
    AlignWithPhysicalPrototype,
    AlignWithVirtualBG
}
