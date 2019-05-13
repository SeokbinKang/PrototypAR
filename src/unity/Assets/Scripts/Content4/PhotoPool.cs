using UnityEngine;
using System.Collections;

public class PhotoPool : MonoBehaviour {

    public GameObject fullcolor;
    public GameObject redgreen;
    public GameObject grayscale;
    public GameObject mono;
    // Use this for initialization
    void Start () {
        //SetPhotoType("Black & White");
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    public void SetPhotoType(string type)
    {        
        if (type== "Black & White")
        {
            mono.SetActive(true);
            fullcolor.SetActive(false);
            redgreen.SetActive(false);            
        } else if (type == "Red & Green")
        {
            mono.SetActive(false);
            fullcolor.SetActive(false);
            redgreen.SetActive(true);
        } else if (type == "Full Color" || type == "GRAYSCALE")
        {
            mono.SetActive(false);
            fullcolor.SetActive(true);
            redgreen.SetActive(false);
        } else
        {
            mono.SetActive(false);
            fullcolor.SetActive(false);
            redgreen.SetActive(false);
        }
    }
}
