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

public class SignDetector {	
    private static bool bInit=false;

    private static List<UserDescriptionInfo> trainedSignMarkers;


    public static List<UserDescriptionInfo> ConnectivityandBehaviorRecognition(CvMat ROIimg, ref prototypeDef pProto, ref List<UserDescriptionInfo> squareSignmarker, ref List<UserDescriptionInfo> rectmarker, ref List<UserDescriptionInfo> BVmarker, ref List<UserDescriptionInfo> connectionList)
    {
        //sign detection


        //find square contours
        squareSignmarker.Clear();
        rectmarker.Clear();
       // BVmarker.Clear();
        List<UserDescriptionInfo> anyMarkerList = new List<UserDescriptionInfo>();        
        CVProc.findRectMarkers(ROIimg, ref anyMarkerList, ref squareSignmarker, ref rectmarker);
        int q = 0;
       //   debugSignMarkers(behaviorsignmarker);


        // SignDetection(ref squareSignmarker);

//        CVProc.textLabelDetection(ref rectmarker);


    //    BehaviorVariableDetection(ref pProto, ref rectmarker, ref BVmarker);
        ConnectionDetection(ref pProto, ref anyMarkerList, ref connectionList);
        Debug.Log(anyMarkerList.Count + "any markers found");
        Debug.Log(squareSignmarker.Count + "square markers found");
        Debug.Log(rectmarker.Count + "rect markers found");
        Debug.Log(BVmarker.Count + "BVs found");
        Debug.Log(connectionList.Count + "Connections found");
        //  SignDetector.SignDetectionKazeFeature(signImg, signImg);

        //should return sign sturucture array
        return squareSignmarker;
    }

