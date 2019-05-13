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

    public int  ErrorTolerance;
    public GameObject[] prototypePane;
    public GameObject UISaveButton;
    public GameObject UINextButton;
    public GameObject UIPrevButton;
    public GameObject SubjectsHint;
    private prototypeInstance mPrototype_Queue;
    private List<prototypeInstance> mPrototypes;
    private Vector3 initPosition;
    private List<int> selectedPrototypeIndex;

    private int VisibleIndexStart = 0;
    public static PrototypeInstanceManager mActiveInstance;

    private int SwappingIndexRR = 0;
    // Use this for initialization
    void Start() {
        mPrototypes = new List<prototypeInstance>();
        mPrototype_Queue = null;
        initPosition = this.transform.position;
        //Debug.Log("[DEBUG] init position: " + initPosition);
        movetoHide();
        selectedPrototypeIndex = new List<int>();
        VisibleIndexStart = 0;
        mActiveInstance = this;
        SwappingIndexRR = 0;
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
        //sort prototypes based on gear ratio
        clusteringPrototypes(false);
        UpdateMultiveUI();
        this.transform.position = initPosition;
    }
    public void clusteringPrototypes(bool Execute)
    {

        List<int> invalidIndex = new List<int>();
        List<prototypeInstance> SortedList = new List<prototypeInstance>();
        foreach(var t in mPrototypes)
        {
            if (t.mSimulationParam.C2_GearRatio > 0) SortedList.Add(t);
        }
        if (SortedList.Count <= 5)
        {
            if (!Execute) this.SubjectsHint.SetActive(false);
            return;
        }
        if (!Execute)
        {
            this.SubjectsHint.SetActive(true);
            return;
        }
        SortedList = SortedList.OrderBy(o => o.mSimulationParam.C2_GearRatio).ToList();
        bool Front = true;
        while (SortedList.Count > 3)
        {
            if(Front)
            {
                SortedList.RemoveAt(1);
            } else
            {
                SortedList.RemoveAt(SortedList.Count - 2);
            }
            Front = !Front;
        }
        foreach(var t in mPrototypes)
        {
            if (SortedList.Contains(t)) continue;
            SortedList.Add(t);
        }
        mPrototypes = SortedList;
        selectedPrototypeIndex.Clear();
        selectedPrototypeIndex.Add(0);
        selectedPrototypeIndex.Add(1);
        selectedPrototypeIndex.Add(2);
        UpdateMultiveUI();

    }
    public void movetoHide()
    {
        UpdateMultiveUI();
        this.transform.position = new Vector3(30, 1700, 1);
    }
    public void prototypeSelectedforSim(int Panelidx)
    {
        Debug.Log("[DEBUG]prototype selected " + Panelidx);
        if (ApplicationControl.ActiveInstance.ContentType == DesignContent.BicycleGearSystem)
        {
            for (int i = 0; i < selectedPrototypeIndex.Count; i++)
            {
                if (VisibleIndexStart+Panelidx == selectedPrototypeIndex[i])
                {
                    return;
                }
            }
            if (selectedPrototypeIndex.Count >= 3)
            {
                
                selectedPrototypeIndex[SwappingIndexRR] = VisibleIndexStart + Panelidx;
                SwappingIndexRR++;
                if (SwappingIndexRR >= 3) SwappingIndexRR = 0;
            } else selectedPrototypeIndex.Add(VisibleIndexStart + Panelidx);


            //selectedPrototypeIndex.RemoveAt(0);
            //selectedPrototypeIndex.Add(VisibleIndexStart+Panelidx);
            UpdateMultiveUI();
        }
        if (ApplicationControl.ActiveInstance.ContentType == DesignContent.CameraSystem)
        {
            selectedPrototypeIndex.Clear();
            selectedPrototypeIndex.Add(VisibleIndexStart+Panelidx);
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
    public void AddcompletePrototypeInstance(Texture2D prototypeImgTxt, List<FeedbackToken> flist,SimulationParam simParam)
    {

        mPrototype_Queue = new prototypeInstance(prototypeImgTxt, simParam, flist);
        if (IsThisNewPrototype(mPrototype_Queue))
        {
            SavethePrototypeinQueue();
            //pop-up to prompt testing
            //  Debug.Log("N of Feedback==================" + flist.Count);
            if (flist == null || flist.Count <= ErrorTolerance)
            {
                if (ApplicationControl.ActiveInstance.ContentType == DesignContent.BicycleGearSystem)
                {
                    PromptUI.ActiveInstance.OnTestNewBike();
                }
                else if (ApplicationControl.ActiveInstance.ContentType == DesignContent.CameraSystem)
                {
                    PromptUI.ActiveInstance.OnTestNewCamera();
                }
            }
        }
        UpdateMultiveUI();
        Debug.Log("[DEBUG PrototypeInstanceManager] Adding new COMPLETE prototype instance...");
       // Debug.Log("[DEBUG PrototypeInstanceManager] rearGear: " + simParam.C2_rearGearSize + "  frontGear: " + simParam.C2_frontGearSize);
    }
    public void SaveandSelectLastOne()
    {
        if (mPrototype_Queue != null)
        {
            SavethePrototypeinQueue();
        }
        if (mPrototypes.Count < 1) return;
        if (ApplicationControl.ActiveInstance.ContentType == DesignContent.BicycleGearSystem)
        {
            
            
            bool isExist = false;
            foreach(var ii in selectedPrototypeIndex)
            {
                if (ii == mPrototypes.Count - 1) isExist = true;
            }
            if (!isExist)
            {
                if (selectedPrototypeIndex.Count >= 3)
                {

                    selectedPrototypeIndex[SwappingIndexRR] = mPrototypes.Count - 1;
                    SwappingIndexRR++;
                    if (SwappingIndexRR >= 3) SwappingIndexRR = 0;
                }
                else selectedPrototypeIndex.Add(mPrototypes.Count - 1);
                
            }
        }
        if (ApplicationControl.ActiveInstance.ContentType == DesignContent.CameraSystem)
        {
            selectedPrototypeIndex.Clear();

            selectedPrototypeIndex.Add(0);
            
            //  movetoHide();
        }
        UpdateMultiveUI();
    }
    private bool IsThisNewPrototype(prototypeInstance p)
    {
        bool isNew = true;
        if (mPrototypes == null) return true;
        for(int i = 0; i < mPrototypes.Count; i++)
        {
            if (p.DistanceTo(mPrototypes[i]) == 0)
            {
                isNew = false;
                break;
            }
        }
        return isNew;
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

        //  ColorDetector.CaptureImage();
        mPrototypes.Insert(0, mPrototype_Queue);
        //mPrototypes.Add(mPrototype_Queue);
        mPrototype_Queue = null;
        UpdateMultiveUI();
    }
    private void UpdateMultiveUI()
    {
        int startIndx = 0;
   //     Debug.Log("[DEBUG PrototypeInstanceManager] Updating Multipane UI...");
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
          //  Debug.Log("[DEBUG] selected prototype idx= ");
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
                t.a = 0.8f;
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
    public Texture2D GetPrototypePhysicalImage()
    {
        if (selectedPrototypeIndex == null || selectedPrototypeIndex.Count == 0)
        {
            return null;
        }
        return mPrototypes[selectedPrototypeIndex[0]].mPrototypeImgTexture;
        
    }
    public void GetPrototypeProperties_Content2(int idx, ref float frontgearsize, ref float reargearsize,ref string name)
    {
        
        //default;
        frontgearsize = 0;
        reargearsize = 0;
        name = "?";
        if (mPrototypes == null ) return;
        if (idx >= mPrototypes.Count ) return;
        //when prototype is not selected        
        if (idx >= selectedPrototypeIndex.Count) return;
        int prototypeIdx = selectedPrototypeIndex[idx];
        if (prototypeIdx < 0 || prototypeIdx >= mPrototypes.Count) return;
        if (mPrototypes[prototypeIdx]== null || mPrototypes[prototypeIdx].mSimulationParam == null) return;
        //get the selected prototype
        frontgearsize = mPrototypes[prototypeIdx].mSimulationParam.C2_frontGearSize;
        reargearsize = mPrototypes[prototypeIdx].mSimulationParam.C2_rearGearSize;
        name = mPrototypes[prototypeIdx].name;
    }
    public bool CheckPrototypeError_Content2(int idx)
    {
        if (mPrototypes == null) return true;
        if (idx >= mPrototypes.Count) return true;
        //when prototype is not selected        
        if (idx >= selectedPrototypeIndex.Count) return true;
        int prototypeIdx = selectedPrototypeIndex[idx];
        if (prototypeIdx < 0 || prototypeIdx >= mPrototypes.Count) return true;
        if (mPrototypes[prototypeIdx] == null || mPrototypes[prototypeIdx].mSimulationParam == null) return true;

        if (mPrototypes[prototypeIdx].mSimulationParam.C2_rearGearAnimSpeed == 0) return false;
        if (mPrototypes[prototypeIdx].mSimulationParam.C2_chainAnimSpeed == 0) return false;
        if (mPrototypes[prototypeIdx].mSimulationParam.C2_frontGearAnimSpeed == 0) return false;
        if (mPrototypes[prototypeIdx].mSimulationParam.C2_pedalAnimSpeed == 0) return false;
        return true;
    }
    public Texture2D GetPrototypePhysicalImage(int idx)
    {
        if (mPrototypes == null) return null;
        if (idx >= mPrototypes.Count) return null;
        //when prototype is not selected        
        if (idx >= selectedPrototypeIndex.Count) return null;
        int prototypeIdx = selectedPrototypeIndex[idx];
        if (prototypeIdx < 0 || prototypeIdx >= mPrototypes.Count) return null;
        if (mPrototypes[prototypeIdx] == null || mPrototypes[prototypeIdx].mSimulationParam == null) return null;
        return mPrototypes[prototypeIdx].mPrototypeImgTexture;

    }
    public void RemovePrototype(int indexinPanel)
    {
        if (VisibleIndexStart+indexinPanel >= mPrototypes.Count) return;
        mPrototypes.RemoveAt(VisibleIndexStart+indexinPanel);
        if(VisibleIndexStart>=mPrototypes.Count) {
            VisibleIndexStart -= 6;
            if (VisibleIndexStart < 0) VisibleIndexStart = 0;
        }
        for(int i=0;i<selectedPrototypeIndex.Count;i++)
        {
          if (selectedPrototypeIndex[i] == VisibleIndexStart+indexinPanel) selectedPrototypeIndex.RemoveAt(i);            
        }
        for (int i = 0; i < selectedPrototypeIndex.Count; i++)
        {
            if (selectedPrototypeIndex[i] > VisibleIndexStart + indexinPanel)
            {
                selectedPrototypeIndex[i]--;                
            }
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
    public int NofFeedback;
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
    public int DistanceTo(prototypeInstance p)
    {
        int dist = 0;
        if (p == null & this.mSimulationParam==null) return 0;
        if (p == null || this.mSimulationParam == null) return 100;
        return p.mSimulationParam.DistanceTo(this.mSimulationParam);
        
    }
    public prototypeInstance(Texture2D prototypeImgTxt,  SimulationParam simParam, List<FeedbackToken> feedbackList)
    {
        mDesignFeedback = null;
        mSimulationParam = null;
        statidGlobalID++;
        prototypeID = statidGlobalID;
        name = "Design #"+ prototypeID;
        
        if (simParam != null) mSimulationParam = simParam;
        if (feedbackList != null)
        {
            NofFeedback = feedbackList.Count;
            //mDesignFeedback = feedbackList;  //memory allocated to feedback
        }
        else NofFeedback = 0;
        mPrototypeImgTexture = prototypeImgTxt;
    }


}

