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
    public GameObject UISaveButton;
    private prototypeInstance mPrototype_Queue;
    private List<prototypeInstance> mPrototypes;
    private Vector3 initPosition;
    private List<int> selectedPrototypeIndex;
    // Use this for initialization
    void Start() {
        mPrototypes = new List<prototypeInstance>();
        mPrototype_Queue = null;
        initPosition = this.transform.position;
        //Debug.Log("[DEBUG] init position: " + initPosition);
        movetoHide();
        selectedPrototypeIndex = new List<int>();
    }

    // Update is called once per frame
    void Update() {
        if (mPrototype_Queue != null)
        {
            UISaveButton.SetActive(true);
        } else
        {
            UISaveButton.SetActive(false);
        }
    }
    public void movetoShow()
    {
        UpdateMultiveUI();
        this.transform.position = initPosition;
    }
    public void movetoHide()
    {
        UpdateMultiveUI();
        this.transform.position = new Vector3(30, 1700, 1);
    }
    public void prototypeSelectedforSim(int idx)
    {
        Debug.Log("[DEBUG]prototype selected " + idx);
        if (ApplicationControl.ActiveInstance.ContentType == DesignContent.BicycleGearSystem)
        {
            for (int i = 0; i < selectedPrototypeIndex.Count; i++)
            {
                if (idx == selectedPrototypeIndex[i])
                {
                    return;
                }
            }
            while (selectedPrototypeIndex.Count > 2)
                selectedPrototypeIndex.RemoveAt(0);
            selectedPrototypeIndex.Add(idx);
            UpdateMultiveUI();
        }
        if (ApplicationControl.ActiveInstance.ContentType == DesignContent.CameraSystem)
        {
            selectedPrototypeIndex.Clear();
            selectedPrototypeIndex.Add(idx);
            UpdateMultiveUI();
          //  movetoHide();
        }
    }
    private void randomlyFillSelectPrototypes()
    {
        if (selectedPrototypeIndex.Count >= 3) return;
        for (int i = 0; i < mPrototypes.Count && selectedPrototypeIndex.Count < 3; i++)
        {
            bool selected = false;
            foreach (var t in selectedPrototypeIndex)
            {
                if (t == i) selected = true;
            }
            if (!selected) selectedPrototypeIndex.Add(i);
        }
    }
    public void AddIncompletePrototypeInstance(Texture2D prototypeImgTxt, List<FeedbackToken> feedbackList,SimulationParam sp) {
        mPrototype_Queue = new prototypeInstance(prototypeImgTxt, sp, feedbackList);
        //mPrototypes.Add(new prototypeInstance(prototypeImgTxt, null, feedbackList));
        UpdateMultiveUI();
    }
    public void AddcompletePrototypeInstance(Texture2D prototypeImgTxt, SimulationParam simParam)
    {
        mPrototype_Queue = new prototypeInstance(prototypeImgTxt, simParam, null);
        //mPrototypes.Add(new prototypeInstance(prototypeImgTxt, simParam, null));
        UpdateMultiveUI();
        Debug.Log("[DEBUG PrototypeInstanceManager] Adding new prototype instance...");
        Debug.Log("[DEBUG PrototypeInstanceManager] rearGear: " + simParam.C2_rearGearSize + "  frontGear: " + simParam.C2_frontGearSize);
    }
    public bool isPrototypeinQueue()
    {
        if (mPrototype_Queue == null) return false;
        return true;
    }
    public void SavethePrototypeinQueue()
    {
        if (!isPrototypeinQueue()) {
            Debug.Log("[DEBUG] No prototype in the prototype queue");
            return;
        }

        ColorDetector.CaptureImage();
        mPrototypes.Add(mPrototype_Queue);
        mPrototype_Queue = null;
        UpdateMultiveUI();
    }
    private void UpdateMultiveUI()
    {
        int startIndx = 0;
        Debug.Log("[DEBUG PrototypeInstanceManager] Updating Multipane UI...");
        Color invisible = Color.black;
        invisible.a = 0;
        for (int i = 0; i < mPrototypes.Count; i++)
        {
            if (multivewPane != null && multivewPane.Length > i && multivewPane[i] != null)
            {
                multivewPane[i].GetComponent<PrototypeInventoryPane>().loadTexture(mPrototypes[i].mPrototypeImgTexture);
                multivewPane[i].GetComponent<PrototypeInventoryPane>().UpdateInfo(mPrototypes[i]);
                //multivewPane[i].GetComponent<PrototypeInventoryPane>().SetName(mPrototypes[i].name);
                multivewPane[i].GetComponent<PrototypeInventoryPane>().Selected(invisible);
            }
        }
        for (int i = mPrototypes.Count; i < multivewPane.Length; i++)
        {
            multivewPane[i].GetComponent<PrototypeInventoryPane>().UpdateInfo((prototypeInstance) null);
            multivewPane[i].GetComponent<PrototypeInventoryPane>().loadTexture(null);
            multivewPane[i].GetComponent<PrototypeInventoryPane>().Selected(invisible);
        }
        if (selectedPrototypeIndex != null)
        {
            Debug.Log("[DEBUG] selected prototype idx= ");
            for (int i = 0; i < selectedPrototypeIndex.Count; i++)
            {
                Debug.Log("  " + selectedPrototypeIndex[i]);
                if (i == 0)
                {
                    multivewPane[selectedPrototypeIndex[i]].GetComponent<PrototypeInventoryPane>().Selected(Color.red);
                }
                if (i == 1)
                {
                    multivewPane[selectedPrototypeIndex[i]].GetComponent<PrototypeInventoryPane>().Selected(Color.green);
                }
                if (i == 2)
                {
                    multivewPane[selectedPrototypeIndex[i]].GetComponent<PrototypeInventoryPane>().Selected(Color.yellow);
                }
            }
        }
        //allocate slot and update UI texture
    }
    public SimulationParam GetPrototypeProperties_Content4(out string PrototypeName)
    {
        SimulationParam ret = new SimulationParam();
        ret.C4_focalLength = 100;
        ret.C4_sensorType = "color";
        ret.C4_shutterSpeed = 500;
        PrototypeName = "";
        if (selectedPrototypeIndex==null || selectedPrototypeIndex.Count==0)  {
            return ret;
        }
        PrototypeName = mPrototypes[selectedPrototypeIndex[0]].name;
        return mPrototypes[selectedPrototypeIndex[0]].mSimulationParam;
    }
    public void GetPrototypeProperties_Content2(int idx, ref float frontgearsize, ref float reargearsize,ref string name)
    {
        //default;
        frontgearsize = 200;
        reargearsize = 200;
        name = "default";
        if (mPrototypes == null ) return;
        if (idx >= mPrototypes.Count) return;
        //when prototype is not selected        
        if (idx >= selectedPrototypeIndex.Count) return;
        int prototypeIdx = selectedPrototypeIndex[idx];
        //get the selected prototype
        frontgearsize = mPrototypes[prototypeIdx].mSimulationParam.C2_frontGearSize;
        reargearsize = mPrototypes[prototypeIdx].mSimulationParam.C2_rearGearSize;
        name = mPrototypes[prototypeIdx].name;



    }
    public void RemovePrototype(int index)
    {
        if (index >= mPrototypes.Count) return;
        mPrototypes.RemoveAt(index);
        for(int i=0;i<selectedPrototypeIndex.Count;i++)
        {
          if (selectedPrototypeIndex[i] == index) selectedPrototypeIndex.RemoveAt(i);            
        }
        for (int i = 0; i < selectedPrototypeIndex.Count; i++)
        {
            if (selectedPrototypeIndex[i] > index) selectedPrototypeIndex[i]--;
        }
        UpdateMultiveUI();
        return;
    }
    public void UpdatePrototypeName(int index)
    {
        mPrototypes[index].name = multivewPane[index].GetComponent<PrototypeInventoryPane>().GetName();
    }
}

public class prototypeInstance
{
    private static int statidGlobalID = -1;
    public Texture2D mPrototypeImgTexture;
    public int prototypeID;
    public SimulationParam mSimulationParam;
    public List<FeedbackToken> mDesignFeedback;
    public string name;
    public prototypeInstance()
    {
        mDesignFeedback =null;
        mSimulationParam = null;
        statidGlobalID++;
        prototypeID = statidGlobalID;
        name = "userprototype"+prototypeID;
       
    }
    public bool isComplete()
    {
        if (this.mSimulationParam == null || mDesignFeedback != null) return false;
        return true;
    }
    public prototypeInstance(Texture2D prototypeImgTxt,  SimulationParam simParam, List<FeedbackToken> feedbackList)
    {
        mDesignFeedback = null;
        mSimulationParam = null;
        statidGlobalID++;
        prototypeID = statidGlobalID;
        name = "Design #"+ prototypeID;
        if (simParam != null) mSimulationParam = simParam;
        if (feedbackList != null) mDesignFeedback = feedbackList;
        mPrototypeImgTexture = prototypeImgTxt;
    }


}
