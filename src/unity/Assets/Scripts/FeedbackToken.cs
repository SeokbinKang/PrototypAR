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

public class FeedbackToken
{
    static int feedbackInstanceCount = 0;
    public EvaluationResultCategory type;
    public int feedbackInstanceID;
    public ModelCategory modelType;
    public int modelInstanceID;
    public ModelDef model;
    public BehaviorDef behavior;
    public Point[] ShapeSuggestedOutline;
    public Vector3 PositionSuggestedDirectiontoMove;
    public UserDescriptionInfo connectivity_wrongDesc;
    public KeyValuePair<ModelDef, ModelDef> connectivity_missingConn;
    public BehaviorCategory behaviorType;
    //structure existence feedback

    public FeedbackToken(EvaluationResultCategory type_, ModelCategory modeltype_, int modelID,ModelDef model_)
    {
        this.type = type_;
        this.modelType = modeltype_;
        this.model = model_;
        this.modelInstanceID = modelID;
        this.feedbackInstanceID = feedbackInstanceCount++;
    }

    //behavior variable feedback
    public FeedbackToken(EvaluationResultCategory type_, BehaviorDef userbdesign_)
    {
        this.type = type_;
        this.behavior = userbdesign_;
        this.behaviorType = userbdesign_.behaviorType;
        this.feedbackInstanceID = feedbackInstanceCount++;
    }

    //Behavior feedback
    public FeedbackToken(EvaluationResultCategory type_, ModelDef userstr, BehaviorCategory behaviortype_, BehaviorDef userbehavior)
    {
        this.type = type_;
        this.behaviorType = behaviortype_;
        this.model = userstr;
        this.behavior = userbehavior;
        this.feedbackInstanceID = feedbackInstanceCount++;
    }
    //shape feedback
    public FeedbackToken(EvaluationResultCategory type_, int modelID, Point[] suggestedShape_)
    {
        this.type = type_;
        modelInstanceID = modelID;
        ShapeSuggestedOutline = suggestedShape_;
        this.feedbackInstanceID = feedbackInstanceCount++;

    }
    //position feedback
    public FeedbackToken(EvaluationResultCategory type_, int modelID, Vector3 direction)
    {
        this.type = type_;
        modelInstanceID = modelID;
        this.PositionSuggestedDirectiontoMove = direction;
        this.feedbackInstanceID = feedbackInstanceCount++;
    }
    //connectivity feedback
    public FeedbackToken(EvaluationResultCategory type_, UserDescriptionInfo wrongConnDesc, KeyValuePair<ModelDef, ModelDef> missingConnPair)
    {
        this.type = type_;
        this.connectivity_wrongDesc = wrongConnDesc;
        this.connectivity_missingConn = missingConnPair;
        this.feedbackInstanceID = feedbackInstanceCount++;


    }
    //connectivity feedback

    public void DebugPrint()
    {
        if (type == EvaluationResultCategory.Shape_existence_missing)
        {
            Debug.Log("[FeedbackToken] Missing Object type = " + modelType);
        }
        if (type == EvaluationResultCategory.Shape_existence_redundant)
        {
            Debug.Log("[FeedbackToken] Extra Object id = " + modelInstanceID + "type :"+this.model.modelType);
        }
        if (type == EvaluationResultCategory.Behavior_missing)
        {
            Debug.Log("[FeedbackToken] Behavior missing for str object = " + model.instanceID);
        }
        if (type == EvaluationResultCategory.Behavior_Unnecessary)
        {
            Debug.Log("[FeedbackToken] Extra Behavior found, type = " + Content.getBaviorLabelText(this.behaviorType));
        }
        if (type == EvaluationResultCategory.Behavior_relocate)
        {
            Debug.Log("[FeedbackToken] Extra Behavior found, type = " + Content.getBaviorLabelText(this.behaviorType) + " to = " + Content.getOrganName(this.model.modelType));
        }
        if (type == EvaluationResultCategory.Behavior_variableUnchecked)
        {
            Debug.Log("[FeedbackToken] BV undescribed, type = " + Content.getBaviorLabelText(this.behaviorType));
        }
        if (type == EvaluationResultCategory.Connectivity_missing)
        {   
            Debug.Log("[FeedbackToken] Connection undescribed, type = " + Content.getOrganName(this.connectivity_missingConn.Key.modelType)+ " and " + Content.getOrganName(this.connectivity_missingConn.Value.modelType));
        }

    }
    public PreLoadedObjects getFeedbackObjectID()
    {
        PreLoadedObjects ret = PreLoadedObjects.None;
        if (this.type == EvaluationResultCategory.Shape_existence_missing)
        {
            if (this.modelType == ModelCategory.LungLeft || this.modelType == ModelCategory.LungRight)
                ret = PreLoadedObjects.STR_missing_c1_lung;
            if (this.modelType == ModelCategory.Diaphragm)
                ret = PreLoadedObjects.STR_missing_c1_dia;
            if (this.modelType == ModelCategory.Airways)
                ret = PreLoadedObjects.STR_missing_c1_air;

            if (this.modelType == ModelCategory.FrontChainring)
                ret = PreLoadedObjects.STR_missing_c2_frontgear;
            if (this.modelType == ModelCategory.RearSprocket)
                ret = PreLoadedObjects.STR_missing_c2_reargear;
            if (this.modelType == ModelCategory.PedalCrank)
                ret = PreLoadedObjects.STR_missing_c2_pedal;
            if (this.modelType == ModelCategory.LowerChain)
                ret = PreLoadedObjects.STR_missing_c2_chain;
            if (this.modelType == ModelCategory.UpperChain)
                ret = PreLoadedObjects.STR_missing_c2_chain;

            if (this.modelType == ModelCategory.C4_lens)
                ret = PreLoadedObjects.STR_missing_c4_lens;
            if (this.modelType == ModelCategory.C4_sensor)
                ret = PreLoadedObjects.STR_missing_c4_sensor;
            if (this.modelType == ModelCategory.C4_shutter)
                ret = PreLoadedObjects.STR_missing_c4_shutter;


        }
        return ret;
    }
}