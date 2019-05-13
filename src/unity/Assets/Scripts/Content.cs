﻿using UnityEngine;
using System.Collections;
using OpenCvSharp;
using OpenCvSharp.Blob;
using OpenCvSharp.CPlusPlus;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System;

public class Content {

  
    private static List<KeyValuePair<BehaviorCategory,string>> behaviorDefinitionList=null;
    private static List<KeyValuePair<BehaviorCategory, List<string>>> CategoricalBVDefs = null;
    public static string getOrganName(ModelCategory mc)
    {
        if (mc == ModelCategory.None) return "undefined";
        if (mc == ModelCategory.Airways) return "airways";
        
        if (mc == ModelCategory.Diaphragm) return "diaphragm";
        if (mc == ModelCategory.Lung) return "lungs";
        if (mc == ModelCategory.LungLeft) return "lungLeft";
        if (mc == ModelCategory.LungRight) return "lungRight";
        if (mc == ModelCategory.Stomach) return "stomach";
        if (mc == ModelCategory.FrontChainring) return "FrontChainring";
        if (mc == ModelCategory.RearSprocket) return "FreeWheel";
        if (mc == ModelCategory.Chain) return "Chain";
        if (mc == ModelCategory.UpperChain) return "UpperChain";
        if (mc == ModelCategory.LowerChain) return "LowerChain";
        if (mc == ModelCategory.PedalCrank) return "PedalCrank";
        if (mc == ModelCategory.Fish) return "Fish";
        if (mc == ModelCategory.Bacteria) return "Bacteria";
        if (mc == ModelCategory.Plant) return "Plant";
        if (mc == ModelCategory.AirPump) return "AirPump";
        if (mc == ModelCategory.C4_lens) return "Lens";
        if (mc == ModelCategory.C4_sensor) return "Sensor";
        if (mc == ModelCategory.C4_shutter) return "Shutter";

        return "undefined";

    }
    public static string getBaviorLabelText(BehaviorCategory bc)
    {
        if (bc == BehaviorCategory.Empty) return "";
        if (bc == BehaviorCategory.CONSUME) return "CONSUME";
        if (bc == BehaviorCategory.CONTRACT) return "CONTRACT";
        if (bc == BehaviorCategory.CONVERT) return "CONVERT";
        if (bc == BehaviorCategory.DIFFUSE) return "DIFFUSE";
        if (bc == BehaviorCategory.DRIVE) return "DRIVE";
        if (bc == BehaviorCategory.EAT) return "EAT";
        if (bc == BehaviorCategory.PEDAL) return "PEDAL";
        if (bc == BehaviorCategory.PASS) return "PASS";
        if (bc == BehaviorCategory.REDUCE) return "REDUCE";
        if (bc == BehaviorCategory.ROTATE) return "ROTATE";
        if (bc == BehaviorCategory.ENGAGE) return "ENGAGE";        
        if (bc == BehaviorCategory.PROPEL) return "PROPEL";
        if (bc == BehaviorCategory.SUPPLY) return "SUPPLY";
        if (bc == BehaviorCategory.TRANSFER) return "TRANSFER";
        if (bc == BehaviorCategory.CLEAN) return "CLEAN";
        if (bc == BehaviorCategory.C4_FOCUS) return "FOCUS";
        if (bc == BehaviorCategory.C4_ALLOW) return "ALLOW";
        if (bc == BehaviorCategory.C4_CAPTURE) return "CAPTURE";
        return "undefined";

    }
    public static int getBVHorizontalBorder(BehaviorCategory bc)
    {
        if (bc == BehaviorCategory.CONTRACT) return 69;
        if (bc == BehaviorCategory.PEDAL) return 58;
        if (bc == BehaviorCategory.REDUCE) return 73;
        return 0;
    }
    private static void CreateBehaviorDefinition()
    {
        behaviorDefinitionList = new List<KeyValuePair<BehaviorCategory, string>>();       
        foreach(BehaviorCategory behavior in System.Enum.GetValues(typeof(BehaviorCategory)))
        {
            behaviorDefinitionList.Add(new KeyValuePair<BehaviorCategory,string>(behavior, getBaviorLabelText(behavior)));
        }
        //create categorical behaviors
        CategoricalBVDefs = new List<KeyValuePair<BehaviorCategory, List<string>>>();

        List<string> cameraSensorBVs = new List<string>();
        cameraSensorBVs.Add("Black & White");
        cameraSensorBVs.Add("GRAYSCALE");
        cameraSensorBVs.Add("Red & Green");
        cameraSensorBVs.Add("Full Color");
        CategoricalBVDefs.Add(new KeyValuePair<BehaviorCategory, List<string>>(BehaviorCategory.C4_CAPTURE, cameraSensorBVs));
    }
    public static List<string> getCategoricalBVdefinition(BehaviorCategory t)
    {
        if (behaviorDefinitionList == null) CreateBehaviorDefinition();        
        if (CategoricalBVDefs == null) return null;
        foreach (KeyValuePair<BehaviorCategory,List<string>> c in CategoricalBVDefs)
        {
            if (c.Key == t)
            {
                return c.Value;
            }
        }
        return null;
    }
    public static BehaviorCategory DetermineBehaviorType(string labelText, out int LDistance)
    {
        BehaviorCategory ret = BehaviorCategory.None;
        if (behaviorDefinitionList == null) CreateBehaviorDefinition();
        Regex rgx = new Regex("[^a-zA-Z0-9 -]");
        labelText = rgx.Replace(labelText, "");
        labelText = labelText.ToUpper();
        int[] Ldist = new int[behaviorDefinitionList.Count];
        for (int i = 0; i < behaviorDefinitionList.Count; i++)
        {            
            Ldist[i] = CalcLevenshteinDistance(labelText, behaviorDefinitionList[i].Value);
        }
        int maxIndex = Ldist.ToList().IndexOf(Ldist.Min());
        //Debug.Log("[DEBUG]behavior recognized for text(" + labelText + ") as type(" + getBaviorLabelText(behaviorDefinitionList[maxIndex].Key));
        LDistance = Ldist.Min();
        ret = behaviorDefinitionList[maxIndex].Key;
        if (ret == BehaviorCategory.None || ret == BehaviorCategory.Empty) LDistance = int.MaxValue;
        return ret;


        
    }
    private static int CalcLevenshteinDistance(string a, string b)
    {
        if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b)) return int.MaxValue;

        int lengthA = a.Length;
        int lengthB = b.Length;
        var distances = new int[lengthA + 1, lengthB + 1];
        for (int i = 0; i <= lengthA; distances[i, 0] = i++) ;
        for (int j = 0; j <= lengthB; distances[0, j] = j++) ;

        for (int i = 1; i <= lengthA; i++)
            for (int j = 1; j <= lengthB; j++)
            {
                int cost = b[j - 1] == a[i - 1] ? 0 : 1;
                distances[i, j] = Mathf.Min(Mathf.Min(distances[i - 1, j] + 1, distances[i, j - 1] + 1),
                    distances[i - 1, j - 1] + cost);
            }
        return distances[lengthA, lengthB];
    }
    
    public static ModelCategory[] loadColorObjectMap(out int NColors)
    {
        ModelCategory[] ret = new ModelCategory[5];
        NColors = 0;
        if (FBSModel.ContentType== DesignContent.HumanRespiratorySystem)
        {
            ret[0] = ModelCategory.Diaphragm;
            ret[1] = ModelCategory.Lung;
            ret[2] = ModelCategory.Airways;
            ret[3] = ModelCategory.None;
            ret[4] = ModelCategory.None;
            NColors = 3;
        }
        if (FBSModel.ContentType == DesignContent.BicycleGearSystem)
        {
            ret[0] = ModelCategory.PedalCrank;
            ret[1] = ModelCategory.RearSprocket;
            ret[2] = ModelCategory.Chain;
            ret[3] = ModelCategory.FrontChainring;
            ret[4] = ModelCategory.None;
            NColors = 4;
        }
        if (FBSModel.ContentType == DesignContent.AquariumEcology)
        {
            ret[0] = ModelCategory.Bacteria;
            ret[1] = ModelCategory.Plant;
            ret[2] = ModelCategory.Fish;
            ret[3] = ModelCategory.AirPump;
            ret[4] = ModelCategory.None;
            NColors = 4;
        }
        if (FBSModel.ContentType == DesignContent.CameraSystem)
        {
            ret[0] = ModelCategory.C4_lens;
            ret[1] = ModelCategory.C4_shutter;
            ret[2] = ModelCategory.C4_sensor;
            ret[3] = ModelCategory.None;
            ret[4] = ModelCategory.None;
            NColors = 3;
        }
        return ret;
    }

    public static bool ForceConvexShape(ModelCategory mc)
    {
       // if (mc == ModelCategory.FrontChainring) return true;
        return false;
    }
    public static void ExtractSimulationParameters(prototypeDef proto, DesignContent cType, ref SimulationParam ret)
    {
        if(cType== DesignContent.HumanRespiratorySystem)
        {
            ret.C1_breathingRate = 0;
            ret.C1_breathingAmountLevel = 0;

            if (proto.mModels ==null || proto.mModels[ModelCategory.Diaphragm] == null || proto.mModels[ModelCategory.Diaphragm].Count == 0) return;

            //C1
            foreach(var dia in proto.mModels[ModelCategory.Diaphragm])
            {
                if (dia.behaviors == null) continue;
                foreach(var beh in dia.behaviors)
                {
                    if (beh.marker == null) continue;
                    ret.C1_breathingRate = (int) beh.marker.InfoNumericalBVValue;
                }
            }

            //C1
            if (proto.mModels[ModelCategory.LungLeft] == null || proto.mModels[ModelCategory.LungRight] == null) return;
            float lungAreaPortion = 0;            
            foreach (var dia in proto.mModels[ModelCategory.LungLeft])
            {
                var geometry = dia.getShapeBuilder();
                if (geometry == null) return;
                lungAreaPortion+=geometry.getAreaPortioninCanvas();
            }
            foreach (var dia in proto.mModels[ModelCategory.LungRight])
            {
                var geometry = dia.getShapeBuilder();
                if (geometry == null) return;
                lungAreaPortion += geometry.getAreaPortioninCanvas();
            }
            Debug.Log("Lung Area = " + lungAreaPortion);
            //range 0.01 to 0.20
            ret.C1_breathingAmountLevel = (int)(lungAreaPortion * 100.0f/ 0.2f);
        }
        if (cType == DesignContent.BicycleGearSystem)
        {
            ret.C2_rearGearSize = -1;
            ret.C2_frontGearSize = -1;
            ret.C2_GearRatio = -1;
            ret.C2_pedallingRate = 30;  //test
            ModelDef frontGearModel = proto.getModelDefbyType(ModelCategory.FrontChainring);
            ModelDef rearGearModel = proto.getModelDefbyType(ModelCategory.RearSprocket);
            ModelDef pedalModel = proto.getModelDefbyType(ModelCategory.PedalCrank);
            ModelDef ChainUpperModel = proto.getModelDefbyType(ModelCategory.UpperChain);
            ModelDef ChainLowerModel = proto.getModelDefbyType(ModelCategory.LowerChain);
            if (pedalModel == null) ret.C2_pedallingRate = 0;
            ret.C2_pedalAnimSpeed = ret.C2_pedallingRate / 12.0f;

            if (frontGearModel != null)
            {
                ret.C2_frontGearSize = Mathf.Sqrt((float)frontGearModel.AreaSize);
                ret.C2_frontGearAnimSpeed = ret.C2_pedalAnimSpeed;
            }
            else ret.C2_frontGearAnimSpeed = 0;
            if (ChainUpperModel != null && ChainLowerModel != null) ret.C2_chainAnimSpeed = ret.C2_frontGearAnimSpeed;
            else ret.C2_chainAnimSpeed = 0;
            if (rearGearModel != null)
            {
                ret.C2_rearGearSize = Mathf.Sqrt((float)rearGearModel.AreaSize);
          
            } else ret.C2_rearGearAnimSpeed = 0;
            if (frontGearModel != null && rearGearModel != null)
            {
                ret.C2_GearRatio = ret.C2_rearGearSize / ret.C2_frontGearSize;
                if (ret.C2_GearRatio > 0) ret.C2_rearGearAnimSpeed = ret.C2_chainAnimSpeed / ret.C2_GearRatio;
                else ret.C2_rearGearAnimSpeed = 0;
            } else ret.C2_rearGearAnimSpeed = 0;
            Debug.Log("Chain SPEED" + ret.C2_chainAnimSpeed+"READ SPEED"+ret.C2_rearGearAnimSpeed);
        }
        if (cType == DesignContent.CameraSystem)
        { //TODO
            ModelDef lensModel = proto.getModelDefbyType(ModelCategory.C4_lens);
            ModelDef shutterModel = proto.getModelDefbyType(ModelCategory.C4_shutter);
            ModelDef sensor = proto.getModelDefbyType(ModelCategory.C4_sensor);
            ret.C4_focalLength = -1;
            ret.C4_shutterSpeed = -1;
            ret.C4_sensorType = "none";
            ret.C4_missingLens = false;
            ret.C4_missingSensor = false;
            ret.C4_missingShutter = false;
            BehaviorDef be = null;
            int ws = WorkSpaceUI.mInstance.GetCurrentStep(); 
            if (lensModel != null) be = lensModel.getBehaviorDef(BehaviorCategory.C4_FOCUS);
            else if(ws ==-1 || ws == 0) ret.C4_missingLens = true;
            if (be != null)
            {
                ret.C4_focalLength = be.getNumericalBV();
                if(be.marker!=null) ret.C4_focusLabelPos = be.marker.center;

            }

            if (shutterModel != null) be = shutterModel.getBehaviorDef(BehaviorCategory.C4_ALLOW);
            else 
            {
                be = null;
                if (ws == -1 || ws == 1) ret.C4_missingShutter = true;
            }
            if (be != null)
            {
               // GlobalRepo.showDebugImage("mm",be.marker.BVImage);
                ret.C4_shutterSpeed = be.getNumericalBV();
                if (be.marker != null) ret.C4_shutterspeedLabelPos = be.marker.center;
            }


            if (sensor != null) be = sensor.getBehaviorDef(BehaviorCategory.C4_CAPTURE);
            else
            {
                be = null;
                if (ws == -1 || ws == 2) ret.C4_missingSensor = true;
            }
            if (be != null)
            {
                ret.C4_sensorType = be.getCategoricalBV();
                if (be.marker != null) ret.C4_sensortypeLabelPos = be.marker.center;
            }

            //check WS and compensate
            
            
        }

    }
   
}

