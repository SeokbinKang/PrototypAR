using UnityEngine;
using System.Collections;
using OpenCvSharp;
using OpenCvSharp.Blob;
using OpenCvSharp.CPlusPlus;
using System.Collections.Generic;
using System.Linq;
public class AnimationManager : MonoBehaviour {



	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	    
	}


    
}
public class Animation2DClip
{
    
    private List<AnimationFrameData> animationSequence2;
    private int sequenceLength;
    private int lastSeqIdx;
    //period ,....

    private static float expandingRatio = 0.18f;
    private static float updownRatio = 0.5f;
    
    public Animation2DClip(Animation2DType aniType, CvMat baseModelImg, int seqLength,Visual2DObject pVisualObj,AnimationParam param)
    {
        
       
     
        if (aniType == Animation2DType.ShapeSuggestion2D)
        {
            CvScalar lineColor = GlobalRepo.GetModelLineColor(pVisualObj.pModelDef.modelType);
            ModelCategory pModelCat = pVisualObj.pModelDef.modelType;
            Point[] suggestedShapeOutline = param.SuggestedShapeOutline;
            if (suggestedShapeOutline == null)
            {
                return;
            }
            CvPoint[][] contours = new CvPoint[1][];
            Asset2DTexture icon_scissor = GlobalRepo.GetTexture2D("icon_scissor");
            if (suggestedShapeOutline != null)
            {
                
                this.animationSequence2 = new List<AnimationFrameData>();
                int idxLastPoint = 0;
                CvPoint margin = new CvPoint(20, 20);
                for (int i = 0; i < seqLength; i++)
                {
                    int frameWidth = (int)((float)baseModelImg.Width * 1.0f)+ margin.X*2;
                    int frameHeight = (int)((float)baseModelImg.Height * 1.0f) + margin.Y*2;
                    CvMat frame = new CvMat(frameHeight, frameWidth, MatrixType.U8C4);
                    //    baseModelImg.Copy(frame);
                    frame.Zero();


                    //actual drawing
                
                    int lineStep = suggestedShapeOutline.Length * (i+1)/ seqLength;
                    contours[0] = new CvPoint[Mathf.Min(lineStep,suggestedShapeOutline.Length)];
                    int j;
                    for (j = 0; j < lineStep && j<suggestedShapeOutline.Length; j++)
                    {
                        
                        contours[0][j] = new CvPoint(suggestedShapeOutline[j].X, suggestedShapeOutline[j].Y)+ margin;
                    }
                    

                    frame.DrawPolyLine(contours, false, lineColor, 2, LineType.AntiAlias);

                    //caculate degree
                    double degree=0;
                    if (j > 1) {
                        float diffX = suggestedShapeOutline[j - 1].X - suggestedShapeOutline[idxLastPoint].X;
                        float diffY = suggestedShapeOutline[j - 1].Y - suggestedShapeOutline[idxLastPoint].Y;
                        if (diffY == 0)
                        {
                            if (diffX >= 0) degree = 0;
                            else degree = Mathf.PI;
                        }
                        else degree = Mathf.Atan2(diffY, diffX);

                    }
                    degree -= Mathf.PI * 0.5f;
                    //CvMat iconImg = icon_scissor.getAnimationFrame((i*3) % icon_scissor.getAnimationLength(), degree* -1f+Mathf.PI);
                    //ImageBlender2D.overlayImgRGBA(frame, iconImg, (CvPoint)suggestedShapeOutline[j - 1] - (CvPoint)icon_scissor.getAnchorPointAbsolute());
                    int dup = param.frameSpeedReverse;
                    while (dup > 0)
                    {
                        CvMat dupFrame = frame.Clone();
                        CvMat iconImg = icon_scissor.getAnimationFrame((animationSequence2.Count) % icon_scissor.getAnimationLength(), degree * -1f + Mathf.PI);
                        ImageBlender2D.overlayImgRGBA(dupFrame, iconImg, (CvPoint)suggestedShapeOutline[j - 1] - (CvPoint)icon_scissor.getAnchorPointAbsolute()+margin);
                        animationSequence2.Add(new AnimationFrameData(dupFrame, pVisualObj.GlobalPosition, pVisualObj.localAnchorPoint, 255, 4));
                        dup--;
                    }

                    idxLastPoint = j - 1;
                }
            }
        }
        if (aniType == Animation2DType.PositionSuggestion2D)
        {
            
            this.animationSequence2 = new List<AnimationFrameData>();
            int marginPixel = 20;
            double frameAlpha = 255;
            double alphaStep = (int)(255 / seqLength);

            CvPoint movingStep = new CvPoint((int)param.direction.x, (int)param.direction.y); //the direction already contains speed factor                
            CvPoint margin = new CvPoint(marginPixel, marginPixel);
            int frameWidth = (int)baseModelImg.Width + marginPixel * 2;
            int frameHeight = (int)baseModelImg.Height + marginPixel * 2;
            CvPoint AdjustedLocalAnchorAbsolute = new CvPoint(baseModelImg.Width * pVisualObj.localAnchorPoint.X / 100 + marginPixel, baseModelImg.Height * pVisualObj.localAnchorPoint.Y / 100 + marginPixel);
            CvPoint AdjustedLocalAnchor;
            AdjustedLocalAnchor.X = AdjustedLocalAnchorAbsolute.X * 100 / frameWidth;
            AdjustedLocalAnchor.Y = AdjustedLocalAnchorAbsolute.Y * 100 / frameHeight;
            CvPoint GlobalPos = pVisualObj.GlobalPosition;
            //Asset2DTexture fingerIcon = GlobalRepo.GetTexture2D("finger_1");
            Asset2DTexture fingerIcon = GlobalRepo.GetTexture2D("icon_finger");
            for (int i = 0; i < seqLength; i++)
            {

                CvMat frame = new CvMat(frameHeight, frameWidth, MatrixType.U8C4);
                CvMat targetRegion;
                frame.Zero();
                Debug.Log("[Creating Animation (" + frame.Width + " , " + frame.Height + ")] BaseModel (" + baseModelImg.Width + " , " + baseModelImg.Height + ")  Margin (" + marginPixel + ")");
                targetRegion = frame.GetSubRect(out targetRegion, new CvRect(marginPixel, marginPixel, baseModelImg.Width, baseModelImg.Height));
                baseModelImg.Copy(targetRegion);
                int iconSeqNum = animationSequence2.Count;
                if (iconSeqNum >= fingerIcon.getAnimationLength()) iconSeqNum = fingerIcon.getAnimationLength() - 1;
                CvMat iconImg = fingerIcon.getAnimationFrame(iconSeqNum, 0);
                if (i < 60)
                {
                  //  double alpha = i * 255 / 10;
                  
                    ImageBlender2D.AlphBlendingImgRGBA(frame, iconImg, (CvPoint)AdjustedLocalAnchorAbsolute - (CvPoint)fingerIcon.getAnchorPointAbsolute() + margin);

                  //  ImageBlender2D.AlphBlendingImgRGBA(frame, fingerIcon.txtBGRAImg, new CvPoint(AdjustedLocalAnchorAbsolute.X - fingerIcon.txtBGRAImg.Width / 2, AdjustedLocalAnchorAbsolute.Y - fingerIcon.txtBGRAImg.Height / 2),alpha);
                    animationSequence2.Add(new AnimationFrameData(frame, GlobalPos, AdjustedLocalAnchor, frameAlpha));                    
                }
                else
                {
                    
                    //ImageBlender2D.overlayImgRGBA(frame, fingerIcon.txtBGRAImg, new CvPoint(AdjustedLocalAnchorAbsolute.X - fingerIcon.txtBGRAImg.Width / 2, AdjustedLocalAnchorAbsolute.Y - fingerIcon.txtBGRAImg.Height / 2));
                    ImageBlender2D.AlphBlendingImgRGBA(frame, iconImg, (CvPoint)AdjustedLocalAnchorAbsolute - (CvPoint)fingerIcon.getAnchorPointAbsolute() + margin);
                    //   animationSequence.Add(frame);
                    animationSequence2.Add(new AnimationFrameData(frame, GlobalPos, AdjustedLocalAnchor, frameAlpha));
                    GlobalPos += movingStep;
                    frameAlpha -= alphaStep;
                }
                

            }

        }
        if (aniType == Animation2DType.ConnectivityMissing2D)
        {
            this.animationSequence2 = new List<AnimationFrameData>();
            if (param.borderPoints.Length < 6) return;
            int nLineSegments = 3;
            int LineSegmentPointCount = param.borderPoints.Length/ nLineSegments;                        

            ArrayList allpoints = new ArrayList();
            CvPoint[][] StitchLines = new CvPoint[nLineSegments][];
            
            Debug.Log("nLines = " + nLineSegments + "\t Length = " + LineSegmentPointCount);
            CvRect regionBox = Cv2.BoundingRect(param.borderPoints);
            regionBox.Inflate(150, 150);
            
            Asset2DTexture penIcon = GlobalRepo.GetTexture2D("icon_pen");
            int totalPoints=0;
            
            for (int i = 0; i < nLineSegments; i++)
            {               
                
                Point[] segment = param.borderPoints.Skip(LineSegmentPointCount*i).Take(LineSegmentPointCount).ToArray<Point>() as Point[];
                Point center = segment[segment.Length / 2];
                CvRect segmentBox = Cv2.BoundingRect(segment);
                center = segmentBox.TopLeft + new CvPoint(segmentBox.Width / 2, segmentBox.Height / 2);
                CVProc.RotatePoints(ref segment, center, Mathf.PI * 45.0f/ 180.0f);
                CVProc.AddjustLenth(ref segment, center, 28);
                CvPoint[] segment30 = new CvPoint[segment.Length];
                int idx = 0;
                for (int j =0; j < segment.Length; j++)
                {
                    segment30[idx++] = (CvPoint)segment[j]-regionBox.TopLeft;
                //    allpoints.Add(segment[j]);
                }
                StitchLines[i] = segment30;
                totalPoints += segment30.Length;
                
            }         
            
            CvPoint globalPosition = regionBox.TopLeft;
            CvPoint localAnchor = new CvPoint(0, 0);
            CvScalar stitchColor = new CvScalar(50, 50, 50, 200);
            
            double PoinstPerFrame = ((double)totalPoints) / ((double)seqLength);
            double accumulatedPointsN = 0;
            // CvPoint penAbsoluteAnchor = new CvPoint(penIcon.txtBGRAImg.Width * penIcon.AnchorPointRelative.X / 100, penIcon.txtBGRAImg.Height * penIcon.AnchorPointRelative.Y / 100);
            CvPoint penAbsoluteAnchor = penIcon.getAnchorPointAbsolute();
            for (int i = 0; i < seqLength; i++)
            {
                
                CvMat frame = new CvMat(regionBox.Width, regionBox.Height, MatrixType.U8C4);                
                frame.Zero();
                ArrayList frameLine = new ArrayList(); ;
                accumulatedPointsN += PoinstPerFrame;
                double addedPointsN = 0;
                CvPoint lastPoint = new CvPoint(0, 0);
                foreach (var l in StitchLines)
                {
                    ArrayList frameLineSegment = new ArrayList();
                    foreach (var p in l)
                    {
                        frame.DrawCircle(p, 1, stitchColor);
                        frameLineSegment.Add(p);
                        lastPoint = p;
                        if (++addedPointsN >= accumulatedPointsN) break;
                    }
                    frameLine.Add(frameLineSegment.ToArray(typeof(CvPoint)) as CvPoint[]);
                    if (++addedPointsN >= accumulatedPointsN) break;
                }
                CvPoint[][] frameLinePoints = frameLine.ToArray(typeof(CvPoint[])) as CvPoint[][];
                frame.DrawPolyLine(frameLinePoints, false, stitchColor, 6, LineType.AntiAlias);
                //frame.DrawPolyLine(StitchLines, false, stitchColor, 6, LineType.AntiAlias);
                
                CvPoint penLeftTopinFrame = lastPoint - penAbsoluteAnchor;
                ImageBlender2D.overlayImgFrameAlphaImgRGBA(frame, penIcon.getAnimationFrame(animationSequence2.Count % penIcon.getAnimationLength(),0), penLeftTopinFrame, 255);

                animationSequence2.Add(new AnimationFrameData(frame, globalPosition, localAnchor, 220));
                Debug.Log("RegionBox " + regionBox.X + " " + regionBox.Y);

            }
            
            }
        if (aniType == Animation2DType.ConnectivityIncorrect2D)
        {
            this.animationSequence2 = new List<AnimationFrameData>();
            CvRect markerRegion=new CvRect(0,0,0,0);
            Asset2DTexture penIcon = GlobalRepo.GetTexture2D("icon_eraser");

            //merge region            

            foreach (UserDescriptionInfo Sign in param.userDescGroup)
            {
                if (markerRegion.Width == 0) markerRegion = Sign.boundingBox;
                    else markerRegion = markerRegion.Union(Sign.boundingBox);
                
            }
            int marginPixel = 90;
            markerRegion.Inflate(marginPixel*2, marginPixel*2);
            markerRegion.Y = markerRegion.Y - marginPixel;
            CvMat markerRegionImg = GlobalRepo.GetRepo(RepoDataType.dRawRegionBGR).GetSubArr(out markerRegionImg, markerRegion);
            
            CvPoint globalPosition = markerRegion.TopLeft;
            CvPoint localAnchor = new CvPoint(0, 0);
            CvSeq<CvPoint>[] targetSignContour = new CvSeq<CvPoint>[param.userDescGroup.Count];
            CvPoint[][] targetSignContourArray = new CvPoint[param.userDescGroup.Count][];
            //preprocessing: adjust signs' coordinates to region frame
            CvMemStorage storage = new CvMemStorage();
            int idx = 0;
            int totalPoints = 0;
            foreach (UserDescriptionInfo Sign in param.userDescGroup)
            {
                CvPoint[] adjustedContour = (CvPoint[])Sign.contourPoints.Clone();
                CvPoint cm = Sign.center - globalPosition;
                int inflatePixel = 2;
                for(int i = 0; i < adjustedContour.Length; i++)
                {
                    adjustedContour[i] -= globalPosition;
                    if (adjustedContour[i].X < cm.X) adjustedContour[i].X -= inflatePixel;
                        else if (adjustedContour[i].X > cm.X) adjustedContour[i].X += inflatePixel;
                    if (adjustedContour[i].Y < cm.Y) adjustedContour[i].Y -= inflatePixel;
                    else if (adjustedContour[i].Y > cm.Y) adjustedContour[i].Y += inflatePixel;
                }
                targetSignContourArray[idx] = adjustedContour;
                targetSignContour[idx++] = CvSeq<CvPoint>.FromArray(adjustedContour, SeqType.Contour, storage);
                totalPoints += adjustedContour.Length;
            }

            CvScalar fillColor = new CvScalar(255, 255, 255, 255);
            /*
            for (int i = 0; i < seqLength; i++)
            {
                CvMat frame = new CvMat(markerRegion.Width, markerRegion.Height, MatrixType.U8C4);
                frame.Zero();
                for (int k = 0; k < targetSignContour.GetLength(0); k++)
                {

                    //frame.DrawContours(targetSignContour[k], fillColor, fillColor,0);
                    frame.FillConvexPoly(targetSignContourArray[k], fillColor, LineType.AntiAlias);

                }
                animationSequence2.Add(new AnimationFrameData(frame, globalPosition, localAnchor, 200));
                if (i == 0) GlobalRepo.showDebugImage("RegionToEliminateMarkers", frame);
                //adjust marker's coordinates to 
            }*/
            double PoinstPerFrame = ((double)totalPoints) / (((double)seqLength)*0.8f);
            double accumulatedPointsN = 0;
            CvPoint penAbsoluteAnchor = penIcon.getAnchorPointAbsolute();
            for (int i = 0; i < seqLength; i++)
            {

                CvMat frame = new CvMat(markerRegion.Width, markerRegion.Height, MatrixType.U8C4);
                frame.Zero();
                ArrayList frameLine = new ArrayList(); ;
                accumulatedPointsN += PoinstPerFrame;
                double addedPointsN = 0;
                CvPoint lastPoint = new CvPoint(0, 0);
                for (int k = 0; k < targetSignContourArray.GetLength(0); k++)
                {
                    ArrayList frameLineSegment = new ArrayList();
                    foreach (var p in targetSignContourArray[k])
                    {                        
                        frameLineSegment.Add(p);
                        lastPoint = p;
                        if (++addedPointsN >= accumulatedPointsN) break;
                    }
                    frameLine.Add(frameLineSegment.ToArray(typeof(CvPoint)) as CvPoint[]);
                    if (++addedPointsN >= accumulatedPointsN) break;
                }
                CvPoint[][] frameLinePoints = frameLine.ToArray(typeof(CvPoint[])) as CvPoint[][];
                for(int k=0;k<frameLinePoints.GetLength(0);k++)
                    frame.FillConvexPoly(frameLinePoints[k], fillColor, LineType.AntiAlias);
                //frame.DrawPolyLine(frameLinePoints, false, fill, 6, LineType.AntiAlias);
                //frame.DrawPolyLine(StitchLines, false, stitchColor, 6, LineType.AntiAlias);
               
                CvPoint penLeftTopinFrame = lastPoint - penAbsoluteAnchor;
                ImageBlender2D.overlayImgFrameAlphaImgRGBA(frame, penIcon.getAnimationFrame(animationSequence2.Count % penIcon.getAnimationLength(), 0), penLeftTopinFrame, 255);
                
             

                animationSequence2.Add(new AnimationFrameData(frame, globalPosition, localAnchor, 200));                

            }

            storage.Dispose();
            /*
            CvMat fullRegion = GlobalRepo.GetRepo(RepoDataType.dRawRegionBGR).Clone();
            CvMat maskingImg = new CvMat(markerRegion.Width, markerRegion.Height, MatrixType.U8C4);
            maskingImg.Zero();
            for (int k = 0; k < targetSignContour.GetLength(0); k++)
            {
                //frame.DrawContours(targetSignContour[k], fillColor, fillColor,0);
                maskingImg.FillConvexPoly(targetSignContourArray[k], fillColor, LineType.AntiAlias);
            }
            CvMat testImg = markerRegionImg.Clone();
            foreach(Point p in param.borderPoints)
            {
                CvPoint padjusted = (CvPoint)p - globalPosition;
                testImg.DrawCircle(padjusted, 1, CvColor.Red);
                fullRegion.DrawCircle(p, 1, CvColor.Red);
            }
            GlobalRepo.showDebugImage("bordertest", testImg);
            GlobalRepo.showDebugImage("bordertest2", fullRegion);*/


        }
        if (aniType == Animation2DType.BehaviorMissing2D)
        {
            
            ModelCategory pModelCat = pVisualObj.pModelDef.modelType;
            Point[] suggestedShapeOutline = param.SuggestedShapeOutline;
            Asset2DTexture textMsgIcon = GlobalRepo.GetTexture2D("text_missingbehavior1");
            CvPoint[] linePoint =new CvPoint[2];
            linePoint[0]= param.AnchorPoint;
            float lineLength = 100;

            CvRect globalRegionBox = GlobalRepo.GetRegionBox(false);
            Vector2 directionVector = new Vector2(linePoint[0].X - ( globalRegionBox.Width / 2), linePoint[0].Y - (globalRegionBox.Height / 2));
            directionVector.Normalize();
            
            linePoint[1] = linePoint[0];
            linePoint[1].X += (int)( directionVector.x * lineLength);
            linePoint[1].Y += (int)(directionVector.y * lineLength);

            
            int UpDownMargin = 20;            
            int verticalDirection = (int)Mathf.Sign(directionVector.y);
            if (verticalDirection == 0) verticalDirection = -1;
            CvRect msgBox = new CvRect(linePoint[1].X - textMsgIcon.txtBGRAImg.Width / 2, linePoint[1].Y + textMsgIcon.txtBGRAImg.Height * verticalDirection, textMsgIcon.txtBGRAImg.Width, textMsgIcon.txtBGRAImg.Height+UpDownMargin);

            CvRect overlayRegion = Cv.BoundingRect(linePoint);

            overlayRegion=overlayRegion.Union(msgBox);
            overlayRegion.Inflate(4, 40);
            CvPoint msgLocalTopLeft = msgBox.TopLeft - overlayRegion.TopLeft;
            CvPoint LocalRelativeAnchor = CVProc.CalculateRelativePosinBox(linePoint[0], overlayRegion);
            CvPoint GlobalPosition = linePoint[0];
            this.animationSequence2 = new List<AnimationFrameData>();
            CvScalar lineColor = new CvScalar(50, 50, 50, 200);
         //   Debug.Log("line 0:" + linePoint[0].X + "," + linePoint[0].Y + " line 1:" + linePoint[1].X + "," + linePoint[1].Y + " msgLocalTopleft" + msgLocalTopLeft.X + "," + msgLocalTopLeft.Y+ "LocalRelativeAnchor("+ LocalRelativeAnchor.X+","+ LocalRelativeAnchor.Y+")");
            for (int i = 0; i < seqLength; i++)
            {
                int frameWidth = overlayRegion.Width;
                int frameHeight = overlayRegion.Height;
                CvMat frame = new CvMat(frameHeight, frameWidth, MatrixType.U8C4);
                frame.Zero();
                frame.DrawCircle(linePoint[0] - overlayRegion.TopLeft, 5, lineColor, -1,LineType.AntiAlias);
                frame.DrawLine(linePoint[0]-overlayRegion.TopLeft, linePoint[1]- overlayRegion.TopLeft, lineColor, 4, LineType.AntiAlias);               

                CvMat msgFrame = frame.GetSubArr(out msgFrame, new CvRect(msgLocalTopLeft, textMsgIcon.txtBGRAImg.GetSize()));
                ImageBlender2D.overlayImgFrameAlphaImgRGBA(frame, textMsgIcon.txtBGRAImg, msgLocalTopLeft, 200);

                animationSequence2.Add(new AnimationFrameData(frame, GlobalPosition, LocalRelativeAnchor, 200));
                if (i < seqLength / 2)
                {
                    msgLocalTopLeft.Y = msgLocalTopLeft.Y - 1;
                }
                else msgLocalTopLeft.Y = msgLocalTopLeft.Y + 1;
            }

        }
        if (aniType == Animation2DType.BehaviorTakeOut2D)
        {

            Asset2DTexture IncorrectIcon = GlobalRepo.GetTexture2D("icon_incorrect1");            
            CvPoint GlobalPosition = param.AnchorPoint;
            CvPoint LocalRelativeAnchor = IncorrectIcon.AnchorPointRelative;
            
            


            int UpDownMargin = 20;
            

            CvRect objectLocalbBox = new CvRect(0, 0, IncorrectIcon.txtBGRAImg.Width, IncorrectIcon.txtBGRAImg.Height);

            int frameWidth = objectLocalbBox.Width;
            int frameHeight = objectLocalbBox.Height; ;

            this.animationSequence2 = new List<AnimationFrameData>();
            CvScalar lineColor = new CvScalar(50, 50, 50, 200);
            float oscilatingRadialVelocity = 2.0f * Mathf.PI / ((float)seqLength);
            float oscilatingPhase = 0;
            float alpha;
            for (int i = 0; i < seqLength; i++)
            {
                alpha = 255.0f * Mathf.Sin(oscilatingPhase);
                oscilatingPhase += oscilatingRadialVelocity;
                CvMat frame = new CvMat(frameHeight, frameWidth, MatrixType.U8C4);
                frame.Zero();                                
                ImageBlender2D.overlayImgFrameAlphaImgRGBA(frame, IncorrectIcon.txtBGRAImg, objectLocalbBox.TopLeft, alpha);

                animationSequence2.Add(new AnimationFrameData(frame, GlobalPosition, LocalRelativeAnchor, 200));                              
            }

        }
        if (aniType == Animation2DType.BehaviorRelocate2D)
        {

            
            CvPoint GlobalPosition = param.AnchorPoint;
            CvPoint LocalRelativeAnchor;
            CvPoint[] GlobalCurvePoints = new CvPoint[2];
            GlobalCurvePoints[0] = param.AnchorPoint;
            GlobalCurvePoints[1] = param.destPoint;
            float circleRadius = 50;


            int UpDownMargin = 20;


            CvRect frameBox = Cv.BoundingRect(GlobalCurvePoints);
            frameBox.Inflate((int)circleRadius * 2, (int)circleRadius * 2);
            CvPoint[] LocalCurvePoints = GlobalCurvePoints.Clone() as CvPoint[];
            for (int i = 0; i < LocalCurvePoints.Length; i++)
                LocalCurvePoints[i] = LocalCurvePoints[i] - frameBox.TopLeft;
            int frameWidth = frameBox.Width;
            int frameHeight = frameBox.Height; ;

            this.animationSequence2 = new List<AnimationFrameData>();
            CvScalar lineColor = new CvScalar(200, 15, 15, 200);

            float oscilatingRadialVelocity = 2.0f * Mathf.PI / ((float)(seqLength/2));
            float oscilatingPhase = 0;
            float alpha;
            CvPoint currentSegment=new CvPoint(0,0);
            CvMat frame = new CvMat(frameHeight, frameWidth, MatrixType.U8C4);
            frame.Zero(); //stack up frames
            LocalRelativeAnchor = CVProc.CalculateRelativePosinBox(GlobalCurvePoints[0], frameBox);
            CvPoint[] linePointsArray = null;

            Vector2 lineDirection = new Vector2((GlobalCurvePoints[1] - GlobalCurvePoints[0]).X, (GlobalCurvePoints[1] - GlobalCurvePoints[0]).Y);
            oscilatingPhase = Mathf.Atan2(lineDirection.y, lineDirection.x);
            lineDirection.Normalize();
            alpha = 30;
            double linePointStep = 0;
            double lineDrawnIndex = 0;
            double minDistancefromCircletoDest = double.MaxValue;
            CvPoint closestCirclePoint = LocalCurvePoints[0];
            for (int i = 0; i < seqLength; i++)
            {
                
                if (i<seqLength/2)
                {//draw circle
                    for(float theta = oscilatingPhase;theta< oscilatingPhase + oscilatingRadialVelocity;theta+=0.25f )
                    {
                        currentSegment.X =(int)((float) LocalCurvePoints[0].X + ( circleRadius * Mathf.Sin(theta)));
                        currentSegment.Y = (int)((float)LocalCurvePoints[0].Y + (circleRadius * Mathf.Cos(theta)));
                        frame.DrawCircle(currentSegment, 3, lineColor,-1);
                        double distToDest = currentSegment.DistanceTo(LocalCurvePoints[1]);
                        if (distToDest < minDistancefromCircletoDest)
                        {
                            minDistancefromCircletoDest = distToDest;
                            closestCirclePoint = currentSegment;
                        }
                    }

                    oscilatingPhase += oscilatingRadialVelocity;
                    alpha += + 170.0f/((float)seqLength/2);
                    if (alpha > 255) alpha = 255;
                } else
                {//draw line move
                    if(linePointsArray==null)
                    {//create line points    
                        lineDirection = new Vector2((LocalCurvePoints[1] - closestCirclePoint).X, (LocalCurvePoints[1] - closestCirclePoint).Y);
                        lineDirection.Normalize();
                        lineDirection = lineDirection * 8.0f;
                        ArrayList tmpPoints = new ArrayList();      
                        Vector2 curPoint = new Vector2(closestCirclePoint.X, closestCirclePoint.Y);
                        CvPoint p = new CvPoint(40000, 40000);
                        int q = 0;
                        while (LocalCurvePoints[1].DistanceTo(p) > 10)
                        {
                            p.X = (int)curPoint.x;
                            p.Y = (int)curPoint.y;
                            tmpPoints.Add(p);
                            curPoint += lineDirection;                            
                        }
                        linePointsArray = tmpPoints.ToArray(typeof(CvPoint)) as CvPoint[];
                        linePointStep = ((double)(linePointsArray.Length)) / ((double) (seqLength / 2));
                        lineDrawnIndex = 0;
                    }
                    for(int k = (int) lineDrawnIndex; k < lineDrawnIndex + (int) linePointStep && k<linePointsArray.Length; k++)
                    {
                        frame.DrawCircle(linePointsArray[k], 3, lineColor, -1);
                    }
                    lineDrawnIndex += linePointStep;
                  //  Debug.Log("[Debug] Line Drawing " + lineDrawnIndex + "\t" + linePointsArray);
                 //   double dist = currentSegment.DistanceTo(LocalCurvePoints[1]);
                 //    frame.DrawLine(currentSegment, LocalCurvePoints[1], lineColor, 4);                        
                    if(i==seqLength-1)
                    {
                        for (int k = (int) lineDrawnIndex; k < linePointsArray.Length; k++)
                        {
                            frame.DrawCircle(linePointsArray[k], 3, lineColor, -1);
                        }
                        
                    }
              //      Debug.Log("[Debug] Line Drawing " + lineDrawnIndex + "\t" + linePointsArray);
                }               
                
                
                //ImageBlender2D.overlayImgFrameAlphaImgRGBA(frame, IncorrectIcon.txtBGRAImg, objectLocalbBox.TopLeft, alpha);

                animationSequence2.Add(new AnimationFrameData(frame.Clone(), GlobalPosition, LocalRelativeAnchor, alpha));
            }

        }
        lastSeqIdx = 0;
        sequenceLength = animationSequence2.Count;
        Debug.Log("Animation Created!");

    }
    


