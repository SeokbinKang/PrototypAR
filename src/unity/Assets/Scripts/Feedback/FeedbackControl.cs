using UnityEngine;
using System.Collections;
using OpenCvSharp;
using OpenCvSharp.Blob;
using OpenCvSharp.CPlusPlus;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
public class FeedbackControl : MonoBehaviour {

    public int HintRotationInterval;
    public GameObject NumberOfFeedbackVisible;
    public GameObject FeedbackContent2Pool;
    public GameObject FeedbackContent4Pool;


    public static FeedbackControl mActiveInstance;
    public int HowManyFeedbacksForNextLevel;
    private GameObject go;
    private List<GameObject> mFeedbackList;

    private float lastUpdate;
    private Dictionary<PreLoadedObjects, int> FeedbackTriggeredTimes;
    // Use this for initialization
    void Start() {
        mFeedbackList = new List<GameObject>();
        FeedbackTriggeredTimes = new Dictionary<PreLoadedObjects, int>();
        mActiveInstance = this;
    }

    // Update is called once per frame
    void Update() {

        if(Time.time-lastUpdate>HintRotationInterval)
        {
            ShowFeedbackRoundRobin();
            lastUpdate = Time.time;
        }
    }
    void OnEnable()
    {
        Reset();
    }
    public void Reset()
    {

        if (mFeedbackList != null)
        {
            foreach (var f in mFeedbackList)
            {
                f.SetActive(false);
                Destroy(f);
            }
            mFeedbackList.Clear();
        }
        lastUpdate = -3;
    }
    private void ShowFeedbackRoundRobin()
    {
        if (mFeedbackList == null || mFeedbackList.Count==0 ) return ;
        
        for (int i = 0; i < mFeedbackList.Count; i++)
        {
            if (mFeedbackList[i].activeInHierarchy )
            {
                if (mFeedbackList[i].GetComponent<Hint>().IsOpened())
                {
                    return;
                } else mFeedbackList[i].SetActive(false);
            }
        //    mFeedbackList[i].SetActive(false);
        }
        if (mFeedbackList.Count > 0)
        {
            GameObject o = mFeedbackList[0];
            mFeedbackList.RemoveAt(0);
            o.SetActive(true);
            mFeedbackList.Add(o);
        }
    }
    public int GetNofFeedback()
    {
        if (mFeedbackList == null) return 0;
        return mFeedbackList.Count();
    }
    private GameObject InstantiateUIPrefab(GameObject prefab)
    {
        GameObject go = Instantiate(prefab, prefab.transform.position, prefab.transform.rotation) as GameObject;
        go.transform.parent = this.transform;
        go.GetComponent<RectTransform>().localPosition = prefab.GetComponent<RectTransform>().localPosition;
        go.GetComponent<RectTransform>().localScale = prefab.GetComponent<RectTransform>().localScale;        
        return go;
    }
    public GameObject AddFeedback(FeedbackToken f)
    {
        if (f == null) return null;
        
        GameObject fobject = CreateFeedback(f);
        if (fobject == null) return null; 
        fobject.SetActive(false);
        mFeedbackList.Add(fobject);
        return fobject;
        
    }
    public void AddFeedbacks(List<FeedbackToken> flist)
    {
        if (flist == null) return;
        for(int i = 0; i < flist.Count; i++)
        {
            GameObject fobject = CreateFeedback(flist[i]);
            if (fobject == null) continue;
            fobject.SetActive(false);
            mFeedbackList.Add(fobject);
        }
    }
    public GameObject CreateFeedback(FeedbackToken f)
    {
        GameObject ret = null;
        if (f.type == EvaluationResultCategory.Shape_existence_missing) ret= CreateStrMissingFeedback(f);
        else if (f.type == EvaluationResultCategory.Position_direction) ret = CreateStrPositionFeedback(f);
        else if (f.type == EvaluationResultCategory.Shape_suggestion) ret = CreateStrShapeFeedback(f);
        else if (f.type == EvaluationResultCategory.Behavior_missing) ret = CreateBLMissingFeedback(f);
        else if (f.type == EvaluationResultCategory.Behavior_variableUnchecked) ret = CreateBLUnspeicifiedFeedback(f);

        if(!WorkSpaceUI.mInstance.IsUIObjectwithinWS(ret))
        {
            GameObject.Destroy(ret);
            return null;
        }
        return ret;
    }
    public void RemoveFeedback(GameObject o)
    {
        for (int i = 0; i < mFeedbackList.Count; i++)
        {
            if(mFeedbackList[i]==o)
            {
                mFeedbackList.RemoveAt(i);
                o.SetActive(false);
                GameObject.Destroy(o);
                return;
            }
        }
    }
    private GameObject CreateStrShapeFeedback(FeedbackToken f)
    {
        GameObject preFabFeedback = null;
        if (f.modelType == ModelCategory.FrontChainring)
            preFabFeedback = FeedbackContent2Pool.GetComponent<FeedbackContent2>().Content2_StrShape;
        if (f.modelType == ModelCategory.RearSprocket)
            preFabFeedback = FeedbackContent2Pool.GetComponent<FeedbackContent2>().Content2_StrShape;

        if (f.modelType == ModelCategory.C4_lens)
            preFabFeedback = FeedbackContent4Pool.GetComponent<FeedbackContent4>().Content4_StrShape;
        if (f.modelType == ModelCategory.C4_sensor)
            preFabFeedback = FeedbackContent4Pool.GetComponent<FeedbackContent4>().Content4_StrShape;
        if (f.modelType == ModelCategory.C4_shutter)
            preFabFeedback = FeedbackContent4Pool.GetComponent<FeedbackContent4>().Content4_StrShape;
        if (preFabFeedback == null) return null;

        GameObject FeedbackObject = InstantiateUIPrefab(preFabFeedback);
        Vector3 screenPosition = SceneObjectManager.RegiontoScreen(f.RegionPosition);
        Vector3 objRectPos = new Vector3();
     //   Debug.Log("Feedback region pos: " + screenPosition);
        objRectPos.x = screenPosition.x - Screen.width / 2;
        objRectPos.y = screenPosition.y - Screen.height / 2;
        objRectPos.x = objRectPos.x * (1200f / Screen.width);
        objRectPos.y = objRectPos.y * (900f / Screen.height);
        FeedbackObject.GetComponent<RectTransform>().localPosition = objRectPos;

        return FeedbackObject;
    }


