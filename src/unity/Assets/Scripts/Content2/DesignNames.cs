using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class DesignNames : MonoBehaviour {
    public GameObject[] elements;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
    public void SetName(int row, string name)
    {
        if (elements == null ) return;
        if (elements[row] == null ) return;
        elements[row].GetComponent<Text>().text = name;        

    }
    public void resetLabels()
    {
        this.gameObject.SetActive(true);
        foreach (GameObject t in elements)
        {
            Text te = t.GetComponent<Text>();
            if (te == null) continue;
            te.text = "";
        }        
    }
}
