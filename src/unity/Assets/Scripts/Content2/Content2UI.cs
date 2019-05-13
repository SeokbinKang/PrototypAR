using UnityEngine;
using System.Collections;

public class Content2UI : MonoBehaviour {
    public GameObject[] UIGOsForDesignMode;
    public GameObject[] UIGOsForTestMode;
    public GameObject[] DefaultInactiveObjects;
    // Use this for initialization

    private Content2_UIModes mCurrentMode;
    private GlobalRepo.UserStep lastUserMode = GlobalRepo.UserStep.design;
    // Use this for initialization
    void Start () {
        SetUIMode(Content2_UIModes.Design);
        foreach (var i in DefaultInactiveObjects)
        {
            i.SetActive(false);
        }
    }
	
	// Update is called once per frame
	void Update () {
        if (GlobalRepo.UserMode != lastUserMode && GlobalRepo.UserMode == GlobalRepo.UserStep.design)
        {
            ModeDesign();
        }
        lastUserMode = GlobalRepo.UserMode;
    }
    public void ModeTest()
    {
        SetUIMode(Content2_UIModes.TestRace);
    }
    public void ModeDesign()
    {
        SetUIMode(Content2_UIModes.Design);
    }
    public void SetUIMode(Content2_UIModes m)
    {
        mCurrentMode = m;
        if (m == Content2_UIModes.Design)
        {
            foreach (var i in UIGOsForDesignMode)
            {
                i.SetActive(true);
            }
            foreach (var i in UIGOsForTestMode)
            {
                i.SetActive(false);
            }
        }     
        else if (m == Content2_UIModes.TestRace)
        {
            foreach (var i in UIGOsForDesignMode)
            {
                i.SetActive(false);
            }
            foreach (var i in UIGOsForTestMode)
            {
                i.SetActive(true);
            }
        }

    }
}


public enum Content2_UIModes
{
    None,
    Design,    
    TestRace
}
