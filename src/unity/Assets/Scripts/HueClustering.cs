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

public class HueClustering {

    private static CvMat BufferHSVImage=null;
    private static CvMat BufferClusterDataU8 = null;
    private static CvMat BufferClusterDataF32 = null;
    private static CvMat BufferClusterLabel = null;
    private static CvMat[] imgHSVchannels=new CvMat[4];
    private static int paramClusteringIteration = 10;

	public static CvMat HueClustring(CvMat img, int Nclusters)
    {
        
        CvMat RGBImage;
        CvMat HSVImage;
        

        if(BufferHSVImage==null || BufferClusterLabel==null || img.Cols!=BufferHSVImage.Cols || img.Rows != BufferHSVImage.Rows)
        {
            BufferHSVImage = new CvMat(img.Rows, img.Cols, MatrixType.U8C3);
            BufferClusterDataF32 =  new CvMat(img.Rows * img.Cols,1, MatrixType.F32C1);            
            BufferClusterLabel = new CvMat(1, img.Rows * img.Cols, MatrixType.S32C1);
            imgHSVchannels[0]= new CvMat(img.Rows, img.Cols, MatrixType.U8C1);
            imgHSVchannels[1] = new CvMat(img.Rows, img.Cols, MatrixType.U8C1);
            imgHSVchannels[2] = new CvMat(img.Rows, img.Cols, MatrixType.U8C1);
            imgHSVchannels[3] = new CvMat(img.Rows, img.Cols, MatrixType.U8C1);
        }

        img.CvtColor(BufferHSVImage, ColorConversion.RgbaToBgr);
        BufferHSVImage.CvtColor(BufferHSVImage, ColorConversion.BgrToHsv);

        Cv.Split(BufferHSVImage, imgHSVchannels[0], imgHSVchannels[1], imgHSVchannels[2], null);

        //BufferClusterData = new CvMat(img.Rows * img.Cols,1,MatrixType.U8C1, imgHSVchannels[0].Data);
        BufferClusterDataU8 = new CvMat(img.Rows * img.Cols, 1,MatrixType.U8C1, imgHSVchannels[0].Data);
        Cv.Convert(BufferClusterDataU8, BufferClusterDataF32);
        int[] labels = new int[img.Rows * img.Cols];
        
        Cv.KMeans2(BufferClusterDataF32, 4, BufferClusterLabel, new CvTermCriteria(10, 1.0));
        
        //Cv.KMeans2(BufferClusterData.DataArrayByte,MatrixType.U8C1, 4, labels, new CvTermCriteria(10, 1.0));

        
        return BufferClusterLabel;
    }
}
