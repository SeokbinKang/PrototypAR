using UnityEngine;
using System.Collections;

public class Content4_UIControl : MonoBehaviour {

    public GameObject[] ObjectSystemComponents;
    public GameObject[] ObjectApplicationPhotography;

    public GameObject[] DefaultInactiveObjects;
    // Use this for initialization
    private Content4_UIModes mCurrentMode = Content4_UIModes.None;
    private GlobalRepo.UserStep lastUserMode = GlobalRepo.UserStep.design;
    void Start () {
        SetUIMode(Content4_UIModes.Design);
        foreach (var i in DefaultInactiveObjects)
        {
            i.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (GlobalRepo.UserMode != lastUserMode && GlobalRepo.UserMode== GlobalRepo.UserStep.design)
        {
            ModeDesign();
        }
        lastUserMode = GlobalRepo.UserMode;
    }
    public void ModeApplication()
    {
        SetUIMode(Content4_UIModes.ApplicationPhotography);
        WorkSpaceUI.mInstance.gameObject.SetActive(false);
    }
    public void ModeDesign()
    {
        SetUIMode(Content4_UIModes.Design);
        WorkSpaceUI.mInstance.gameObject.SetActive(true);
    }
    public void SetUIMode(Content4_UIModes m)
    {
        mCurrentMode = m;
        if(m== Content4_UIModes.Design)
        {
            foreach(var i in ObjectSystemComponents)
            {
                i.SetActive(false);
            }
            foreach (var i in ObjectApplicationPhotography)
            {
                i.SetActive(false);
            }
        } else if (m == Content4_UIModes.SystemComponents)
        {
            foreach (var i in ObjectSystemComponents)
            {
                i.SetActive(true);
            }
            foreach (var i in ObjectApplicationPhotography)
            {
                i.SetActive(false);
            }
        }
        else if (m == Content4_UIModes.ApplicationPhotography)
        {
            foreach (var i in ObjectSystemComponents)
            {
                i.SetActive(false);
            }
            foreach (var i in ObjectApplicationPhotography)
            {
                i.SetActive(true);
            }
        }

    }
}

public enum Content4_UIModes
{
    None,
    Design,
    SystemComponents,
    ApplicationPhotography
}
