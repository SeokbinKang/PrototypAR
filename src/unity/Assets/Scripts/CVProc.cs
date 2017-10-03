using UnityEngine;
using System.Collections;
using OpenCvSharp;
using OpenCvSharp.Blob;
using OpenCvSharp.CPlusPlus;
using System.Collections.Generic;
using System;

using System.Linq;
using Tesseract;

public class CVProc {


    private static TesseractEngine mTesseractEngine = null;


    public static float measureHorizontalLevel(CvMat barImgBinary)
    {
        int width = barImgBinary.Width;
        int height = barImgBinary.Height;
        float cntFilled = 0;
        float cntBlank = 0;
        for (int x = 0; x < width; x++)
        {
            CvMat xCol = barImgBinary.GetCol(x);
            int cntBlack = 0;
            for (int y = 0; y < height; y++)
            {
                if (xCol.Get1D(y) > 0) cntBlack--;
                else cntBlack++;
            }
            if (cntBlack > 0) cntFilled++;
            else cntBlank++;

        }
        if (cntFilled + cntBlank == 0) return 0;
        return cntFilled * 100 / (cntFilled + cntBlank);
    }
    public static float measureHorizontalLevelLeftAlign(CvMat barImgBinary)
    {
        int width = barImgBinary.Width;
        int height = barImgBinary.Height;
        float cntFilled = 0;
        float cntBlank = 0;
        for (int x = 0; x < width; x++)
        {
            CvMat xCol = barImgBinary.GetCol(x);
            int cntBlack = 0;
            for (int y = 0; y < height; y++)
            {
                if (xCol.Get1D(y) > 0) cntBlack--;
                else cntBlack++;
            }
            if (cntBlack > height / 4) cntFilled++;
            else cntBlank++;

        }
        if (cntFilled + cntBlank == 0) return 0;
        return cntFilled * 100 / (cntFilled + cntBlank);
    }
    public static bool isPointinBox(CvPoint p, CvRect box)
    {
        return box.Contains(p);
    }
    public static bool isSigninModel(UserDescriptionInfo sign, ModelDef model)
    {
        bool ret = true;
        foreach (CvPoint p in sign.contourPoints)
        {
            double isIn;
            isIn = Cv2.PointPolygonTest(model.contourArray, new Point2f(p.X, p.Y), false);
            if (isIn < 0) ret = false;
        }
        return ret;
    }
    public static bool isSignontheborderofModel(UserDescriptionInfo sign, ModelDef model, double distThresh,ref CvPoint directionVector)
    {
        bool ret = true;
        double dist;
        dist = Cv2.PointPolygonTest(model.contourArray, new Point2f(sign.center.X, sign.center.Y), true);
        if (dist < (distThresh * -1)) ret = false;
        // Debug.Log(sign.instanceID + "'s distance to " + model.instanceID + " = " + dist+" In = "+ret);

        directionVector = sign.center; 
        return ret;
    }
    public static void textLabelDetection(ref List<UserDescriptionInfo> textlabelList)
    {
        for (int l = 0; l < textlabelList.Count; l++)
        {

            CvMat inputImageUC8 = new CvMat(textlabelList[l].image.Rows, textlabelList[l].image.Cols, MatrixType.U8C1);
            if (textlabelList[l].image.GetElemType() != MatrixType.U8C1) textlabelList[l].image.CvtColor(inputImageUC8, ColorConversion.BgraToGray);
            else textlabelList[l].image.Copy(inputImageUC8);
            //            inputImageUC8.Threshold(inputImageUC8, 200, 255, ThresholdType.Binary);


            //   string rText = textRecognitionTesseract(inputImageUC8); 
            float confidence = 0;
            string rText = textRecognitionTesseractviaFile(inputImageUC8, out confidence);

            textlabelList[l].InfoTextLabelstring = rText;
        }

        return;
    }

    static int q = 0;
    public static string textRecognitionTesseractviaFile(CvMat textImg, out float Confidence)
    {
        if (mTesseractEngine == null)
        {
            mTesseractEngine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default);
        }

        //System.Drawing.Bitmap t = new System.Drawing.Bitmap(textImg.Width, textImg.Height, textImg.Step, System.Drawing.Imaging.PixelFormat.Format8bppIndexed, textImg.Data);

        //bitmap width should be x4
        textImg.SaveImage("temporary.bmp");

