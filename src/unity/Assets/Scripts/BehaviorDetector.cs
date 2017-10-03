using UnityEngine;
using System.Collections;
using OpenCvSharp;
using OpenCvSharp.Blob;
using OpenCvSharp.CPlusPlus;
using System.Collections.Generic;

using System.IO;
using System.Text;

using System.Runtime.Serialization.Formatters.Binary;
using System;
using System.Text.RegularExpressions;
public class BehaviorDetector : MonoBehaviour {


    private List<UserDescriptionInfo> mBehaviorList = null;
    // Use this for initialization
    void Start () {
        mBehaviorList= new List<UserDescriptionInfo>();

    }
	
	// Update is called once per frame
	void Update () {
        if (GlobalRepo.getLearningCount() <= 0) return;

        //initial build
        if (mBehaviorList.Count == 0) initialBuildMarkers();
        else iterativeBuildMarkers();
        //iterative build. skip searching for region but update text label.
        debugBehaviorList();

    }
    public List<UserDescriptionInfo> exportBehaviorList()
    {
        
        return mBehaviorList;
    }
    
    public void debugBehaviorList()
    {
        
        foreach(var b in mBehaviorList)
        {
            Debug.Log("[DEBUG-Behavior-"+ new System.Random().Next(0,99999)+"] text:" + b.InfoTextLabelstring + "  dist:" + b.BehaviorDistance +" type:"+Content.getBaviorLabelText(b.InfoBehaviorCategory));
        }
    }
    
