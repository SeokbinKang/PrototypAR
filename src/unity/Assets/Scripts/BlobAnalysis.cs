using UnityEngine;
using System.Collections;

using OpenCvSharp;
using OpenCvSharp.Blob;
using OpenCvSharp.CPlusPlus;
using System.Collections.Generic;

using System.IO;
using System.Text;
public class BlobAnalysis {

    private static bool config_byspass_buildgshape = true;
    public static Point[] GetOverlapShapeOutline(ObjectBlobInfo virtualObjInfo, ObjectBlobInfo userObjInfo, double transX, double transY)
    {
                
        Point[] ret = (Point[])virtualObjInfo.scaledContour.Clone();
        Point CMTransitionVector = userObjInfo.center - virtualObjInfo.scaledCenter;
        CMTransitionVector.X = CMTransitionVector.X + (int)transX;
        CMTransitionVector.Y = CMTransitionVector.Y + (int)transY;
        for(int i=0;i<ret.Length;i++)
        {
            ret[i] = ret[i] + CMTransitionVector;
            if (ret[i].X < 0) ret[i].X = 0;
            else if (ret[i].X >= userObjInfo.width) ret[i].X = userObjInfo.width - 1;
            if (ret[i].Y < 0) ret[i].Y = 0;
            else if (ret[i].Y >= userObjInfo.height) ret[i].Y = userObjInfo.height - 1;

        }       
        
        return ret;
    }
    public static Point[] GetOverlapShapeOutline(ObjectBlobInfo virtualObjInfo, ObjectBlobInfo userObjInfo, double transX, double transY,double rotate)
    {
       // Point[] smoothedContour = Cv2.ApproxPolyDP(virtualObjInfo.scaledContour, Cv2.ArcLength(virtualObjInfo.scaledContour, true) * 0.002f, true);


        //Debug.Log("Contour smoothed!! " + virtualObjInfo.scaledContour.Length + "  -->  " + smoothedContour.Length);
        Mat inMat = new Mat(1, virtualObjInfo.scaledContour.Length, MatType.CV_32SC2);
        Mat outMat = new Mat(1, virtualObjInfo.scaledContour.Length, MatType.CV_32SC2);
        inMat.SetArray(0, 0, virtualObjInfo.scaledContour);
        Point[] ret = (Point[])virtualObjInfo.scaledContour.Clone();

    /*    Mat inMat = new Mat(1, smoothedContour.Length, MatType.CV_32SC2);
        Mat outMat = new Mat(1, smoothedContour.Length, MatType.CV_32SC2);
        inMat.SetArray(0, 0, smoothedContour);
        Point[] ret = (Point[])smoothedContour.Clone();*/

        Mat rotMat = Cv2.GetRotationMatrix2D(new Point2f(virtualObjInfo.scaledCenter.X, virtualObjInfo.scaledCenter.Y), rotate, 1);
        Cv2.Transform(inMat, outMat, rotMat);
        outMat.GetArray(0, 0, ret);



        Point CMTransitionVector = userObjInfo.center - virtualObjInfo.scaledCenter;
        CMTransitionVector.X = CMTransitionVector.X + (int)transX;
        CMTransitionVector.Y = CMTransitionVector.Y + (int)transY;
        for (int i = 0; i < ret.Length; i++)
        {
            ret[i] = ret[i] + CMTransitionVector;
            if (ret[i].X < 0) ret[i].X = 0;
            else if (ret[i].X >= userObjInfo.width) ret[i].X = userObjInfo.width - 1;
            if (ret[i].Y < 0) ret[i].Y = 0;
            else if (ret[i].Y >= userObjInfo.height) ret[i].Y = userObjInfo.height - 1;

        }

        return ret;
    }
    public static double GetInclusionRatio(ObjectBlobInfo virtualObjInfo, ObjectBlobInfo userObjInfo, double transX, double transY, ref double[] distanceResult)
    {

        double param_inclusion_margin = 2;
        Point[] tmpContour = (Point[])virtualObjInfo.scaledContour.Clone();
        Mat inMat = new Mat(1, virtualObjInfo.scaledContour.Length, MatType.CV_32SC2);
        Mat outMat = new Mat(1, virtualObjInfo.scaledContour.Length, MatType.CV_32SC2);
        /* Debug.Log("virtualobj"+virtualObjInfo.scaledContour.Length);
         Debug.Log("virtualobj" + inMat.Size());
         Debug.Log("userobj" + userObjInfo.contour.Length);*/

        inMat.SetArray(0, 0, virtualObjInfo.scaledContour);
        double maxRet = 0;
        double maxRot = 0;
        double[] distanceResult_temp = new double[tmpContour.Length];
        distanceResult = null;
       
            double ret = 0;
           
          //  Cv2.Transform(inMat, outMat, rotMat);
            inMat.GetArray(0, 0, tmpContour);

            Point CMTransitionVector = userObjInfo.center - virtualObjInfo.scaledCenter;
            CMTransitionVector.X = CMTransitionVector.X + (int)transX;
            CMTransitionVector.Y = CMTransitionVector.Y + (int)transY;

            for (int i = 0; i < tmpContour.Length; i++)
            {
                Point trans_p = CMTransitionVector + tmpContour[i];
                double dist;
                if (trans_p.X < 0 || trans_p.X >= userObjInfo.width || trans_p.Y < 0 || trans_p.Y >= userObjInfo.height) continue;
                distanceResult_temp[i] = Cv2.PointPolygonTest(userObjInfo.contour, trans_p, true);
                if (distanceResult_temp[i] >= 0) ret++;
            }
            if (ret > maxRet)
            {
                maxRet = ret;                
                distanceResult = distanceResult_temp;
                distanceResult_temp = new double[tmpContour.Length];

            }
       
        
        return maxRet / ((double)virtualObjInfo.scaledContour.Length);

    }
    public static double GetInclusionRatio(ObjectBlobInfo virtualObjInfo, ObjectBlobInfo userObjInfo, double transX, double transY, out double rotate, ref double[] distanceResult)
    {
        
        double param_inclusion_margin = 2;
        Point[] tmpContour = (Point[])virtualObjInfo.scaledContour.Clone();
        Mat inMat = new Mat(1,virtualObjInfo.scaledContour.Length, MatType.CV_32SC2);
        Mat outMat = new Mat(1, virtualObjInfo.scaledContour.Length, MatType.CV_32SC2);
       /* Debug.Log("virtualobj"+virtualObjInfo.scaledContour.Length);
        Debug.Log("virtualobj" + inMat.Size());
        Debug.Log("userobj" + userObjInfo.contour.Length);*/
    
        inMat.SetArray(0, 0, virtualObjInfo.scaledContour);
        double maxRet=0;
        double maxRot = 0;
        double[] distanceResult_temp = new double[tmpContour.Length];
        distanceResult = null;
        for (double rotDegree = -24; rotDegree <= 24; rotDegree += 6)
        {
            double ret = 0;
            Mat rotMat = Cv2.GetRotationMatrix2D(new Point2f(virtualObjInfo.scaledCenter.X, virtualObjInfo.scaledCenter.Y), rotDegree, 1);
            Cv2.Transform(inMat, outMat, rotMat);
            outMat.GetArray(0, 0, tmpContour);

            Point CMTransitionVector = userObjInfo.center - virtualObjInfo.scaledCenter;
            CMTransitionVector.X = CMTransitionVector.X + (int)transX;
            CMTransitionVector.Y = CMTransitionVector.Y + (int)transY;

            for(int i=0;i<tmpContour.Length;i++)            
            {
                Point trans_p = CMTransitionVector + tmpContour[i];
                double dist;
                if (trans_p.X < 0 || trans_p.X >= userObjInfo.width || trans_p.Y < 0 || trans_p.Y >= userObjInfo.height) continue;
                distanceResult_temp[i] = Cv2.PointPolygonTest(userObjInfo.contour, trans_p, true);
                if (distanceResult_temp[i] >= 0) ret++;
            }
            if(ret>maxRet)
            {
                maxRet = ret;
                maxRot = rotDegree;                
                distanceResult = distanceResult_temp;
                distanceResult_temp = new double[tmpContour.Length];

            }
        }
        rotate = maxRot;
        return maxRet / ((double)virtualObjInfo.scaledContour.Length);

    }
    public static double GetInclusionRatio(ObjectBlobInfo virtualObjInfo, ObjectBlobInfo userObjInfo, double transX, double transY)
    {
        double ret = 0;
        double param_inclusion_margin = 5;
        Point[] transVirtualObjectContour = (Point[])virtualObjInfo.scaledContour.Clone();
        Point CMTransitionVector = userObjInfo.center - virtualObjInfo.scaledCenter;
        CMTransitionVector.X = CMTransitionVector.X + (int)transX;
        CMTransitionVector.Y = CMTransitionVector.Y + (int) transY;
        
        foreach (Point p in virtualObjInfo.scaledContour)
        {
            Point trans_p = CMTransitionVector + p;
            double dist;
            if (trans_p.X < 0 || trans_p.X >= userObjInfo.width || trans_p.Y<0 || trans_p.Y >= userObjInfo.height ) continue;
            dist = Cv2.PointPolygonTest(userObjInfo.contour, trans_p, true);
            if (dist > (param_inclusion_margin * -1)) ret++;
        }
        ret = ret / ((double)virtualObjInfo.contour.Length);
        return ret;

    }
    public static Point[] ExtractContour(CvMat InputRGBAImg, ContourChain approx)
    {// this purely extract contour points without any transformation, preserving the coordinate
        Point[] ret=null;
        if (InputRGBAImg == null) return null;
        CvMat GrayImage = new CvMat(InputRGBAImg.Rows, InputRGBAImg.Cols, MatrixType.U8C1);
        if (InputRGBAImg.ElemChannels == 4) InputRGBAImg.CvtColor(GrayImage, ColorConversion.RgbaToGray);
        if (InputRGBAImg.ElemChannels == 3) InputRGBAImg.CvtColor(GrayImage, ColorConversion.RgbToGray);
        OpenCvSharp.CPlusPlus.Mat tInput = new Mat(GrayImage);
        Point[][] contours; //vector<vector<Point>> contours;
        HierarchyIndex[] hierarchyIndexes; //vector<Vec4i> hierarchy;        
        Cv2.FindContours(
                       tInput,
                       out contours,
                       out hierarchyIndexes,
                       ContourRetrieval.External, approx);
      

        if (contours.Length == 0)
        {
            Debug.Log("ErrorEventArgs: Fail to extract Blob info");
            GlobalRepo.showDebugImage("ERRRORR", InputRGBAImg);
            InputRGBAImg.SaveImage("error.jpg");
            return null;

        }
        var contourIndex = 0;
        double maxContourArea = 0;
        while ((contourIndex >= 0))
        {
            if(approx==ContourChain.ApproxNone)
            {
                //Debug.Log("max contour Area:" + maxContourArea + "\t contouridx:" + contourIndex+" // "+contours.Length+"\t size:" + Cv2.ContourArea(contours[contourIndex]));
            }
            if (Cv2.ContourArea(contours[contourIndex]) < 50 || maxContourArea > Cv2.ContourArea(contours[contourIndex]))
            {
                contourIndex = hierarchyIndexes[contourIndex].Next;
                continue;
            }
            maxContourArea = Cv2.ContourArea(contours[contourIndex]);
            OpenCvSharp.CPlusPlus.Rect rect20 = Cv2.BoundingRect(contours[contourIndex]);

            //ret = new CvMat(rect20.Height, rect20.Width, InputRGBAImg.GetElemType());

            //ret.contour = (Point[])contours[contourIndex].Clone();
            /*  if(approx==ContourChain.ApproxNone)    ret =  reduceContourPoints(contours[contourIndex], 3);
              else ret = reduceContourPoints(contours[contourIndex], 10);*/
            //ret = Cv2.ApproxPolyDP(contours[contourIndex], 2.0f, true);
            ret = contours[contourIndex].Clone() as Point[];
            contourIndex = hierarchyIndexes[contourIndex].Next;
          
        }

        return ret;

    }
    public static ObjectBlobInfo ExtractBlobInfo(CvMat InputRGBAImg, bool convexUsrObj)
    { //assume both input images are cropped to the boundary
        ObjectBlobInfo ret = new ObjectBlobInfo();
        if (InputRGBAImg == null) return null;
        CvMat GrayImage = new CvMat(InputRGBAImg.Rows, InputRGBAImg.Cols, MatrixType.U8C1);
        if(InputRGBAImg.ElemChannels==4)      InputRGBAImg.CvtColor(GrayImage, ColorConversion.RgbaToGray);
        if(InputRGBAImg.ElemChannels == 3) InputRGBAImg.CvtColor(GrayImage, ColorConversion.RgbToGray);
        
        OpenCvSharp.CPlusPlus.Mat tInput = new Mat(GrayImage);
        Point[][] contours; //vector<vector<Point>> contours;
        HierarchyIndex[] hierarchyIndexes; //vector<Vec4i> hierarchy;        
        Cv2.FindContours(
                       tInput,
                       out contours,
                       out hierarchyIndexes,
                       ContourRetrieval.External, ContourChain.ApproxSimple);

        if (contours.Length == 0)
        {
            Debug.Log("ErrorEventArgs: Fail to extract Blob info");
            GlobalRepo.showDebugImage("ERRRORR", InputRGBAImg);
            InputRGBAImg.SaveImage("error.jpg");
            return null;

        }
        var contourIndex = 0;
        double maxContourArea = 0;
        while ((contourIndex >= 0))
        {
            if (Cv2.ContourArea(contours[contourIndex]) < 50 || maxContourArea > Cv2.ContourArea(contours[contourIndex]))
            {
                contourIndex = hierarchyIndexes[contourIndex].Next;
                continue;
            }
         
            maxContourArea = Cv2.ContourArea(contours[contourIndex]);

            OpenCvSharp.CPlusPlus.Rect rect20 = Cv2.BoundingRect(contours[contourIndex]);

            //ret = new CvMat(rect20.Height, rect20.Width, InputRGBAImg.GetElemType());
            CvPoint c;
            Moments m = Cv2.Moments(contours[contourIndex]);
            c.X = (int)(m.M10 / m.M00);
            c.Y = (int)(m.M01 / m.M00);
            ret.center = c;            
            ret.contour = reduceContourPoints(contours[contourIndex],7);
            ret.areaSize = Cv2.ContourArea(contours[contourIndex]);
            contourIndex = hierarchyIndexes[contourIndex].Next;
           
            break;

        }
        ret.width = InputRGBAImg.Width;
        ret.height = InputRGBAImg.Height;
        if(convexUsrObj)
        {
            Point[] ReducedContour_convex;
            ReducedContour_convex = Cv2.ConvexHull(ret.contour);
            ret.contour = ReducedContour_convex;
            Moments m = Cv2.Moments(ret.contour);
            ret.center.X = (int)(m.M10 / m.M00);
            ret.center.Y = (int)(m.M01 / m.M00);
            ret.areaSize = Cv2.ContourArea(ret.contour);
        }
        

        return ret;

    }
    public static Point[] reduceContourPoints(Point[] contour, double paramApproximationDistance)
    {
        //Point[] reducedContour = new Point[contour.Length];
        
        ArrayList reducedContour = new ArrayList();
        Point lastPoint = new Point(-500,500);
        double dist=0;
        foreach (Point p in contour)
        {
             dist = lastPoint.DistanceTo(p);
            if (dist < paramApproximationDistance) continue;
            lastPoint = p;
            reducedContour.Add(p);
        }
        if (reducedContour.Count % 2 == 1) reducedContour.RemoveAt(0); //has to be even number?
        Point[] ret = reducedContour.ToArray(typeof(Point)) as Point[];
        //Debug.Log("[DEBUG] contour reduced from " + contour.Length + " to " + ret.Length);
        return ret;
        

    }
    public static CvMat ExtractBoundedBlobImage(CvMat InputRGBAImg,ref CvRect boundingBox,ref CvPoint centeroid)
    {
       
        CvMat ret = null;

        if (InputRGBAImg == null) return null;
        CvMat GrayImage = new CvMat(InputRGBAImg.Rows, InputRGBAImg.Cols, MatrixType.U8C1);
        InputRGBAImg.CvtColor(GrayImage, ColorConversion.RgbaToGray);
        OpenCvSharp.CPlusPlus.Mat tInput = new Mat(GrayImage);
        Point[][] contours; //vector<vector<Point>> contours;
        HierarchyIndex[] hierarchyIndexes; //vector<Vec4i> hierarchy;        
        Cv2.FindContours(
                       tInput,
                       out contours,
                       out hierarchyIndexes,
                       ContourRetrieval.CComp, ContourChain.ApproxSimple);

        if (contours.Length == 0)
        {
            Debug.Log("[ERROR] No Blob For Object ");
            return null;

        }
        var contourIndex = 0;
        double maxContourArea = 0;
        while ((contourIndex >= 0))
        {

            OpenCvSharp.CPlusPlus.Rect rect20= Cv2.BoundingRect(contours[contourIndex]);
            if (Cv2.ContourArea(contours[contourIndex]) < 100 || maxContourArea > Cv2.ContourArea(contours[contourIndex]))
            {
                contourIndex = hierarchyIndexes[contourIndex].Next;
                continue;
            }       
            maxContourArea = Cv2.ContourArea(contours[contourIndex]);
            //ret = new CvMat(rect20.Height, rect20.Width, InputRGBAImg.GetElemType());
            boundingBox = new CvRect(rect20.X, rect20.Y, rect20.Width, rect20.Height);
            InputRGBAImg.GetSubArr(out ret, boundingBox);
            ret = ret.Clone();
            CvPoint c;
            Moments m = Cv2.Moments(contours[contourIndex]);
            c.X = (int)(m.M10 / m.M00);
            c.Y = (int)(m.M01 / m.M00);
            centeroid = c;


            contourIndex = hierarchyIndexes[contourIndex].Next;
            

        }

        return ret;
    }
    public static CvRect ExtractLargestBoundingBox(CvMat inputMat)
    {
        CvMat GrayImage = inputMat;

        if (inputMat.ElemChannels == 4)
        {
            GrayImage = new CvMat(inputMat.Rows, inputMat.Cols, MatrixType.U8C1);
            inputMat.CvtColor(GrayImage, ColorConversion.RgbaToGray);

        }
        else if (inputMat.ElemChannels == 3)
        {
            GrayImage = new CvMat(inputMat.Rows, inputMat.Cols, MatrixType.U8C1);
            inputMat.CvtColor(GrayImage, ColorConversion.RgbToGray);
        } else
        {
            GrayImage = inputMat.Clone();
        }
        CvSeq<CvPoint> contoursRaw;
        double maxArea = 0;
        CvRect maxRect = new CvRect();
        using (CvMemStorage storage = new CvMemStorage())
        {
            //find contours
            Cv.FindContours(GrayImage, storage, out contoursRaw, CvContour.SizeOf, ContourRetrieval.External, ContourChain.ApproxSimple);
            //Taken straight from one of the OpenCvSharp samples
            using (CvContourScanner scanner = new CvContourScanner(GrayImage, storage, CvContour.SizeOf, ContourRetrieval.External, ContourChain.ApproxSimple))
            {
                foreach (CvSeq<CvPoint> c in scanner)
                {
                    //Some contours are negative so make them all positive for easy comparison
                    double area = System.Math.Abs(c.ContourArea());
                    //Uncomment below to see the area of each contour
                    //Console.WriteLine(area.ToString());
        //            Debug.Log("ARAEREAR: " + area);
                    if (area > maxArea)
                    {
                        maxRect = Cv.BoundingRect(c);
                        maxArea = area;
                      
                    }
                }
            }

        }
        return maxRect;
    }
    public static CvPoint ExtractBlobCenterfromBGRAImage(CvMat inputMat)
    {

        CvMat grayimg = new CvMat(inputMat.Rows, inputMat.Cols, MatrixType.U8C1);
        CvPoint ret = new CvPoint(0, 0);
        inputMat.CvtColor(grayimg, ColorConversion.BgraToGray);

        OpenCvSharp.CPlusPlus.Mat tInput = new Mat(grayimg);
        Point[][] contours; //vector<vector<Point>> contours;
        HierarchyIndex[] hierarchyIndexes; //vector<Vec4i> hierarchy;        
        Cv2.FindContours(
                       tInput,
                       out contours,
                       out hierarchyIndexes,
                       ContourRetrieval.CComp, ContourChain.ApproxSimple);

        if (contours.Length == 0)
        {
            return ret;
        }
        var componentCount = 0;
        var contourIndex = 0;        
        while ((contourIndex >= 0))
        {
            for (int i = 0; i < contours[contourIndex].Length; i++)
            {
                //                Cv.DrawCircle(debugImage, new CvPoint(contours[contourIndex][i].X, contours[contourIndex][i].Y), 1, CvColor.Red);
                //debugImage.Set2D(contours[contourIndex][i].X, contours[contourIndex][i].Y, CvColor.Pink);                

            }
            Moments m = Cv2.Moments(contours[contourIndex]);
            ret.X = (int)(m.M10 / m.M00);
            ret.Y = (int)(m.M01 / m.M00);            
            componentCount++;
            contourIndex = hierarchyIndexes[contourIndex].Next;
        }

        return ret;
    }
    public static List<ModelDef> ExtractStrcutureInfoFromImageBlob(CvMat GreyImgColorBlobLabeled,CvMat debugImage,ModelCategory modelType)
    {
        List<ModelDef> modelList = new List<ModelDef>();
        OpenCvSharp.CPlusPlus.Mat tInput = new Mat(GreyImgColorBlobLabeled);
        Point[][] contours; //vector<vector<Point>> contours;
        HierarchyIndex[] hierarchyIndexes; //vector<Vec4i> hierarchy;      
        CvMat t = GreyImgColorBlobLabeled.Clone();
        Cv2.FindContours(
                       tInput,
                       out contours,
                       out hierarchyIndexes,
                       ContourRetrieval.CComp, ContourChain.ApproxSimple);  //

        if (contours.Length == 0)
        {
            return null;
            
        }
        
        var componentCount = 0;
        var contourIndex = 0;
        double wholeAreaSize = GreyImgColorBlobLabeled.Width * GreyImgColorBlobLabeled.Height;
        int minPixelSize = GlobalRepo.getParamInt("minBloxPixelSize");
        
        while ((contourIndex >= 0))
        {            
            for(int i = 0; i < contours[contourIndex].Length; i++)
            {
//                Cv.DrawCircle(debugImage, new CvPoint(contours[contourIndex][i].X, contours[contourIndex][i].Y), 1, CvColor.Red);
                //debugImage.Set2D(contours[contourIndex][i].X, contours[contourIndex][i].Y, CvColor.Pink);                
                
            }
            if (Cv2.BoundingRect(contours[contourIndex]).Width < minPixelSize || Cv2.BoundingRect(contours[contourIndex]).Height < minPixelSize)
            {
                contourIndex = hierarchyIndexes[contourIndex].Next;
                continue;
            }
            Moments m;
            Point[] reducedContour = reduceContourPoints(contours[contourIndex], 20);
            if(contours[contourIndex].Length>40) m = Cv2.Moments(reducedContour);
                else m = Cv2.Moments(contours[contourIndex]);

            ModelDef model = new ModelDef();
          

            model.setMoment(m, new CvPoint(GreyImgColorBlobLabeled.Width / 2, GreyImgColorBlobLabeled.Height / 2), GreyImgColorBlobLabeled);
            model.setContour(contours[contourIndex]);
            model.bBox = Cv2.BoundingRect(contours[contourIndex]);
            model.modelType = (ModelCategory)modelType;

            //DEBUG
        
            if(model.modelType == ModelCategory.Lung)
            {
                if (model.centeroidRelative.X < 0) model.modelType = ModelCategory.LungLeft;
                    else model.modelType = ModelCategory.LungRight;
            }
            
            model.AreaSizeNormalized = model.AreaSize * 100 / wholeAreaSize;
            
            if ((model.bBox.Width >= minPixelSize || model.bBox.Height >= minPixelSize) && model.AreaSize >= 600)
            {
               // Debug.Log("New Model type = " + (int)model.modelType + "Area Size = " + model.AreaSize);
                modelList.Add(model);
                int validColorN = 0;
                ModelCategory[] colorTable = Content.loadColorObjectMap(out validColorN);
                int colorIdx =  System.Array.IndexOf(colorTable, modelType);
                model.mBaseHueValue = ColorDetector.mCP.mProfileList[colorIdx].HueClass1;
                if (config_byspass_buildgshape) model.initShapeBuilder(contours[contourIndex], model.bBox, model.centeroidAbsolute);
                    else model.initShapeBuilder();

                if (modelType == ModelCategory.FrontChainring)
                {

                    t.DrawCircle(model.centeroidAbsolute, 5, CvColor.White);
                    t.PutText(contours[contourIndex].Length.ToString() + " " + reducedContour.Length.ToString(), model.centeroidAbsolute, new CvFont(FontFace.HersheyTriplex, 1.0f, 1.0f),CvColor.White);

                }
            }
            componentCount++;
            contourIndex = hierarchyIndexes[contourIndex].Next;
        }
        if (modelType == ModelCategory.FrontChainring)  GlobalRepo.showDebugImage("front gear"+contours.Length, t);
        return modelList;


            
    }
	
    
	
	
}
public class ObjectBlobInfo
{
    public Point[] contour;
    public Point[] scaledContour;
    public Point center;
    public Point scaledCenter;
    public double areaSize;
    public int width;
    public int height;

