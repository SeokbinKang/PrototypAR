using UnityEngine;
using System.Collections;

public class C4_lens : MonoBehaviour {

    public GameObject focus;

    
	// Use this for initialization
	void Start () {
     
    }
	
	// Update is called once per frame
	void Update () {
	
	}
    public Vector3 getFocusPos()
    {
        if (focus.activeSelf) return focus.transform.position;
        else
        {
            Vector3 ret = this.transform.position;
            ret.x += 1+Random.value * 5;
            ret.y += Random.value * 1f - 0.5f;
            return ret;
        }
    }
    public void SetFocalLength(float fVal)
    {
        
        if (fVal < 0) focus.SetActive(false);
            else focus.SetActive(true);
        float minXOffset = 4f;
        float maxXOffset = 12f;
        //focal length 10 - 200        
        float XOffset = CVProc.linearMap(fVal, 10, 200, minXOffset, maxXOffset);
        Vector3 ret = Vector3.zero;
      
        ret.x += XOffset;
        focus.transform.localPosition = ret;
    }
}