    private GameObject CreateBLUnspeicifiedFeedback(FeedbackToken f)
    {
        PreLoadedObjects feedbackObj = f.getFeedbackObjectID();
        GameObject preFabFeedback = null;

        if (feedbackObj == PreLoadedObjects.BEH_BV_unspecified_focus)
        {
            preFabFeedback = FeedbackContent4Pool.GetComponent<FeedbackContent4>().Content4_BLHowFocus;
        }
        else if (feedbackObj == PreLoadedObjects.BEH_BV_unspecified_allow)
        {
            preFabFeedback = FeedbackContent4Pool.GetComponent<FeedbackContent4>().Content4_BLHowAllow;
        }
        else if (feedbackObj == PreLoadedObjects.BEH_BV_unspecified_capture)
        {
            preFabFeedback = FeedbackContent4Pool.GetComponent<FeedbackContent4>().Content4_BLHowCapture;
        }

        if (preFabFeedback == null) return null;

        GameObject FeedbackObject = InstantiateUIPrefab(preFabFeedback);
        int userLevel = 0;
        if (!FeedbackTriggeredTimes.ContainsKey(feedbackObj)) FeedbackTriggeredTimes[feedbackObj] = 0;
        if (FeedbackTriggeredTimes[feedbackObj] >= HowManyFeedbacksForNextLevel) userLevel = 1;
        FeedbackObject.GetComponent<Hint>().SetType(feedbackObj);
        FeedbackObject.GetComponent<Hint>().SetUserLevel(userLevel);

        Vector3 screenPosition = SceneObjectManager.RegiontoScreen(f.RegionPosition);
        Vector3 objRectPos = new Vector3();
        //   Debug.Log("Feedback region pos: " + screenPosition);
        objRectPos.x = screenPosition.x - Screen.width / 2;
        objRectPos.y = screenPosition.y - Screen.height / 2;
        objRectPos.x = objRectPos.x * (1200f / Screen.width);
        objRectPos.y = objRectPos.y * (900f / Screen.height);
        FeedbackObject.GetComponent<RectTransform>().localPosition = objRectPos;


        return FeedbackObject;
    }
    private GameObject CreateBLMissingFeedback(FeedbackToken f)
    {
        PreLoadedObjects feedbackObj = f.getFeedbackObjectID();
        GameObject preFabFeedback = null;

        if (feedbackObj == PreLoadedObjects.BL_missing_focus)
        {
            preFabFeedback = FeedbackContent4Pool.GetComponent<FeedbackContent4>().Content4_BLMissFocus;
        }
        else if (feedbackObj == PreLoadedObjects.BL_missing_allow)
        {
            preFabFeedback = FeedbackContent4Pool.GetComponent<FeedbackContent4>().Content4_BLMissAllow;
        }
        else if (feedbackObj == PreLoadedObjects.BL_missing_capture)
        {
            preFabFeedback = FeedbackContent4Pool.GetComponent<FeedbackContent4>().Content4_BLMissCapture;
        }

        if (preFabFeedback == null) return null;

        GameObject FeedbackObject = InstantiateUIPrefab(preFabFeedback);
        int userLevel = 0;
        if (!FeedbackTriggeredTimes.ContainsKey(feedbackObj)) FeedbackTriggeredTimes[feedbackObj] = 0;
        if (FeedbackTriggeredTimes[feedbackObj] >= HowManyFeedbacksForNextLevel) userLevel = 1;
        FeedbackObject.GetComponent<Hint>().SetType(feedbackObj);
        FeedbackObject.GetComponent<Hint>().SetUserLevel(userLevel);

        Vector3 screenPosition = SceneObjectManager.RegiontoScreen(f.RegionPosition);
        Vector3 objRectPos = new Vector3();
        //   Debug.Log("Feedback region pos: " + screenPosition);
        objRectPos.x = screenPosition.x - Screen.width / 2;
        objRectPos.y = screenPosition.y - Screen.height / 2;
        objRectPos.x = objRectPos.x * (1200f / Screen.width);
        objRectPos.y = objRectPos.y * (900f / Screen.height);
        FeedbackObject.GetComponent<RectTransform>().localPosition = objRectPos;
        return FeedbackObject;
    }
    private GameObject CreateStrMissingFeedback(FeedbackToken f)
    {
        PreLoadedObjects feedbackObj = f.getFeedbackObjectID();
        GameObject preFabFeedback = null;
       
        if (feedbackObj == PreLoadedObjects.STR_missing_c2_frontgear)
        {
            preFabFeedback = FeedbackContent2Pool.GetComponent<FeedbackContent2>().Content2_StrMissFrontGear;
        }
        else if (feedbackObj == PreLoadedObjects.STR_missing_c2_reargear)
        {
            preFabFeedback = FeedbackContent2Pool.GetComponent<FeedbackContent2>().Content2_StrMissRearGear;
        }
        else if (feedbackObj == PreLoadedObjects.STR_missing_c2_pedal)
        {
            preFabFeedback = FeedbackContent2Pool.GetComponent<FeedbackContent2>().Content2_StrMissPedal;
        }
        else if (feedbackObj == PreLoadedObjects.STR_missing_c2_chain)
        {
            preFabFeedback = FeedbackContent2Pool.GetComponent<FeedbackContent2>().Content2_StrMissChains;
        }
        else if (feedbackObj == PreLoadedObjects.STR_missing_c4_lens)
        {
            preFabFeedback = FeedbackContent4Pool.GetComponent<FeedbackContent4>().Content4_StrMissLens;
        }
        else if (feedbackObj == PreLoadedObjects.STR_missing_c4_shutter)
        {
            preFabFeedback = FeedbackContent4Pool.GetComponent<FeedbackContent4>().Content4_StrMissShutter;
        }
        else if (feedbackObj == PreLoadedObjects.STR_missing_c4_sensor)
        {
            preFabFeedback = FeedbackContent4Pool.GetComponent<FeedbackContent4>().Content4_StrMissSensor;
        }

        if (preFabFeedback == null) return null;

        GameObject FeedbackObject = InstantiateUIPrefab(preFabFeedback);
        int userLevel = 0;
        if (!FeedbackTriggeredTimes.ContainsKey(feedbackObj)) FeedbackTriggeredTimes[feedbackObj] = 0;
        if (FeedbackTriggeredTimes[feedbackObj] >= HowManyFeedbacksForNextLevel) userLevel = 1;
        FeedbackObject.GetComponent<Hint>().SetType(feedbackObj);
        FeedbackObject.GetComponent<Hint>().SetUserLevel(userLevel);
        return FeedbackObject;
    }
    public void IncreaseFeedbackCounter(PreLoadedObjects t)
    {
        if (!FeedbackTriggeredTimes.ContainsKey(t)) FeedbackTriggeredTimes[t] = 0;
        FeedbackTriggeredTimes[t]++;
    }
    private GameObject CreateStrPositionFeedback(FeedbackToken f)
    {
        GameObject preFabFeedback = null;
        if (f.modelType == ModelCategory.FrontChainring)
            preFabFeedback = FeedbackContent2Pool.GetComponent<FeedbackContent2>().Content2_strPosFrontGear;
        else if (f.modelType == ModelCategory.RearSprocket)
            preFabFeedback = FeedbackContent2Pool.GetComponent<FeedbackContent2>().Content2_strPosRearGear;
        else if (f.modelType == ModelCategory.PedalCrank)
            preFabFeedback = FeedbackContent2Pool.GetComponent<FeedbackContent2>().Content2_strPosPedal;
        else if (f.modelType == ModelCategory.LowerChain)
            preFabFeedback = FeedbackContent2Pool.GetComponent<FeedbackContent2>().Content2_strPosChains;
        else if (f.modelType == ModelCategory.UpperChain)
            preFabFeedback = FeedbackContent2Pool.GetComponent<FeedbackContent2>().Content2_strPosChains;
        else if (f.modelType == ModelCategory.C4_lens)
            preFabFeedback = FeedbackContent4Pool.GetComponent<FeedbackContent4>().Content4_strPosLens;
        else if (f.modelType == ModelCategory.C4_shutter)
            preFabFeedback = FeedbackContent4Pool.GetComponent<FeedbackContent4>().Content4_strPosShutter;
        else if (f.modelType == ModelCategory.C4_sensor)
            preFabFeedback = FeedbackContent4Pool.GetComponent<FeedbackContent4>().Content4_strPosSensor;
        if (preFabFeedback == null) return null;

        GameObject FeedbackObject = InstantiateUIPrefab(preFabFeedback);
        Vector3 screenPosition = SceneObjectManager.RegiontoScreen(f.RegionPosition);
        Vector3 objRectPos = new Vector3();
        Debug.Log("Feedback region pos: " + screenPosition);
        objRectPos.x = screenPosition.x - Screen.width / 2;
        objRectPos.y = screenPosition.y - Screen.height / 2;        
        objRectPos.x = objRectPos.x * (1200f / Screen.width);
        objRectPos.y = objRectPos.y * (900f / Screen.height);
        FeedbackObject.GetComponent<RectTransform>().localPosition = objRectPos;

        return FeedbackObject;
    }
    
}
