using UnityEngine;
using System.Collections;

public class C4_sensor : MonoBehaviour {

    public GameObject imagePanel;
    
    private string sensorType;

    public Sprite[] ColorFrames;
    public Sprite[] MonoFrames;
    // Use this for initialization
    void Start () {
        sensorType = "";

    }
	
	// Update is called once per frame
	void Update () {
	
	}
    public void SetType(string t)
    {
        sensorType = t;
    }
   
    
    public void OnLightReceived()
    {
        GameObject focusGO = GameObject.FindGameObjectWithTag("C4_focus");
        int frameIndx = 10;
        if (focusGO != null)
        {
            bool isCloseToFocus = focusGO.GetComponent<Collider2D>().bounds.Intersects(this.GetComponent<Collider2D>().bounds);
            if (isCloseToFocus) frameIndx = 0;
                else
            {
                float dist = (focusGO.transform.position - this.transform.position).magnitude;
                frameIndx = (int ) CVProc.linearMap(dist, 0f, 0.5f, 0f, (float)ColorFrames.Length - 1);
                
                //Debug.Log("DISTTTT" + dist);
            }
        }
        if (sensorType == "Full Color" || sensorType == "Red & Green")
        {
            imagePanel.GetComponent<SpriteRenderer>().sprite = ColorFrames[frameIndx];
            imagePanel.GetComponent<Animator>().Play("show");
        }
        if (sensorType == "GRAYSCALE" || sensorType == "Black & White")
        {
           // Debug.Log("Animate Mono sensor");
            imagePanel.GetComponent<SpriteRenderer>().sprite = MonoFrames[frameIndx];
            imagePanel.GetComponent<Animator>().Play("show");
        }
    }

}
