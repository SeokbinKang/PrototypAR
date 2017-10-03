using UnityEngine;
using System.Collections;

using OpenCvSharp;
using OpenCvSharp.Blob;
using OpenCvSharp.CPlusPlus;
using System.Collections.Generic;

using System.IO;
using System.Text;

public class IP_ObjectShape : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
    private static double Param_MatchObject_ShrinkStep = 0.03f;
    private static double Param_MatchObject_ShrinkMin = 0.2f;

    private static double Param_MatchObject_TransitionRange = 0.2f;
    private static double Param_MatchObject_TransitionStep = 0.05f;

    private static double Param_MatchObject_InclusionThreshold = 0.95f;
    public static Point[] IPMatchObjectShapes(CvMat userObject, CvMat virtualObject, bool convexUsrObj)
    {
        //we assume that both images are RGBA

        ObjectBlobInfo userObjInfo = BlobAnalysis.ExtractBlobInfo(userObject, convexUsrObj);
        
        ObjectBlobInfo virtualObjInfo = BlobAnalysis.ExtractBlobInfo(virtualObject, convexUsrObj);
        

        Point[] shapeOutline;
        //initial scaling
        if(userObjInfo==null || virtualObjInfo==null)
        {
            Debug.Log("ERROR: ObjectInfo = null");
            return null;
        }
        CvMat debugI = virtualObject.Clone();
        foreach(Point t in virtualObjInfo.contour)
        {
            debugI.DrawCircle(new CvPoint(t.X, t.Y), 2, CvColor.Red);
        }
        GlobalRepo.showDebugImage("virtualobj", debugI);
        float scalefactor = 1;
        if (virtualObjInfo.areaSize < userObjInfo.areaSize) scalefactor = (float)( userObjInfo.areaSize / virtualObjInfo.areaSize);
        virtualObjInfo.ScaletoObject(userObjInfo, scalefactor);
        Point transitionVector = new Point(0, 0);
        double rotatevalue=0;
        double optimalRotation = 0;
        //size iteration
        double[] maxDistanceArray = null;
        for (double sizeRatio = 1 - Param_MatchObject_ShrinkStep; sizeRatio > Param_MatchObject_ShrinkMin; sizeRatio -= Param_MatchObject_ShrinkStep)
        {  // scale variant
            double maxInclusionRatio = -1;
            virtualObjInfo.ScaletoObject(userObjInfo, sizeRatio);
            for (double transX = -1 * ((double)userObjInfo.width) * Param_MatchObject_TransitionRange; transX <= ((double)userObjInfo.width) * Param_MatchObject_TransitionRange; transX += ((double)userObjInfo.width) * Param_MatchObject_TransitionStep)
            {
                for (double transY = -1 * ((double)userObjInfo.width) * Param_MatchObject_TransitionRange; transY <= ((double)userObjInfo.width) * Param_MatchObject_TransitionRange; transY += ((double)userObjInfo.width) * Param_MatchObject_TransitionStep)
                {
                    double inclusionRatio;
                    double inclusionRatioRotate;

                    //  inclusionRatio = BlobAnalysis.GetInclusionRatio(virtualObjInfo, userObjInfo, transX, transY);
                    double[] distanceArray = null;
                    inclusionRatioRotate = BlobAnalysis.GetInclusionRatio(virtualObjInfo, userObjInfo, transX, transY,out rotatevalue,ref distanceArray);
              //      Debug.Log("inc :" + inclusionRatioRotate + "rot :" + rotatevalue + "size: "+sizeRatio);
                    if (inclusionRatioRotate > maxInclusionRatio)
                    {
                        transitionVector.X = (int)transX;
                        transitionVector.Y = (int)transY;
                        maxInclusionRatio = inclusionRatioRotate;
                        optimalRotation = rotatevalue;
                        maxDistanceArray = distanceArray;
                    }


                }
            }
            if (maxInclusionRatio >= Param_MatchObject_InclusionThreshold)
            {
                if(GlobalRepo.Setting_ShowDebugImgs())
                    Debug.Log("Max Inclusion Ratio : " + maxInclusionRatio + "size ratio : " + sizeRatio + "transition : (" + transitionVector.X + " , " + transitionVector.Y + ")");

                //examine distanceArray to see if the shape is good enough 
                bool shapeQuality = ExamineShapeQuality(maxDistanceArray,userObjInfo);
                if (shapeQuality) return null;
                else return BlobAnalysis.GetOverlapShapeOutline(virtualObjInfo, userObjInfo, transitionVector.X, transitionVector.Y, optimalRotation);
                


            }
            else
            {
                if (maxInclusionRatio < 0.8f) sizeRatio -= Param_MatchObject_ShrinkStep;
               Debug.Log("Inclusion Ratio : " + maxInclusionRatio + "size ratio : " + sizeRatio + "transition : (" + transitionVector.X + " , " + transitionVector.Y + ")");
            }

        }
        return null;

    }
    private static bool ExamineShapeQuality(double[] distanceArray, ObjectBlobInfo userobj)
    {
        bool ret = false;

        //testout
        double[] quality_threshold = new double[5] { 10, 20, 30, 40, 50 };
        double[] count = new double[quality_threshold.Length];
        double[] percent = new double[quality_threshold.Length];
        double proportional_threshold = Mathf.Min(userobj.width, userobj.height) * 0.05f;

        for (int i = 0; i < quality_threshold.Length; i++)
            quality_threshold[i] = proportional_threshold * (i + 1);

        foreach (var d in distanceArray)
        {
            for(int i = 0; i < quality_threshold.Length; i++)
            {
                if (d > quality_threshold[i]) count[i]++;
            }
        }
        for (int i = 0; i < quality_threshold.Length; i++)
        {
            percent[i] = count[i] * 100 / (double)distanceArray.Length ;
        }
        if (GlobalRepo.Setting_ShowDebugImgs())
        {
            Debug.Log("Shape Examination Result");
            for (int i = 0; i < quality_threshold.Length; i++)
            {
                Debug.Log("% of distance over " + quality_threshold[i] + " : " + percent[i] + "\t count:" + count[i]);
            }
        }
        if (percent[1] > 10) ret = false;       //the distance over the 1/10 of width/height is more than 10% of points. then rejest the shape.
        else ret = true;    
            return ret;
    }
    
}
