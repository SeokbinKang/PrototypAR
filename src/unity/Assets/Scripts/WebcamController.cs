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
using System.Runtime.InteropServices;
using Uk.Org.Adcock.Parallel;
public class WebcamController : MonoBehaviour {



    public Transform ColorDetectorInstance = null;

    [Tooltip("GUI-texture used to display the color camera feed on the scene background.")]
    public GUITexture livestreamImage;

    // Use this for initialization

    public int imageWidth = 1600;
    public int imageHeight = 1200;


    public byte[] colorImageBGRA;
    public byte[] colorImageRGBA;
    public byte[] colorImageARTextureFull;
    public byte[] colorImageBGR;

    private int BytesPerPixel = 4;

    private CvCapture m_cvCap=null;
    private string[] testInputImages = { "./Assets/TestInput/BV_1.png", "./Assets/TestInput/connectivity_0.png" };
    private Texture2D AR2DTexture = null;
    private CvMat frameBGRA;
    private CvMat frameBGR;
    WebCamTexture webcamTexture_;

    [DllImport("msvcrt.dll", EntryPoint = "memcpy")]
    public static extern void CopyMemory(IntPtr dest, IntPtr src, uint count);

    private int counter=0;

    private static IplImage BackupImage = null;
    private WebCamTexture webcamTexture = null;
    private Color32[] webcamdata=null;
    private Texture2D liveStreamTexture = null;

    private Texture2D temporaryTexture= null;
    void Start() {

        
        
        /*
        m_cvCap = CvCapture.FromCamera(0);
        
        if (m_cvCap != null) {
            m_cvCap.SetCaptureProperty(CaptureProperty.FrameWidth, 1920);
            m_cvCap.SetCaptureProperty(CaptureProperty.FrameHeight, 1080);
         
            Debug.Log("Camera Initialized W:" + m_cvCap.GetCaptureProperty(CaptureProperty.FrameWidth) + "  H:" + m_cvCap.GetCaptureProperty(CaptureProperty.FrameHeight));
            
        
     }*/
     

        int LengthInPixels = imageWidth * imageHeight;

        this.colorImageBGRA = new byte[BytesPerPixel * LengthInPixels];
        this.colorImageARTextureFull = new byte[BytesPerPixel * LengthInPixels];
        this.frameBGRA = new CvMat(imageHeight, imageWidth, MatrixType.U8C4);
        this.frameBGR = new CvMat(imageHeight, imageWidth, MatrixType.U8C3);
        this.colorImageBGR = new byte[LengthInPixels * 3];

        WebCamDevice[] devices = WebCamTexture.devices;
        String cameraDeviceName = "";
        foreach(var t in devices)
        {
            Debug.Log(t.name);
            if (t.name.IndexOf("IPEVO") >= 0) cameraDeviceName = t.name;
        }
        if (cameraDeviceName == "") return;
        webcamTexture_ = new WebCamTexture(cameraDeviceName,imageWidth, imageHeight, 30);
        //   backgroundImage.texture = webcamTexture;
        //  livestreamImage.texture = webcamTexture_;
        
        
        webcamTexture_.Play();
        webcamdata = new Color32[imageWidth * imageHeight];
        if (!GlobalRepo.isInit()) GlobalRepo.initRepo();
                
    }
	
