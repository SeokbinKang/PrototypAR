using UnityEngine;
using System.Collections;

public class CameraControl : MonoBehaviour {

    public int MainCameraFoV;
    public GameObject MainCamera;
    public GameObject ARCamera;
    // Use this for initialization
    void Start () {
        MainCameraFoV = 60;
	}

    // Update is called once per frame
    void Update() {
        adjustFOV();


    }
    private void adjustFOV()
    {
        if (MainCamera == null || MainCamera.GetComponent<Camera>() == null) return;
        Camera mc= MainCamera.GetComponent<Camera>();
        Camera mc2 = ARCamera.GetComponent<Camera>();
        float gap = MainCameraFoV - mc.fieldOfView;
        if (gap == 0) return;
        if (Mathf.Abs(gap) <= 0.4f)
        {
            mc.fieldOfView = MainCameraFoV;
            mc2.fieldOfView = MainCameraFoV;
            return;
        }
        //float adjustment = Mathf.Max(Mathf.Sqrt(Mathf.Abs(gap)) * 0.1f, 0.1f) * Mathf.Sign(gap);        
        float adjustment = 0.35f * Mathf.Sign(gap);
        mc.fieldOfView = mc.fieldOfView + adjustment;
        mc2.fieldOfView = mc2.fieldOfView + adjustment;
    }
}
