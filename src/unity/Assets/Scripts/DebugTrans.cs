using UnityEngine;
using System.Collections;
using System.Collections;
using OpenCvSharp;
using OpenCvSharp.Blob;
using OpenCvSharp.CPlusPlus;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
public class DebugTrans {

	public static string ToString(CvPoint p)
    {
        return "(" + p.X + " , " + p.Y + ")";
    }
}
