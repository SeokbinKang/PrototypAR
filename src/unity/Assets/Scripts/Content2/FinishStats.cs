using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class FinishStats : MonoBehaviour {

    public GameObject[] placementLabel;
    public GameObject[] statsLabel;
    public GameObject soundControlObject;
	// Use this for initialization
	void Start () {
       resetLabels();
    }
	
	// Update is called once per frame
	void Update () {
	
	}
    public void SetStat(int row, string placement, string stats)
    {
        if (placementLabel == null || statsLabel == null) return;
        if (placementLabel[row] == null || statsLabel[row] == null) return;
        placementLabel[row].GetComponent<Text>().text = placement;
        statsLabel[row].GetComponent<Text>().text = "Gear Ratio(F/R)\n= "+stats;
        playStopSound();

    }
    private void playStopSound()
    {
        soundControlObject.GetComponent<SoundControl>().onStop();
    }
    public void resetLabels()
    {
        foreach (GameObject t in placementLabel)
        {
            Text te = t.GetComponent<Text>();
            if (te == null) continue;
            te.text = "";
        }
        foreach (GameObject t in statsLabel)
        {
            Text te = t.GetComponent<Text>();
            if (te == null) continue;
            te.text = "";
        }
    }
}
