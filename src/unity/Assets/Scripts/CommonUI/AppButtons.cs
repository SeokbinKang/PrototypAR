using UnityEngine;
using System.Collections;

public class AppButtons : MonoBehaviour {
    public GameObject content2;
    public GameObject content4;
	// Use this for initialization
	void Start () {

        UpdateButton();
    }
	
	// Update is called once per frame
	void Update () {
	
	}
    private void UpdateButton()
    {
        GameObject appControl = GameObject.Find("ApplicationControl");
        if (appControl == null)
        {
            Debug.Log("[ERROR] Could not find ApplicationControl obejct");
            return;
            
        }
        ApplicationControl appControlInstance = appControl.GetComponent<ApplicationControl>();
        if (appControlInstance == null)
        {
            Debug.Log("[ERROR] Could not find ApplicationControl instance");
            return;
        }
        DesignContent con = appControlInstance.getContentType();
        if (con == DesignContent.BicycleGearSystem)
        {
            this.content2.SetActive(true);
            this.content4.SetActive(false);                
        } else if (con == DesignContent.CameraSystem)
        {
            this.content2.SetActive(false);
            this.content4.SetActive(true);
        }

    }
}
