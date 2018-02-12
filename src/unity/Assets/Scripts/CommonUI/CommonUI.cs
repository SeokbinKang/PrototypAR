using UnityEngine;
using System.Collections;

public class CommonUI : MonoBehaviour {

    public GameObject ARSystem;
    public GameObject ApplicationControl_;
    public GameObject feedbackButton;

	// Use this for initialization
	void Start () {
	    
	}
	
	// Update is called once per frame
	void Update () {
	    
	}
    public void UpdateFeedbackButton(int NumberofFeedbacks)
    {
        if (NumberofFeedbacks > 0) feedbackButton.SetActive(true);
            else feedbackButton.SetActive(false);
    }
    public void OnFeedbackButton()
    {
        //generate the feedback
        ARSystem.GetComponent<Visual2DModelManager>().ShowScaffoldingFeedback();
    }
    public void OnHideButton()
    {
        ApplicationControl_.GetComponent<ApplicationControl>().Reset();
    }
}
