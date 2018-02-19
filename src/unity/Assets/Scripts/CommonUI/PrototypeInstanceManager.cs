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

    public GameObject[] prototypePane;
    public GameObject UISaveButton;
    public GameObject UINextButton;
    public GameObject UIPrevButton;
    private prototypeInstance mPrototype_Queue;
    private List<prototypeInstance> mPrototypes;
    private Vector3 initPosition;
    private List<int> selectedPrototypeIndex;

    private int VisibleIndexStart = 0;
    // Use this for initialization
    void Start() {
        mPrototypes = new List<prototypeInstance>();
        mPrototype_Queue = null;
        initPosition = this.transform.position;
        //Debug.Log("[DEBUG] init position: " + initPosition);
        movetoHide();
        selectedPrototypeIndex = new List<int>();
        VisibleIndexStart = 0;
    }

    // Update is called once per frame
    void Update() {
        UpdateSaveButton();
        UpdatePrevNextButtons();
    }
    private void UpdatePrevNextButtons()
    {
        if(VisibleIndexStart+6 < mPrototypes.Count)
        {
            UINextButton.SetActive(true);
        } else
        {
            UINextButton.SetActive(false);
        }

        if (VisibleIndexStart -6 >=0)
        {
            UIPrevButton.SetActive(true);
        }
        else
        {
            UIPrevButton.SetActive(false);
        }
    }
    private void UpdateSaveButton()
    {
        if (mPrototype_Queue != null)
        {
            UISaveButton.SetActive(true);
        }
        else
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
        int paneIndex = 0;
        if (prototypePane == null) return;
        for (int i = this.VisibleIndexStart; i < mPrototypes.Count && paneIndex< prototypePane.Length; i++)
        {
            if (prototypePane[paneIndex] != null)
            {
                prototypePane[paneIndex].GetComponent<PrototypeInventoryPane>().loadTexture(mPrototypes[i].mPrototypeImgTexture);
                prototypePane[paneIndex].GetComponent<PrototypeInventoryPane>().UpdateInfo(mPrototypes[i]);             
                prototypePane[paneIndex].GetComponent<PrototypeInventoryPane>().Selected(invisible);
                paneIndex++;
            }
        }
        for (int i = paneIndex; i < prototypePane.Length; i++)
        {
            prototypePane[i].GetComponent<PrototypeInventoryPane>().UpdateInfo((prototypeInstance) null);
            prototypePane[i].GetComponent<PrototypeInventoryPane>().loadTexture(null);
            prototypePane[i].GetComponent<PrototypeInventoryPane>().Selected(invisible);
        }
        if (selectedPrototypeIndex != null)
        {
            Debug.Log("[DEBUG] selected prototype idx= ");
            for (int i = 0; i < selectedPrototypeIndex.Count; i++)
            {
                Debug.Log("  " + selectedPrototypeIndex[i]);
                Color t = Color.red;
                if (i == 0)
                {
                    t = Color.red;                    
                } else if (i == 1)
                {
                    t = Color.green;                    
                } else if (i == 2)
                {
                    t = Color.yellow;                    
                }
                int SelectedPaneIndex = selectedPrototypeIndex[i] - VisibleIndexStart;
                if(SelectedPaneIndex>=0 && SelectedPaneIndex< prototypePane.Length)
                {
                    prototypePane[SelectedPaneIndex].GetComponent<PrototypeInventoryPane>().Selected(t);
                }
            }
        }
        //allocate slot and update UI texture
    }
    public SimulationParam GetPrototypeProperties_Content4(out string PrototypeName)
    {
        //default parameters
        SimulationParam ret = new SimulationParam();
        ret.C4_focalLength = 20;
        ret.C4_sensorType = "color";
        ret.C4_shutterSpeed = 50;
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
        mPrototypes[index].name = prototypePane[index].GetComponent<PrototypeInventoryPane>().GetName();
    }
    public void NextPrototypes()
    {
        if(VisibleIndexStart+6 < mPrototypes.Count)
        {
            VisibleIndexStart += 6;
            UpdateMultiveUI();
        }
    }
    public void PrevPrototypes()
    {
        if (VisibleIndexStart - 6 >=0)
        {
            VisibleIndexStart -= 6;
            UpdateMultiveUI();
        }
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
