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
    public void loadTexture(Texture2D txt)
    {
        RawImage txtdest = this.GetComponent<RawImage>();
        if (txtdest == null) return;
        txtdest.texture = txt;
        refresh();
    }
    public string GetName()
    {
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
        } else
        {
            EnableImageView();
            EnableButtons();
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
