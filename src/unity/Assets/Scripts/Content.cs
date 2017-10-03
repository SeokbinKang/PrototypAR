using UnityEngine;
using System.Collections;
using OpenCvSharp;
using OpenCvSharp.Blob;
using OpenCvSharp.CPlusPlus;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
public class Content {

  
    private static List<KeyValuePair<BehaviorCategory,string>> behaviorDefinitionList=null;
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
        if (mc == ModelCategory.FreeWheel) return "FreeWheel";
        if (mc == ModelCategory.Chain) return "Chain";
        if (mc == ModelCategory.UpperChain) return "UpperChain";
        if (mc == ModelCategory.LowerChain) return "LowerChain";
        if (mc == ModelCategory.PedalCrank) return "PedalCrank";
        if (mc == ModelCategory.Fish) return "Fish";
        if (mc == ModelCategory.Bacteria) return "Bacteria";
        if (mc == ModelCategory.Plant) return "Plant";
        if (mc == ModelCategory.AirPump) return "AirPump";

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
    }
    public static BehaviorCategory DetermineBehaviorType(string labelText, out int LDistance)
    {
        BehaviorCategory ret = BehaviorCategory.None;
        if (behaviorDefinitionList == null) CreateBehaviorDefinition();
        Regex rgx = new Regex("[^a-zA-Z0-9 -]");
        labelText = rgx.Replace(labelText, "");        
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
            ret[0] = ModelCategory.FreeWheel;
            ret[1] = ModelCategory.FrontChainring;
            ret[2] = ModelCategory.Chain;
            ret[3] = ModelCategory.PedalCrank;
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
        return ret;
    }
    public static bool ForceConvexShape(ModelCategory mc)
    {
        if (mc == ModelCategory.FrontChainring) return true;
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
    }
   
}

public class SimulationParam
{
    public int C1_breathingRate;
    public int C1_breathingAmountLevel;

    public int C2_pedallingRate;
    public int C2_GearRatio;
    public int C3_Torque;
}
public enum DesignContent
{
    NONE,
    HumanRespiratorySystem,
    BicycleGearSystem,
    AquariumEcology
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
    FreeWheel,
    Chain,
    UpperChain,
    LowerChain,
    PedalCrank,
    Fish,
    Plant,
    AirPump,
    Bacteria,
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
    TotalNumberofBahaviors

}