    private static void BehaviorVariableDetection(ref prototypeDef pProto, ref List<UserDescriptionInfo> listMarkers, ref List<UserDescriptionInfo> listBVs)
    {
        if (pProto == null || pProto.mModels == null || pProto.mModels.Count == 0) return;

        //debug
      
        int qq = 100;
        foreach (KeyValuePair<ModelCategory, List<ModelDef>> kvp in pProto.mModels)
        {
            foreach (ModelDef model in kvp.Value)
            {                
                foreach (UserDescriptionInfo sign in listMarkers)
                {
                    //check if marker is in a model
                    
                    if (!CVProc.isSigninModel(sign, model)) continue;
                    // needs to change it to entire marker is in the model
                    sign.image.Threshold(sign.image, 100, 255, ThresholdType.Binary);
                    sign.InfoNumericalBVPercent = CVProc.measureHorizontalLevel(sign.image);
                    //this is a sign in a model
                    listBVs.Add(sign);
                  
                    //check if it's ...sth
                }
            }
        }
        BVDebug(ref listBVs);
    }
    private static void ConnectionDetection(ref prototypeDef pProto, ref List<UserDescriptionInfo> listMarkers, ref List<UserDescriptionInfo> listConns)
    {
        
        foreach (UserDescriptionInfo sign in listMarkers)
        {
            bool found = false;
            if (pProto.mModels == null) return;
            CvPoint VectorToConn=new CvPoint();
            CvPoint VectorToConn2=new CvPoint();
            foreach (KeyValuePair<ModelCategory, List<ModelDef>> kvp in pProto.mModels)
            {
                foreach (ModelDef model in kvp.Value)
                {
                    if (CVProc.isSignontheborderofModel(sign, model, 20,ref VectorToConn))
                    {
                        foreach (KeyValuePair<ModelCategory, List<ModelDef>> kvp2 in pProto.mModels)
                        {
                            foreach (ModelDef model2 in kvp2.Value)
                            {
                                if (model2.instanceID == model.instanceID) continue;
                                if (CVProc.isSignontheborderofModel(sign, model2, 20, ref VectorToConn2))
                                {
                                    if(model.instanceID < model2.instanceID)  sign.StrConnectivity = new KeyValuePair<int, int>(model.instanceID, model2.instanceID);
                                        else sign.StrConnectivity = new KeyValuePair<int, int>(model2.instanceID, model.instanceID);

                                    //update vector from CM to Connectivity here.
                                    listConns.Add(sign);
                                    model.ConnPoint = VectorToConn;
                                    model2.ConnPoint = VectorToConn2;
                                    Debug.Log("Connection Sign Found");
                                    found = true;
                                    break;
                                }
                            }
                            if (found) break;
                        }
                    }
                    if (found) break;

                }
                if (found) break;

            }
            
        }

        //erase duplicates

        for (int i = 0; i < listConns.Count; i++)
        {
            listConns[i].StrConnectivitySignGroup = new List<UserDescriptionInfo>();
            listConns[i].StrConnectivitySignGroup.Add(listConns[i]);
            for (int j=i+1; j < listConns.Count; j++)
            {
                if(listConns[i].StrConnectivity.Key== listConns[j].StrConnectivity.Key && listConns[i].StrConnectivity.Value == listConns[j].StrConnectivity.Value)
                {
                    listConns[i].StrConnectivitySignGroup.Add(listConns[j]);
                    listConns.RemoveAt(j);
                    j--;
                }
            }
        }
        ConnectionDebug(ref listConns);
        
    }
    public static void ConnectionDebug(ref List<UserDescriptionInfo> listConns)
    {
        CvRect rBox = GlobalRepo.GetRegionBox(true);
        CvMat tmp = new CvMat(rBox.Height, rBox.Width, MatrixType.U8C1);
        int heightStep = 70;
        int id = 0;
        foreach (UserDescriptionInfo t in listConns)
        {
//            t.image.Copy(tmp.GetRows(heightStep, heightStep + t.image.Rows).GetCols(20, 20 + t.image.Cols));

            tmp.PutText("Connection ID="+t.instanceID + " Connected Model IDs = "+t.StrConnectivity.Key+" , "+t.StrConnectivity.Value, new CvPoint(50, heightStep), new CvFont(FontFace.HersheyTriplex, 0.7f, 0.7f), CvColor.White);
            tmp.PutText(t.instanceID.ToString() , new CvPoint(t.center.X, t.center.Y), new CvFont(FontFace.HersheyTriplex, 0.7f, 0.7f), CvColor.White);


            heightStep += t.image.Rows + 70;

        }
        GlobalRepo.showDebugImage("Conn", tmp);
    }
    public static void BVDebug(ref List<UserDescriptionInfo> listBVs)
    {
        CvMat tmp = new CvMat(1000, 1000, MatrixType.U8C1);
        int heightStep = 100;
        int id = 0;
        foreach (UserDescriptionInfo t in listBVs)
        {
            t.image.Copy(tmp.GetRows(heightStep, heightStep + t.image.Rows).GetCols(20, 20 + t.image.Cols));

            tmp.PutText("ObjId=" +(id++)+ "\t level="+t.InfoNumericalBVPercent+"%", new CvPoint(400, heightStep), new CvFont(FontFace.HersheyTriplex, 1.0f, 1.0f), CvColor.White);


            heightStep += t.image.Rows + 80;
           
        }
        GlobalRepo.showDebugImage("BV" , tmp);


    }
    private static void SignDetection(ref List<UserDescriptionInfo> markerList)
    {
        if (!bInit) TrainTemplates();
        var bfMatcher20 = new BFMatcher(NormType.L2, false);
        float[] scores = new float[trainedSignMarkers.Count];
        for(int l = 0; l < markerList.Count; l++) { 
                    
            CvMat inputImageUC8 = new CvMat(markerList[l].image.Rows, markerList[l].image.Cols, MatrixType.U8C1);
            if (markerList[l].image.GetElemType() != MatrixType.U8C1) markerList[l].image.CvtColor(inputImageUC8, ColorConversion.BgraToGray);
            else markerList[l].image.Copy(inputImageUC8);
            inputImageUC8.Threshold(inputImageUC8, 200, 255, ThresholdType.BinaryInv);
            new CvWindow("signdebug4", inputImageUC8);
            OpenCv30Sharp.Mat inputMarkerMat = new OpenCv30Sharp.Mat(markerList[l].image.Rows, markerList[l].image.Cols, OpenCv30Sharp.MatType.CV_8UC1, inputImageUC8.Data);
            OpenCv30Sharp.KeyPoint[] keypointsInputMarker;
            var descInputMarker = new OpenCv30Sharp.Mat();
            keypointsInputMarker = OpenCv30Sharp.AKAZE.customDetect(inputMarkerMat, null);
            OpenCv30Sharp.AKAZE.customCompute(inputMarkerMat, ref keypointsInputMarker, descInputMarker);
            var descInput20Marker = CVProc.ConvertMat3toMat2UC8(descInputMarker);
            
            for (int i = 0; i < trainedSignMarkers.Count; i++)
            {
                DMatch[][] bfMatches = bfMatcher20.KnnMatch(trainedSignMarkers[i].desc20, descInput20Marker, 3, null, false);
                float score = 0;
                for (int k = 0; k < bfMatches.GetLength(0); k++)
                {
                    if (bfMatches[k][0].Distance < 0.75 * bfMatches[k][2].Distance)
                    {
                        //Cv.DrawCircle(DebugImage, new CvPoint((int)keypointsInput[bfMatches[i][0].TrainIdx].Pt.X, (int)keypointsInput[bfMatches[i][0].TrainIdx].Pt.Y), 4, CvColor.Blue);
                        score++;
                    }
                }
                scores[i] = score / ((float)bfMatches.GetLength(0));
                Debug.Log("Mmarker ID[" + markerList[l].instanceID + "] :: sub - score: " + score + " / " + bfMatches.GetLength(0));
            }
            int maxIndex = scores.ToList().IndexOf(scores.Max());
            markerList[l].InfoBehaviorTypeId = maxIndex + 1;
            if (markerList[l].InfoBehaviorTypeId != 4) markerList[l].InfoBehaviorTypeId = 1;
            /*SignMarkerInfo tt = markerList[l];
                        tt.typeId = maxIndex + 1;
                        markerList[l] = tt;*/


        }
    }
    public static void TrainTemplates()
    {
        bInit = true;
        trainedSignMarkers = new List<UserDescriptionInfo>();        
        int NumberofMarkers = 4;

        for (int i = 1; i <= NumberofMarkers; i++)
        {
            UserDescriptionInfo marker = new UserDescriptionInfo();
            OpenCv30Sharp.Mat templateImg = new OpenCv30Sharp.Mat("behaviorMarkers_id"+i+".png", OpenCv30Sharp.ImreadModes.GrayScale);
            templateImg.Threshold(100, 255, OpenCv30Sharp.ThresholdTypes.BinaryInv);
            OpenCv30Sharp.KeyPoint[] keypointsTemplate;
            var descTemplate = new OpenCv30Sharp.Mat();
            keypointsTemplate = OpenCv30Sharp.AKAZE.customDetect(templateImg, null);
            OpenCv30Sharp.AKAZE.customCompute(templateImg, ref keypointsTemplate, descTemplate);
            var descriptors1 = CVProc.ConvertMat3toMat2UC8(descTemplate);

            marker.keypoints = keypointsTemplate;
            marker.InfoBehaviorTypeId = 0;
            marker.desc30 = descTemplate;
            marker.desc20 = descriptors1;
            trainedSignMarkers.Add(marker);
        }

    }
    public static void SignDetectionKazeFeature(CvMat inputImage, CvMat DebugImage)
    {
        CvMat inputImageUC8 = new CvMat(inputImage.Rows, inputImage.Cols, MatrixType.U8C1);
            
        if (!bInit) TrainTemplates();

        if (inputImage.GetElemType() != MatrixType.U8C1) inputImage.CvtColor(inputImageUC8, ColorConversion.BgraToGray);

        OpenCv30Sharp.Mat inputImg = new OpenCv30Sharp.Mat(inputImage.Rows, inputImage.Cols, OpenCv30Sharp.MatType.CV_8UC1, inputImageUC8.Data);
        OpenCv30Sharp.KeyPoint[] keypointsInput;
        var descInput = new OpenCv30Sharp.Mat();

        keypointsInput = OpenCv30Sharp.AKAZE.customDetect(inputImg, null);
        OpenCv30Sharp.AKAZE.customCompute(inputImg, ref keypointsInput, descInput);
        var descInput2 = CVProc.ConvertMat3toMat2UC8(descInput);
        var bfMatcher20 = new BFMatcher(NormType.L2, false);
        for (int im = 0; im < trainedSignMarkers.Count; im++)
        {
            DMatch[][] bfMatches = bfMatcher20.KnnMatch(trainedSignMarkers[im].desc20, descInput2, 3, null, false);

            for (int i = 0; i < bfMatches.GetLength(0); i++)
            {
                if (bfMatches[i][0].Distance < 0.9f * bfMatches[i][2].Distance)
                {
                    Cv.DrawCircle(DebugImage, new CvPoint((int)keypointsInput[bfMatches[i][0].TrainIdx].Pt.X, (int)keypointsInput[bfMatches[i][0].TrainIdx].Pt.Y), 4, CvColor.Blue);
                }
            }
        }
    }
    public static void TemplateMatchingKazeFeatures()
    {
        OpenCv30Sharp.Mat templateImg = new OpenCv30Sharp.Mat("behaviorMarkers_id1.jpg",OpenCv30Sharp.ImreadModes.GrayScale);
        OpenCv30Sharp.Mat inputImg = new OpenCv30Sharp.Mat("behaviorMarkers_id1_mix.jpg", OpenCv30Sharp.ImreadModes.GrayScale);
           CvMat debugImg = new CvMat("behaviorMarkers_id1_mix.jpg");

        OpenCv30Sharp.KeyPoint[] keypointsTemplate;
        OpenCv30Sharp.KeyPoint[] keypointsInput;
        var descInput = new OpenCv30Sharp.Mat();
        var descTemplate = new OpenCv30Sharp.Mat();
        
        keypointsInput = OpenCv30Sharp.AKAZE.customDetect(inputImg, null);
        keypointsTemplate = OpenCv30Sharp.AKAZE.customDetect(templateImg, null);
        
        OpenCv30Sharp.AKAZE.customCompute(templateImg, ref keypointsTemplate, descTemplate);
        OpenCv30Sharp.AKAZE.customCompute(inputImg, ref keypointsInput, descInput);

        var bfMatcher20 = new BFMatcher(NormType.L2, false);
        var descriptors1 = CVProc.ConvertMat3toMat2UC8(descTemplate);
        var descriptors2 = CVProc.ConvertMat3toMat2UC8(descInput);
        DMatch[][] bfMatches = bfMatcher20.KnnMatch(descriptors1, descriptors2, 3, null, false);

        for (int i = 0; i < bfMatches.GetLength(0); i++)
        {
            if (bfMatches[i][0].Distance < 0.9f * bfMatches[i][2].Distance)
            {
                Cv.DrawCircle(debugImg, new CvPoint((int)keypointsInput[bfMatches[i][0].TrainIdx].Pt.X, (int)keypointsInput[bfMatches[i][0].TrainIdx].Pt.Y), 4, CvColor.Blue);
            }
        }
        new CvWindow("debug", debugImg);

     

    }
    
