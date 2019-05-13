using UnityEngine;
using System.Collections;

public class SystemConfig : MonoBehaviour {
    public float UserSensitivity;
    public float UserInactivityTimeoutSec;
    public float saturationThresholdMax255;
    public float binaryWhiteThresholdMax255;
    public int PrototypeCompleteNFeedback;
    public int MaxNumberOfObjectsforShapeEval;
    public bool Structure_Missing_objects;
    public bool Structure_Extra_objects;
    public bool Structure_shape;
    public bool Structure_position;
    public bool Structure_connectivity;
    public bool Behavior_Missing_labels;
    public bool Behavior_Extra_labels;
    public bool Behavior_Move_labels;
    public bool Behavior_Unmarked_labels;

    public static SystemConfig ActiveInstnace;
    // Use this for initialization
    void Start () {
        SystemConfig.ActiveInstnace = this;

    }

	// Update is called once per frame
	void Update () {
	
	}
    public void Set(CVConfigItem item, float val)
    {
        if (item == CVConfigItem._User_ActionSensitivity) this.UserSensitivity = val;
        if (item == CVConfigItem._Image_SaturationMin) this.saturationThresholdMax255 = (int) val;
        if (item == CVConfigItem._User_InactivityTimeout) this.UserInactivityTimeoutSec = (int)val;
        if (item == CVConfigItem._Image_WhiteThreshold) this.binaryWhiteThresholdMax255 = (int)val;
    }
    public float Get(CVConfigItem item)
    {
        if (item == CVConfigItem._User_ActionSensitivity) return this.UserSensitivity;
        if (item == CVConfigItem._Image_SaturationMin) return this.saturationThresholdMax255;
        if (item == CVConfigItem._Image_WhiteThreshold) return this.binaryWhiteThresholdMax255;
        if (item == CVConfigItem._User_InactivityTimeout) return this.UserInactivityTimeoutSec;
        return 0;
    }
    public void Set(EvaluationConfigItem item, bool val)
    {
        if (item == EvaluationConfigItem._Structure_Missing_objects) this.Structure_Missing_objects = val;
        else if (item == EvaluationConfigItem._Structure_Extra_objects) this.Structure_Extra_objects = val;
        else if (item == EvaluationConfigItem._Structure_shape) this.Structure_shape = val;
        else if (item == EvaluationConfigItem._Structure_position) this.Structure_position = val;
        else if (item == EvaluationConfigItem._Structure_connectivity) this.Structure_connectivity = val;
        else if (item == EvaluationConfigItem._Behavior_Missing_labels) this.Behavior_Missing_labels = val;
        else if (item == EvaluationConfigItem._Behavior_Extra_labels) this.Behavior_Extra_labels = val;
        else if (item == EvaluationConfigItem._Behavior_Move_labels) this.Behavior_Move_labels = val;
        else if (item == EvaluationConfigItem._Behavior_Unmarked_labels) this.Behavior_Unmarked_labels = val;
    }
    public bool Get(EvaluationConfigItem item)
    {
        if (item == EvaluationConfigItem._Structure_Missing_objects) return this.Structure_Missing_objects;
        else if (item == EvaluationConfigItem._Structure_Extra_objects) return this.Structure_Extra_objects;
        else if (item == EvaluationConfigItem._Structure_shape) return this.Structure_shape;
        else if (item == EvaluationConfigItem._Structure_position) return this.Structure_position;
        else if (item == EvaluationConfigItem._Structure_connectivity) return this.Structure_connectivity;
        else if (item == EvaluationConfigItem._Behavior_Missing_labels) return this.Behavior_Missing_labels;
        else if (item == EvaluationConfigItem._Behavior_Extra_labels) return this.Behavior_Extra_labels;
        else if (item == EvaluationConfigItem._Behavior_Move_labels) return this.Behavior_Move_labels;
        else if (item == EvaluationConfigItem._Behavior_Unmarked_labels) return this.Behavior_Unmarked_labels;

        return false;
    }
}

public class RainbowConfig
{
    public EvaluationConfig evaluation;

    public RainbowConfig()
    {
        
    }

    
    
    public class EvaluationConfig
    {
        public bool Structure_Missing_objects;
        public bool Structure_Extra_objects;
        public bool Structure_shape;
        public bool Structure_position;
        public bool Structure_connectivity;
        public bool Behavior_Missing_labels;
        public bool Behavior_Extra_labels;
        public bool Behavior_Move_labels;
        public bool Behavior_Unmarked_labels;

    }
}

public enum EvaluationConfigItem
{
    _none,
    _Structure_Missing_objects,
    _Structure_Extra_objects,
    _Structure_shape,
    _Structure_position,
    _Structure_connectivity,
    _Behavior_Missing_labels,
    _Behavior_Extra_labels,
    _Behavior_Move_labels,
    _Behavior_Unmarked_labels
}
public enum CVConfigItem
{
    _none,
    _User_ActionSensitivity,
    _Image_SaturationMin,
    _Image_WhiteThreshold,
    _User_InactivityTimeout
}