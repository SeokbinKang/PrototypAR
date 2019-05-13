using UnityEngine;
using System.Collections;

public class Content2 : MonoBehaviour {


    public GameObject UIControl;
	// Use this for initialization
	void Start () {
        init();

    }
	
	// Update is called once per frame
	void Update () {
        backgroundcontrol();

    }
    private void init()
    {
        UIControl.SetActive(true);
    }
    private void backgroundcontrol()
    {
        GlobalRepo.UserStep lastUserPhase = GlobalRepo.UserMode;
        if (lastUserPhase == GlobalRepo.UserStep.design)
        {

            SceneObjectManager.getActiveInstance().adjustAlphaSpriteRendereFadeIn(PreLoadedObjects.Content2_BGPartial, 50);
            SceneObjectManager.getActiveInstance().adjustAlphaSpriteRendere(PreLoadedObjects.Content2_BGFull, 0);
            SceneObjectManager.getActiveInstance().MoveToScreenRelativePos(PreLoadedObjects.Content2_BGPartial, new Vector2(0.5f, 0.5f));
        }
        else if (lastUserPhase == GlobalRepo.UserStep.feedback)
        {
            SceneObjectManager.getActiveInstance().adjustAlphaSpriteRendere(PreLoadedObjects.Content2_BGPartial, 10);
            SceneObjectManager.getActiveInstance().adjustAlphaSpriteRendere(PreLoadedObjects.Content2_BGFull, 0);
        }
        else if (lastUserPhase == GlobalRepo.UserStep.simulation)
        {

            SceneObjectManager.getActiveInstance().adjustAlphaSpriteRendere(PreLoadedObjects.Content2_BGPartial, 20);
            SceneObjectManager.getActiveInstance().adjustAlphaSpriteRendere(PreLoadedObjects.Content2_BGFull, 200);
        }
    }
}
