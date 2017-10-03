using UnityEngine;
using System.Collections;
using System.Text;

public class WebcamToForeground : MonoBehaviour 
{
	[Tooltip("Foreground material applied to the detected users' silhouettes.")]
	public Material foregroundMaterial;
	
	[Tooltip("Selected web-camera name. If left empty, the first available web camera will be selected.")]
	public string webcamName;
	
	[Tooltip("Whether the web-camera output needs to be flipped horizontally or not.")]
	public bool flipHorizontally = false;
	
	// the web-camera texture
	private WebCamTexture webcamTex;
	

	void Start () 
	{
		if(string.IsNullOrEmpty(webcamName))
		{
			// get available webcams
			WebCamDevice[] devices = WebCamTexture.devices;
			
			if(devices != null && devices.Length > 0)
			{
				// print available webcams
				StringBuilder sbWebcams = new StringBuilder();
				sbWebcams.Append("Available webcams:").AppendLine();

				foreach(WebCamDevice device in devices)
				{
					sbWebcams.Append(device.name).AppendLine();
				}

				Debug.Log(sbWebcams.ToString());

				// get the 1st webcam name
				webcamName = devices[0].name;
			}
		}

		// create the texture
		if(!string.IsNullOrEmpty(webcamName))
		{
			webcamTex = new WebCamTexture(webcamName.Trim());
		}
		
		if(foregroundMaterial)
		{
			foregroundMaterial.SetInt("_ColorFlipH", flipHorizontally ? 1 : 0);
			foregroundMaterial.SetTexture("_ColorTex", webcamTex);
		}
		
		if(webcamTex && !string.IsNullOrEmpty(webcamTex.deviceName))
		{
			webcamTex.Play();
		}
	}
	
}
