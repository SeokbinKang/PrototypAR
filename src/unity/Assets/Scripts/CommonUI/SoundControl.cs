using UnityEngine;
using System.Collections;

public class SoundControl : MonoBehaviour {
    public GameObject sClick;
    public GameObject sChange;
    public GameObject sStop;
    public GameObject sRoll;
    public GameObject sShutter;
    public GameObject sGood;
    public GameObject sHint;

    public static SoundControl mActiveInstance;
	// Use this for initialization
	void Start () {
        mActiveInstance = this;

    }
	
	// Update is called once per frame
	void Update () {
	
	}
    public void onHint()
    {
        sHint.GetComponent<AudioSource>().Play();
    }
    public void onClick()
    {
        sClick.GetComponent<AudioSource>().Play();
    }
    public void onChange()
    {
        sChange.GetComponent<AudioSource>().Play();
    }
    public void onStop()
    {
        sStop.GetComponent<AudioSource>().Play();
    }
          public void onRoll()
    {
        sRoll.GetComponent<AudioSource>().Play();
    }
    public void onShutter()
    {
        sShutter.GetComponent<AudioSource>().Play();
    }

}