    /*
    public Animation2DClip(Animation2DType aniType, CvMat baseImg, int seqLength)
    {
        this.animationSequence = new List<CvMat>();
        if(aniType==Animation2DType.Expanding)
        {
            float expansionStep = expandingRatio / ((float)seqLength / 2);
            float curExpRatio = 1.0f;
            for (int i = 0; i < seqLength/2; i++)
            {
                int frameWidth = (int)((float)baseImg.Width * curExpRatio);
                int frameHeight = (int)((float)baseImg.Height * curExpRatio);
                CvMat frame = new CvMat(frameHeight, frameWidth, MatrixType.U8C4);
                Cv.Resize(baseImg, frame);
                curExpRatio += expansionStep;
                animationSequence.Add(frame);
            }
            for (int i = 0; i < seqLength / 2; i++)
            {
                int frameWidth = (int)((float)baseImg.Width * curExpRatio);
                int frameHeight = (int)((float)baseImg.Height * curExpRatio);
                CvMat frame = new CvMat(frameHeight, frameWidth, MatrixType.U8C4);
                Cv.Resize(baseImg, frame);
                curExpRatio -= expansionStep;
                animationSequence.Add(frame);
            }

        }
        if (aniType == Animation2DType.UpDown)
        {
            int movingStep =(int)( ((float)baseImg.Height * updownRatio) / ((float)seqLength / 2));
            int curOffset = 0;
            int frameWidth = (int)baseImg.Width;
            int frameHeight = (int)baseImg.Height+ movingStep * seqLength / 2;
            for (int i = 0; i < seqLength / 2; i++)
            {
                
                CvMat frame = new CvMat(frameHeight, frameWidth, MatrixType.U8C4);
                CvMat targetRegion;
                frame.Zero();
                targetRegion = frame.GetSubRect(out targetRegion, new CvRect(0, curOffset, frameWidth, baseImg.Height));
                baseImg.Copy(targetRegion);
                curOffset += movingStep;
                animationSequence.Add(frame);
            }
            for (int i = 0; i < seqLength / 2; i++)
            {
                CvMat frame = new CvMat(frameHeight, frameWidth, MatrixType.U8C4);                
                CvMat targetRegion;
                frame.Zero();
                targetRegion = frame.GetSubRect(out targetRegion, new CvRect(0, curOffset, frameWidth, baseImg.Height));
                
                baseImg.Copy(targetRegion);
                curOffset -= movingStep;
                animationSequence.Add(frame);
            }

        }
        lastSeqIdx = 0;
        sequenceLength = seqLength;

    }*/
    /*
    public CvMat getCurrentFrame()
    {
        if (animationSequence == null || lastSeqIdx > animationSequence.Count - 1) return null;
        CvMat frame = animationSequence[lastSeqIdx];
        lastSeqIdx++;
        if (lastSeqIdx > sequenceLength - 1) lastSeqIdx = 0;
        return frame;
    }*/
    public AnimationFrameData getCurrentFrameData()
    {
        if (animationSequence2 == null || animationSequence2.Count == 0 ) return null;
        // if (animationSequence2 == null || lastSeqIdx > animationSequence2.Count - 1) return null;
        AnimationFrameData fd = animationSequence2[lastSeqIdx % animationSequence2.Count];
        lastSeqIdx++;
        //if (lastSeqIdx > sequenceLength - 1) lastSeqIdx = 0;
        return fd;
    }
    private void ContextAwareFill(ref CvMat fillImage, ref CvMat refImage)
    {
        for(int x=0;x<fillImage.Width;x++)
        {
            for(int y=0;y<fillImage.Height;y++)
            {

            }
        }
    }
}

