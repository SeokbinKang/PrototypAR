using UnityEngine;
using System.Collections;

public class UserActiveStatus : MonoBehaviour {
    public GameObject waiting;
    public GameObject active;
    public GameObject inactive;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
    public void  updateStatus(bool isActive, bool isWaitingforActivity, float InactiveTime, float InactiveTimeThreshold)
    {
       // Debug.Log("Inactivity Time: " + InactiveTime +"\t Acitve: "+isActive);
        if(isWaitingforActivity)
        {
            waiting.SetActive(true);
            active.SetActive(false);
            inactive.SetActive(false);
            return;
        }
        if (isActive)
        {
            waiting.SetActive(false);
            active.SetActive(true);
            inactive.SetActive(false);
            return;
        } else
        {
            waiting.SetActive(false);
            active.SetActive(false);
            inactive.SetActive(true);
            float scaleFactor = CVProc.linearMap(InactiveTime, 0, InactiveTimeThreshold, 0.1f, 1f);
            Vector3 s = inactive.GetComponent<RectTransform>().localScale;
            s.x = scaleFactor;
            s.y = scaleFactor;
            inactive.GetComponent<RectTransform>().localScale = s;
            return;
        }
    }
}
