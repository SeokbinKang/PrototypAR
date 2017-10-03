using UnityEngine;
using System.Collections;

using OpenCvSharp;
using OpenCvSharp.Blob;
using OpenCvSharp.CPlusPlus;
using System.Collections.Generic;

using System.IO;
using System.Text;

public class ImageProcessingBed : MonoBehaviour {


    private int test_MatchObject = 1;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
     /*   if(test_MatchObject>0)
        {
            test_MatchObject--;
            Asset2DTexture virtualObj = GlobalRepo.GetTexture2D(ModelCategory.LungLeft);
            CvMat userObj = new CvMat("./log/testImageMatch/userinput1.png", LoadMode.Unchanged);
            GlobalRepo.showDebugImage("TestInput1", userObj);
            GlobalRepo.showDebugImage("TestVirtual1", virtualObj.txtBGRAImg);
           IP_ObjectShape.IPMatchObjectShapes(userObj, virtualObj.txtBGRAImg);
        }*/
	
	}

   
    
}

