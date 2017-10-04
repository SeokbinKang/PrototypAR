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

public class Simulation_Artifact_Chain : MonoBehaviour {

    public Vector3 StartPoint;
    public Vector3 EndPoint;

    public float chainLengthRatio;
    public float chainScaleFactor = 1;
    public GameObject[] prefab;

    public float speedparam;

    private float speedScaling = 0.002f;
    private List<chainElement> mChainElements;

    private List<Vector3> mChainPath;
	// Use this for initialization
	void Start () {
        //do nothing
        mChainElements = new List<chainElement>();
        mChainPath = new List<Vector3>();
   //     InitChain(StartPoint, EndPoint, speedparam);
    }
	
	// Update is called once per frame
	void Update () {
        MoveChain();

    }
    private void MoveChain()
    {
        foreach(var c in mChainElements)
        {
            c.move(speedparam* speedScaling, mChainPath);
        }
    }
    public void destoryChain()
    {
        mChainElements.Clear();
        mChainPath.Clear();
        foreach (Transform child in this.transform)
        {
            GameObject.Destroy(child.gameObject);

        }

    }
    public void InitChain(ModelDef m, prototypeDef proto)
    {
        if (m == null || proto == null || proto.mConnections == null || (m.modelType!=ModelCategory.UpperChain && m.modelType != ModelCategory.LowerChain)) return;
        //check all the connectivity mark from the proto
        var ConnectionPoints = proto.GetAllConnectivityofModel(m);
        if (ConnectionPoints.Count != 2) return;
        CvPoint[] startend = new CvPoint[2];
        
        foreach (var conn in ConnectionPoints)
        {
            var connectedObjectID = conn.StrConnectivity.Key == m.instanceID ? conn.StrConnectivity.Value : conn.StrConnectivity.Key;
            var connectedModel = proto.getModelDefbyID(connectedObjectID);
            if (connectedModel == null) return;
            if (connectedModel.modelType == ModelCategory.FrontChainring) startend[0] = conn.center;
                else if (connectedModel.modelType == ModelCategory.FreeWheel) startend[1] = conn.center;
                else
            {
                Debug.Log("[ERROR] chain object is connected to Non-gear object");
                return;
            }
        }
        Vector3 startWorldPos, endWorldPos;
        //set start and end position  
        if(m.modelType==ModelCategory.UpperChain)
        {
            startWorldPos = SceneObjectManager.regionToWorld(startend[1]);
            endWorldPos = SceneObjectManager.regionToWorld(startend[0]);

        }
        else if (m.modelType == ModelCategory.LowerChain)
        {
            startWorldPos = SceneObjectManager.regionToWorld(startend[0]);
            endWorldPos = SceneObjectManager.regionToWorld(startend[1]);

        } else
        {
            Debug.Log("[ERROR] trying to create chain object out of non-chain entity");
            return;
        }
        //give some extension
        float extensionLength = 0.1f;
        Vector3 forwardVector = endWorldPos - startWorldPos;
        forwardVector.Normalize();
        forwardVector = forwardVector * extensionLength;
        endWorldPos += forwardVector;
        startWorldPos -= forwardVector;
        this.InitChain(startWorldPos, endWorldPos);
        //upper : left-to right
        //lower : right to left
        //default : same a upper




    }
    public void InitChain(Vector3 startPointWorldCoord, Vector3 endPointWorldCoord)
    {
        /*   if (mChainElements == null) mChainElements = new List<chainElement>();
           else mChainElements.Clear();
           if (mChainPath == null) mChainPath = new List<Vector3>();
               else     mChainPath.Clear();*/
        mChainElements.Clear();
        mChainPath.Clear();
         Vector3 lastChainPoint = startPointWorldCoord;
       
        System.Random rnd = new System.Random();

        int q = 50;
        int pathidx = 0;

        
        while (true && q>0)
        {
            q--;
            int rndChainIdx = rnd.Next(0, prefab.Length-1);
            Vector3 forwardVector = (endPointWorldCoord - startPointWorldCoord);
            float rotationAngle = Mathf.Atan2(forwardVector.y, forwardVector.x) * 180/Mathf.PI;
            forwardVector.Normalize();
            float chainLength = chainLengthRatio;

            //1. randomly pick a chain
            //1b. calculate direction vector
            //2. place at the last position
            UnityEngine.GameObject newChainElement = Instantiate(prefab[rndChainIdx], lastChainPoint,Quaternion.identity) as GameObject;
            Vector3 rot = newChainElement.transform.eulerAngles;
            rot.z = rotationAngle;
            //3. rotate toward end point
            newChainElement.transform.eulerAngles = rot;
            newChainElement.transform.parent = this.gameObject.transform;
            Vector3 scale = newChainElement.transform.localScale;
            scale = scale * chainScaleFactor;
            scale.z = 1;
            newChainElement.transform.localScale = scale;

            //store item
            chainElement ce = new chainElement(lastChainPoint, rot, newChainElement, pathidx);
            mChainElements.Add(ce);
            mChainPath.Add(lastChainPoint);
            pathidx++;
            //4. update last position (approximate distance * direction vector);
            Debug.Log("[DEBUG-SIMULATION] Chain point:" + lastChainPoint);
            lastChainPoint = lastChainPoint + forwardVector * chainLength;
            Debug.Log("[DEBUG-SIMULATION] Chain point:" + lastChainPoint);
            Vector3 newForwardVector = (endPointWorldCoord - lastChainPoint);

            if (newForwardVector.x * forwardVector.x < 0 || newForwardVector.y * forwardVector.y < 0)
            {
           //     ce = new chainElement(lastChainPoint, rot, newChainElement, pathidx-1);
            //    mChainElements.Add(ce);
                mChainPath.Add(lastChainPoint); //1 extra in the end
                break;
            }
            
        }
      
        


    }
    

}

public class chainElement
{
    Vector3 position;
    Vector3 rotation;
    GameObject go;
    int pathIdx;
    Vector3 lastMoveVector;
    public chainElement(Vector3 pos, Vector3 rot, GameObject go_, int pathIdx_)
    {
        position = pos;
        rotation = rot;
        go = go_;
        pathIdx = pathIdx_+1; //next index
        
    }
    public void move(float speed, List<Vector3> chainPath)
    {
        if (pathIdx >= chainPath.Count - 1) revive(chainPath);
        if (speed == 0) return;
        //caculate move vector
        Vector3 moveVector = chainPath[pathIdx] - position;
        moveVector.Normalize();
        moveVector = moveVector * speed;
        //examine move vector's direction changed
        if (pathIdx == 1) Debug.Log(lastMoveVector.x + "\t" + moveVector.x);
        if(Math.Sign(moveVector.x) != Math.Sign(lastMoveVector.x) || Math.Sign(moveVector.y)!= Math.Sign(lastMoveVector.y))
        {
            pathIdx++;
            if (pathIdx >= chainPath.Count - 1) revive(chainPath);
            //if yes, increment index and re-calculate move vector

            moveVector = chainPath[pathIdx] - position;
            moveVector.Normalize();
            moveVector = moveVector * speed;
        }
        position = position + moveVector;
        go.transform.position = position;

        //move 

        //update last move vector
        lastMoveVector = moveVector;
    }
    private void revive(List<Vector3> chainPath)
    { //TODO
        position = chainPath[1];
        pathIdx = 2;
    }
}