    public void ScaletoObject(ObjectBlobInfo refObj, double ratio)
    {
        if (refObj.areaSize <= 0 || this.areaSize<=0) return;
        
        ratio = this.GetScaleFactor(refObj) * ratio;
     //   Debug.Log("Scale virtual : " + this.areaSize + " user :" + refObj.areaSize +" scale factor:"+ratio);
      //  if (this.areaSize < refObj.areaSize) ratio = 1;
        scaledContour = (Point[])contour.Clone();
        for(int i = 0; i < this.contour.Length; i++)
        {
            scaledContour[i].X = (int) (((double)scaledContour[i].X) * ratio);
            scaledContour[i].Y = (int)(((double)scaledContour[i].Y) * ratio);
        }
        //  this.areaSize = areaSize * ratio;
        scaledCenter.X = (int)(((double)center.X) * ratio);
        scaledCenter.Y = (int)(((double)center.Y) * ratio);
      //  width = (int)((double)width * ratio);
      //  height = (int)((double)height * ratio);

    }
    private double GetScaleFactor(ObjectBlobInfo refObj)
    {
        float objAspectRatio = (float)refObj.width / (float)refObj.height;
        float textureAspectRatio = (float)this.width / (float)this.height;
        //adjust texture img so as to fit with the target object image
        float adjTxtWidth, adjTxtHeight;
        if (objAspectRatio > textureAspectRatio)
        {
            adjTxtWidth = refObj.width;
            adjTxtHeight = adjTxtWidth / textureAspectRatio;
        }
        else
        {
            adjTxtHeight = refObj.height;
            adjTxtWidth = adjTxtHeight * textureAspectRatio;
        }
        return adjTxtWidth / this.width;
    }

}
