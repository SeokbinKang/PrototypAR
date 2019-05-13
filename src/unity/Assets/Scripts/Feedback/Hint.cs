using UnityEngine;
using System.Collections;

public class Hint : MonoBehaviour {

    public GameObject icon;
    public GameObject[] msg;
    
    private GameObject ARoverlayItem; //2D system-drawn images. this can be either overlay texture or UI texture. UI texture might be implementable
                                      // Use this for initialization
    private Visual2DObject ARTxt2D=null;
    private int UserLevel = 0;
    public bool isClosed;
    private PreLoadedObjects type;
	void Start () {
        if (icon != null) icon.SetActive(true);
        if (msg != null)
        {
            foreach(var m in msg)
                m.SetActive(false);
        }
        isClosed = true;
        
    }
	
	// Update is called once per frame
	void Update () {
	
	}
    public void SetARTxt2D(Visual2DObject v)
    {

        
        this.ARTxt2D = v;
    }
    public bool IsOpened()
    {
        return !isClosed;
    }
    public void SetUserLevel(int level)
    {
        UserLevel = level;
    }
    public void SetType(PreLoadedObjects t)
    {
        this.type = t;
    }
    public void OnClickIcon()
    {
        if (icon != null) icon.SetActive(false);
        if (msg != null) 
            {            
                foreach (var m in msg)
                    m.SetActive(false);
            if (UserLevel >= 0 && UserLevel < msg.Length)
                msg[UserLevel].SetActive(true);
            else if (UserLevel >= 0) msg[0].SetActive(true);
            }
        isClosed = false;

        if (ARTxt2D != null)
        {
         
            ARTxt2D.enabled = true;
        }
        
        IncreaseFeedbackCounter();
        ApplicationControl.ActiveInstance.HintOpened() ;
        SoundControl.mActiveInstance.onHint();
        SystemLogger.activeInstance.WriteUserEvent("Hint Opened");
    }
    public void OnClickClose()
    {
        ApplicationControl.ActiveInstance.HaltRecognition5();
       // Debug.Log("SDFDSCLOSESESEESESESEES");
        if (icon != null) icon.SetActive(false);
        if (msg != null)
        {
            foreach (var m in msg)
                m.SetActive(false);
        }
        isClosed = true;
        if (ARTxt2D != null)
        {
            ARTxt2D.enabled = false;
            ARTxt2D = null;        
        }
        //notify feedback control to delete this
        RemoveFromFeedbackList();
        SoundControl.mActiveInstance.onClick();
    }
    private void IncreaseFeedbackCounter()
    {
        FeedbackControl parent_ = this.transform.GetComponentInParent<FeedbackControl>();
        if (parent_ != null) parent_.IncreaseFeedbackCounter(this.type);
    }
    private void RemoveFromFeedbackList()
    {
        FeedbackControl parent_ = this.transform.GetComponentInParent<FeedbackControl>();
        if (parent_ != null) parent_.RemoveFeedback(this.gameObject);
    }
}
