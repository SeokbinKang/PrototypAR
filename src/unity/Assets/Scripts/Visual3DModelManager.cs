using UnityEngine;
using System.Collections;
using OpenCvSharp;
using OpenCvSharp.Blob;
using OpenCvSharp.CPlusPlus;
using System.Collections.Generic;

using UnityEngine.UI;
public class Visual3DModelManager : MonoBehaviour {

    private int modelCount = 0;

    public Transform lung_right;
    public Transform lung_left;
    public Transform diaphragm;
    public Transform airways;

    private List<Transform> ProtoModelList;
    public Visual3DModelManager()
    {
        ProtoModelList = new List<Transform>();
        //initialize
    }
    //

    public void create3DModel(prototypeDef proto)
    {

        //create a root 3Dprototype node
        //Transform t = Object.Instantiate(null, new Vector3(0, 0, 0), Quaternion.Euler(0, 180, 0)) as Transform;
        GameObject go = new GameObject();
        Transform t = go.transform;
        
        t.rotation = Quaternion.Euler(0, 0, 0);
        ProtoModelList.Add(t);
        //add organ nodes
        proto.pGameObject = t;
        //that's it!    
        if (proto.mModels == null) return;
        foreach (KeyValuePair<ModelCategory, List<ModelDef>> item in proto.mModels)
        {
            if(item.Key==ModelCategory.Lung)
            {
                float horizontalConversionRate = -0.0032f; //100 to 0.32
                float verticalConversionRate = -0.0032f;
                float sizeConversion = 0.3f;
                foreach(var modelDesc in item.Value)
                {
                    float xConverted = ((float)modelDesc.centeroidRelative.X) * horizontalConversionRate;
                    float yConverted = ((float)modelDesc.centeroidRelative.Y) * verticalConversionRate;
                    float scaleFactor = (float)modelDesc.AreaSizeNormalized * sizeConversion;
                    if (modelDesc.centeroidRelative.X<0)
                    { // left lung
                       
                        Transform lungL = Object.Instantiate(this.lung_left, new Vector3(xConverted, yConverted, 0), Quaternion.Euler(270, 180, 0)) as Transform;                        
                        lungL.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
                        lungL.SetParent(t);
                        modelDesc.pGameObject = lungL;
                        

                    } else
                    { // right lung
                      
                        Transform lungR = Object.Instantiate(this.lung_right, new Vector3(xConverted, yConverted, 0), Quaternion.Euler(270, 180, 0)) as Transform;
                        lungR.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
                        lungR.SetParent(t);
                        modelDesc.pGameObject = lungR;
                    }
                }
                /*
                if (item.Value.Count == 2) {
                    
                    if (item.Value[0].centeroid.X > item.Value[1].centeroid.X)
                    {
                        float xConverted = ((float)item.Value[0].centeroid.X) * horizontalConversionRate;
                        float yConverted = ((float)item.Value[0].centeroid.Y) * verticalConversionRate;
                        Transform lungL = Object.Instantiate(this.lung_left, new Vector3(xConverted, yConverted, 0), Quaternion.Euler(270, 180, 0)) as Transform;
                        float scaleFactor = (float)item.Value[0]
                        lungL.localScale = new Vector3(((float)item.Value[0].AreaSize) * sizeConversion, ((float)item.Value[0].AreaSize) * sizeConversion, ((float)item.Value[0].AreaSize) * sizeConversion);
                        lungL.SetParent(t);
                        xConverted = ((float)item.Value[1].centeroid.X) * horizontalConversionRate;
                        yConverted = ((float)item.Value[1].centeroid.Y) * verticalConversionRate;

                        Transform lungR = Object.Instantiate(this.lung_right, new Vector3(xConverted, yConverted, 0), Quaternion.Euler(270, 180, 0)) as Transform;
                        
                        lungR.localScale = new Vector3(((float)item.Value[1].AreaSize) * sizeConversion, ((float)item.Value[1].AreaSize) * sizeConversion, ((float)item.Value[1].AreaSize) * sizeConversion);
                        lungR.SetParent(t);
                    }
                    else
                    {
                        float xConverted = ((float)item.Value[1].centeroid.X) * horizontalConversionRate;
                        float yConverted = ((float)item.Value[1].centeroid.Y) * verticalConversionRate;
                        Transform lungL = Object.Instantiate(this.lung_left, new Vector3(xConverted, yConverted, 0), Quaternion.Euler(270, 180, 0)) as Transform;
                        lungL.SetParent(t);
                        lungL.localScale = new Vector3(((float)item.Value[1].AreaSize) * sizeConversion, ((float)item.Value[1].AreaSize) * sizeConversion, ((float)item.Value[1].AreaSize) * sizeConversion);
                        xConverted = ((float)item.Value[0].centeroid.X) * horizontalConversionRate;
                        yConverted = ((float)item.Value[0].centeroid.Y) * verticalConversionRate;

                        Transform lungR = Object.Instantiate(this.lung_right, new Vector3(xConverted, yConverted, 0), Quaternion.Euler(270, 180, 0)) as Transform;
                        lungR.localScale = new Vector3(((float)item.Value[0].AreaSize) * sizeConversion, ((float)item.Value[0].AreaSize) * sizeConversion, ((float)item.Value[0].AreaSize) * sizeConversion);
                        lungR.SetParent(t);
                    }
                }*/

                // find left and right lung
               
            } else if (item.Key == ModelCategory.Diaphragm)
            {
                float horizontalConversionRate = -0.0032f; //100 to 0.32
                float verticalConversionRate = -0.007f;
                float sizeConversion = 0.3f;

                foreach (var modelDesc in item.Value)
                {
                    float xConverted = ((float)modelDesc.centeroidRelative.X) * horizontalConversionRate;
                    float yConverted = ((float)modelDesc.centeroidRelative.Y) * verticalConversionRate;
                    float scaleFactor = (float)modelDesc.AreaSizeNormalized * sizeConversion;
                    Transform dia = Object.Instantiate(this.diaphragm, new Vector3(xConverted, yConverted, 0), Quaternion.Euler(270, 180, 0)) as Transform;
                    dia.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
                    dia.SetParent(t);
                    modelDesc.pGameObject = dia;
                }


            } else if(item.Key == ModelCategory.Airways)
            {
                float horizontalConversionRate = -0.0032f; //100 to 0.32
                float verticalConversionRate = -0.004f;
                float sizeConversion = 0.5f;

                foreach (var modelDesc in item.Value)
                {
                    float xConverted = ((float)modelDesc.centeroidRelative.X) * horizontalConversionRate;
                    float yConverted = ((float)modelDesc.centeroidRelative.Y) * verticalConversionRate;
                    float scaleFactor = (float)modelDesc.AreaSizeNormalized * sizeConversion;
                    Transform air = Object.Instantiate(this.airways, new Vector3(xConverted, yConverted, 0), Quaternion.Euler(270, 180, 0)) as Transform;
                    air.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
                    air.SetParent(t);
                    modelDesc.pGameObject = air;
                }
            }
 



            CvColor modelColor = ColorDetector.getColorforModelIndex((int)item.Key);
            foreach (ModelDef m in item.Value)
            {
            
            }
            if (item.Value.Count > 0)
            {
            
            }
            
        }

        t.position = new Vector3(0.04f, 0.92f, 1.5f);
    }
    void Start()
    {

    }
    void Update()
    {
        
            //update the position of models
    }
    void updatePrototypeModels()
    {
        //KinectManager manager = KinectManager.Instance;

    }


}
