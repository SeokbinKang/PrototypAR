using UnityEngine;
using System.Collections;

public class minimapUI : MonoBehaviour {
    public GameObject startLine;
    public GameObject finishLine;
    public GameObject startminimap;
    public GameObject finishminimap;


    public GameObject[] icons;
    public GameObject[] bikes;
    
    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        UpdatIconPosition();

    }
    private void UpdatIconPosition()
    {
        if (startLine == null || finishLine == null || startminimap == null || finishminimap == null || icons == null || bikes == null) return;
        float trackLength = finishLine.transform.position.x - startLine.transform.position.x;
        float minimapLength = finishminimap.transform.position.x - startminimap.transform.position.x;
        Vector3 temp;
        for(int i = 0; i < icons.Length; i++)
        {
            float bike_pos_x = bikes[i].transform.position.x;
            bike_pos_x = bike_pos_x - startLine.transform.position.x;
            float minimap_icon_pos = startminimap.transform.position.x;
            minimap_icon_pos += (bike_pos_x / trackLength) * minimapLength;
            temp = icons[i].transform.position;
            temp.x = minimap_icon_pos;
            icons[i].transform.position = temp;
        }
    }
}
