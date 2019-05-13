using UnityEngine;
using System.Collections;

public class PromptUI : MonoBehaviour {

    public GameObject TestNewBike;
    public GameObject TestNewCamera;
    public static PromptUI ActiveInstance;
    public GameObject[] Bike_DesignIdeas;
    public GameObject[] Camera_DesignIdeas;
    public GameObject[] Analysis_CameraPhotos;
    private float NoHandsLastTIme = -800;
    public GameObject NoHands;
    // Use this for initialization
    private float DesignIdeasGenerated = float.MaxValue;
	void Start () {
        TestNewBike.SetActive(false);
        TestNewCamera.SetActive(false);
        ActiveInstance = this;
        NoHands.SetActive(false);
        foreach (var t in Bike_DesignIdeas)
        {
            t.SetActive(false);
        }
        foreach (var t in Camera_DesignIdeas)
        {
            t.SetActive(false);
        }
        foreach (var t in Analysis_CameraPhotos)
        {
            t.SetActive(false);
        }
        DesignIdeasGenerated = float.MaxValue;
    }
	
	// Update is called once per frame
	void Update () {

        if (GlobalRepo.UserMode != GlobalRepo.UserStep.design)
        {

            foreach (var t in Camera_DesignIdeas)
            {
                t.SetActive(false);
            }
          
            //DesignIdeasGenerated = float.MaxValue;
            NoHands.SetActive(false);
            return;
        }
        if (Time.time - DesignIdeasGenerated>8)
        {
            if (ApplicationControl.ActiveInstance.ContentType == DesignContent.BicycleGearSystem)
            {
                foreach (var t in Bike_DesignIdeas)
                {
                    t.SetActive(false);
                }
                DesignIdeasGenerated = float.MaxValue;
            }
            else if (ApplicationControl.ActiveInstance.ContentType == DesignContent.CameraSystem)
            {
                foreach (var t in Camera_DesignIdeas)
                {
                    t.SetActive(false);
                }
                DesignIdeasGenerated = float.MaxValue;
            }
            NoHands.SetActive(false);
        }
        if (Time.time - NoHandsLastTIme > 3)
        {
            NoHands.SetActive(false);
        }
    }
    public void OnNoHands()
    {
        if (Time.time - NoHandsLastTIme < 600)
        {
            return;
        }
        NoHands.SetActive(true);
        NoHandsLastTIme = Time.time;
    }
    public void OnTestNewBike()
    {
        TestNewBike.SetActive(true);
    }
    public void OnTestNewCamera()
    {
        TestNewCamera.SetActive(true);
    }

    public void onCameraDesignIdeas()
    {
        if (Bike_DesignIdeas == null) return;
        int ScaffoldingStep=WorkSpaceUI.mInstance.GetCurrentStep();
        if (ScaffoldingStep < 0)
        {
            System.Random r = new System.Random();
            int idx = r.Next(0, Camera_DesignIdeas.Length+2);
            if (idx <= Camera_DesignIdeas.Length - 1)
            {
                Camera_DesignIdeas[idx].SetActive(true);
                DesignIdeasGenerated = Time.time; NoHands.SetActive(false); NoHandsLastTIme = Time.time;
            }
        } else
        {
            System.Random r = new System.Random();
            int idx = r.Next(0, 2);
            if (idx > 0)
            {
                Camera_DesignIdeas[ScaffoldingStep].SetActive(true);
                DesignIdeasGenerated = Time.time; NoHands.SetActive(false); NoHandsLastTIme = Time.time;
            }
        }
    }
    public void onBikeDesignIdeas() //this is called by UI
    {
        if (Bike_DesignIdeas == null) return;
        System.Random r = new System.Random();
        int idx = r.Next(0, Bike_DesignIdeas.Length);
        Bike_DesignIdeas[idx].SetActive(true);
        DesignIdeasGenerated = Time.time;
    }

    public void OnAnalysis(PhotoShot l, PhotoShot r, string param)
    {
        if (param == "focallength")
        {
            Analysis_CameraPhotos[0].GetComponent<PromptAnalysis>().LoadPhotos(l, r, param);
            Analysis_CameraPhotos[0].SetActive(true);
        }
        else if (param == "shutterspeed")
        {
            Analysis_CameraPhotos[0].GetComponent<PromptAnalysis>().LoadPhotos(l, r, param);
            Analysis_CameraPhotos[0].SetActive(true);
        }
        else if (param == "sensortype")
        {
            Analysis_CameraPhotos[0].GetComponent<PromptAnalysis>().LoadPhotos(l, r, param);
            Analysis_CameraPhotos[0].SetActive(true);
        }
    }
}