public enum Animation2DType
{
    None,
    ShapeSuggestion2D,
    PositionSuggestion2D,
    ConnectivityMissing2D,
    ConnectivityIncorrect2D,
    BehaviorMissing2D,
    BehaviorTakeOut2D,
    BehaviorRelocate2D,
    Expanding,
    UpDown
}
public class AnimationParam
{
    public Vector3 direction ;
    public Point[] SuggestedShapeOutline=null;
    public CvPoint AnchorPoint;
    public CvMat ImagePatch;
    public Point[] borderPoints = null;
    public List<UserDescriptionInfo> userDescGroup = null;
    public CvPoint destPoint;
    public int frameSpeedReverse=1;
    
    

}
public class AnimationFrameData
{
    public CvMat frameImage;
    public CvPoint GlobalAnchor;
    public CvPoint LocalAnchor;
    public double frameAlpha;
    public int frameLength;
    public AnimationFrameData(CvMat frameMat, CvPoint GlobalAnchor_, CvPoint LocalAnchor_)
    {
        frameImage = frameMat;
        GlobalAnchor = GlobalAnchor_;
        LocalAnchor = LocalAnchor_;
        frameLength = 1;
    }
    public AnimationFrameData(CvMat frameMat, CvPoint GlobalAnchor_, CvPoint LocalAnchor_,double alpha)
    {
        frameImage = frameMat;
        GlobalAnchor = GlobalAnchor_;
        LocalAnchor = LocalAnchor_;
        frameAlpha = alpha;
        frameLength = 1;
    }
    public AnimationFrameData(CvMat frameMat, CvPoint GlobalAnchor_, CvPoint LocalAnchor_, double alpha, int frameLength_)
    {
        frameImage = frameMat;
        GlobalAnchor = GlobalAnchor_;
        LocalAnchor = LocalAnchor_;
        frameAlpha = alpha;
        frameLength = frameLength_;
    }
}