    public static void TestTemplateMatching()
    {
       TemplateMatchingKazeFeatures();
        return;
        /*
        CvMat templateImg = new CvMat("behaviorMarkers_id1.jpg");
        CvMat inputImg = new CvMat("behaviorMarkers_id1_mix.jpg");
        Mat templateImg2 = new Mat("behaviorMarkers_id1.jpg");
        Mat inputImg2 = new Mat("behaviorMarkers_id1_mix.jpg");



        
        
        var fast = new FastFeatureDetector(10);
        var descriptorExtractor = new BriefDescriptorExtractor(32);

        KeyPoint[] keypoints1, keypoints2;
        var descriptors1 = new Mat();
        var descriptors2 = new Mat();

        keypoints1 = fast.Run(templateImg2, null);
        descriptorExtractor.Compute(templateImg2, ref keypoints1, descriptors1);

        keypoints2 = fast.Run(inputImg2, null);
        descriptorExtractor.Compute(inputImg2, ref keypoints2, descriptors2);

        // Match descriptor vectors
        var bfMatcher = new BFMatcher(NormType.L2, false);

        DMatch[][] bfMatches = bfMatcher.KnnMatch(descriptors2, descriptors1, 3, null, false);

        for (int i = 0; i < bfMatches.GetLength(0); i++)
        {
            if (bfMatches[i][0].Distance < 0.7f * bfMatches[i][2].Distance)
            {
                Cv.DrawCircle(inputImg, new CvPoint((int)keypoints2[bfMatches[i][0].QueryIdx].Pt.X, (int)keypoints2[bfMatches[i][0].QueryIdx].Pt.Y), 4, CvColor.Blue);
            }
        }
        new CvWindow("debug", inputImg);

        /*
        Cv2.InitModule_NonFree();

        //   SIFT siftInstance = new SIFT();

        CvSURFPoint[] surfPoints1, surfPoints2;

        KeyPoint[] keypoints1,keypoints2;
        float[][] descriptors1, descriptors2;
        DMatch[][] dmatches;

        CvSURFParams surfParam = new CvSURFParams(400,true);
        Cv.ExtractSURF(templateImg, null, out surfPoints1, out descriptors1, surfParam);
        Cv.ExtractSURF(templateImg, null, out surfPoints2, out descriptors2, surfParam);

        // keypoints1=siftInstance.Detect(templateImg2);
        //keypoints2 =siftInstance.Detect(inputImg2);
        Mat desc1 = new Mat();            
        Mat desc2 = new Mat();

        //siftInstance.Compute(templateImg2, ref keypoints1, desc1);
        //siftInstance.Compute(inputImg2, ref keypoints2, desc2);

        BFMatcher bfm = new BFMatcher();
        dmatches = bfm.KnnMatch(desc2, desc1, 2);*/

        

    }
}/*
public interface SignMarkerInfoModifier
{
    int typeId { set; get; }
    string labelString { set; get; }
}
public struct SignMarkerInfo : SignMarkerInfoModifier
{
    public CvMat image;
   public int typeId { get; set; }
    public int instanceID;
    public OpenCv30Sharp.KeyPoint[] keypoints;
    public OpenCv30Sharp.Mat desc30;
    public Mat desc20;
    public CvPoint center;
    public string labelstring { set; get; }
    
}*/


public class UserDescriptionInfo
{
    public CvMat image;

    //behavior
    public int InfoBehaviorTypeId;
    public string InfoTextLabelstring;
    public double InfoTextLabelConfidence;
    public int BehaviorDistance;
    public BehaviorCategory InfoBehaviorCategory;
    public float InfoNumericalBVPercent;
    public float InfoNumericalBVValue;
    public string InfoCategoricalBVValue;
    public KeyValuePair<int, int> StrConnectivity;
    public List<UserDescriptionInfo> StrConnectivitySignGroup;
    public int instanceID;
    public OpenCv30Sharp.KeyPoint[] keypoints;
    public OpenCv30Sharp.Mat desc30;
    public Mat desc20;
    public CvPoint center;
    public CvPoint[] contourPoints;
    public CvPoint[] contourPointsLocal;
    public CvRect boundingBox;
    public CvMat BVImage;
    public UserDescriptionInfo()    
    {
        
    }
    

};
