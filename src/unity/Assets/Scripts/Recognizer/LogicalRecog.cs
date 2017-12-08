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

public class LogicalRecog : MonoBehaviour {

    private List<UserDescriptionInfo> LogicalConnectionSignList=null;
    // Use this for initialization
    void Start () {
        LogicalConnectionSignList = new List<UserDescriptionInfo>();

    }
	
	// Update is called once per frame
	void Update () {
      
        if (GlobalRepo.getLearningCount() <= 0) return;
        if (GlobalRepo.getLearningCount() ==2) Detect2DGestureBlack();

    }

    void Detect2DGestureBlack()
    {
        Debug.Log("Sdf");
        CvMat grayRegionImage = GlobalRepo.GetRepo(RepoDataType.dRawRegionGray);
        if (grayRegionImage == null)
        {
            Debug.Log("[ERROR] Fail to retrieve Gray Region Image");
            return;
        }
        CvMat inputImageUC8 = grayRegionImage;

        if (inputImageUC8 == null) return;
        if (inputImageUC8.ElemType != MatrixType.U8C1)
        {
            inputImageUC8 = new CvMat(grayRegionImage.Rows, grayRegionImage.Cols, MatrixType.U8C1);
            grayRegionImage.CvtColor(inputImageUC8, ColorConversion.BgraToGray);

        }
        else
        {
            inputImageUC8 = grayRegionImage.Clone();
        }
        inputImageUC8.Threshold(inputImageUC8, 160, 255, ThresholdType.BinaryInv);
        
        SplitSigns(inputImageUC8);
        ThninningFindSkeleton();
        ContourAnalysisFindSkeleton(inputImageUC8);

    }
    private void ContourAnalysisFindSkeleton(CvMat BWImg)
    {
        CvMat GraphImg = new CvMat(400, 400, MatrixType.U8C3);
        //for (int i = 0; i < LogicalConnectionSignList.Count; i++)
        List<int> zerocrossingIdx = new List<int>();
        CvMat FullDebugImg = new CvMat(BWImg.Rows, BWImg.Cols, MatrixType.U8C3);
        BWImg.CvtColor(FullDebugImg, ColorConversion.GrayToBgr);
        for (int i = 0; i < LogicalConnectionSignList.Count; i++)
        {
            //create an array of distances to the centeroid
            if (LogicalConnectionSignList[i].contourPoints.Length < 5) continue;
            zerocrossingIdx.Clear();
            CvPoint centeroid = LogicalConnectionSignList[i].center;
            double[] distanceArray = new double[LogicalConnectionSignList[i].contourPoints.Length];
            double[] differenceArray = new double[LogicalConnectionSignList[i].contourPoints.Length];
            double[] differenceArrayPadding = new double[LogicalConnectionSignList[i].contourPoints.Length+4];
            for (int k = 0; k < distanceArray.Length; k++)
            {
                distanceArray[k] = LogicalConnectionSignList[i].contourPoints[k].DistanceTo(centeroid);
            }

            // low pass filter 0.015
            CVProc.LowPassFilter(distanceArray, 1, 0.015);
            //need to debug the distribution
           
            Debug.Log("======ContourLength: " + i+"   "+ distanceArray.Length);
            int paddingSize = 2;
            for (int k = 0; k < distanceArray.Length; k++)
            {
                //    Debug.Log(distanceArray[k]);
                if (k == 0) differenceArrayPadding[k + 2] = distanceArray[k] - distanceArray[distanceArray.Length - 1];
                    else differenceArrayPadding[k+2] = distanceArray[k] - distanceArray[k - 1];
                
            }
            differenceArrayPadding[0] = differenceArrayPadding[distanceArray.Length];
            differenceArrayPadding[1] = differenceArrayPadding[distanceArray.Length+1];
            differenceArrayPadding[distanceArray.Length +  2] = differenceArrayPadding[2];
            differenceArrayPadding[distanceArray.Length +  3] = differenceArrayPadding[3];


            double sign;
            string gradient = "";
            for (int k = 2; k < distanceArray.Length+2; k++)
            {
                gradient += differenceArrayPadding[k] + " ";
                if (differenceArrayPadding[k] == 0 && differenceArrayPadding[k + 1] * differenceArrayPadding[k - 1] < 0 && differenceArrayPadding[k + 1] < 0)
                {
                    zerocrossingIdx.Add(k - 2);
                } else if (differenceArrayPadding[k] < 0 && differenceArrayPadding[k-1] > 0)
                {                    
                    zerocrossingIdx.Add(k - 2);
                }
                if(zerocrossingIdx.Count>2)
                {
                    double minDist = double.MaxValue;
                    int minIdx = -1;
                    for(int l = 0; l < zerocrossingIdx.Count; l++)
                    {
                        if(distanceArray[l]<minDist)
                        {
                            minIdx = l;
                            minDist = distanceArray[l];
                        }
                    }
                    if(minIdx>=0)
                    {
                        zerocrossingIdx.RemoveAt(minIdx);
                    }
                }
                
            }
            Debug.Log("grad: "+gradient);

            //debug image;
            GraphImg.SetZero();
            CvMat debugSign = new CvMat(LogicalConnectionSignList[i].image.Rows, LogicalConnectionSignList[i].image.Cols, MatrixType.U8C3);
            LogicalConnectionSignList[i].image.CvtColor(debugSign, ColorConversion.GrayToBgr);
            string distance = "";
            for (int k = 0; k < distanceArray.Length; k++)
            {
                //    Debug.Log(distanceArray[k]);                                
                GraphImg.DrawLine(new CvPoint(k * 2, 350), new CvPoint(k * 2, 350 - (int)distanceArray[k]), CvColor.White, 2);
                distance += (int)distanceArray[k] + " ";
              //  debugSign.DrawCircle(LogicalConnectionSignList[i].contourPointsLocal[k], 2, CvColor.Blue, 1);
            }
            Debug.Log("dist: " + distance);
            
            
            foreach (var ii in zerocrossingIdx)
            {
                GraphImg.DrawLine(new CvPoint(ii * 2, 350), new CvPoint(ii * 2, 350 - (int)distanceArray[ii]), CvColor.Red, 4);
                Debug.Log("zero crossing idx: " + ii);
                debugSign.DrawCircle(LogicalConnectionSignList[i].contourPointsLocal[ii], 6, CvColor.Red, 3);
                GraphImg.SaveImage("log/testLogicalRec/"+i+" Histogram.jpg");
                debugSign.SaveImage("log/testLogicalRec/"+i+" EndPoints.jpg");
                //GlobalRepo.showDebugImage("EndPoint#" + i, debugSign);
                FullDebugImg.DrawCircle(LogicalConnectionSignList[i].contourPoints[ii], 9, CvColor.Red, 5);
            }
           
            // check zero crossing

            // list all the candidates

            // sort by peak distance

            // see
        }
        FullDebugImg.SaveImage("log/testLogicalRec/AllEdges.jpg");
        GlobalRepo.showDebugImage("Edges", FullDebugImg);
    }
    private void ThninningFindSkeleton()
    {
        
        IplConvKernel element = Cv.CreateStructuringElementEx(3, 3, 1, 1, ElementShape.Cross, null);

        
        for (int i = 0; i < LogicalConnectionSignList.Count; i++)
        {
            int  done = 15;
            CvMat signImg = LogicalConnectionSignList[i].image.Clone();
            CvMat eroded = new CvMat(signImg.Rows, signImg.Cols, signImg.GetElemType());
            CvMat temp = new CvMat(signImg.Rows, signImg.Cols, signImg.GetElemType());
            CvMat skel = new CvMat(signImg.Rows, signImg.Cols, signImg.GetElemType());
            
            signImg.Dilate(signImg);
            skel.SetZero();
            while (done>0)
            {
                Cv.Erode(signImg, eroded, element);
                Cv.Dilate(eroded, temp, element);
                signImg.Sub(temp, temp);
                skel.Or(temp, skel);
                signImg = eroded.Clone();
                if (Cv.CountNonZero(signImg) == 0) done = 0;
                done--;
            }
            CvMat skelImg = new CvMat(signImg.Rows, signImg.Cols, MatrixType.U8C3);
            CvMat blueImg = new CvMat(signImg.Rows, signImg.Cols, MatrixType.U8C3);
            skelImg.SetZero();
            blueImg.Set(CvColor.Blue);
            LogicalConnectionSignList[i].image.CvtColor(skelImg, ColorConversion.GrayToBgr);
            blueImg.Copy(skelImg, skel);
            skelImg.SaveImage("log/testLogicalRec/" + i + " Skeletonization.jpg");
            //GlobalRepo.showDebugImage("LogicalRecog_Skel#" + i, skel);

        }
    }
    private void SplitSigns(CvMat BWImg)
    {
        CvSeq<CvPoint> contoursRaw;
        LogicalConnectionSignList.Clear();
        int id = 0;
        GlobalRepo.showDebugImage("logicalrecog1", BWImg);

        BWImg.SaveImage("log/testLogicalRec/Signs.jpg");
        CvMat rawImg = BWImg.Clone();
        using (CvMemStorage storage = new CvMemStorage())
        {
            //find contours
            Cv.FindContours(BWImg, storage, out contoursRaw, CvContour.SizeOf, ContourRetrieval.External, ContourChain.ApproxSimple);
            //Taken straight from one of the OpenCvSharp samples
            using (CvContourScanner scanner = new CvContourScanner(BWImg, storage, CvContour.SizeOf, ContourRetrieval.External, ContourChain.ApproxSimple))
            {
                foreach (CvSeq<CvPoint> c in scanner)
                {
                    //Some contours are negative so make them all positive for easy comparison
                    double area = Math.Abs(c.ContourArea());
                
                    List<CvPoint[]> points = new List<CvPoint[]>();                  
                     CvSeq<CvPoint> cApprox = c.ApproxPoly(CvContour.SizeOf, storage, ApproxPolyMethod.DP, c.ArcLength() * 0.002f, true);
                    cApprox = c;

                    CvPoint centeroid;
                    CvMoments m = cApprox.ContoursMoments();
                    centeroid.X = (int)(m.M10 / m.M00);
                    centeroid.Y = (int)(m.M01 / m.M00);
                    UserDescriptionInfo marker = new UserDescriptionInfo();
                    CvRect roi = Cv.BoundingRect(cApprox);
                //    roi.Inflate(4, 4);
                    CvMat contourRegionBW = new CvMat(roi.Height, roi.Width, MatrixType.U8C1);
                    
                    contourRegionBW.Set(CvColor.Black);                    
                    contourRegionBW = rawImg.GetSubRect(out contourRegionBW, roi).Clone();
                    marker.contourPoints = cApprox.ToArray();
                    marker.contourPointsLocal = (CvPoint[]) marker.contourPoints.Clone();
                    marker.boundingBox = roi;
                    marker.center = centeroid;
                    marker.image = contourRegionBW;
                    marker.instanceID = id++;
                    marker.InfoBehaviorTypeId = 999;

                    for(int k = 0; k < marker.contourPointsLocal.Length; k++)
                    {
                        marker.contourPointsLocal[k] = marker.contourPointsLocal[k] - roi.TopLeft;
                    }

                    LogicalConnectionSignList.Add(marker);
                }
            }
        }

      /*  for (int i = 0; i < LogicalConnectionSignList.Count; i++)
            GlobalRepo.showDebugImage("LogicalRecog##" + i, LogicalConnectionSignList[i].image);*/

    }
                      

    //everytick: accumulate gray image (can do this  in GlobalRepo...but in the future implementation)
    //after learning: processing detection
    //accumulate gray i
}