public class SimulationParam : ICloneable
{
    public int C1_breathingRate;
    public int C1_breathingAmountLevel;

    public int C2_pedallingRate;
    public float C2_GearRatio;
    
    public float C2_frontGearSize;
    public float C2_rearGearSize;
    public float C2_frontGearAnimSpeed;
    public float C2_rearGearAnimSpeed;
    public float C2_chainAnimSpeed;
    public float C2_pedalAnimSpeed;

    public float C4_focalLength;
    public float C4_shutterSpeed;
    public string C4_sensorType;
    public bool C4_missingLens;
    public bool C4_missingShutter;
    public bool C4_missingSensor;

    public CvPoint C4_focusLabelPos;
    public CvPoint C4_shutterspeedLabelPos;
    public CvPoint C4_sensortypeLabelPos;

    public int NumberOfFeedback;

    public object Clone()
    {
        return this.MemberwiseClone();
    }
    public void DebugPrint()
    {
        Debug.Log("[DEBUG] Simulation Param C4: " + C4_focalLength + "\t" + C4_shutterSpeed + "\t" + C4_sensorType+"\t feedback N: " +NumberOfFeedback);
    }

    public int DistanceTo(SimulationParam sp)

    {
        int dist = 0;
        
        if (ApplicationControl.ActiveInstance.ContentType == DesignContent.BicycleGearSystem)
        {
            if (sp == null) return 3;
            if (Math.Abs(C2_GearRatio - sp.C2_GearRatio) >= 0.05) dist++;
            if (Math.Abs(C2_frontGearSize - sp.C2_frontGearSize) >= 30) dist++;
            if (Math.Abs(C2_rearGearSize - sp.C2_rearGearSize) >= 30) dist++;
        }
        if (ApplicationControl.ActiveInstance.ContentType == DesignContent.CameraSystem)
        {
            if (sp == null) return 3;
            if (Math.Abs(C4_focalLength - sp.C4_focalLength) >= 10) dist++;
            if (Math.Abs(this.C4_shutterSpeed - sp.C4_shutterSpeed) >= 50) dist++;
            if (this.C4_sensorType!=sp.C4_sensorType) dist++;
        }
        return dist;
    }
    public int DistanceTo(SimulationParam sp, ref List<string> parameterNames, out float diffLevel)

