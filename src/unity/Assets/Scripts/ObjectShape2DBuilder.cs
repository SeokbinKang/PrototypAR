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

public class ObjectShape2DBuilder
{
    //object builder
    private List<CvPoint> BuilderSeedPointPool = null;
    public CvMat BuildingImg = null;    
    public Point[] Fullcontour = null;
    public Point[] ReducedContour = null;
    public Point center;
    public CvRect bBox;

    private CvMat maskImg = null;
    private float dominantH;
    private float dominantCount;
    private bool dominantHSaturated;
    private bool BuildingFinished;
    private bool forceConvex;
    private float mbaseHueVal;

    //param
    private static int paramHueRangeLow = 4;
    private static int paramHueRangeHigh = 4;
    private static int paramHueSaturationSampleCount =100;
    private static int paramWinSize = 3;
    private static int paramPixelsperIteration = 800;
    private static int paramMinSaturation = 40;
    private static int paramFastWinSize = 5;
    //param

    //static data
    private static int[,] cornerIndex_8corners = new int[,] { { -1, -1 }, { -1, 0 }, { -1, 1 }, { 0, -1 }, { 0, 1 }, { 1, -1 }, { 1, 0 }, { 1, 1 } };


    //debug
    private CvWindow mDebugWindow = null;
    private int debugbuilderID;
    private static int globalbuilderID = 0;

