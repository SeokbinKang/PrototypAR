using UnityEngine;
using System.Collections;

public class SystemModeControl : MonoBehaviour {
    public GameObject designModeCamera;
    public GameObject Content2_App;
    public GameObject Content4_App;
    public GameObject DesignUIGO;
    public bool BackgroundProcessing = false;
    public float UserActivityThreshold = 0.05f;
    private int warmupFrame = 100;

    private int AvailableFeedbacks =0;
    
    // Use this for initialization
    void Start () {
        warmupFrame = 100;
        
    }
	
	// Update is called once per frame
	void Update () {
        if(warmupFrame--<0 && GlobalRepo.UserMode==GlobalRepo.UserStep.design) CheckRecognition();
       

        //DEBUG

    }
    
    private void CheckRecognition()
    {
        bool need = GlobalRepo.readyForRecognition(UserActivityThreshold);
        if (need)
        {
            //should clean up prototype model, color detector, and BV detector, ..., simulation             
            this.GetComponentInParent<ApplicationControl>().StartLearning();
        
        }

        //logic
        //design mode -> feedback (live stream)
        //design mode -> partial simulation  (live stream)
        //design mode -> full simulation (no live stream) (UserStep becomes simulation)
        //
    }
    private void ResetData()
    {
       
        this.GetComponentInParent<ApplicationControl>().Reset();
    }
    
    
    public void switchToDesign()
    {
        
        switchTo(userAppMode.design);
    }
    public void switchToContent2Race()
    {
        // ResetData();
        this.GetComponentInParent<ApplicationControl>().ResetSimulationData();
        switchTo(userAppMode.content2_race);
    }
    public void switchToContent4Photo()
    {
        //ResetData();
        this.GetComponentInParent<ApplicationControl>().ResetSimulationData();
        switchTo(userAppMode.content4_photography);
    }
    public void switchTo(userAppMode mode)
    {
        if (mode == userAppMode.design)
        {
            GlobalRepo.SetUserPhas(GlobalRepo.UserStep.design);
            designModeCamera.SetActive(true);
            Content2_App.SetActive(false);
            Content4_App.SetActive(false);
        }
        if (mode == userAppMode.content2_race)
        {
            designModeCamera.SetActive(false);
            Content2_App.SetActive(true);
            Content4_App.SetActive(false);
        }
        if (mode == userAppMode.content4_photography)
        {
            GlobalRepo.SetUserPhas(GlobalRepo.UserStep.AppContent4);
            designModeCamera.SetActive(false);
            Content2_App.SetActive(false);
            Content4_App.SetActive(true);
        }
    }

}

public enum userAppMode
{
    design,
    content2_race,
    content4_photography
}