    {
        int dist = 0;
        parameterNames.Clear();
        diffLevel = 0;
        if (ApplicationControl.ActiveInstance.ContentType == DesignContent.BicycleGearSystem)
        {
            if (sp == null) return 3;
            if (Math.Abs(C2_GearRatio - sp.C2_GearRatio) >= 0.05) dist++;
            if (Math.Abs(C2_frontGearSize - sp.C2_frontGearSize) >= 30) dist++;
            if (Math.Abs(C2_rearGearSize - sp.C2_rearGearSize) >= 30) dist++;
        }

        if (ApplicationControl.ActiveInstance.ContentType == DesignContent.CameraSystem)
        {
            if (sp == null) return 99;
            if (Math.Abs(C4_focalLength - sp.C4_focalLength) >= 10)
            {
                dist++;
                parameterNames.Add("focallength");
                diffLevel = Math.Abs(C4_focalLength - sp.C4_focalLength);
            }
            if (Math.Abs(this.C4_shutterSpeed - sp.C4_shutterSpeed) >= 50)
            {
                dist++;
                parameterNames.Add("shutterspeed");
                diffLevel = Math.Abs(this.C4_shutterSpeed - sp.C4_shutterSpeed);
            }
            if (this.C4_sensorType != sp.C4_sensorType)
            {
                dist++;
                parameterNames.Add("sensortype");
                diffLevel = 1;         
            }
        }
        return dist;
    }
    public bool HasAllParameters(DesignContent content)
    {
        if (content == DesignContent.BicycleGearSystem)
        {
            if (C2_frontGearSize > 0 && C2_rearGearSize > 0 && C2_GearRatio > 0) return true;
        }
        if (content == DesignContent.CameraSystem)
        {
            if (C4_focalLength>0 && C4_sensorType!="none" && C4_shutterSpeed>0) return true;
        }
        return false;
    }
}
public enum DesignContent
{
    NONE,
    HumanRespiratorySystem,
    BicycleGearSystem,
    AquariumEcology,
    CameraSystem
}


