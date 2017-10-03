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
public class AnimationSync
{
    public int phaseCounter = 0;
    public float phasePeriod = 0;
    public int phaseInterval = 0;
    private float timePast = 0;
    public List<animationControl3D> childAnimations;
    public AnimationSync(int phaseInterval, float phasePeriod)
    {
        this.phaseCounter = 0;
        this.phasePeriod = phasePeriod;

        this.phaseInterval = 2;
        this.timePast = phasePeriod / 2;
        childAnimations = new List<animationControl3D>();
    }
    public void addAnimationControl(animationControl3D ac)
    {
        childAnimations.Add(ac);
    }
    public int getPhase()
    {
        return phaseCounter % phaseInterval;
    }
    public void Tick()
    {
        timePast += Time.deltaTime;
        if (timePast > phasePeriod)
        {
            phaseCounter++;
            timePast = 0;
        }
        foreach (var t in childAnimations)
        {
            t.Tick(getPhase());
        }
    }
}
public class animationControl3D
{
    public int syncGroupID;

    public ModelDef baseStructure = null;
    public BehaviorInstanceType animType;

    private Vector3 type1_oscillationRange; //+,-    
    private Vector3 type1_velocity;
    private Vector3[] boundaries = new Vector3[2];

    public animationControl3D(ModelDef model, BehaviorInstanceType type, Vector3 param1, Vector3 param2, float period)
    {
        this.animType = type;
        if (type == BehaviorInstanceType.MoveUpDown)
        {
            this.type1_oscillationRange = param1;
            this.baseStructure = model;

            Vector3 initPos = model.pGameObject.localPosition;
            boundaries[0] = initPos - this.type1_oscillationRange;
            boundaries[1] = initPos + this.type1_oscillationRange;
            float halfPeriod = period / 2;
            type1_velocity = this.type1_oscillationRange * 2 / halfPeriod; // at phase =0;

        }
        if (type == BehaviorInstanceType.Expand)
        {
            this.type1_oscillationRange = param1;
            this.baseStructure = model;
            Vector3 initScale = model.pGameObject.localScale;
            boundaries[0] = initScale - this.type1_oscillationRange;
            boundaries[1] = initScale + this.type1_oscillationRange;
            float halfPeriod = period / 2;
            type1_velocity = this.type1_oscillationRange * 2 / halfPeriod; // at phase =0;
        }

    }
    public void Tick(int phase)
    {
        if (this.baseStructure == null) return;
        if (animType == BehaviorInstanceType.MoveUpDown)
        {
            if (phase == 0)
            {
                Vector3 prevPos = baseStructure.pGameObject.localPosition;
                prevPos += this.type1_velocity * Time.deltaTime;
                baseStructure.pGameObject.localPosition = prevPos;
            }
            else if (phase == 1)
            {
                Vector3 prevPos = baseStructure.pGameObject.localPosition;
                prevPos -= this.type1_velocity * Time.deltaTime;
                baseStructure.pGameObject.localPosition = prevPos;
            }
        }
        else if (animType == BehaviorInstanceType.Expand)
        {
            if (phase == 0)
            {
                Vector3 prevScale = baseStructure.pGameObject.localScale;
                prevScale -= this.type1_velocity * Time.deltaTime;
                baseStructure.pGameObject.localScale = prevScale;
            }
            else if (phase == 1)
            {
                Vector3 prevScale = baseStructure.pGameObject.localScale;
                prevScale += this.type1_velocity * Time.deltaTime;
                baseStructure.pGameObject.localScale = prevScale;
            }
        }
    }

}