	// Update is called once per frame
	void Update()
    {
        
        if (counter++ % 1000== 0) Debug.Log("FPS"+1.0f / Time.deltaTime);

        /*
        if (m_cvCap != null && AR2DImageProc.NeedLiveStream()) {
            //Debug.Log("QueryFrame Counter " + (counter++));
            
            m_cvImg = m_cvCap.QueryFrame();
            if (m_cvImg == null) return;
            if (m_cvImg.ElemChannels != 3)
            {
                Debug.Log("[ERROR] Webcam Frame Channels = " + m_cvImg.ElemChannels);
                return;
            }
            int t = imageHeight * imageWidth * 3;
            GlobalRepo.updateRepo(RepoDataType.dRawBGR, m_cvImg.ImageData, m_cvImg.Width, m_cvImg.Height, 3);
            GlobalRepo.updateInternalRepo(true);
            BackupImage = null;
            Debug.Log("Feeding live stream...");
        } else
        {
           
            //frameBGRA = new CvMat(testInputImages[1], LoadMode.Unchanged);
        }
        if(m_cvCap != null && AR2DImageProc.NeedBackupStream())
        {
            if(BackupImage==null)
            {
                m_cvImg = m_cvCap.QueryFrame();
                BackupImage = m_cvImg.Clone();
            }
            GlobalRepo.updateRepo(RepoDataType.dRawBGR, BackupImage.ImageData, BackupImage.Width, BackupImage.Height, 3);
            GlobalRepo.updateInternalRepo(false);
            Debug.Log("Feeding Backup stream.$#%$#%$#%$#%#$%@#$..");
        }
        if (!AR2DImageProc.NeedStream()) return;*/
   //     Debug.Log("Cam update0 , user mode: " + GlobalRepo.UserMode);
        if (webcamTexture_ != null && GlobalRepo.NeedLiveStream())
        {
           
            webcamTexture_.GetPixels32(webcamdata);
            TextureToCvMatUC3(webcamdata, colorImageBGR,webcamTexture_.width, webcamTexture_.height);
            GlobalRepo.updateRepoRaw(RepoDataType.dRawBGR, colorImageBGR, webcamTexture_.width, webcamTexture_.height, 3);
            GlobalRepo.updateInternalRepo(true);
            BackupImage = null;  
        }
        
       // GlobalRepo.tickLearningCount();
        if (webcamTexture_ != null && GlobalRepo.NeedLiveStream())
        {
           
            CvRect regionBox = GlobalRepo.GetRegionBox(false);

            if (liveStreamTexture == null || liveStreamTexture.width != regionBox.Width || liveStreamTexture.height != regionBox.Height)
            {
                liveStreamTexture = new Texture2D(regionBox.Width, regionBox.Height, TextureFormat.RGBA32, false);
            }
            liveStreamTexture.LoadRawTextureData(GlobalRepo.getByteStream(RepoDataType.dRawRegionRGBAByte));

            liveStreamTexture.Apply();
            if (livestreamImage && (livestreamImage.texture == null))
            {

                livestreamImage.texture = liveStreamTexture;
            }
        }
        if( counter % 100 == -1)
        {
            CvRect regionBox = GlobalRepo.GetRegionBox(false);

            if (temporaryTexture == null || temporaryTexture.width != regionBox.Width || temporaryTexture.height != regionBox.Height)
            {
                temporaryTexture = new Texture2D(regionBox.Width, regionBox.Height, TextureFormat.RGBA32, false);
            }
            temporaryTexture.LoadRawTextureData(GlobalRepo.getByteStream(RepoDataType.dRawRegionRGBAByte));

            temporaryTexture.Apply();
            if (temporaryTexture != null)
            {
                GameObject goTestTxt = GameObject.Find("RawImage1");
                UnityEngine.UI.RawImage tempUIRawImage = goTestTxt.GetComponent<UnityEngine.UI.RawImage>();
                tempUIRawImage.texture = temporaryTexture;
                
            }
        }
    }


    void TextureToCvMatUC3(Color32[] _data, byte[] dest,int width,int height)
    {
        // Color32 array : r, g, b, a
        if (dest.Length != _data.Length * 3)
        {
            Debug.Log("[ERROR] size mismatch");
        }

        // Parallel for loop
        // convert Color32 object to Vec3b object
        // Vec3b is the representation of pixel for Mat
        
        int destidx = 0;
        int endofRowidx = -1;
        int widthStep = width * 3;
        /*
                for (var i= 0;i< _data.Length;i++){
                    dest[destidx] = _data[i].b;
                    dest[destidx+1] = _data[i].g;
                    dest[destidx+2] = _data[i].r;
                    destidx += 3;
                    if(i%width ==0)
                        {
                            endofRowidx += widthStep;
                            destidx = endofRowidx;                 
                        }                
                        dest[destidx] = _data[i].r;
                        dest[destidx-1] = _data[i].g;
                        dest[destidx-2] = _data[i].b;
                        destidx -= 3;
            }*/
        //parallel ver.
        Parallel.For(0, height, i => {
            var destidxlocal = (height - i - 1) * widthStep;
            var srcdxlocal = i * width;
            for (var j = 0; j < width; j++)
            {
                
          //      var destidxlocal = ((height - i - 1) * width + j)*3;                                            
                dest[destidxlocal] = _data[srcdxlocal].b;
                dest[destidxlocal + 1] = _data[srcdxlocal].g;
                dest[destidxlocal + 2] = _data[srcdxlocal].r;
                //destidx += 3;
                // set pixel to an array
                //  dest[j + i * width] = vec3;
                destidxlocal += 3;
                srcdxlocal++;
            }
        });

        /*
        for (var i = 0; i < _data.Length; i++)
        {
            dest[destidx] = _data[i].b;
            dest[destidx + 1] = _data[i].g;
            dest[destidx + 2] = _data[i].r;
            destidx += 3;
        }*/
          
    }
    void OnDestroy()
    {
        if (m_cvCap != null)
        {
            m_cvCap.Dispose();
        }
    }
   
    private void updateAR()
    {

       // GlobalRepo.updateRepo(RepoDataType.pRawRGBA, new CvMat(imageHeight, imageWidth , MatrixType.U8C4, colorImageBGRA));
      //  GlobalRepo.updateRepo(RepoDataType.pARTxtRegionRGBA, new CvMat(imageHeight, imageWidth, MatrixType.U8C4, colorImageARTextureFull));
      
    
      //  ImageBlender2D.ApplyBlending(this.colorImageBGRA, this.colorImageARTextureFull, imageWidth, imageHeight);        
        

    }
}
