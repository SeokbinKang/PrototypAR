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
        mPhotos.Add(p);
        
        //mPrototypes.Add(new prototypeInstance(prototypeImgTxt, simParam, null));
        UpdateMultiveUI();
        
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

    public void RemovePrototype(int index)
    {
        if (index >= mPhotos.Count) return;
        mPhotos.RemoveAt(index);       
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
