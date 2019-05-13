using UnityEngine;
using System.Collections;

public class SystemModeControl : MonoBehaviour {
    public GameObject designModeCamera;
    public GameObject Content2;
    public GameObject Content2_Test;
    public GameObject Content4;
    public GameObject Content4_Test;
    public GameObject DesignUIGO;
    public bool BackgroundProcessing = false;
    public float UserActivityThreshold = 0.05f;
    private int warmupFrame = 100;

    private int AvailableFeedbacks =0;
    private float TimeLastActivityChecked = 0;
    // Use this for initialization
    void Start () {
        warmupFrame = 100;
        
    }
	
	// Update is called once per frame
	void Update () {
        if(warmupFrame--<0 && GlobalRepo.UserMode==GlobalRepo.UserStep.design) CheckRecognition();
       

        //DEBUG

    }
    public void HaltRecognitionAfterScreenInteraction()
    {
        float halttime = 10;
        TimeLastActivityChecked = Time.time + halttime;
        GlobalRepo.PingRecognitionDone();
    }
    public void HaltRecognition6sec()
    {
        float halttime = 6;
        TimeLastActivityChecked = Time.time + halttime;
        GlobalRepo.PingRecognitionDone();
    }
    private void CheckRecognition()
    {
        float minimalTimeInterval = 0.5f;
        if(Time.time - TimeLastActivityChecked<=minimalTimeInterval)
        {
            return;
        }
        TimeLastActivityChecked = Time.time;
        bool need = GlobalRepo.readyForRecognition(SystemConfig.ActiveInstnace.Get(CVConfigItem._User_ActionSensitivity));
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
        if(ApplicationControl.ActiveInstance.ContentType==DesignContent.BicycleGearSystem)
        {
            
            switchTo(userAppMode.content2_design);
        } else if (ApplicationControl.ActiveInstance.ContentType == DesignContent.CameraSystem)
        {
            
            switchTo(userAppMode.content4_design);
        }

    }
    
    public void switchToContent2Race()
    {
        // ResetData();
        PrototypeInstanceManager.mActiveInstance.SaveandSelectLastOne();
        this.GetComponentInParent<ApplicationControl>().ResetSimulationData();
        this.GetComponentInParent<ApplicationControl>().ResetFeedback();
        switchTo(userAppMode.content2_race);
    }
    public void switchToContent4Photo()
    {
        //ResetData();
        PrototypeInstanceManager.mActiveInstance.SaveandSelectLastOne();
        this.GetComponentInParent<ApplicationControl>().ResetSimulationData();
        this.GetComponentInParent<ApplicationControl>().ResetFeedback();
        switchTo(userAppMode.content4_photography);
    }
    public void switchTo(userAppMode mode)
    {
        if (mode == userAppMode.design)
        {
            GlobalRepo.SetUserPhas(GlobalRepo.UserStep.design);
            designModeCamera.SetActive(true);
            Content2.SetActive(false);
            Content2_Test.SetActive(false);
            Content4.SetActive(false);
            Content4_Test.SetActive(false);
            
        } else if (mode == userAppMode.content2_design)
        {
            GlobalRepo.SetUserPhas(GlobalRepo.UserStep.design);
            designModeCamera.SetActive(true);
            Content2.SetActive(true);
            Content2_Test.SetActive(false);
            Content4.SetActive(false);
            Content4_Test.SetActive(false);
        } else if (mode == userAppMode.content2_race)
        {
            GlobalRepo.SetUserPhas(GlobalRepo.UserStep.AppContent2);
            designModeCamera.SetActive(false);
            Content2.SetActive(true);
            Content2_Test.SetActive(true);
            Content4.SetActive(false);
            Content4_Test.SetActive(false);
        }
        else if (mode == userAppMode.content4_design)
        {
            GlobalRepo.SetUserPhas(GlobalRepo.UserStep.design);
            designModeCamera.SetActive(true);
            Content2.SetActive(false);
            Content2_Test.SetActive(false);
            Content4.SetActive(true);
            Content4_Test.SetActive(false);
        } else if (mode == userAppMode.content4_photography)
        {
            GlobalRepo.SetUserPhas(GlobalRepo.UserStep.AppContent4);
            designModeCamera.SetActive(false);
            Content2.SetActive(false);
            Content2_Test.SetActive(false);
            Content4.SetActive(true);
            Content4_Test.SetActive(true);
        }
    }

}

public enum userAppMode
{
    design,
    content2_design,
    content2_race,
    content4_design,
    content4_photography
}
