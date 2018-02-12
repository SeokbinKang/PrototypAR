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
    
    private Vector3 initPosition;
    private List<PhotoShot> mPrototypes;
    // Use this for initialization
    void Start () {
        mPrototypes = new List<PhotoShot>();
        
        initPosition = this.transform.position;
        //Debug.Log("[DEBUG] init position: " + initPosition);
        movetoHide();        
    }
	
	// Update is called once per frame
	void Update () {
	
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
    public void AddcompletePrototypeInstance(PhotoShot p)
    {
        if (p == null) return;
        mPrototypes.Add(p);
        
        //mPrototypes.Add(new prototypeInstance(prototypeImgTxt, simParam, null));
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
                multivewPane[i].GetComponent<PrototypeInventoryPane>().loadTexture(mPrototypes[i].txt2D);
                multivewPane[i].GetComponent<PrototypeInventoryPane>().UpdateInfo(mPrototypes[i]);
                //multivewPane[i].GetComponent<PrototypeInventoryPane>().SetName(mPrototypes[i].name);
            
            }
        }
        for (int i = mPrototypes.Count; i < multivewPane.Length; i++)
        {
            multivewPane[i].GetComponent<PrototypeInventoryPane>().UpdateInfo((PhotoShot)null);
            multivewPane[i].GetComponent<PrototypeInventoryPane>().loadTexture(null);
            
        }
        
        //allocate slot and update UI texture
    }

    public void RemovePrototype(int index)
    {
        if (index >= mPrototypes.Count) return;
        mPrototypes.RemoveAt(index);       
        UpdateMultiveUI();
        return;
    }
}