public enum BehaviorVariableType
{
    None,
    Numeric,
    Categorical,
    TotalNumberofTypes

}
public enum PosEvalType
{
    None,
    CentralProximitytoPoint,
    ContourProximitytoPoint,
    HorizontalProximitytoPoint,
    VerticalProximitytoPoint
}
public enum ShapeEvalType
{
    None,
    ActualShape,
    AspectRatio,
    DoNot,
    TotalNumberofShapeEvals
}
public enum StrEntityEvalType
{
    None,
    OneonOne,
    atLeastOne,
    TotalNumberofStrEntityEvals
}
public enum EvaluationResultCategory
{
    None,
    Shape_existence_missing,
    Shape_existence_redundant,
    Shape_suggestion,
    Position_direction,
    Connectivity_wrong,
    Connectivity_missing,
    Behavior_missing,
    Behavior_Unnecessary,    
    Behavior_relocate,
    Behavior_variableUnchecked,
    TotalNumberofFeedbackTypes,
}
public enum ModelCategory
{ //this also defines the order of simulation 
    None = 0x00,
    Diaphragm,
    Lung,
    Stomach,
    Airways,
    AirParticleLeft,
    AirParticleRight,        
    LungLeft,
    LungRight,    
    FrontChainring,
    RearSprocket,
    Chain,
    UpperChain,
    LowerChain,
    PedalCrank,
    Fish,
    Plant,
    AirPump,
    Bacteria,
    C4_lens,
    C4_shutter,
    C4_sensor,    
    TotalNumberOfModels

}
public enum BehaviorCategory
{
    None,
    Empty,
    PEDAL,
    EAT,
    CONSUME,
    SUPPLY,
    REDUCE,
    ROTATE,
    PROPEL,
    ENGAGE,
    DRIVE,
    TRANSFER,
    CONVERT,
    CONTRACT,
    PASS,
    DIFFUSE,
    PRODUCE,    
    CLEAN,
    C4_FOCUS,
    C4_ALLOW,
    C4_CAPTURE,
    TotalNumberofBahaviors

}

