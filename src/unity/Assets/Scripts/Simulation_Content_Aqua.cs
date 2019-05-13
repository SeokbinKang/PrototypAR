using UnityEngine;
using System.Collections;
using OpenCvSharp;
using OpenCvSharp.Blob;
using OpenCvSharp.CPlusPlus;
using System.Collections.Generic;

using System.IO;
using System.Text;

using System.Runtime.Serialization.Formatters.Binary;
using System;

public class Simulation_Content_Aqua : MonoBehaviour {

    public GameObject[] FishPrefabs;
    public GameObject[] BacteriaPrefabs;
    public GameObject[] PlantPrefabs;
    public GameObject[] AirPumpPrefabs;

    public float Oxygen;
    public float Ammonia;
    public float Nitrate;
    public GameObject StatsObject;
    public GameObject bubbleBG;
    private List<Aqua_Element> mFish;
    private List<Aqua_Element> mBacteria;
    private List<Aqua_Element> mPlant;
    private List<Aqua_Element> mAirPump;

    // Use this for initialization
    private bool enable=false;
	void Start () {
       /* mFish = new List<Aqua_Element>();
        mBacteria = new List<Aqua_Element>();
        mPlant = new List<Aqua_Element>();
        mAirPump = new List<Aqua_Element>();
        Oxygen = 500;
        Ammonia = 500;
        Nitrate = 500;*/
	}
	
