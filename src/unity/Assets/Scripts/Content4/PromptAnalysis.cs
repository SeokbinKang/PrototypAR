using UnityEngine;
using System.Collections;

public class PromptAnalysis : MonoBehaviour {

    public GameObject LeftPanel;
    public GameObject RightPanel;
    public Color c_focal;
    public Color c_shutter;
    public Color c_typesensor;
    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void LoadPhotos(PhotoShot left, PhotoShot right,string paramName)
    {
        
        if (left == null || right == null) return;
        PrototypeInventoryPane lp = LeftPanel.GetComponent<PrototypeInventoryPane>();
        PrototypeInventoryPane rp = RightPanel.GetComponent<PrototypeInventoryPane>();
        lp.loadTexture(left.txt2D);
        lp.SetName("Taken by "+left.prototypeName);
        if (paramName == "focallength") lp.PanelInfo.GetComponent<PrototypeInventoryInfo>().SetParamBar_Numerical(0, "Focal\nLength", left.parameter.C4_focalLength, 20, 200, (int) left.parameter.C4_focalLength + " mm", c_focal);
        else if (paramName == "shutterspeed") lp.PanelInfo.GetComponent<PrototypeInventoryInfo>().SetParamBar_Numerical(0, "Shutter\nSpeed", left.parameter.C4_shutterSpeed, 10, 1000, (int)left.parameter.C4_shutterSpeed + " ms", c_shutter);
        else if (paramName == "sensortype") lp.PanelInfo.GetComponent<PrototypeInventoryInfo>().SetParamBar_Categorical(0, "Sensor\nType",left.parameter.C4_sensorType,c_typesensor);

        rp.loadTexture(right.txt2D);
        rp.SetName("Taken by " + right.prototypeName);
        if (paramName == "focallength") rp.PanelInfo.GetComponent<PrototypeInventoryInfo>().SetParamBar_Numerical(0, "Focal\nLength", right.parameter.C4_focalLength, 20, 200, (int)right.parameter.C4_focalLength + " mm", c_focal);
        else if (paramName == "shutterspeed") rp.PanelInfo.GetComponent<PrototypeInventoryInfo>().SetParamBar_Numerical(0, "Shutter\nSpeed", right.parameter.C4_shutterSpeed, 10, 1000, (int)right.parameter.C4_shutterSpeed + " ms", c_shutter);
        else if (paramName == "sensortype") rp.PanelInfo.GetComponent<PrototypeInventoryInfo>().SetParamBar_Categorical(0, "Sensor\nType", right.parameter.C4_sensorType, c_typesensor);


    }
}
