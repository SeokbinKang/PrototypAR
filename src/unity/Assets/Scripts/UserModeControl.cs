using UnityEngine;
using System.Collections;

public class UserModeControl : MonoBehaviour {
    public GameObject designModeCamera;
    public GameObject Content2_App;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {        
	
	}
    public void switchToContent2Race()
    {
        switchTo(userAppMode.content2_race);
    }
    public void switchTo(userAppMode mode)
    {
        if (mode == userAppMode.design)
        {
            designModeCamera.SetActive(true);
            Content2_App.SetActive(false);
        }
        if (mode == userAppMode.content2_race)
        {
            designModeCamera.SetActive(false);
            Content2_App.SetActive(true);
        }
    }

}

public enum userAppMode
{
    design,
    content2_race
}