	// Update is called once per frame
	void Update () {
        Simulate();
    }
    private void initLocal()
    {
        mFish = new List<Aqua_Element>();
        mBacteria = new List<Aqua_Element>();
        mPlant = new List<Aqua_Element>();
        mAirPump = new List<Aqua_Element>();
        Oxygen = 200;
        Ammonia = 200;
        Nitrate = 200;
    }
    private void Simulate()
    {
        float oxygenConsumed = 0;
        float ammoniaGenerated = 0;
        float ammoniaReduce = 0;
        float nitrateGenerated = 0;
        float oxygenSupplied = 0;
        float nitratedConsumed = 0;
        if (mFish != null)
        {
            foreach (var f in mFish)
            {
                ammoniaGenerated += f.simulatonAmount;
                oxygenConsumed += f.simulatonAmount;
            }
        }
        if (mBacteria != null)
        {
            foreach (var b in mBacteria)
            {
                ammoniaReduce += b.simulatonAmount;
                nitrateGenerated += b.simulatonAmount * 3.0f;
            }
        }
        if (mPlant != null)
        {
            foreach (var p in mPlant)
            {
                nitratedConsumed += p.simulatonAmount;
            }
        }
        if (mAirPump != null)
        {
            foreach (var a in mAirPump)
            {
                oxygenSupplied = a.simulatonAmount * 10;
            }
        }
        Debug.Log("Oxygen Supply:" + oxygenSupplied + "\t ammonia +:" + ammoniaGenerated + "\t ammonia -:" + ammoniaReduce + "\t nitrated +:" + nitrateGenerated + "\t nitrate -:" + nitratedConsumed);
        this.Oxygen += (oxygenSupplied - oxygenConsumed)*Time.deltaTime;
        this.Ammonia += (ammoniaGenerated - ammoniaReduce) * Time.deltaTime;
        this.Nitrate += (nitrateGenerated - nitratedConsumed) * Time.deltaTime;

      

        updateState();
    }
    private void updateState()
    {
        if (StatsObject == null) return;
        HorizontalBar hb = StatsObject.GetComponent<HorizontalBar>();
        if (hb == null) return;
        if (this.Oxygen > 650) this.Oxygen = 650;
        if (this.Ammonia > 800)
        {
            this.Ammonia = 800;
            hb.SetBlink(1);
        }
        if (this.Nitrate > 800)
        {
            this.Nitrate = 800;
            hb.SetBlink(2);
        }
        if(Oxygen < 150) hb.SetBlink(0);
        hb.updateLevel(0, Oxygen * 100 / 1000f);
        hb.updateLevel(1, Ammonia * 100 / 1000f);
        hb.updateLevel(2, Nitrate * 100 / 1000f);


        
    }
    public void InitScene(prototypeDef userPrototype)
    {
        //iterate though objlist
        if (userPrototype.mModels == null) return;

        foreach (var designItem in userPrototype.mModels)
        {
            if (designItem.Value == null) continue;
            foreach (var modelInstance in designItem.Value)
            {
                //Instantiabe Obejct
                InstantiateObject(modelInstance);
                //add created object to a mOBJECT pool
            }
        }
    }
    private void InstantiateObject(ModelDef model)
    {
        GameObject[] Prefabs = null;
        List<Aqua_Element> objPool = null;
        if (this.mFish == null) initLocal();
        if (model.modelType == ModelCategory.Fish)
        {
            Prefabs = FishPrefabs;
            objPool = this.mFish;
        } else if (model.modelType == ModelCategory.Bacteria)
        {
            Prefabs = BacteriaPrefabs;
            objPool = this.mBacteria;
        }
        else if (model.modelType == ModelCategory.Plant)
        {
            Prefabs = PlantPrefabs;
            objPool = this.mPlant;
        }
        else if (model.modelType == ModelCategory.AirPump)
        {
            Prefabs = AirPumpPrefabs;
            objPool = this.mAirPump;
        }
        if (Prefabs == null) return;
        CvRect modelbbox = model.getShapeBuilder().bBox;
        float modelASratio = ((float)modelbbox.Height) / ((float)modelbbox.Width);
        GameObject closestPrefab=null;
        float ASRatioDistance = float.MaxValue;
        foreach(var prefab in Prefabs)
        {            
            SpriteRenderer sr = prefab.GetComponent<SpriteRenderer>();
            if(sr==null)
            {
                Debug.Log("[ERROR] No SpriteRendered found");
                continue;
            }
            Debug.Log("Sprite size : W: " + sr.sprite.rect.width + " H:" + sr.sprite.rect.height);
            float prefab_ASRatio = sr.sprite.rect.height / sr.sprite.rect.width;
            if(Mathf.Abs(prefab_ASRatio - modelASratio)<ASRatioDistance)
            {
                closestPrefab = prefab;
                ASRatioDistance = Mathf.Abs(prefab_ASRatio - modelASratio);
            }
        }
        if (closestPrefab == null) return;
        Vector3 goCenter = new Vector3();
        Vector3 goSize = new Vector3();
        SceneObjectManager.MeasureObjectInfoinScreenCoord(closestPrefab, ref goCenter, ref goSize);
        Vector3 userObjBoxLT = SceneObjectManager.RegiontoScreen(modelbbox.TopLeft);
        Vector3 userObjBoxRB = SceneObjectManager.RegiontoScreen(modelbbox.BottomRight);
        Vector3 userObjCenter = SceneObjectManager.RegiontoScreen(model.getShapeBuilder().center);
        Vector3 userobjSize = userObjBoxRB - userObjBoxLT;
        userobjSize.z = 0;
        userobjSize.y = userObjBoxLT.y - userObjBoxRB.y;
        //Debug.Log("[DEBUG simulation] userbox " + modelbbox.TopLeft+"\t"+userObjBoxLT + "|||||||| \t"+modelbbox.BottomRightuserObjBoxRB);
        Debug.Log("[DEBUG simulation] type : " + Content.getOrganName(model.modelType) + " go size " + goSize + "\t use size" + userobjSize);
        float scaleRatio;
        float sc = 0;
        float goDiagLength = goSize.magnitude;
        float userobjDiagLength = userobjSize.magnitude;
        Debug.Log("[DEBUG-SIMULATION] Fitting diag size user: " + userobjDiagLength + " virtual: " + goDiagLength);
        scaleRatio = userobjDiagLength / goDiagLength;       

        UnityEngine.GameObject go = Instantiate(closestPrefab, Camera.main.ScreenToWorldPoint(userObjCenter), Quaternion.identity) as GameObject;
        go.transform.localScale = go.transform.localScale * scaleRatio;
        go.transform.parent = this.gameObject.transform;

        objPool.Add(new Aqua_Element(userobjDiagLength, go));
    }
}
public class Aqua_Element
{
    GameObject go;
    float blobDiagLength;
    public float simulatonAmount;
    public Aqua_Element()
    {
        go = null;
        blobDiagLength = 0;
        simulatonAmount = 0;
    }
    public Aqua_Element(float diagLength, GameObject go_)
    {
        float paramSimulationMin = 1.0f;
        float paramSimulationMax = 20.0f;
        float paramdiagmin = GlobalRepo.getParamInt("minBloxPixelSize");
        float paramdiagmax = 1200;

        go = go_;
        blobDiagLength = diagLength;
        simulatonAmount = Mathf.Lerp(paramSimulationMin, paramSimulationMax, diagLength / paramdiagmax);
    }

}