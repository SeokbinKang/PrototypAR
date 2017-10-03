using UnityEngine;
using System.Collections;
//using Windows.Kinect;


public class JointOverlayer : MonoBehaviour 
{
	[Tooltip("GUI-texture used to display the color camera feed on the scene background.")]
	public GUITexture backgroundImage;

	[Tooltip("Camera that will be used to overlay the 3D-objects over the background.")]
	public Camera foregroundCamera;
	
	[Tooltip("Index of the player, tracked by this component. 0 means the 1st player, 1 - the 2nd one, 2 - the 3rd one, etc.")]
	public int playerIndex = 0;
	
	[Tooltip("Kinect joint that is going to be overlayed.")]
	public KinectInterop.JointType trackedJoint = KinectInterop.JointType.HandRight;

	[Tooltip("Game object used to overlay the joint.")]
	public Transform overlayObject;
    //public float smoothFactor = 10f;

    //public GUIText debugText;

    public Transform ColorDetectorInstance = null;

	private Quaternion initialRotation = Quaternion.identity;

    private Texture2D AR2DTexture =null;
	
	void Start()
	{
		if(overlayObject)
		{
			// always mirrored
			initialRotation = Quaternion.Euler(new Vector3(0f, 180f, 0f));
			overlayObject.rotation = Quaternion.identity;
		}
        
    }
	
	void Update () 
	{
		KinectManager manager = KinectManager.Instance;
        updateAR();
        if (manager && manager.IsInitialized() && foregroundCamera)
		{
			//backgroundImage.renderer.material.mainTexture = manager.GetUsersClrTex();
			
			if(AR2DTexture == null)
            {
                AR2DTexture = new Texture2D(manager.GetSensorData().colorImageWidth, manager.GetSensorData().colorImageHeight, TextureFormat.RGBA32, false);
            }
            AR2DTexture.LoadRawTextureData(manager.GetSensorData().colorImagePostProc);
            AR2DTexture.Apply();
            if (backgroundImage && (backgroundImage.texture == null))
            {
                //backgroundImage.texture = manager.GetUsersClrTex();
                backgroundImage.texture = AR2DTexture;
            }
            

			// get the background rectangle (use the portrait background, if available)
			Rect backgroundRect = foregroundCamera.pixelRect;
			PortraitBackground portraitBack = PortraitBackground.Instance;
			
			if(portraitBack && portraitBack.enabled)
			{
				backgroundRect = portraitBack.GetBackgroundRect();
			}

			// overlay the joint
			int iJointIndex = (int)trackedJoint;

			if(manager.IsUserDetected())
			{
				long userId = manager.GetUserIdByIndex(playerIndex);
				
				if(manager.IsJointTracked(userId, iJointIndex))
				{
					Vector3 posJoint = manager.GetJointPosColorOverlay(userId, iJointIndex, foregroundCamera, backgroundRect);

					if(posJoint != Vector3.zero)
					{
//						if(debugText)
//						{
//							debugText.GetComponent<GUIText>().text = string.Format("{0} - {1}", trackedJoint, posJoint);
//						}

						if(overlayObject)
						{
							overlayObject.position = posJoint;

							Quaternion rotJoint = manager.GetJointOrientation(userId, iJointIndex, false);
							rotJoint = initialRotation * rotJoint;

							overlayObject.rotation = rotJoint;
						}
					}
				}
				
			}
			
		}

        
	}

    private void updateAR()
    {
        KinectManager manager = KinectManager.Instance;
        KinectInterop.SensorData sensor = manager.GetSensorData();
        if (manager && manager.IsInitialized() && ColorDetectorInstance)
        {
            
            ColorDetector pDetector = ColorDetectorInstance.GetComponent<ColorDetector>();
            //pDetector.UpdateColorImage(sensor.colorImage, sensor.colorImageWidth, sensor.colorImageHeight);
            pDetector.UpdateColorImage(sensor.colorImageBGRA, sensor.colorImagePostProc, sensor.colorImageWidth, sensor.colorImageHeight);
            ImageBlender2D.ApplyBlending_OBSOLTE(sensor.colorImageBGRA,sensor.colorImagePostProc, sensor.colorImageWidth, sensor.colorImageHeight);
        }
            
    }
}
