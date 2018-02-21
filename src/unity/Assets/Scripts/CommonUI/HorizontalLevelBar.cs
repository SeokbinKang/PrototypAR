using UnityEngine;
using System.Collections;
using OpenCvSharp;
using OpenCvSharp.Blob;
using OpenCvSharp.CPlusPlus;
using UnityEngine.UI;
public class HorizontalLevelBar : MonoBehaviour {
    public GameObject levelBarFill;
    public GameObject textLabel;

    private float numericalVal;
    private float finalLevel;  // 0 ~ 1f
    private float currentLevel; // 0 ~ 1f;
    // Use this for initialization
    void Start () {
	    
	}	

	// Update is called once per frame
	void Update () {	

	}    
    void onEnable()
    {
        // start animation;
    }
    public void SetNumericalValue(float val, float low, float high, string valString)
    {
        this.gameObject.SetActive(true);
        SetLevelBar(val, low, high);
        if (val < 0)
        {
            textLabel.GetComponent<Text>().text = "?";

        }
        else
        {
            textLabel.GetComponent<Text>().text = valString;
        }

    }
    private void SetAlpha(float a)
    {
        
    }
    public void SetCategoricalValue(string valString)
    {
        this.gameObject.SetActive(true);
        if (valString == "none")
        {
            SetLevelBar(0, 0, 100);
            textLabel.GetComponent<Text>().text = "?";
        } else
        {
            SetLevelBar(100, 0, 100);
            textLabel.GetComponent<Text>().text = valString;
        }
    }


    private void SetLevelBar(float val, float low, float high)
    {
        Vector3 scale = levelBarFill.transform.localScale;
        scale.x = CVProc.linearMap(val, low, high, 0f, 1f);
        levelBarFill.transform.localScale = scale;
        numericalVal = val;
    }
    
}
