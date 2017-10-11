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
using Uk.Org.Adcock.Parallel;

public class PrototypeInstanceManager : MonoBehaviour {

    public GameObject[] multivewPane;
    

    private List<prototypeInstance> mPrototypes;
    private Vector3 initPosition;
    // Use this for initialization
    void Start() {
        mPrototypes = new List<prototypeInstance>();
        initPosition = this.transform.position;
        Debug.Log("[DEBUG] init position: "+initPosition );
        movetoHide();
    }

    // Update is called once per frame
    void Update() {
    }
    public void movetoShow()
    {
        
        this.transform.position = initPosition;
    }
    public void movetoHide()
    {
        this.transform.position = new Vector3(30, 1700, 1);
    }
    public void AddIncompletePrototypeInstance(Texture2D prototypeImgTxt,  List<FeedbackToken> feedbackList){
        mPrototypes.Add(new prototypeInstance(prototypeImgTxt, null, feedbackList));
        UpdateMultiveUI();
    }
    public void AddcompletePrototypeInstance(Texture2D prototypeImgTxt, SimulationParam simParam)
    {
        mPrototypes.Add(new prototypeInstance(prototypeImgTxt, simParam, null));
        UpdateMultiveUI();
        Debug.Log("[DEBUG PrototypeInstanceManager] Adding new prototype instance...");
        Debug.Log("[DEBUG PrototypeInstanceManager] rearGear: " + simParam.C2_rearGearSize + "  frontGear: " + simParam.C2_frontGearSize);
    }
    private void UpdateMultiveUI()
    {
        if (mPrototypes.Count > 3) mPrototypes.RemoveAt(0);
        for(int i = 0; i < mPrototypes.Count; i++)
        {
            if(multivewPane!=null && multivewPane.Length>i && multivewPane[i] != null)
            {
                UnityEngine.UI.RawImage tempUIRawImage = multivewPane[i].GetComponent<UnityEngine.UI.RawImage>();
                if(tempUIRawImage !=null)    tempUIRawImage.texture = mPrototypes[i].mPrototypeImgTexture;
                Debug.Log("[DEBUG PrototypeInstanceManager] Updating Multipane UI..."+i);
            }
        }
        //allocate slot and update UI texture
    }
    public void GetPrototypeProperties_Content2(int idx,ref float frontgearsize, ref float reargearsize)
    {
        //default;
        frontgearsize = 300;
        reargearsize = 300;
        if (mPrototypes == null || mPrototypes.Count <= idx) return;
        frontgearsize = mPrototypes[idx].mSimulationParam.C2_frontGearSize;
        reargearsize = mPrototypes[idx].mSimulationParam.C2_rearGearSize;
    }

}

public class prototypeInstance
{
    private static int statidGlobalID = -1;
    public Texture2D mPrototypeImgTexture;
    public int prototypeID;
    public SimulationParam mSimulationParam;
    public List<FeedbackToken> mDesignFeedback;
    public prototypeInstance()
    {
        mDesignFeedback =null;
        mSimulationParam = null;
        statidGlobalID++;
        prototypeID = statidGlobalID;
       
    }
    public prototypeInstance(Texture2D prototypeImgTxt,  SimulationParam simParam, List<FeedbackToken> feedbackList)
    {
        mDesignFeedback = null;
        mSimulationParam = null;
        statidGlobalID++;
        prototypeID = statidGlobalID;
        if (simParam != null) mSimulationParam = simParam;
        if (feedbackList != null) mDesignFeedback = feedbackList;
        mPrototypeImgTexture = prototypeImgTxt;
    }


}
