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
using UnityEngine.UI;
public class PrototypeInventoryPane : MonoBehaviour {

    public GameObject selectedMark;
    public GameObject PanelInfo;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        
	}
    public void Selected(Color c)
    {
        Image i = selectedMark.GetComponent<Image>();
        i.color = c;
    }
    public void UpdateInfo(PhotoShot p)
    {
        if (p == null)
        {
            this.SetName("");
            return;
        }
        this.SetName("Taken by "+p.prototypeName);
        //udpate parameters
        if (ApplicationControl.ActiveInstance.ContentType == DesignContent.CameraSystem)
        {
            
            if(p.parameter.C4_focalLength<0) PanelInfo.GetComponent<PrototypeInventoryInfo>().SetParam(0, "focal length",  "?");
                else PanelInfo.GetComponent<PrototypeInventoryInfo>().SetParam(0, "focal length", p.parameter.C4_focalLength + " mm");
            if(p.parameter.C4_shutterSpeed<0) PanelInfo.GetComponent<PrototypeInventoryInfo>().SetParam(1, "shutter speed", "?");
                else PanelInfo.GetComponent<PrototypeInventoryInfo>().SetParam(1, "shutter speed", p.parameter.C4_shutterSpeed+" ms");
            if (p.parameter.C4_sensorType=="none") PanelInfo.GetComponent<PrototypeInventoryInfo>().SetParam(2, "sensor type", "?");
                else PanelInfo.GetComponent<PrototypeInventoryInfo>().SetParam(2, "sensor type", p.parameter.C4_sensorType);
        }
    }
    
    public void UpdateInfo(prototypeInstance p)
    {
        if (p == null)
        {
            this.SetName("");
            return;
        }
        this.SetName(p.name);
        //udpate parameters
        if (ApplicationControl.ActiveInstance.ContentType == DesignContent.BicycleGearSystem)
        {
            if (p.mSimulationParam.C2_GearRatio < 0) PanelInfo.GetComponent<PrototypeInventoryInfo>().SetParam(0, "Gear Ratio", "?");
            else PanelInfo.GetComponent<PrototypeInventoryInfo>().SetParam(0, "Gear Ratio", p.mSimulationParam.C2_GearRatio.ToString("n2"));
            if (p.mSimulationParam.C2_frontGearSize < 0) PanelInfo.GetComponent<PrototypeInventoryInfo>().SetParam(1, "front gear size", "?");
            else PanelInfo.GetComponent<PrototypeInventoryInfo>().SetParam(1, "Front Gear Size",  (int)p.mSimulationParam.C2_frontGearSize/6 + " mm");
            if (p.mSimulationParam.C2_rearGearSize <0) PanelInfo.GetComponent<PrototypeInventoryInfo>().SetParam(2, "rear gear suze", "?");
            else PanelInfo.GetComponent<PrototypeInventoryInfo>().SetParam(2, "Rear Gear Size", (int)p.mSimulationParam.C2_rearGearSize/6 +" mm");
        }
        if (ApplicationControl.ActiveInstance.ContentType == DesignContent.CameraSystem)
        {
            if (p.mSimulationParam.C4_focalLength < 0) PanelInfo.GetComponent<PrototypeInventoryInfo>().SetParam(0, "focal length", "?");
            else PanelInfo.GetComponent<PrototypeInventoryInfo>().SetParam(0, "focal length", (int)p.mSimulationParam.C4_focalLength + " mm");
            if (p.mSimulationParam.C4_shutterSpeed < 0) PanelInfo.GetComponent<PrototypeInventoryInfo>().SetParam(1, "shutter speed", "?");
            else PanelInfo.GetComponent<PrototypeInventoryInfo>().SetParam(1, "shutter speed", (int)p.mSimulationParam.C4_shutterSpeed + " ms");
            if (p.mSimulationParam.C4_sensorType == "none") PanelInfo.GetComponent<PrototypeInventoryInfo>().SetParam(2, "sensor type", "?");
            else PanelInfo.GetComponent<PrototypeInventoryInfo>().SetParam(2, "sensor type", p.mSimulationParam.C4_sensorType);       
        }
            
    }
    public void loadTexture(Texture2D txt)
    {
        RawImage txtdest = this.GetComponent<RawImage>();
        if (txtdest == null) return;
        txtdest.texture = txt;
        
        
        refresh();
    }
    public string GetName()
    {
        return PanelInfo.GetComponent<PrototypeInventoryInfo>().GetName();
        for (int i = 0; i < this.transform.childCount; i++)
        {
            InputField namefield = this.transform.GetChild(i).gameObject.GetComponent<InputField>();
            if (namefield == null) continue;
            return namefield.text;
        }
        return "";
    }
    public void SetName(string name_)
    {
        PanelInfo.GetComponent<PrototypeInventoryInfo>().SetName(name_);
        return;
        for (int i = 0; i < this.transform.childCount; i++)
        {
            InputField namefield = this.transform.GetChild(i).gameObject.GetComponent<InputField>();
            if (namefield == null) continue;
            namefield.text = name_;
        }
    }

           
    public void refresh()
    {
        RawImage txt = this.GetComponent<RawImage>();
        if (txt.texture == null)
        {
            DisableImageView();
            DisableButtons();
            PanelInfo.SetActive(false);
        } else
        {
           // Debug.Log("[DEBUG] Loding texture...");
            EnableImageView();
            EnableButtons();
            PanelInfo.SetActive(true);
        }

    }

    private void DisableImageView()
    {
        RawImage txtdest = this.GetComponent<RawImage>();
        if (txtdest == null) return;
        Color c = txtdest.color;
        c.a = 0;
        txtdest.color = c;
    }
    private void EnableImageView()
    {
        RawImage txtdest = this.GetComponent<RawImage>();
        if (txtdest == null) return;
        Color c = txtdest.color;
        c.a = 1;
        txtdest.color = c;
        
    }
    private void DisableButtons()
    {
        for (int i = 0; i < this.transform.childCount; i++)
            this.transform.GetChild(i).gameObject.SetActive(false);
    }
    private void EnableButtons()
    {
        for (int i = 0; i < this.transform.childCount; i++)
            this.transform.GetChild(i).gameObject.SetActive(true);
    }

}
