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

public class PhotoReviewUI : MonoBehaviour {
    public GameObject[] multivewPane;
    public GameObject UINextButton;
    public GameObject UIPrevButton;

    private Vector3 initPosition;
    private List<PhotoShot> mPhotos;
    private int VisibleIndexStart = 0;

    // Use this for initialization
    void Start () {
        mPhotos = new List<PhotoShot>();
        
        initPosition = this.transform.position;
        //Debug.Log("[DEBUG] init position: " + initPosition);
        movetoHide();
        VisibleIndexStart = 0;
    }
	
	// Update is called once per frame
	void Update () {
        UpdatePrevNextButtons();
    }
    private void UpdatePrevNextButtons()
    {
        
        if (VisibleIndexStart + 6 < mPhotos.Count)
        {
            UINextButton.SetActive(true);
        }
        else
        {
            UINextButton.SetActive(false);
        }

        if (VisibleIndexStart - 6 >= 0)
        {
            UIPrevButton.SetActive(true);
        }
        else
        {
            UIPrevButton.SetActive(false);
        }
    }
    public void movetoShow()
    {
        UpdateMultiveUI();
        this.transform.position = initPosition;
        VisibleIndexStart = 0;
    }
    public void movetoHide()
    {
        UpdateMultiveUI();
        this.transform.position = new Vector3(30, 1700, 1);
    }
    public void AddcompletePrototypeInstance(PhotoShot p)
    {
        if (p == null) return;
        mPhotos.Insert(0, p);
        //   mPhotos.Add(p);

        //mPrototypes.Add(new prototypeInstance(prototypeImgTxt, simParam, null));
        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        sw.Start();
        SortPhotosforrandomfeature();
        UpdateMultiveUI();
        HintComparison();
        sw.Stop();
        SystemLogger.activeInstance.AddPerformance(3, sw.ElapsedMilliseconds);

    }
    private void SortPhotosforrandomfeature()
    {
        if (mPhotos.Count < 3) return;

        List<PhotoShot> PhotoswithDifferentF = new List<PhotoShot>();
        List<PhotoShot> PhotoswithDifferentS = new List<PhotoShot>();
        List<PhotoShot> PhotoswithDifferentT = new List<PhotoShot>();
        List<string> paramNames = new List<string>();
        List<string> WhichSet = new List<string>();
        int idx = 0;
        for (int i = 0; i < mPhotos.Count; i++)
        {
            if (i == idx) continue;
            float diff;
            if (mPhotos[idx].place == mPhotos[i].place && mPhotos[idx].parameter.DistanceTo(mPhotos[i].parameter, ref paramNames, out diff) == 1)
            {
                //these two differ in only one parameter
                if (paramNames.Count == 0) continue;
                if (paramNames[0] == "focallength")
                {
                    mPhotos[i].tmpDist = diff;
                    mPhotos[i].tmpIdx = i;
                    PhotoswithDifferentF.Add(mPhotos[i]);

                }
                else
                    if (paramNames[0] == "shutterspeed")
                {
                    mPhotos[i].tmpDist = diff; mPhotos[i].tmpIdx = i;
                    PhotoswithDifferentS.Add(mPhotos[i]);
                }
                else if (paramNames[0] == "sensortype")
                {
                    mPhotos[i].tmpDist = diff; mPhotos[i].tmpIdx = i;
                    PhotoswithDifferentT.Add(mPhotos[i]);
                }
            }
        }
        if (PhotoswithDifferentF.Count > 0) WhichSet.Add("f");
        if (PhotoswithDifferentS.Count > 0) WhichSet.Add("s");
        if (PhotoswithDifferentT.Count > 0) WhichSet.Add("t");
        if (WhichSet.Count == 0) return ;  // nothing to compare
        System.Random r = new System.Random();
        int randomIdx = r.Next(0, WhichSet.Count);
        if (WhichSet[randomIdx] == "f")
        {
            PhotoShot[] tmplist = PhotoswithDifferentF.ToArray();
            var sortedlist = tmplist.OrderByDescending(ps => ps.tmpDist).ToList();
            MoveToIndex1(sortedlist[0]);
            if (sortedlist.Count > 1)
            {
                MoveToIndex1(sortedlist[r.Next(1, sortedlist.Count)]);
            }
        }
        if (WhichSet[randomIdx] == "s")
        {
            PhotoShot[] tmplist = PhotoswithDifferentS.ToArray();
            var sortedlist = tmplist.OrderByDescending(ps => ps.tmpDist).ToList();
            MoveToIndex1(sortedlist[0]);
            if (sortedlist.Count > 1)
            {
                MoveToIndex1(sortedlist[r.Next(1, sortedlist.Count)]);
            }
            
        }
        if (WhichSet[randomIdx] == "t")
        {
            PhotoShot[] tmplist = PhotoswithDifferentT.ToArray();
            var sortedlist = tmplist.OrderByDescending(ps => ps.tmpDist).ToList();
            MoveToIndex1(sortedlist[0]);
            if (sortedlist.Count > 1)
            {
                MoveToIndex1(sortedlist[r.Next(1, sortedlist.Count)]);
            }
        }

    }
    private void MoveToIndex1(PhotoShot p)
    {
        Debug.Log("looking for swap......");
        if (mPhotos.Count < 3) return;
        for (int i = 0; i < mPhotos.Count; i++)
        {
            if (mPhotos[i] == p)
            {
                mPhotos.RemoveAt(i);
                mPhotos.Insert(1, p);
                return;
            }
        }
        Debug.Log("looking for swap......FAILEDs");
    }
    public void HintComparison()
    {
        if (mPhotos.Count < 2) return;
        string diffparam;
        PhotoShot subject1 = mPhotos[0];
        PhotoShot subject2 = SearchDistantPhotoforComparison(0,out diffparam);
        if (subject2 == null) return;

        PromptUI.ActiveInstance.OnAnalysis(subject1, subject2, diffparam);
    }
    private PhotoShot SearchDistantPhotoforComparison(int idx,out string diffparam)
    {
        //this looks for photos to compare with mphotos[idx].
        List<PhotoShot> PhotoswithDifferentF = new List<PhotoShot>();
        List<PhotoShot> PhotoswithDifferentS = new List<PhotoShot>();
        List<PhotoShot> PhotoswithDifferentT = new List<PhotoShot>();
        List<string> paramNames = new List<string>();
        List<string> WhichSet = new List<string>();
        diffparam = "";
        for (int i = 0; i < mPhotos.Count; i++)
        {
            if (i == idx) continue;
            float diff;
            if(mPhotos[idx].place == mPhotos[i].place && mPhotos[idx].parameter.DistanceTo(mPhotos[i].parameter, ref paramNames,out diff) == 1)
            {
                //these two differ in only one parameter
                if (paramNames.Count == 0) continue;
                if(paramNames[0]== "focallength")
                {
                    mPhotos[i].tmpDist = diff;
                    PhotoswithDifferentF.Add(mPhotos[i]);
                    
                } else
                    if (paramNames[0] == "shutterspeed")
                {
                    mPhotos[i].tmpDist = diff;
                    PhotoswithDifferentS.Add(mPhotos[i]);
                }
                else if (paramNames[0] == "sensortype")
                {
                    mPhotos[i].tmpDist = diff;
                    PhotoswithDifferentT.Add(mPhotos[i]);
                }
            }
        }
        if (PhotoswithDifferentF.Count > 0) WhichSet.Add("f");
        if (PhotoswithDifferentS.Count > 0) WhichSet.Add("s");
        if (PhotoswithDifferentT.Count > 0) WhichSet.Add("t");
        if (WhichSet.Count == 0) return null;  // nothing to compare
        System.Random r = new System.Random();
        int randomIdx = r.Next(0, WhichSet.Count);
        if (WhichSet[randomIdx] == "f")
        {
            PhotoShot[] tmplist = PhotoswithDifferentF.ToArray();
            var sortedlist = tmplist.OrderByDescending(ps => ps.tmpDist).ToList();
            diffparam = "focallength";
            return sortedlist[0];
        }
        if (WhichSet[randomIdx] == "s")
        {
            PhotoShot[] tmplist = PhotoswithDifferentS.ToArray();
            var sortedlist = tmplist.OrderByDescending(ps => ps.tmpDist).ToList();
            diffparam = "shutterspeed";
            return sortedlist[0];
        }
        if (WhichSet[randomIdx] == "t")
        {
            PhotoShot[] tmplist = PhotoswithDifferentT.ToArray();
            var sortedlist = tmplist.OrderByDescending(ps => ps.tmpDist).ToList();
            diffparam = "sensortype";
            return sortedlist[0];
        }
        return null;

    }
    private void UpdateMultiveUI()
    {
        int startIndx = 0;
        Debug.Log("[DEBUG PrototypeInstanceManager] Updating Multipane UI...");
        Color invisible = Color.black;
        invisible.a = 0;
        int paneIndex = 0;
        if (multivewPane == null) return;


        for (int i = this.VisibleIndexStart; i < mPhotos.Count && paneIndex < multivewPane.Length; i++)
        {
            if (multivewPane[paneIndex] != null)
            {
                multivewPane[paneIndex].GetComponent<PrototypeInventoryPane>().loadTexture(mPhotos[i].txt2D);
                multivewPane[paneIndex].GetComponent<PrototypeInventoryPane>().UpdateInfo(mPhotos[i]);            
                paneIndex++;
            }
        }
        for (int i = paneIndex; i < multivewPane.Length; i++)
        {
            if (multivewPane[i] != null)
            {
                multivewPane[i].GetComponent<PrototypeInventoryPane>().UpdateInfo((PhotoShot)null);
                multivewPane[i].GetComponent<PrototypeInventoryPane>().loadTexture(null);
            }
        }

        /*
        for (int i = 0; i < mPhotos.Count; i++)
        {
            if (multivewPane != null && multivewPane.Length > i && multivewPane[i] != null)
            {
                multivewPane[i].GetComponent<PrototypeInventoryPane>().loadTexture(mPhotos[i].txt2D);
                multivewPane[i].GetComponent<PrototypeInventoryPane>().UpdateInfo(mPhotos[i]);
                //multivewPane[i].GetComponent<PrototypeInventoryPane>().SetName(mPrototypes[i].name);
            
            }
        }
        for (int i = mPhotos.Count; i < multivewPane.Length; i++)
        {
            multivewPane[i].GetComponent<PrototypeInventoryPane>().UpdateInfo((PhotoShot)null);
            multivewPane[i].GetComponent<PrototypeInventoryPane>().loadTexture(null);
            
        }*/
        
        //allocate slot and update UI texture
    }

    public void RemovePrototype(int Panelindex)
    {
        if (Panelindex+VisibleIndexStart >= mPhotos.Count) return;
        mPhotos.RemoveAt(Panelindex+ VisibleIndexStart);
        if (VisibleIndexStart >= mPhotos.Count)
        {
            VisibleIndexStart -= 6;
            if (VisibleIndexStart < 0) VisibleIndexStart = 0;
        }
        UpdateMultiveUI();
        return;
    }
    public void NextPrototypes()
    {
        if (VisibleIndexStart + 6 < mPhotos.Count)
        {
            VisibleIndexStart += 6;
            UpdateMultiveUI();
        }
    }
    public void PrevPrototypes()
    {
        if (VisibleIndexStart - 6 >= 0)
        {
            VisibleIndexStart -= 6;
            UpdateMultiveUI();
        }
    }
}