    private void iterativeBuildMarkers()
    {
        CvMat grayRegionImage = GlobalRepo.GetRepo(RepoDataType.dRawRegionGray);
        if (grayRegionImage == null)
        {
            Debug.Log("[ERROR] Fail to retrieve Gray Region Image");
            return;
        }
        List<UserDescriptionInfo> newBlist = new List<UserDescriptionInfo>();
        CVProc.findRectangularMarkers(grayRegionImage, ref newBlist);
        seperateBVImg(ref newBlist);
        this.updateBehaviorText(ref newBlist);
        updateBehaviorList(ref newBlist);
    }
    private void updateBehaviorList(ref List<UserDescriptionInfo> newbehaviorlist)
    {
        for (int i = 0; i < newbehaviorlist.Count; i++)
        {
            bool foundMatchBehavior = false; ;
            for (int j = 0; j < mBehaviorList.Count; j++)
            {
                if (newbehaviorlist[i].center.DistanceTo(mBehaviorList[j].center) > 25) continue;

                if (newbehaviorlist[i].BehaviorDistance < mBehaviorList[j].BehaviorDistance)
                {
                    mBehaviorList.RemoveAt(j);
                    mBehaviorList.Add(newbehaviorlist[i]);
                }
                foundMatchBehavior = true;
                break;

            }
            if(!foundMatchBehavior)
            {
                mBehaviorList.Add(newbehaviorlist[i]);
            }
        }
    }
    private void initialBuildMarkers()
    {
        CvMat grayRegionImage = GlobalRepo.GetRepo(RepoDataType.dRawRegionGray);
        if (grayRegionImage == null)
        {
            Debug.Log("[ERROR] Fail to retrieve Gray Region Image");
            return;
        }
        mBehaviorList.Clear();
        CVProc.findRectangularMarkers(grayRegionImage, ref mBehaviorList);
        seperateBVImg(ref mBehaviorList);
        this.updateBehaviorText(ref mBehaviorList);
    }
    private void seperateBVImg(ref List<UserDescriptionInfo> blist)
    {
        for (int l = 0; l < blist.Count; l++)
        {
            CvMat BBVImageUC8 = blist[l].image.Clone();
            BBVImageUC8.Threshold(BBVImageUC8, 150, 255, ThresholdType.Binary);
            blist[l].BVImage = null;
            //horizontal scan
            bool BVBorderFound = false;
            int successiveBorder = 0;
            for (int pos = BBVImageUC8.Width / 2; pos < BBVImageUC8.Width * 9 / 10; pos++)
            {
                CvMat xCol = BBVImageUC8.GetCol(pos);
                int cntBlack = 0;
                for (int y = 0; y < BBVImageUC8.Height; y++)
                {
                    if (xCol.Get1D(y) == 0) cntBlack++;
                }
                if (cntBlack > BBVImageUC8.Height * 7 / 10)
                {
                    successiveBorder++;
                }
                else successiveBorder--;
                if (successiveBorder >= 3)
                {
                    BVBorderFound = true;
                    CvRect BImgRect = new CvRect(0, 0, pos - successiveBorder, BBVImageUC8.Height);
                    CvRect BVImgRect = new CvRect(pos, 0, BBVImageUC8.Width - pos -1, BBVImageUC8.Height);
                    blist[l].BVImage = blist[l].image.GetSubArr(out blist[l].BVImage, BVImgRect).Clone();
                    blist[l].image = blist[l].image.GetSubArr(out blist[l].image, BImgRect).Clone();
                    GlobalRepo.showDebugImage("bv image", blist[l].BVImage);
                    GlobalRepo.showDebugImage("b image", blist[l].image);
                    break;
                }
                else if (successiveBorder < 0) successiveBorder = 0;

            }

        }
    }
    private void updateBehaviorText(ref List<UserDescriptionInfo> blist)
    {    
        
        for (int l = 0; l < blist.Count; l++)
        {
            //update behavior images with pre-saved mask
          
            CvMat inputImageUC8 = blist[l].image;    
            
            float confidence = 0;
            string rText = CVProc.textRecognitionTesseractviaFile(inputImageUC8, out confidence);
            Regex rgx = new Regex("[^a-zA-Z0-9]");
            rText = rgx.Replace(rText, "");
            blist[l].InfoTextLabelstring = rText;
            int newDistance; 
            BehaviorCategory type_ = Content.DetermineBehaviorType(rText, out newDistance);
            blist[l].BehaviorDistance = newDistance;
            blist[l].InfoBehaviorCategory = type_;
            blist[l].InfoTextLabelConfidence = confidence;
            blist[l].InfoTextLabelstring = rText;
          
        }
    }
    public void recognizeBehaviorVariables(FBSModel pFBSModel)
    {
        if (pFBSModel == null || this.mBehaviorList == null) return;

        for(int i = 0; i < mBehaviorList.Count; i++)
        {
            UserDescriptionInfo bdesc = mBehaviorList[i];
            BehaviorVariableEntity be = pFBSModel.GetBehaviorEntity(bdesc.InfoBehaviorCategory);
            int borderHorizontalPos = Content.getBVHorizontalBorder(bdesc.InfoBehaviorCategory);
            if (be == null || borderHorizontalPos==0) continue;

            //extract BV image
            //    CvMat BVImg = CVProc.getSubimgHorizontalBorder(bdesc.image, borderHorizontalPos).Clone();
            if (bdesc.BVImage == null) continue;
            CvMat BVImg = bdesc.BVImage.Clone();
           
            float measureLevel = measureBV(BVImg, be.VariableType);
            if (be.VariableType == BehaviorVariableType.Numeric)
            {
                bdesc.InfoNumericalBVPercent = measureLevel;
                bdesc.InfoNumericalBVValue = (float)be.numericalValueRange.Key;
                if (measureLevel != -1) bdesc.InfoNumericalBVValue += (be.numericalValueRange.Value - be.numericalValueRange.Key) * measureLevel / 100.0f;
            }
            if (be.VariableType == BehaviorVariableType.Categorical)
            {
                bdesc.InfoCategoricalBVValue = "";
            }
                Debug.Log("[DEBUG]BV type="+be.VariableType+"Measure BV=" + measureLevel);

            //detect according to the types
        }

    }
    private float measureBV(CvMat BVImg, BehaviorVariableType bvt)
    {
        int ret = -1;
        if (bvt == BehaviorVariableType.None) return ret;
        if (bvt==BehaviorVariableType.Numeric)
        {
         
            BVImg.Threshold(BVImg, 80, 255, ThresholdType.Binary);
            
            ret = (int)CVProc.measureHorizontalLevelLeftAlign(BVImg);
            if (ret < 8) ret = -1;
        }
        if (bvt == BehaviorVariableType.Categorical)
        {
            ret = -1;
        }


        return (float)ret;
    }
}