        string resultString;
        Confidence = 0;
        using (var img = Pix.LoadFromFile("temporary.bmp"))
        {

            using (Page tessPage = mTesseractEngine.Process(img))
            {
                string text = tessPage.GetText();
                //    Debug.Log("Mean confidence: " + tessPage.GetMeanConfidence());
                //     Debug.Log("Text (GetText): \r\n" + text);
                //     Debug.Log("Text (iterator):");
                resultString = text;
                Confidence = tessPage.GetMeanConfidence();
                using (var iter = tessPage.GetIterator())
                {
                    iter.Begin();

                    do
                    {
                        do
                        {
                            do
                            {
                                do
                                {
                                    if (iter.IsAtBeginningOf(PageIteratorLevel.Block))
                                    {

                                    }


                                    //  Debug.Log(iter.GetText(PageIteratorLevel.Word));


                                    if (iter.IsAtFinalOf(PageIteratorLevel.TextLine, PageIteratorLevel.Word))
                                    {
                                        //    Console.WriteLine();
                                    }
                                } while (iter.Next(PageIteratorLevel.TextLine, PageIteratorLevel.Word));

                                if (iter.IsAtFinalOf(PageIteratorLevel.Para, PageIteratorLevel.TextLine))
                                {
                                    //Console.WriteLine();
                                }
                            } while (iter.Next(PageIteratorLevel.Para, PageIteratorLevel.TextLine));
                        } while (iter.Next(PageIteratorLevel.Block, PageIteratorLevel.Para));
                    } while (iter.Next(PageIteratorLevel.Block));
                }
            }
        }
        return resultString;

    }
    private static string textRecognitionTesseract(CvMat textImg)
    {
        if (mTesseractEngine == null)
        {
            mTesseractEngine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default);
        }

        //System.Drawing.Bitmap t = new System.Drawing.Bitmap(textImg.Width, textImg.Height, textImg.Step, System.Drawing.Imaging.PixelFormat.Format8bppIndexed, textImg.Data);

        //bitmap width should be x4
        CvMat textImagex4 = new CvMat(textImg.Rows - (textImg.Rows % 4), textImg.Cols - (textImg.Cols % 4), MatrixType.U8C1);
        Cv.Resize(textImg, textImagex4);
        /* CvMat textImagex4 = new CvMat("testTextRecog.bmp", LoadMode.GrayScale);*/
        Debug.Log(textImagex4.Step + "   " + textImagex4.Width);
        Debug.Log(textImg.Step + "   " + textImg.Width);
        if (textImg.GetElemType() == textImagex4.GetElemType()) Debug.Log("Same Mat Grayscale");
        new CvWindow("textlabel", textImagex4);
        System.Drawing.Bitmap t = new System.Drawing.Bitmap(textImagex4.Width, textImagex4.Height, textImagex4.Step, System.Drawing.Imaging.PixelFormat.Format8bppIndexed, (IntPtr)textImagex4.Data);
        t.Save("textImageBMP" + (q++) + ".bmp", System.Drawing.Imaging.ImageFormat.Bmp);
        textImagex4.SaveImage("textImage" + (q++) + ".bmp");
        string resultString;
        using (Page tessPage = mTesseractEngine.Process(t))
        {
            string text = tessPage.GetText();
            Debug.Log("Mean confidence: " + tessPage.GetMeanConfidence());
            Debug.Log("Text (GetText): \r\n" + text);
            Debug.Log("Text (iterator):");
            resultString = text;
            using (var iter = tessPage.GetIterator())
            {
                iter.Begin();

                do
                {
                    do
                    {
                        do
                        {
                            do
                            {
                                if (iter.IsAtBeginningOf(PageIteratorLevel.Block))
                                {

                                }


                                Debug.Log(iter.GetText(PageIteratorLevel.Word));


                                if (iter.IsAtFinalOf(PageIteratorLevel.TextLine, PageIteratorLevel.Word))
                                {
                                    //    Console.WriteLine();
                                }
                            } while (iter.Next(PageIteratorLevel.TextLine, PageIteratorLevel.Word));

                            if (iter.IsAtFinalOf(PageIteratorLevel.Para, PageIteratorLevel.TextLine))
                            {
                                //Console.WriteLine();
                            }
                        } while (iter.Next(PageIteratorLevel.Para, PageIteratorLevel.TextLine));
                    } while (iter.Next(PageIteratorLevel.Block, PageIteratorLevel.Para));
                } while (iter.Next(PageIteratorLevel.Block));
            }
        }
        return resultString;

    }


    public static void testTesseract()
    {
        var testImagePath = "./phototest.tif";
        try
        {
            using (var engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default))
            {

                using (Pix img = Pix.LoadFromFile(testImagePath))
                {


                    using (var page = engine.Process(img))
                    {
                        var text = page.GetText();
                        Console.WriteLine("Mean confidence: {0}", page.GetMeanConfidence());

                        Console.WriteLine("Text (GetText): \r\n{0}", text);
                        Console.WriteLine("Text (iterator):");
                        using (var iter = page.GetIterator())
                        {
                            iter.Begin();

                            do
                            {
                                do
                                {
                                    do
                                    {
                                        do
                                        {
                                            if (iter.IsAtBeginningOf(PageIteratorLevel.Block))
                                            {
                                                Console.WriteLine("<BLOCK>");
                                            }

                                            Console.Write(iter.GetText(PageIteratorLevel.Word));
                                            Debug.Log(iter.GetText(PageIteratorLevel.Word));
                                            Console.Write(" ");

                                            if (iter.IsAtFinalOf(PageIteratorLevel.TextLine, PageIteratorLevel.Word))
                                            {
                                                Console.WriteLine();
                                            }
                                        } while (iter.Next(PageIteratorLevel.TextLine, PageIteratorLevel.Word));

                                        if (iter.IsAtFinalOf(PageIteratorLevel.Para, PageIteratorLevel.TextLine))
                                        {
                                            Console.WriteLine();
                                        }
                                    } while (iter.Next(PageIteratorLevel.Para, PageIteratorLevel.TextLine));
                                } while (iter.Next(PageIteratorLevel.Block, PageIteratorLevel.Para));
                            } while (iter.Next(PageIteratorLevel.Block));
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
            Debug.Log("Unexpected Error: " + e.Message);
            Debug.Log("Details: ");
            Debug.Log(e.ToString());
        }
        Console.Write("Press any key to continue . . . ");
    }
    public static CvPoint CalculateRelativePosinBox(CvPoint absolutePoint, CvRect box)
    {
        CvPoint t = new CvPoint(0, 0);
        //  if (!box.Contains(absolutePoint)) return t;
        t.X = (absolutePoint.X - box.TopLeft.X) * 100 / box.Width;
        t.Y = (absolutePoint.Y - box.TopLeft.Y) * 100 / box.Height;
        return t;
    }
    public static void findRectangularMarkers(CvMat GrayImage, ref List<UserDescriptionInfo> rectMarkerList)
    {
        CvMat inputImageUC8 = GrayImage;

        if (inputImageUC8 == null) return;
        if (inputImageUC8.ElemType != MatrixType.U8C1)
        {
            inputImageUC8 = new CvMat(GrayImage.Rows, GrayImage.Cols, MatrixType.U8C1);
            GrayImage.CvtColor(inputImageUC8, ColorConversion.BgraToGray);

        } else
        {
            inputImageUC8 = GrayImage.Clone();
        }

        inputImageUC8.Threshold(inputImageUC8, 160, 255, ThresholdType.BinaryInv);
        CvSeq<CvPoint> contoursRaw;

        int id = 0;

        using (CvMemStorage storage = new CvMemStorage())
        {
            //find contours
            Cv.FindContours(inputImageUC8, storage, out contoursRaw, CvContour.SizeOf, ContourRetrieval.External, ContourChain.ApproxSimple);
            //Taken straight from one of the OpenCvSharp samples
            using (CvContourScanner scanner = new CvContourScanner(inputImageUC8, storage, CvContour.SizeOf, ContourRetrieval.External, ContourChain.ApproxSimple))
            {
                foreach (CvSeq<CvPoint> c in scanner)
                {
                    //Some contours are negative so make them all positive for easy comparison
                    double area = Math.Abs(c.ContourArea());
                    //Uncomment below to see the area of each contour
                    //Console.WriteLine(area.ToString());
                    if (area >= 30)
                    {
                        List<CvPoint[]> points = new List<CvPoint[]>();
                        List<CvPoint> point = new List<CvPoint>();
                        CvSeq<CvPoint> cApprox = c.ApproxPoly(CvContour.SizeOf, storage, ApproxPolyMethod.DP, c.ArcLength() * 0.02f, true);

                        foreach (CvPoint p in cApprox.ToArray())
                            point.Add(p);
                        bool isRect = false;
                        if (point.Count == 4)
                        {

                            double maxCosine = 0;
                            double[] sideLength = new double[4];
                            for (int j = 2; j < 5; j++)
                            {
                                // find the maximum cosine of the angle between joint edges
                                double cosine = Math.Abs(angle(point[j % 4], point[j - 2], point[j - 1]));
                                maxCosine = Math.Max(maxCosine, cosine);

                                double dx = point[j - 1].X - point[j % 4].X;
                                double dy = point[j - 1].Y - point[j % 4].Y;
                                sideLength[j - 2] = Math.Sqrt(dx * dx + dy * dy);
                            }
                            if (maxCosine < 0.3)
                            {
                                isRect = true;
                                //rectangle found
                                //check if square or rectangle
                                double squareRange = 0.1f;
                                bool isSquare = true;

                                for (int i = 1; i < 3; i++)
                                {

                                    if (Math.Abs(sideLength[i] - sideLength[i - 1]) > sideLength[i] * squareRange)
                                    {
                                        isSquare = false;
                                    }
                                }

                                UserDescriptionInfo marker = new UserDescriptionInfo();

                                CvRect roi = Cv.BoundingRect(cApprox);
                                marker.contourPoints = cApprox.ToArray();
                                marker.boundingBox = roi;
                                if (isSquare)
                                {
                                    //do nothing


                                }
                                else
                                {  // rectangluar label   
                                    float tilt;
                                    CvPoint markerCenter;
                                    markerCenter = measureTiltandCenter(marker.contourPoints, out tilt);
                                    Debug.Log("[DEBUG] tilt degree = " + tilt + "CM:" + markerCenter.X + "," + markerCenter.Y);

                                    marker.image = getTiltCorrectedRectImage(GrayImage, cApprox.ToArray<CvPoint>(), markerCenter, tilt);
                                    marker.instanceID = id++;
                                    marker.InfoBehaviorTypeId = 999;
                                    marker.center = markerCenter;
                                    rectMarkerList.Add(marker);
                                    /*
                                    roi = shrinkROI(roi);
                                    CvMat mask = new CvMat(inputImageUC8.Rows, inputImageUC8.Cols, MatrixType.U8C1);
                                    mask.SetZero();
                                    
                                    Cv.DrawContours(mask, cApprox, CvColor.White, CvColor.White,0,-1);
                                    CvPoint centeroid;
                                    CvMoments m = cApprox.ContoursMoments();
                                    centeroid.X = (int)(m.M10 / m.M00);
                                    centeroid.Y = (int)(m.M01 / m.M00);
                                    

                                    CvMat contourRegion;
                                    CvMat contourRegion2 = new CvMat(roi.Height, roi.Width, GrayImage.GetElemType());
                                    CvMat contourRegionBW = new CvMat(roi.Height, roi.Width, MatrixType.U8C1);                                    
                                    CvMat imageROI = new CvMat(GrayImage.Rows, GrayImage.Cols, GrayImage.GetElemType());
                                    imageROI.Set(CvColor.White);
                                    GrayImage.Copy(imageROI, mask);
                                    imageROI.GetSubArr(out contourRegion, roi);
                                    
                                    if (contourRegion.ElemType == MatrixType.U8C4) contourRegion.CvtColor(contourRegionBW, ColorConversion.BgraToGray);
                                    else contourRegion.Copy(contourRegionBW);                                    

                                    marker.image = contourRegionBW;
                                    marker.instanceID = id++;
                                    marker.InfoBehaviorTypeId = 999;
                                    marker.center = centeroid;
                                    */
                                    //     rectMarkerList.Add(marker);


                                }




                            }
                        }




                    }
                }
            }
        }


        return;

    }
    public static void findRectMarkers(CvMat img, ref List<UserDescriptionInfo> anyMarkerList, ref List<UserDescriptionInfo> SquaremarkerList, ref List<UserDescriptionInfo> rectMarkerList)
    {
        CvMat inputImageUC8 = new CvMat(img.Rows, img.Cols, MatrixType.U8C1);
        Debug.Log("Func findSquares: img size " + img.Rows + "  " + img.Cols);
        if (img == null) return;
        if (img.ElemType != MatrixType.U8C1)
        {
            img.CvtColor(inputImageUC8, ColorConversion.BgraToGray);
        } else
        {
            img.Copy(inputImageUC8);
        }

        inputImageUC8.Threshold(inputImageUC8, GlobalRepo.getParamInt("BlackThreshold"), 255, ThresholdType.BinaryInv);
        GlobalRepo.showDebugImage("binaryRegion", inputImageUC8);
        CvSeq<CvPoint> contoursRaw;

        HierarchyIndex[] hierarchyIndexes; //vector<Vec4i> hierarchy;       
        Point[][] contours; //vector<vector<Point>> contours;
        OpenCvSharp.CPlusPlus.Mat WtInput = new Mat(inputImageUC8);

        int id = 0;
        using (CvMemStorage storage = new CvMemStorage())
        {
            //find contours
            Cv.FindContours(inputImageUC8, storage, out contoursRaw, CvContour.SizeOf, ContourRetrieval.External, ContourChain.ApproxSimple);
            //Taken straight from one of the OpenCvSharp samples
            using (CvContourScanner scanner = new CvContourScanner(inputImageUC8, storage, CvContour.SizeOf, ContourRetrieval.External, ContourChain.ApproxSimple))
            {
                foreach (CvSeq<CvPoint> c in scanner)
                {
                    //Some contours are negative so make them all positive for easy comparison
                    double area = Math.Abs(c.ContourArea());
                    //Uncomment below to see the area of each contour
                    //Console.WriteLine(area.ToString());
                    if (area >= 30)
                    {
                        List<CvPoint[]> points = new List<CvPoint[]>();
                        List<CvPoint> point = new List<CvPoint>();
                        CvSeq<CvPoint> cApprox = c.ApproxPoly(CvContour.SizeOf, storage, ApproxPolyMethod.DP, c.ArcLength() * 0.02f, true);

                        foreach (CvPoint p in cApprox.ToArray())
                            point.Add(p);
                        for (int k = 0; k < point.Count; k++)
                        {
                            Cv.DrawCircle(img, point[k], 3, CvColor.Red);
                        }
                        bool isRect = false;


                        if (point.Count == 4)
                        {

                            double maxCosine = 0;
                            double[] sideLength = new double[4];
                            for (int j = 2; j < 5; j++)
                            {
                                // find the maximum cosine of the angle between joint edges
                                double cosine = Math.Abs(angle(point[j % 4], point[j - 2], point[j - 1]));
                                maxCosine = Math.Max(maxCosine, cosine);

                                double dx = point[j - 1].X - point[j % 4].X;
                                double dy = point[j - 1].Y - point[j % 4].Y;
                                sideLength[j - 2] = Math.Sqrt(dx * dx + dy * dy);
                            }
                            if (maxCosine < 0.3)
                            {
                                isRect = true;
                                //rectangle found
                                //check if square or rectangle
                                double squareRange = 0.1f;
                                bool isSquare = true;

                                for (int i = 1; i < 3; i++)
                                {

                                    if (Math.Abs(sideLength[i] - sideLength[i - 1]) > sideLength[i] * squareRange)
                                    {
                                        isSquare = false;
                                    }
                                }

                                UserDescriptionInfo marker = new UserDescriptionInfo();
                                marker.contourPoints = cApprox.ToArray();
                                CvRect roi = Cv.BoundingRect(cApprox);
                                marker.boundingBox = roi;
                                if (isSquare)
                                {


                                    CvMat mask = new CvMat(img.Rows, img.Cols, MatrixType.U8C1);
                                    mask.SetZero();

                                    Cv.DrawContours(mask, cApprox, CvColor.White, CvColor.White, 0);
                                    CvPoint centeroid;
                                    CvMoments m = cApprox.ContoursMoments();
                                    centeroid.X = (int)(m.M10 / m.M00);
                                    centeroid.Y = (int)(m.M01 / m.M00);
                                    mask.FloodFill(centeroid, CvColor.White);

                                    CvMat contourRegion;
                                    CvMat contourRegion2 = new CvMat(roi.Height, roi.Width, img.GetElemType());
                                    CvMat contourRegionBW = new CvMat(roi.Height, roi.Width, MatrixType.U8C1);
                                    CvMat imageROI = new CvMat(img.Rows, img.Cols, img.GetElemType());
                                    imageROI.Set(CvColor.White);
                                    img.Copy(imageROI, mask);
                                    imageROI.GetSubArr(out contourRegion, roi);
                                    //contourRegion.Copy(contourRegion2);
                                    if (contourRegion.ElemType == MatrixType.U8C4) contourRegion.CvtColor(contourRegionBW, ColorConversion.BgraToGray);
                                    else contourRegion.Copy(contourRegionBW);
                                    CvMat largeImage = new CvMat(200, 200 * img.Cols / img.Rows, MatrixType.U8C1);
                                    Cv.Resize(contourRegionBW, largeImage);
                                    marker.image = largeImage;
                                    marker.instanceID = id++;
                                    marker.InfoBehaviorTypeId = 999;
                                    marker.center = centeroid;
                                    SquaremarkerList.Add(marker);

                                    //square marker
                                } else
                                {  // rectangluar label    
                                    /*                              
                                    roi = shrinkROI(roi);
                                    CvMat mask = new CvMat(img.Rows, img.Cols, MatrixType.U8C1);
                                    mask.SetZero();

                                    Cv.DrawContours(mask, cApprox, CvColor.White, CvColor.White, 0,-1);
                                    CvPoint centeroid;
                                    CvMoments m = cApprox.ContoursMoments();
                                    centeroid.X = (int)(m.M10 / m.M00);
                                    centeroid.Y = (int)(m.M01 / m.M00);                                  

                                    CvMat contourRegion;
                                    CvMat contourRegion2 = new CvMat(roi.Height, roi.Width, img.GetElemType());
                                    CvMat contourRegionBW = new CvMat(roi.Height, roi.Width, MatrixType.U8C1);                              
                                    CvMat imageROI = new CvMat(img.Rows, img.Cols, img.GetElemType());
                                    imageROI.Set(CvColor.White);
                                    img.Copy(imageROI, mask);
                                    imageROI.GetSubArr(out contourRegion, roi);
                                
                                    if (contourRegion.ElemType == MatrixType.U8C4) contourRegion.CvtColor(contourRegionBW, ColorConversion.BgraToGray);
                                    else contourRegion.Copy(contourRegionBW);

                               

                                    marker.image = contourRegionBW;
                                    marker.instanceID = id++;
                                    marker.InfoBehaviorTypeId = 999;
                                    marker.center = centeroid;
                                    rectMarkerList.Add(marker);*/


                                }




                            }
                        }
                        if (!isRect)
                        {

                            {
                                //save to anymarker superset
                                UserDescriptionInfo marker = new UserDescriptionInfo();
                                marker.contourPoints = cApprox.ToArray();
                                CvRect roi = Cv.BoundingRect(cApprox);
                                marker.boundingBox = roi;
                                CvMat mask = new CvMat(img.Rows, img.Cols, MatrixType.U8C1);
                                mask.SetZero();
                                CvPoint[][] tmpCountour = new CvPoint[1][];
                                tmpCountour[0] = cApprox.ToArray();
                                mask.FillPoly(tmpCountour, CvColor.White);

                                CvPoint centeroid;
                                CvMoments m = cApprox.ContoursMoments();
                                centeroid.X = (int)(m.M10 / m.M00);
                                centeroid.Y = (int)(m.M01 / m.M00);

                                //Cv.DrawContours(mask, cApprox, CvColor.White, CvColor.White, 0);
                                //mask.FloodFill(centeroid, CvColor.White);

                                CvMat contourRegion;

                                CvMat contourRegionBW = new CvMat(roi.Height, roi.Width, MatrixType.U8C1);
                                CvMat imageROI = new CvMat(img.Rows, img.Cols, img.GetElemType());
                                imageROI.Set(CvColor.White);
                                img.Copy(imageROI, mask);
                                imageROI.GetSubArr(out contourRegion, roi);
                                //contourRegion.Copy(contourRegion2);
                                if (contourRegion.ElemType == MatrixType.U8C4) contourRegion.CvtColor(contourRegionBW, ColorConversion.BgraToGray);
                                else contourRegion.Copy(contourRegionBW);
                                marker.image = contourRegionBW;
                                marker.instanceID = id++;
                                marker.InfoBehaviorTypeId = 999;
                                marker.center = centeroid;
                                anyMarkerList.Add(marker);
                            }
                        }

                        GlobalRepo.showDebugImage("CVPROC", img);


                        //-1 means fill the polygon
                        //output.DrawContours(c, CvColor.White, CvColor.Green, 0, -1, LineType.AntiAlias);

                        //Uncomment two lines below to see contours being drawn gradually
                        //Cv.ShowImage("Window", output);
                        //Cv.WaitKey();
                    }
                }
            }
        }

        //if (debugWindow3 != null) debugWindow3.ShowImage(img);
        return;






    }

    private static bool isSquare(CvSeq<CvPoint> contour)
    {
        return false;
    }
    private static double angle(Point pt1, Point pt2, Point pt0)
    {
        double dx1 = pt1.X - pt0.X;
        double dy1 = pt1.Y - pt0.Y;
        double dx2 = pt2.X - pt0.X;
        double dy2 = pt2.Y - pt0.Y;
        return (dx1 * dx2 + dy1 * dy2) / Math.Sqrt((dx1 * dx1 + dy1 * dy1) * (dx2 * dx2 + dy2 * dy2) + 1e-10);
    }
    // Update is called once per frame
    void Update() {

    }
    public static Mat ConvertMat3toMat2UC8(OpenCv30Sharp.Mat inMat)
    {
        Mat outMat = new Mat(inMat.Rows, inMat.Cols, MatType.CV_8UC1, inMat.Data);


        return outMat;

    }
    public static OpenCv30Sharp.Mat ConvertMat2toMat3UC8(CvMat inMat)
    {
        OpenCv30Sharp.Mat outMat = new OpenCv30Sharp.Mat(inMat.Rows, inMat.Cols, OpenCv30Sharp.MatType.CV_8UC1, inMat.Data);
        return outMat;
    }
    public static CvMat FilterFastNM(ref CvMat matIn)
    {
        CvMat ret = new CvMat(matIn.Rows, matIn.Cols, matIn.ElemType);
        OpenCvSharp.CPlusPlus.Mat img = new Mat(matIn);
        OpenCvSharp.CPlusPlus.Mat img2 = new Mat(ret);
        Cv2.FastNlMeansDenoising(img, img2);
        return ret;
    }
    public static CvRect shrinkROI(CvRect roi)
    { //TODO boundary check to image frame
        CvRect ret = new CvRect(roi.X + 4, roi.Y + 4, roi.Width - 8, roi.Height - 8);
        return ret;
    }
    public static CvPoint measureTiltandCenter(CvPoint[] rectPoints, out float degree)
    {
        CvPoint c = new CvPoint();
        degree = 0.0f;
        if (rectPoints.Length != 4) return c;
        CvPoint LT, RT, LB, RB;
        rectPoints = rectPoints.OrderByDescending(x => x.X).ToArray<CvPoint>();
        //01 right side, 23 left side
        if (rectPoints[0].Y > rectPoints[1].Y) {
            RB = rectPoints[0];
            RT = rectPoints[1];
        } else
        {
            RB = rectPoints[1];
            RT = rectPoints[0];
        }
        if (rectPoints[2].Y > rectPoints[3].Y)
        {
            LB = rectPoints[2];
            LT = rectPoints[3];
        }
        else
        {
            LB = rectPoints[3];
            LT = rectPoints[2];
        }
        float[] angles = new float[2];
        angles[0] = Mathf.Atan2((RT - LT).Y * -1, (RT - LT).X);
        angles[1] = Mathf.Atan2((RB - LB).Y * -1, (RB - LB).X);
        degree = angles.Sum() / ((float)angles.Length);
        Debug.Log("[DEBUG TILT] LT:" + LT.X + "," + LT.Y + "  RT:" + RT.X + "," + RT.Y + "  LB:" + LB.X + "," + LB.Y + "  RB:" + RB.X + "," + RB.Y);
        c.X = rectPoints.Sum(x => x.X) / 4;
        c.Y = rectPoints.Sum(x => x.Y) / 4;
        return c;
    }
    public static CvMat getTiltCorrectedRectImage(CvMat refImage, CvPoint[] rect, CvPoint center, float degree)
    {
        CvMat ret;
        CvRect bBox = Cv.BoundingRect(rect);
        bBox.Inflate(50, 50);
        CvMat rawImg = refImage.GetSubArr(out rawImg, bBox).Clone();
        CvMat rotImg = rawImg.Clone();
        CvPoint translatedCenter = center - bBox.TopLeft;
        CvMat rotMat = Cv.GetRotationMatrix2D(translatedCenter, degree * 180.0f / Mathf.PI * -1.0f, 1);
        CvPoint[][] translatedRect = new CvPoint[1][];
        translatedRect[0] = rect.Clone() as CvPoint[];
        for (int i = 0; i < translatedRect[0].Length; i++)
        {
            translatedRect[0][i] = translatedRect[0][i] - bBox.TopLeft;
        }
        Cv.WarpAffine(rawImg, rotImg, rotMat);
        RotatePointsRadian(ref translatedRect[0], translatedCenter, degree);

        //////////




        CvMat mask = new CvMat(rotImg.Rows, rotImg.Cols, MatrixType.U8C1);
        mask.SetZero();

        mask.FillPoly(translatedRect, CvColor.White);


        CvMat contourRegionBW = new CvMat(rotImg.Height, rotImg.Width, MatrixType.U8C1);
        contourRegionBW.Set(CvColor.Black);
        rotImg.Copy(contourRegionBW, mask);

        CvRect tightbBox = Cv.BoundingRect(translatedRect[0]);
        tightbBox = shrinkROI(tightbBox);
        contourRegionBW.GetSubArr(out ret, tightbBox);




        ////////////


        return ret;
    }
    public static CvRect OverlappingRegions(ModelDef obj1, ModelDef obj2, out Point[] overlapPointsOut)
    {
        CvRect ret = new CvRect();
        double paramDistThresh = -7;
        double paramContPoints = 20;
        int paramMaxPatchWidth = 50;
        double dist;
        int maxChunkSize = 0;
       // if (obj1.modelType == ModelCategory.FreeWheel || obj2.modelType == ModelCategory.FreeWheel) paramDistThresh = -3;
        Point[] maxChunkPoints = null;
        ArrayList overlapPoints = new ArrayList();
        overlapPointsOut = null;
        foreach (Point p in obj1.getShapeBuilder().Fullcontour)
        {

            dist = Cv2.PointPolygonTest(obj2.getShapeBuilder().Fullcontour, p, true);
            if (dist > paramDistThresh)
            {
                if (overlapPoints.Count == 0) overlapPoints.Add(p);
                else if (((Point)overlapPoints[overlapPoints.Count - 1]).DistanceTo(p) < paramContPoints)
                {
                    overlapPoints.Add(p);

                } else if (overlapPoints.Count > maxChunkSize) {
                    maxChunkSize = overlapPoints.Count;
                    maxChunkPoints = overlapPoints.ToArray(typeof(Point)) as Point[];
                    overlapPoints.Clear();

                } else
                {
                    overlapPoints.Clear();
                }

            }
        }
        if (overlapPoints.Count > maxChunkSize) {
            maxChunkSize = overlapPoints.Count;
            maxChunkPoints = overlapPoints.ToArray(typeof(Point)) as Point[];
            overlapPoints.Clear();
        }
        if (maxChunkSize > 0)
        {
            if (maxChunkPoints.Length >10)
            {
                int marginalCount = maxChunkPoints.Length / 7;
                
                maxChunkPoints = maxChunkPoints.Skip(marginalCount).Take(maxChunkPoints.Length - marginalCount*2).ToArray<Point>() as Point[];
            }
            ret = Cv2.BoundingRect(maxChunkPoints);
           // ret = new CvRect(maxChunkPoints[maxChunkPoints.Length / 2] - new Point(paramMaxPatchWidth / 2, paramMaxPatchWidth / 2), new CvSize(paramMaxPatchWidth, paramMaxPatchWidth));
            
            overlapPointsOut = maxChunkPoints;
            /*CvMat debugImg = GlobalRepo.GetRepo(RepoDataType.dRawRegionBGR).Clone();
            foreach (Point p in maxChunkPoints)
            {
                debugImg.DrawCircle(p, 2, CvColor.Red);

            }

            GlobalRepo.showDebugImage("overlapRawA", obj1.getShapeBuilder().BuildingImg);
            GlobalRepo.showDebugImage("overlapRawB", obj2.getShapeBuilder().BuildingImg);
            GlobalRepo.showDebugImage("overlapRaw", debugImg);*/

        } else
        {

        }
        return ret;
    }
    public static void RotatePoints(ref Point[] points, Point Center, double degree)
    {
        for (int i = 0; i < points.Length; i++)
        {
            points[i].X = (int)((Math.Cos(degree) * ((double)(points[i].X - Center.X)) - Math.Sin(degree) * ((double)(points[i].Y - Center.Y))) + (double)Center.X);
            points[i].Y = (int)((Math.Sin(degree) * ((double)(points[i].X - Center.X)) + Math.Cos(degree) * ((double)(points[i].Y - Center.Y))) + (double)Center.Y);
        }
    }
    public static void RotatePointsRadian(ref CvPoint[] points, Point Center, double degree)
    {
        CvPoint[] ret = new CvPoint[points.Length];
        for (int i = 0; i < points.Length; i++)
        {
            points[i].X = (int)((Math.Cos(degree) * ((double)(points[i].X - Center.X)) - Math.Sin(degree) * ((double)(points[i].Y - Center.Y))) + (double)Center.X);
            points[i].Y = (int)((Math.Sin(degree) * ((double)(points[i].X - Center.X)) + Math.Cos(degree) * ((double)(points[i].Y - Center.Y))) + (double)Center.Y);
        }

    }
    public static void AddjustLenth(ref Point[] points, Point Center, double length)
    {
        double curLength = points[0].DistanceTo(points[points.Length - 1]);
        double ratio = length / curLength;
        for (int i = 0; i < points.Length; i++)
        {
            points[i].X = (int)((double)Center.X + ((double)(points[i].X - Center.X)) * ratio);
            points[i].Y = (int)((double)Center.Y + ((double)(points[i].Y - Center.Y)) * ratio);
        }
    }
    public static CvMat getSubimgHorizontalBorder(CvMat img, int BorderRelScale100)
    {
        if (img == null || BorderRelScale100 < 0 || BorderRelScale100 > 100) return null;
        int horizontalPos = (img.Width - 1) * BorderRelScale100 / 100;
        CvRect subBox = new CvRect(horizontalPos, 0, img.Width - horizontalPos, img.Height);
        CvMat subImg = img.GetSubArr(out subImg, subBox);

        return subImg;
    }
    //Determine which side of region box borders is the closted to the point.
    public static Direction4Way ClosestBorderofRegionBox(CvPoint p, bool leftrightonly) {
        Direction4Way ret = Direction4Way.None;  // 1 left, 2 right, 3 up, 4 down
        int dist = int.MaxValue;
        CvSize regionSize = GlobalRepo.GetRegionBox(false).Size;
        if (regionSize.Width < 1) return ret; // default

        if (p.X < dist)
        {
            dist = p.X;
            ret = Direction4Way.Left;
        }
        if (!leftrightonly && p.Y < dist)
        {
            dist = p.Y;
            ret = Direction4Way.Up;
        }
        if (regionSize.Width - p.X < dist)
        {
            dist = regionSize.Width - p.X;
            ret = Direction4Way.Right;
        }
        if (!leftrightonly && regionSize.Height - p.Y < dist)
        {
            dist = regionSize.Height - p.Y;
            ret = Direction4Way.Down;
        }

        return ret;
    }
}


public enum Direction4Way
{
    None,
    Up,
    Down,
    Left,
    Right
}