    public ObjectShape2DBuilder(CvPoint seedPoint,float baseHue,bool forceConvex_)
    { //initialize seed from the position within the ROI
        BuilderSeedPointPool = refineSeedPoint(seedPoint, baseHue);
        dominantH = 0;
        dominantCount = 0;
        dominantHSaturated = false;
        BuildingImg = null;        
        debugbuilderID = globalbuilderID++;
        forceConvex = forceConvex_;
        mbaseHueVal = baseHue;
        BuildingFinished = false;
    }
    //create pre-built builder
    public ObjectShape2DBuilder(Point[] pfullContour, Point pcenter, CvRect pBox, bool forceConvex_)
    {
        CvMat regionImgHSV = GlobalRepo.GetRepo(RepoDataType.dRawRegionHSV);
        CvMat pRawRegionImgRGBA = GlobalRepo.GetRepo(RepoDataType.dRawRegionRGBA);
        BuilderSeedPointPool = new List<CvPoint>();
        
        if (BuildingImg == null || BuildingImg.GetSize() != regionImgHSV.GetSize())
        {
            BuildingImg = new CvMat(regionImgHSV.Rows, regionImgHSV.Cols, MatrixType.U8C4);
            
        }
        BuildingImg.Zero();
        if (maskImg == null || maskImg.GetSize() != regionImgHSV.GetSize())
        {
            maskImg = new CvMat(regionImgHSV.Rows , regionImgHSV.Cols , MatrixType.U8C1);
            
        }
        maskImg.Zero();
        if(pfullContour==null)
        {
            Debug.Log("[ERROR] blob contour is NULL! ");
            return;
        }
        debugbuilderID = globalbuilderID++;
        CvMemStorage storage = new CvMemStorage();
        CvPoint[] fullContour_conv = new CvPoint[pfullContour.Length];
        for (int i = 0; i < pfullContour.Length; i++)
            fullContour_conv[i] = pfullContour[i];
        CvSeq<CvPoint> FullContour_seq = CvSeq<CvPoint>.FromArray(fullContour_conv, SeqType.Contour, storage);
        maskImg.DrawContours(FullContour_seq, new CvScalar(255, 255, 255, 255), new CvScalar(255, 255, 255, 255), 0);
        this.center = pcenter;
        this.bBox = pBox;        
        pRawRegionImgRGBA.Copy(BuildingImg, maskImg);
        this.Fullcontour = pfullContour;
        this.ReducedContour = Fullcontour;
        forceConvex = forceConvex_;
        if (this.forceConvex)
        {
            Point[] ReducedContour_convex;
            ReducedContour_convex = Cv2.ConvexHull(this.ReducedContour);
            //    this.ReducedContour = ReducedContour_convex;
            Moments m;
            m = Cv2.Moments(this.ReducedContour);
            this.center.X = (int)(m.M10 / m.M00);
            this.center.Y = (int)(m.M01 / m.M00);
        }
        dominantHSaturated = true;
        BuildingFinished = true;
        if (GlobalRepo.Setting_ShowDebugImgs()) debugBuildingimg();

    }
    private List<CvPoint> refineSeedPoint(CvPoint seed, float baseHue)
    {
        List<CvPoint> ret = new List<CvPoint>();
        CvMat regionImgHSV = GlobalRepo.GetRepo(RepoDataType.dRawRegionHSV);
        Debug.Log("[DEBUG-SHAPEBUILDER] Refine Seend Samples Initial Hue:" + baseHue);
        if (regionImgHSV == null) return null;
        System.Random rnd = new System.Random();
        
        for (int i = 0; i < 15; i++)
        {
            CvPoint sample = new CvPoint(rnd.Next(-20, 20), rnd.Next(-20, 20)) + seed;
            if (sample.X < 0 || sample.X > regionImgHSV.Width - 1 || sample.Y < 0 || sample.Y > regionImgHSV.Height - 1) continue;
            
            CvScalar sampleHSV = regionImgHSV.Get2D(sample.Y, sample.X);
            if (Mathf.Abs((float)sampleHSV.Val0 - baseHue) < paramHueRangeLow) ret.Add(sample);
            else continue;
            Debug.Log("sampeling basehue: " + baseHue + "  sampled hue: " + sampleHSV.Val0);
            

        }
        if(ret.Count==0) Debug.Log("[DEBUG-SHAPEBUILDER] Failed to find the seed samples. base hue: " + baseHue);
        return ret;
    }
    public bool isFinished()
    {
        if (dominantHSaturated && BuilderSeedPointPool.Count == 0) return true;

        return false;
    }
    public void debugBuildingimg()
    {

        if (BuildingImg != null)
        {
            GlobalRepo.showDebugImage("2DBuilder#" + debugbuilderID, BuildingImg);
            Debug.Log("[DEBUG-SHAPEBUILDER] #" + debugbuilderID + "   hue:" + dominantH);
        }
        Cv.WaitKey(1);
    }
    public void BuildShapeFastFloodFill()
    {
        //use Cv.floodfill 
        //1. determine the seeding area's Hue
        //2. floodfill only with mask output
        CvMat regionImgHSV = GlobalRepo.GetRepo(RepoDataType.dRawRegionHSV);
        CvMat pRawRegionImgRGBA = GlobalRepo.GetRepo(RepoDataType.dRawRegionRGBA);
        if (BuildingImg == null || BuildingImg.GetSize() != regionImgHSV.GetSize())
        {
            BuildingImg = new CvMat(regionImgHSV.Rows, regionImgHSV.Cols, MatrixType.U8C4);
            BuildingImg.Zero();
        }
        if (maskImg == null || maskImg.GetSize() != regionImgHSV.GetSize())
        {
            maskImg = new CvMat(regionImgHSV.Rows + 2, regionImgHSV.Cols + 2, MatrixType.U8C1);
            maskImg.Zero();
        }
        dominantCount = 0;
        if (BuilderSeedPointPool.Count <= 0) return;
        CvPoint seedPoint = BuilderSeedPointPool[0];
       
        dominantHSaturated = true;
        CvConnectedComp cp = new CvConnectedComp();
        //Cv.FloodFill(regionImgHSV, seedPoint, new CvScalar(0, 0, 0), new CvScalar(paramHueRangeLow, 4, 4), new CvScalar(paramHueRangeLow, 4, 4), out cp, FloodFillFlag.MaskOnly, maskImg);
        Cv.FloodFill(regionImgHSV, seedPoint, new CvScalar(0, 0, 0), new CvScalar(paramHueRangeLow, 10, 10), new CvScalar(paramHueRangeLow, 10, 10), out cp, FloodFillFlag.MaskOnly | FloodFillFlag.FixedRange, maskImg);         
        CvMat realMask;
        realMask = maskImg.GetSubArr(out realMask, new CvRect(1, 1, BuildingImg.Width, BuildingImg.Height));
        pRawRegionImgRGBA.Copy(BuildingImg, realMask);

        if (GlobalRepo.Setting_ShowDebugImgs()) debugBuildingimg();

        this.Fullcontour = BlobAnalysis.ExtractContour(this.BuildingImg, ContourChain.ApproxNone); //full contour for connectivity check
    
        if(Fullcontour==null)
        {
            Debug.Log("[DEBUG] FastFloodFill failed");
            this.bBox = new CvRect();
            this.center = new Point(0, 0);
            this.ReducedContour = null;
        }
        this.bBox = Cv2.BoundingRect(Fullcontour);
        Moments m = Cv2.Moments(this.Fullcontour);
        this.center.X = (int)(m.M10 / m.M00);
        this.center.Y = (int)(m.M01 / m.M00);

        this.ReducedContour = BlobAnalysis.ExtractContour(this.BuildingImg, ContourChain.ApproxSimple);
        if (this.forceConvex)
        {
            Point[] ReducedContour_convex;
            ReducedContour_convex = Cv2.ConvexHull(this.ReducedContour);
            //    this.ReducedContour = ReducedContour_convex;
            m = Cv2.Moments(this.ReducedContour);
            this.center.X = (int)(m.M10 / m.M00);
            this.center.Y = (int)(m.M01 / m.M00);
        }
    }
    public void BuildFloodFill()
    {
        //use Cv.floodfill 
        //1. determine the seeding area's Hue
        //2. floodfill only with mask output
        CvMat regionImgHSV = GlobalRepo.GetRepo(RepoDataType.dRawRegionHSV);
        CvMat pRawRegionImgRGBA = GlobalRepo.GetRepo(RepoDataType.dRawRegionRGBA);
        if (BuildingImg == null || BuildingImg.GetSize() != regionImgHSV.GetSize())
        {
            BuildingImg = new CvMat(regionImgHSV.Rows, regionImgHSV.Cols, MatrixType.U8C4);
            BuildingImg.Zero();
        }
        if (maskImg == null || maskImg.GetSize() != regionImgHSV.GetSize())
        {
            maskImg = new CvMat(regionImgHSV.Rows+2, regionImgHSV.Cols+2, MatrixType.U8C1);
            maskImg.Zero();
        }
        dominantCount = 0;
        if (BuilderSeedPointPool.Count <= 0) return;
        CvPoint seedPoint = BuilderSeedPointPool[0];
        for (int i = 0; i < BuilderSeedPointPool.Count; i++)
        {            
            CvScalar hsvColor = regionImgHSV.Get2D(BuilderSeedPointPool[i].Y, BuilderSeedPointPool[i].X);
            if (!dominantHSaturated)
            {
                dominantH += ((float)hsvColor.Val0);
                dominantCount++;
            }
        }
        while (dominantCount< paramHueSaturationSampleCount && BuilderSeedPointPool.Count>0)
        {
            int poolSize = Math.Min(BuilderSeedPointPool.Count, 5);
            for (int i = 0; i < poolSize; i++)
            {
                CvPoint centerP = new CvPoint(BuilderSeedPointPool[i].X, BuilderSeedPointPool[i].Y);
                if (centerP.X >= regionImgHSV.Width - paramWinSize || centerP.X <= paramWinSize || centerP.Y >= regionImgHSV.Height - paramWinSize || centerP.Y <= paramWinSize) continue;
                //HSVRegionImg.GetSubArr(out tempHSVImg, new CvRect(pointPool[i].X - winSize / 2, pointPool[i].Y - winSize / 2, winSize, winSize));            
                CvScalar hsvColor = regionImgHSV.Get2D(BuilderSeedPointPool[i].Y, BuilderSeedPointPool[i].X);
                if (!dominantHSaturated)
                {
                    dominantH += ((float)hsvColor.Val0);
                    dominantCount++;
                }


                for (int j = paramWinSize * -1; j <= paramWinSize; j++)
                {
                    for (int k = paramWinSize * -1; k <= paramWinSize; k++)
                    {
                        if (j == 0 && k == 0) continue;
                        int curY = BuilderSeedPointPool[i].Y + j;
                        int curX = BuilderSeedPointPool[i].X + k;
                        if (BuildingImg.Get2D(curY, curX).Val0 > 0) continue;
                        CvScalar sample = regionImgHSV.Get2D(BuilderSeedPointPool[i].Y + j, BuilderSeedPointPool[i].X + k);
                        //Debug.Log("Refcolor" + hsvColor.Val0 + "\t nearColor" +sample.Val0);
                        if (Math.Abs(hsvColor.Val0 - sample.Val0) < paramHueRangeLow)
                        {
                            BuildingImg.Set2D(curY, curX, pRawRegionImgRGBA.Get2D(curY, curX));
                            BuilderSeedPointPool.Add(new CvPoint(curX, curY));
                        }
                    }
                }

            }
            while (poolSize > 0)
            {
                poolSize--;
                BuilderSeedPointPool.RemoveAt(0);

            }
        }
        dominantH = dominantH / dominantCount;
        dominantHSaturated = true;
        CvConnectedComp cp = new CvConnectedComp();
        //Cv.FloodFill(regionImgHSV, seedPoint, new CvScalar(0, 0, 0), new CvScalar(paramHueRangeLow, 4, 4), new CvScalar(paramHueRangeLow, 4, 4), out cp, FloodFillFlag.MaskOnly, maskImg);
        Cv.FloodFill(regionImgHSV, seedPoint, new CvScalar(0, 0, 0), new CvScalar(paramHueRangeLow, 4, 4), new CvScalar(paramHueRangeLow, 4, 4), out cp, FloodFillFlag.MaskOnly , maskImg);
        CvMat realMask;
        realMask = maskImg.GetSubArr(out realMask, new CvRect(1, 1, BuildingImg.Width, BuildingImg.Height));
        pRawRegionImgRGBA.Copy(BuildingImg, realMask);

        if (GlobalRepo.Setting_ShowDebugImgs()) debugBuildingimg();

        this.Fullcontour = BlobAnalysis.ExtractContour(this.BuildingImg, ContourChain.ApproxNone); //full contour for connectivity check
        if (this.forceConvex)
        {
            //    Point[] Fullcontour_convex;
            //    Fullcontour_convex = Cv2.ConvexHull(this.Fullcontour);
            //      this.Fullcontour = Fullcontour_convex;


        }
        this.bBox = Cv2.BoundingRect(Fullcontour);
        Moments m = Cv2.Moments(this.Fullcontour);
        this.center.X = (int)(m.M10 / m.M00);
        this.center.Y = (int)(m.M01 / m.M00);

        this.ReducedContour = BlobAnalysis.ExtractContour(this.BuildingImg, ContourChain.ApproxSimple);
        if (this.forceConvex)
        {
            Point[] ReducedContour_convex;
            ReducedContour_convex = Cv2.ConvexHull(this.ReducedContour);
            //    this.ReducedContour = ReducedContour_convex;
            m = Cv2.Moments(this.ReducedContour);
            this.center.X = (int)(m.M10 / m.M00);
            this.center.Y = (int)(m.M01 / m.M00);
        }
    }
    public void Build()
    {
        BuildShapeFastFloodFill();
        return;
        CvPoint probePoint;
        CvMat tempHSVImg;

        int debugT = 0;
        int poolSize = Math.Min(BuilderSeedPointPool.Count, paramPixelsperIteration);
        CvMat regionImgHSV = GlobalRepo.GetRepo(RepoDataType.dRawRegionHSV);
        CvMat pRawRegionImgRGBA = GlobalRepo.GetRepo(RepoDataType.dRawRegionRGBA);
        if (BuildingImg == null || BuildingImg.GetSize() != regionImgHSV.GetSize())
        {
            BuildingImg = new CvMat(pRawRegionImgRGBA.Rows, pRawRegionImgRGBA.Cols, MatrixType.U8C4);
            BuildingImg.Zero();

        }
        if (BuilderSeedPointPool.Count == 0)
            return;
        if (dominantCount > paramHueSaturationSampleCount && !dominantHSaturated)
        {   //color saturated
            dominantH = dominantH / dominantCount;
            dominantHSaturated = true;
            dominantCount = 0;
        }
        for (int i = 0; i < poolSize; i++)
        {
            CvPoint centerP = new CvPoint(BuilderSeedPointPool[i].X, BuilderSeedPointPool[i].Y);
            if (centerP.X >= regionImgHSV.Width - paramWinSize || centerP.X <= paramWinSize || centerP.Y >= regionImgHSV.Height - paramWinSize || centerP.Y <= paramWinSize) continue;
            //HSVRegionImg.GetSubArr(out tempHSVImg, new CvRect(pointPool[i].X - winSize / 2, pointPool[i].Y - winSize / 2, winSize, winSize));            
            CvScalar hsvColor = regionImgHSV.Get2D(BuilderSeedPointPool[i].Y, BuilderSeedPointPool[i].X);
            if (!dominantHSaturated)
            {
                dominantH += ((float)hsvColor.Val0);
                dominantCount++;
            }
            //examining 8 corners
            
            

            for (int j = paramWinSize * -1; j <= paramWinSize; j++)
            {
                for (int k = paramWinSize * -1; k <= paramWinSize; k++)
                {
                    if (j == 0 && k == 0) continue;
                    int curY = BuilderSeedPointPool[i].Y + j;
                    int curX = BuilderSeedPointPool[i].X + k;
                    if (BuildingImg.Get2D(curY, curX).Val0 > 0) continue;
                    CvScalar sample = regionImgHSV.Get2D(BuilderSeedPointPool[i].Y + j, BuilderSeedPointPool[i].X + k);
                    //Debug.Log("Refcolor" + hsvColor.Val0 + "\t nearColor" +sample.Val0);

                    if (!dominantHSaturated)
                    {
                        if (Math.Abs(hsvColor.Val0 - sample.Val0) < paramHueRangeLow)
                        {
                            BuildingImg.Set2D(curY, curX, pRawRegionImgRGBA.Get2D(curY, curX));
                            BuilderSeedPointPool.Add(new CvPoint(curX, curY));
                            debugT++;
                        }
                        else
                        {
                            /*   if(isCanvas(hsvColor)&& !isCanvas(sample))
                               {
                                   //re instantiate
                                   BuilderSeedPointPool.Clear();
                                   dominantCount = 0;
                                   dominantHSaturated = false;
                                   dominantH = 0;
                                   BuilderSeedPointPool.Add(new CvPoint(curX, curY));
                                   BuildingImg.Zero();
                                   return;
                               }*/
                        }
                    }
                    else
                    {
                        // Debug.Log("Refcolor" + dominantH + "\t nearColor" + sample.Val0);
                        if (sample.Val1 > paramMinSaturation && Math.Abs(dominantH - sample.Val0) < paramHueRangeLow)
                        {
                            BuildingImg.Set2D(curY, curX, pRawRegionImgRGBA.Get2D(curY, curX));
                            BuilderSeedPointPool.Add(new CvPoint(curX, curY));
                            debugT++;
                        }
                    }

                }


            }
        }

        while (poolSize > 0)
        {
            poolSize--;
            BuilderSeedPointPool.RemoveAt(0);

        }
        if (GlobalRepo.Setting_ShowDebugImgs()) debugBuildingimg();
        if (BuilderSeedPointPool.Count == 0)
        {
            //post-processing
            //might need to update blob centeroid...
            this.Fullcontour = BlobAnalysis.ExtractContour(this.BuildingImg, ContourChain.ApproxNone); //full contour for connectivity check
            if (this.forceConvex)
            {
            //    Point[] Fullcontour_convex;
            //    Fullcontour_convex = Cv2.ConvexHull(this.Fullcontour);
          //      this.Fullcontour = Fullcontour_convex;


            }
            this.bBox = Cv2.BoundingRect(Fullcontour);
            Moments m = Cv2.Moments(this.Fullcontour);
            this.center.X = (int)(m.M10 / m.M00);
            this.center.Y = (int)(m.M01 / m.M00);

            this.ReducedContour = BlobAnalysis.ExtractContour(this.BuildingImg, ContourChain.ApproxSimple);
            if (this.forceConvex)
            {
                Point[] ReducedContour_convex;
                ReducedContour_convex = Cv2.ConvexHull(this.ReducedContour);
            //    this.ReducedContour = ReducedContour_convex;
                m = Cv2.Moments(this.ReducedContour);
                this.center.X = (int)(m.M10 / m.M00);
                this.center.Y = (int)(m.M01 / m.M00);
            }

        }
       

    }
    private bool isCanvas(CvScalar hsv)
    {
        int whitehue = 15;
        if (Math.Abs(hsv.Val0 - whitehue) < 15) return true;
        return false;
    }
    public float getAreaPortioninCanvas()
    {
        float ret = 0;
        if (this.bBox == null) return ret;
        float bboxArea = this.bBox.Size.Height * this.bBox.Size.Width;
        CvSize canvasSize = GlobalRepo.GetRegionBox(false).Size;
        float canvasArea = canvasSize.Height * canvasSize.Width;
        ret = bboxArea / canvasArea;
        return ret;
    }


}
