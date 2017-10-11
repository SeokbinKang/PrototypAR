using UnityEngine;
using System.Collections;
using OpenCvSharp;
using OpenCvSharp.Blob;
using OpenCvSharp.CPlusPlus;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

public class Simulation : MonoBehaviour {

    public GameObject Simulation3_Aqua = null;
    private static Dictionary<ModelCategory, GameObject> InactiveGameObjectDict = null;
    private static Dictionary<ModelCategory, SimulationObjectType> SimulationTypeDict = null;
    // Use this for initialization
    private static List<GameObject> SimulationActiveObject;
    public prototypeDef userPrototypeInstance = null;
    private bool SimulationControlDone = false;
    public static Simulation ActiveInstance;
    

    private DesignContent mDesignContent;
    private SimulationState simState;
    void Start () {
        initGameObjectPool();
        initSimulationConfig();
        userPrototypeInstance = null;
        SimulationControlDone = false;
        ActiveInstance = this;
        simState = new SimulationState();
        mDesignContent = this.GetComponentInParent<ApplicationControl>().ContentType;
    }
	
	// Update is called once per frame
	void Update () {
        simState.timeElapsed = simState.timeElapsed+Time.deltaTime;
        //increase alpha

        //syn animation

        //update simulation context
        RevealSimulationObject();
        
        if(mDesignContent==DesignContent.HumanRespiratorySystem)   ControlSimulationContent_1();
            else if (mDesignContent == DesignContent.BicycleGearSystem) ControlSimulationContent_2(); 
    }
    public void reset()
    {
        DestroySimulationContent_2();        
        initGameObjectPool();
        userPrototypeInstance = null;
        SimulationControlDone = false;
        ActiveInstance = this;
        simState = new SimulationState();
        mDesignContent = this.GetComponentInParent<ApplicationControl>().ContentType;
    }
    private void ControlSimulationContent_1()
    {
        if (userPrototypeInstance == null || SimulationActiveObject.Count == 0) return;
        //calculate Breathing rate and Amount

        //breathing rate
        if (!SimulationControlDone)
        {
            SimulationParam sp = new SimulationParam();
            Content.ExtractSimulationParameters(userPrototypeInstance, FBSModel.ContentType, ref sp);
            sp.C1_breathingRate = 10;  //test
                                       //all the animation's priods are set to 5 sec. BR = 10
                                       //reference anim speed =  8 sec(period). 60/8=7.5
            float animationSpeed = sp.C1_breathingRate / 10f;
            simState.c1_halfPeriod = 30 / sp.C1_breathingRate;
            simState.c1_speedBaseline = animationSpeed;
            simState.c1_speedCurrent = animationSpeed;
            simState.timeElapsed = 0;
            if (sp.C1_breathingRate != 0)
            {
                //init breathing rate
                for (int i = 0; i < SimulationActiveObject.Count; i++)
                {
                    //SpriteRenderer sr = SimulationActiveObject[i].GetComponent<SpriteRenderer>();
                    SetSpriteAlphato(SimulationActiveObject[i], 0);
                    for (int c = 0; c < SimulationActiveObject[i].transform.childCount; c++)
                    {
                        GameObject child = SimulationActiveObject[i].transform.GetChild(c).gameObject;
                        Animator animc = child.GetComponent<Animator>();
                        if (animc == null ) continue;
                  //      if (animc.GetCurrentAnimatorStateInfo(0).normalizedTime <= 0 || animc.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1) continue;
                        Debug.Log("set Anim param ->" + child.name +"speed:"+ simState.c1_speedCurrent);
                        child.SetActive(true);
                        animc.SetFloat("speedparam", simState.c1_speedCurrent);
                        SetSpriteAlphato(SimulationActiveObject[i], 0);

                    }
                    //control animation speed

                    //activate animtions
                }
            }
            SimulationControlDone = true;
            

        }
        else
        {
            if (simState.timeElapsed >= simState.c1_halfPeriod)
            {
                simState.timeElapsed = 0;
                simState.c1_speedCurrent *= -1;
            }
            bool BreathIn = (simState.c1_speedCurrent > 0) ? true : false;
            for (int i = 0; i < SimulationActiveObject.Count; i++)
            {
                for (int c = 0; c < SimulationActiveObject[i].transform.childCount; c++)
                {
                    GameObject child = SimulationActiveObject[i].transform.GetChild(c).gameObject;
                    Animator animc = child.GetComponent<Animator>();
                    if (animc == null) continue;
           //         if (animc.GetCurrentAnimatorStateInfo(0).normalizedTime <= 0 || animc.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1) continue;
                    animc.SetFloat("speedparam", simState.c1_speedCurrent);
                    // if (animc.GetCurrentAnimatorStateInfo(0).IsName("forward")) BreathIn = true;
                    //else BreathIn = false;
                    SceneObjectManager.adjustAlphaSpriteRendere(child, 255,0.004f);

                   
                }
            }
            Color t = new Color(); ;
            if (BreathIn)
            {
                t.r = 1;
                t.g = 1;
                t.b = 1;
                t.a = 1;
            }
            else
            {
                t.r = 0.93f;
                t.g = 0.2f;
                t.b = 0.2f;
                t.a = 1;
            }
            GameObject psystem = GameObject.Find("c1_airPRight");       
            psystem.GetComponent<BLINDED_AM_ME.Path_Comp>().Update_Path(BreathIn);
            psystem.GetComponent<ParticleSystemRenderer>().material.color = t;

            psystem = GameObject.Find("c1_airPLeft");
            psystem.GetComponent<BLINDED_AM_ME.Path_Comp>().Update_Path(BreathIn);
            psystem.GetComponent<ParticleSystemRenderer>().material.color = t;

          
           
         
        }
    }
    private void ControlSimulationContent_2()
    {
        if (userPrototypeInstance == null || SimulationActiveObject.Count == 0) return;
        //calculate Breathing rate and Amount

        //breathing rate
        if (!SimulationControlDone)
        {
            SimulationParam sp = new SimulationParam();
            Content.ExtractSimulationParameters(userPrototypeInstance, mDesignContent, ref sp);
            sp.C2_pedallingRate = 30;  //test
                                       //all the animation's priods are set to 6 sec. BR = 10
            sp.C2_rearGearSize =  Mathf.Sqrt((float)userPrototypeInstance.getModelDefbyType(ModelCategory.RearSprocket).AreaSize);
            sp.C2_frontGearSize =  Mathf.Sqrt((float)userPrototypeInstance.getModelDefbyType(ModelCategory.FrontChainring).AreaSize);
            float animationSpeedParam = sp.C2_pedallingRate / 12.0f;
            //1 = 1rotation / 5 sec            
            simState.timeElapsed = 0;
            if (animationSpeedParam != 0)
            {
                //init breathing rate
                for (int i = 0; i < SimulationActiveObject.Count; i++)
                {
                    Animator animc = SimulationActiveObject[i].GetComponent<Animator>();
                    if (animc != null)
                    { 
                        animc.SetFloat("speedparam", animationSpeedParam);
                        continue;
                    }
                    Simulation_Artifact_Chain chainController = SimulationActiveObject[i].GetComponent<Simulation_Artifact_Chain>();
                    if (chainController != null)
                    {
                        chainController.speedparam = animationSpeedParam;
                    }

                    //control animation speed

                    //activate animtions
                }
            }
            SimulationControlDone = true;

            //add the prototype to prototypeInstanceManager
            GameObject go_multiview = GameObject.Find("MultiviewUI");
            PrototypeInstanceManager t = go_multiview.GetComponent<PrototypeInstanceManager>();

            CvRect regionBox = GlobalRepo.GetRegionBox(false);
            Texture2D temporaryTexture=null;
            if (temporaryTexture == null || temporaryTexture.width != regionBox.Width || temporaryTexture.height != regionBox.Height)
            {
                temporaryTexture = new Texture2D(regionBox.Width, regionBox.Height, TextureFormat.RGBA32, false);
            }
            temporaryTexture.LoadRawTextureData(GlobalRepo.getByteStream(RepoDataType.dRawRegionRGBAByte));
            temporaryTexture.Apply();
            if (temporaryTexture != null) {
                t.AddcompletePrototypeInstance(temporaryTexture, sp);
            }
        }     
    }
    private void DestroySimulationContent_2()
    {
        if (userPrototypeInstance == null || SimulationActiveObject.Count == 0) return;
        //calculate Breathing rate and Amount

                float animationSpeedParam = 0;
        //1 = 1rotation / 5 sec            
        simState.timeElapsed = 0;       
            //init breathing rate
            for (int i = 0; i < SimulationActiveObject.Count; i++)
            {
                Animator animc = SimulationActiveObject[i].GetComponent<Animator>();
                if (animc != null)
                {
                    animc.SetFloat("speedparam", animationSpeedParam);
                    continue;
                }
                Simulation_Artifact_Chain chainController = SimulationActiveObject[i].GetComponent<Simulation_Artifact_Chain>();
                if (chainController != null)
                {
                    chainController.speedparam = animationSpeedParam;
                  chainController.destoryChain();
                    //destory chain
                }
                //control animation speed
                //activate animtions
            }

        InactiveGameObjectDict[ModelCategory.PedalCrank].transform.position = new Vector3(6, 2, 1);
        InactiveGameObjectDict[ModelCategory.RearSprocket].transform.position = new Vector3(6, 2, 1);        
        InactiveGameObjectDict[ModelCategory.FrontChainring].transform.position = new Vector3(6, 2, 1);
        InactiveGameObjectDict[ModelCategory.RearSprocket].transform.localScale= new Vector3(0.1f, 0.1f, 1);
        InactiveGameObjectDict[ModelCategory.FrontChainring].transform.localScale = new Vector3(0.1f, 0.1f, 1);
        InactiveGameObjectDict[ModelCategory.PedalCrank].transform.localScale = new Vector3(0.1f, 0.1f, 1);
        SetActiveRecursively(InactiveGameObjectDict[ModelCategory.FrontChainring], true);
        SetActiveRecursively(InactiveGameObjectDict[ModelCategory.RearSprocket], true);


    }

    private void RevealSimulationObject()
    {
        for(int i=0;i<SimulationActiveObject.Count;i++)
        {
            SetSpriteAlphaIncrease(SimulationActiveObject[i], 0.008f);            
        }
    }
    private void HideSimulationObjects()
    {
        for (int i = 0; i < SimulationActiveObject.Count; i++)
        {
          //  SetSpriteAlphato(SimulationActiveObject[i], 0);
           // SimulationActiveObject[i].SetActive(false);
        }
    }
    public static void GenerateSimulation(prototypeDef userPrototype, Visual2DModel visual2DMgr)
    {
        //iterate though objlist
        if (userPrototype.mModels == null) return ;
        if (ActiveInstance.mDesignContent == DesignContent.AquariumEcology)
        {
            if (ActiveInstance.Simulation3_Aqua == null) return;
            ActiveInstance.Simulation3_Aqua.SetActive(true);
            Simulation_Content_Aqua c3 = ActiveInstance.Simulation3_Aqua.GetComponent<Simulation_Content_Aqua>();
            if (c3 == null) return;
            c3.InitScene(userPrototype);
            return;
        }
        foreach (var designItem in userPrototype.mModels)
        {
            if (designItem.Value == null) continue;
            foreach (var modelInstance in designItem.Value)
            {
                //if static model 
                if(SimulationTypeDict.ContainsKey(designItem.Key) && SimulationTypeDict[designItem.Key]==SimulationObjectType.LoadStaticModelConnect)
                {
                    SimulateStaticModel(modelInstance,false,true);
                }
                if (SimulationTypeDict.ContainsKey(designItem.Key) && SimulationTypeDict[designItem.Key] == SimulationObjectType.LoadStaticModelAdaptAspectNoConnect)
                {
                    SimulateStaticModel(modelInstance,true,false);
                }
                if (SimulationTypeDict.ContainsKey(designItem.Key) && SimulationTypeDict[designItem.Key] == SimulationObjectType.TextureTransfer)
                {
                    visual2DMgr.CreateTexturedLoadedObject(modelInstance);                  
                }
                if (SimulationTypeDict.ContainsKey(designItem.Key) && SimulationTypeDict[designItem.Key] == SimulationObjectType.LoadStaticModelMultiChild)
                {
                    //front chainring
                    //free wheel
                    SimulateStaticModelMultiChild(modelInstance, true, false);
                }
                if (SimulationTypeDict.ContainsKey(designItem.Key) && SimulationTypeDict[designItem.Key] == SimulationObjectType.LoadCustomArtifact)
                {
                    SimulateCustomModel(modelInstance, userPrototype);
                }
                if (SimulationTypeDict.ContainsKey(designItem.Key) && SimulationTypeDict[designItem.Key] == SimulationObjectType.LoadStaticModelFixedPosNoConnect)
                {
                    Debug.Log("Loading Pedal Crank....1");
                    SimulateStaticModel(modelInstance, false, false,ModelCategory.FrontChainring);
                }
                //
            }
            //generate
            
        }
        foreach (var designItem in userPrototype.mModels)
        {
            if (designItem.Value == null) continue;
            foreach (var modelInstance in designItem.Value)
            {
                initSimulationContext(modelInstance);
                //
            }
            //generate

        }
        Simulation.ActiveInstance.userPrototypeInstance = userPrototype;
       
    }
   
    private static void initSimulationContext(ModelDef model)
    {
        
        if (model.modelType==ModelCategory.Airways)
        {
            initSimulationContext_Airwyas(model);
        }
    }

    //recursive
    private static void SetSpriteAlphato(GameObject go,float alpha)
    {
        if (go == null) return;
        SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Color spriteColor = sr.color;
            spriteColor.a = alpha;
            sr.color = spriteColor;
        }
        for(int i = 0; i < go.transform.childCount; i++)
        {
            GameObject child = go.transform.GetChild(i).gameObject;
            if (child != null) SetSpriteAlphato(child, alpha);
        }
        
    }
    private static void SetSpriteAlphaIncrease(GameObject go, float alpha)
    {
        if (go == null) return;
        SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Color spriteColor = sr.color;
            spriteColor.a = spriteColor.a + alpha;
            sr.color = spriteColor;
        }
        for (int i = 0; i < go.transform.childCount; i++)
        {
            GameObject child = go.transform.GetChild(i).gameObject;
            if (child != null) SetSpriteAlphaIncrease(child, alpha);
        }

    }
    private static CvPoint[] getVerticalCenterPoints(Point[] points, int N_probePoints)
    {
        CvPoint[] ret = null;
        points = points.OrderByDescending(x => x.Y).ToArray<Point>();
        int verticalScanCount = N_probePoints;
        int yMax = points[0].Y;
        int yMin = points[points.Length - 1].Y;
        int yStep = Mathf.Abs(yMax - yMin) / verticalScanCount;
        int scanidx = 0;       
       
        ArrayList pointArr = new ArrayList();
        for (int scan_y = yMax; scan_y >= yMin; scan_y -= yStep)
        {

            int sumX = 0;
            int count = 0;
            while (scanidx < points.Length-1 && points[scanidx].Y <= scan_y && points[scanidx].Y > (scan_y - yStep))
            {
                int ygap = points[scanidx].Y - points[scanidx + 1].Y;
                if (ygap<2 && ygap>-2)
                {
                    int xgap = points[scanidx].X - points[scanidx + 1].X;
                    if(xgap>30 || xgap <-30)
                    {
                        sumX += points[scanidx].X;
                        sumX += points[scanidx+1].X;
                        count += 2;
                        scanidx += 2;
                        continue;

                    }
                }                
                
                scanidx++;
            }
            if (count == 0) continue;
            int avgX = sumX / count;

            pointArr.Add(new CvPoint(avgX, scan_y - yStep / 2));

        }
        if (pointArr.Count > 0) ret = pointArr.ToArray(typeof(CvPoint)) as CvPoint[];
        return ret;
    }
    private static void initSimulationContext_Airwyas(ModelDef model)
    {
        //analyze anchors in airways
        Point[] modelContour = model.getShapeBuilder().Fullcontour;
        CvPoint p1_airIn;
        CvPoint p2_airDiverge;
        CvPoint p3_airDissolve;
        modelContour = modelContour.OrderByDescending(x => x.Y).ToArray<Point>();
        int verticalScanCount = 6;
        int yMax = modelContour[0].Y;
        int yMin = modelContour[modelContour.Length-1].Y;
        int yStep = Mathf.Abs(yMax - yMin) / verticalScanCount;
        int scanidx = 0;
        CvMat arimg = GlobalRepo.GetRepo(RepoDataType.pRealityARRegionRGBA);
        CvScalar color = new CvScalar(255, 0, 0, 255);
        ArrayList pointArr = new ArrayList();
        for(int scan_y = yMax; scan_y >= yMin; scan_y-=yStep)
        {
            int sumX=0;
            int count = 0;
            while(scanidx < modelContour.Length && modelContour[scanidx].Y <=scan_y && modelContour[scanidx].Y > (scan_y - yStep))
            {
                sumX += modelContour[scanidx].X;
                count++;
                scanidx++;
            }
            if (count == 0) continue;
            int avgX = sumX / count;
            
            pointArr.Add(new CvPoint(avgX, scan_y - yStep / 2));
                       
        }
        p1_airIn = (CvPoint) pointArr[pointArr.Count - 1];
        p2_airDiverge = (CvPoint)pointArr[1];

        CvPoint[] cPoints = getVerticalCenterPoints(modelContour, 6);
        if (cPoints != null)
        {
            Debug.Log("SSSSSSSSSSSSSSS" + cPoints.Length);
            if(cPoints.Length>4)
            {
                p1_airIn = (CvPoint)cPoints[cPoints.Length - 1];
                p2_airDiverge = (CvPoint)cPoints[2];
            }
        }
        if (cPoints != null)
        {
            foreach (var p in cPoints)
                arimg.DrawCircle(p, 5, color, -1);
        }
        //arimg.DrawCircle(p2_airDiverge, 5, color, -1);
        GlobalRepo.showDebugImage("AIRWAYS", arimg);

        //right lung
        //p3_airDissolve;

        Vector3 p1 = SceneObjectManager.regionToWorld(p1_airIn);
        Vector3 p2 = SceneObjectManager.regionToWorld(p2_airDiverge);
        
        Vector3 p3 = getGameObject(ModelCategory.LungRight, true).transform.position;
        GameObject psystem = getGameObject(ModelCategory.AirParticleRight,true);
        GameObject go_p1 = psystem.transform.FindChild("point 0").gameObject;
        GameObject go_p2 = psystem.transform.FindChild("point 1").gameObject;
        GameObject go_p3 = psystem.transform.FindChild("point 2").gameObject;

        go_p1.transform.position = p1;
        go_p2.transform.position = p2;
        go_p3.transform.position = p3;

        psystem.GetComponent<BLINDED_AM_ME.Path_Comp>().Update_Path(true);


        //left lung
        p3 = getGameObject(ModelCategory.LungLeft, true).transform.position;
        psystem = getGameObject(ModelCategory.AirParticleLeft, true);
        go_p1 = psystem.transform.FindChild("point 0").gameObject;
        go_p2 = psystem.transform.FindChild("point 1").gameObject;
        go_p3 = psystem.transform.FindChild("point 2").gameObject;

        go_p1.transform.position = p1;
        go_p2.transform.position = p2;
        go_p3.transform.position = p3;

        psystem.GetComponent<BLINDED_AM_ME.Path_Comp>().Update_Path(true);


    }
    private static void SimulateCustomModel(ModelDef model , prototypeDef userproto)
    {
        if (model.modelType == ModelCategory.UpperChain || model.modelType == ModelCategory.LowerChain || model.modelType == ModelCategory.Chain)
        {
            GameObject chain_go = getGameObject(model.modelType,true);
            if (chain_go == null) return;
            Simulation_Artifact_Chain chain_sim = chain_go.GetComponent<Simulation_Artifact_Chain>();
            if (chain_sim == null) return;
            if(FBSModel.activeFBSInstance.getModelsVirtualPosType(model)==VirtualPosType.SyncwithPhysical)
                chain_sim.InitChain(model, userproto);
            else if (FBSModel.activeFBSInstance.getModelsVirtualPosType(model) == VirtualPosType.Stableasfixed)
            {
                //connect chain to pre-loaded gears
                GameObject frontgear = InactiveGameObjectDict[ModelCategory.FrontChainring];
                GameObject reargear = InactiveGameObjectDict[ModelCategory.RearSprocket];
                chain_sim.InitChain(model, frontgear,reargear);
            }
                SetSpriteAlphato(chain_go, 0f);
            SimulationActiveObject.Add(chain_go);


        }
        //for various finalvis type

    }
    public static void SetActiveRecursively(GameObject rootObject, bool active)
    {
        rootObject.SetActive(active);

        foreach (Transform childTransform in rootObject.transform)
        {
            SetActiveRecursively(childTransform.gameObject, active);
        }
    }
    //pick a childe of which default size is most close to the user model
    //

    private static void SimulateStaticModelMultiChild(ModelDef model, bool fitAspect, bool adjustPivottoConnect)
    {
        if (model == null) return;
        if (getGameObject(model.modelType,true) == null)
        {
            Debug.Log("[ERROR] Cannot find GameObject in SimulateStaticModel");
            return;
        }

        //user object
        ObjectShape2DBuilder userobjGeometry = model.getShapeBuilder();
        if (userobjGeometry == null) return;
        CvRect userobjBox = userobjGeometry.bBox;
        Vector3 userObjBoxLT = SceneObjectManager.RegiontoScreen(userobjBox.TopLeft);
        Vector3 userObjBoxRB = SceneObjectManager.RegiontoScreen(userobjBox.BottomRight);
        Vector3 userobjSize = userObjBoxRB - userObjBoxLT;
        userobjSize.y = userObjBoxLT.y - userObjBoxRB.y;



        GameObject go = getGameObject(model.modelType,true);
        if (go == null) return;
        
        GameObject go_child = null;
        Vector3 goCenter = new Vector3();
        Vector3 goSize = new Vector3();
        Vector3 goCenter_iter = new Vector3();
        GameObject go_child_iter = null;
        Vector3 goSize_iter = new Vector3();
        Vector3 scaleRatio_iter = new Vector3();
        float minSizeGap = float.MaxValue;
        float sizeGap_iter;

        if (GlobalRepo.Setting_ShowDebugImgs()) Debug.Log("[DEBUG-SIMULATION] looking for most similar preloaded object");
        for (int i=0;i<go.transform.childCount;i++)
        {
            go_child_iter = go.transform.GetChild(i).gameObject;
            if (go_child_iter == null) continue;
            
            
            SceneObjectManager.MeasureObjectInfoinScreenCoord(go_child_iter, ref goCenter_iter, ref goSize_iter);
            sizeGap_iter = Mathf.Abs(goSize_iter.x * goSize_iter.y - userobjSize.x * userobjSize.y);
            if (GlobalRepo.Setting_ShowDebugImgs())
            {
    //              Debug.Log("[DEBUG-SIMULATION] child object #" + i + " size gap: " + sizeGap_iter);
       //         Debug.Log("[DEBUG-SIMULATION] child object size in Screen " + goSize_iter);
            }
            if (sizeGap_iter< minSizeGap)
            {
                minSizeGap = sizeGap_iter;
                go_child = go_child_iter;
                goCenter = goCenter_iter;
                goSize = goSize_iter;
            }
            go_child_iter.SetActive(false);
        }
        if (go_child == null) return;
        go_child.SetActive(true);
       // go = go_child;
        //find game object
        //find bounding rect size
        Debug.Log("[DEBUG simulation] type : "+Content.getOrganName(model.modelType)+" go size " + goSize + "\t use size" + userobjSize);
        SpriteRenderer sr = go_child.GetComponent<SpriteRenderer>();
        float sc = 0;
        Vector3 scaleRatio = new Vector3();
        if (fitAspect)
        {
            scaleRatio.x = Mathf.Abs(userobjSize.x / goSize.x);
            scaleRatio.y = Mathf.Abs(userobjSize.y / goSize.y);

        }
        else
        {
            scaleRatio.x = (Mathf.Abs(userobjSize.x / goSize.x) + Mathf.Abs(userobjSize.y / goSize.y)) / 2.0f;
            scaleRatio.y = scaleRatio.x;
            sc = (Mathf.Abs(userobjSize.x / goSize.x) + Mathf.Abs(userobjSize.y / goSize.y)) / 2.0f;
        }

        Vector2 spritePivot = sr.sprite.pivot;
        UnityEngine.Rect spriteRect = sr.sprite.rect;
        spritePivot.x = spritePivot.x / spriteRect.width;
        spritePivot.y = spritePivot.y / spriteRect.height;
        Vector3 userobjPivotinScreen;
        Vector2 pivotChangeScreenCoord = new Vector2(0, 0);
        if (adjustPivottoConnect && (model.ConnPoint.X != 0 && model.ConnPoint.Y != 0))
        {
            CvPoint objBboxCenter = (model.getShapeBuilder().bBox.TopLeft + model.getShapeBuilder().bBox.BottomRight);
            objBboxCenter.X = objBboxCenter.X / 2;
            objBboxCenter.Y = objBboxCenter.Y / 2;
            CvPoint connVector = model.ConnPoint - objBboxCenter;
            Vector2 connVectorNormal = new Vector2(connVector.X, connVector.Y * -1);
            Vector2 originalPivot = spritePivot;
            Vector2 connPivot = SceneObjectManager.MeasureObjectPivotwithDirectionVector(go, connVectorNormal);
            Vector2 PivotChange = connPivot - originalPivot;

            //   Debug.Log("Pivot Change to :"+ PivotChange);
            Vector3 adjustedGoSize = goSize;
            adjustedGoSize.x = adjustedGoSize.x * scaleRatio.x;
            adjustedGoSize.y = adjustedGoSize.y * scaleRatio.y;
            pivotChangeScreenCoord.x = adjustedGoSize.x * PivotChange.x;
            pivotChangeScreenCoord.y = adjustedGoSize.y * PivotChange.y;
            Vector3 connScreenPoint = SceneObjectManager.RegiontoScreen(model.ConnPoint);
            userobjPivotinScreen.x = connScreenPoint.x - pivotChangeScreenCoord.x;
            userobjPivotinScreen.y = connScreenPoint.y - pivotChangeScreenCoord.y;
            userobjPivotinScreen.z = 1;
        }
        else
        {
            userobjPivotinScreen.x = userObjBoxLT.x + userobjSize.x * spritePivot.x;
            userobjPivotinScreen.y = userObjBoxRB.y + userobjSize.y * spritePivot.y;
            userobjPivotinScreen.z = 1;
        }
        if (FBSModel.activeFBSInstance.getModelsVirtualPosType(model) == VirtualPosType.Stableasfixed)
        {
            userobjPivotinScreen = FBSModel.activeFBSInstance.getClosestModelsTruthScreenPos(model);
        }

        Vector3 scale = go.transform.localScale;
        //   Debug.Log("[DEBUG SIMULATION] BEFORE scale " + sc + "\t" + scaleRatio + "\t" + scale);
        scale.x = scale.x * scaleRatio.x;
        scale.y = scale.y * scaleRatio.y;
        //   Debug.Log("[DEBUG SIMULATION] AFTER scale " + sc + "\t" + scaleRatio + "\t" + scale);
        //   Debug.Log("[DEBUG SIMULATION] AFTER scale " + sc + "\t" + scaleRatio.y + "\t" + scale.y);
        //    Debug.Log("[DEBUG simulation] type : " + Content.getOrganName(model.modelType) + " go size " + goSize + "\t use size" + userobjSize);
        go.transform.localScale = scale;


        go.transform.position = Camera.main.ScreenToWorldPoint(userobjPivotinScreen);
        //scale and move using shape-evaluation result

        //move model to the object. has to exactly fit with object guideline

        //adjust 'connectivity'
        SetSpriteAlphato(go_child, 0f);
        SimulationActiveObject.Add(go_child);
    }

    private static void SimulateStaticModel(ModelDef model, bool fitAspect, bool adjustPivottoConnect, ModelCategory alignModel)
    {
        if (model == null) return;
        if (InactiveGameObjectDict[model.modelType]==null || InactiveGameObjectDict[alignModel] == null)
        {
            Debug.Log("[ERROR] Cannot find GameObject in SimulateStaticModel");
            return;
        }
        GameObject go = getGameObject(model.modelType,true);
        Vector3 goCenter = new Vector3();
        Vector3 goSize = new Vector3();
        SceneObjectManager.MeasureObjectInfoinScreenCoord(go, ref goCenter, ref goSize);

        //find game object
        SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
        if (sr == null) return;
        //find bounding rect size
        ObjectShape2DBuilder userobjGeometry = model.getShapeBuilder();
        if (userobjGeometry == null) return;
        CvRect userobjBox = userobjGeometry.bBox;
        Vector3 userObjBoxLT = SceneObjectManager.RegiontoScreen(userobjBox.TopLeft);
        Vector3 userObjBoxRB = SceneObjectManager.RegiontoScreen(userobjBox.BottomRight);
        Vector3 userobjSize = userObjBoxRB - userObjBoxLT;
        userobjSize.z = 0;
        float userobjDiagLength = userobjSize.magnitude;
        userobjSize.y = userObjBoxLT.y - userObjBoxRB.y;
     //   Debug.Log("[DEBUG simulation] type : "+Content.getOrganName(model.modelType)+" go size " + goSize + "\t use size" + userobjSize);
        Vector3 scaleRatio = new Vector3();
        float sc=0;
        if(fitAspect)
        {
            scaleRatio.x = Mathf.Abs(userobjSize.x / goSize.x);
            scaleRatio.y = Mathf.Abs(userobjSize.y / goSize.y);
            scaleRatio.z = 1;
     
        } else
        {
            /* scaleRatio.x = (Mathf.Abs(userobjSize.x / goSize.x) + Mathf.Abs(userobjSize.y / goSize.y)) / 2.0f;
             scaleRatio.y = scaleRatio.x;
             sc = (Mathf.Abs(userobjSize.x / goSize.x) + Mathf.Abs(userobjSize.y / goSize.y)) / 2.0f;*/

            //scale to diagonal size
            Vector3 goSize2 = goSize;
            goSize2.z = 0;
            float goDiagLength = goSize2.magnitude;
            Debug.Log("[DEBUG-SIMULATION] Fitting diag size user: " + userobjDiagLength + " virtual: " + goDiagLength);
            scaleRatio.x = userobjDiagLength / goDiagLength;
            scaleRatio.y = scaleRatio.x;
            scaleRatio.z = 1;
        }        

        Vector2 spritePivot = sr.sprite.pivot;
        UnityEngine.Rect spriteRect = sr.sprite.rect;
        spritePivot.x = spritePivot.x / spriteRect.width;
        spritePivot.y = spritePivot.y / spriteRect.height;
        Vector3 userobjPivotinScreen;
        Vector2 pivotChangeScreenCoord = new Vector2(0, 0);
        if(adjustPivottoConnect && (model.ConnPoint.X!=0 && model.ConnPoint.Y != 0))
        {
            CvPoint objBboxCenter = (model.getShapeBuilder().bBox.TopLeft + model.getShapeBuilder().bBox.BottomRight);
            objBboxCenter.X = objBboxCenter.X / 2;
            objBboxCenter.Y = objBboxCenter.Y / 2;
            CvPoint connVector = model.ConnPoint - objBboxCenter;
            Vector2 connVectorNormal = new Vector2(connVector.X, connVector.Y * -1);
            Vector2 originalPivot = spritePivot;
            Vector2 connPivot = SceneObjectManager.MeasureObjectPivotwithDirectionVector(go, connVectorNormal);
            Vector2 PivotChange = connPivot - originalPivot;
            
         //   Debug.Log("Pivot Change to :"+ PivotChange);
                 Vector3 adjustedGoSize = goSize;
                 adjustedGoSize.x = adjustedGoSize.x * scaleRatio.x;
                 adjustedGoSize.y = adjustedGoSize.y * scaleRatio.y;
                 pivotChangeScreenCoord.x = adjustedGoSize.x * PivotChange.x;
                 pivotChangeScreenCoord.y = adjustedGoSize.y * PivotChange.y;
            Vector3 connScreenPoint = SceneObjectManager.RegiontoScreen(model.ConnPoint);
            userobjPivotinScreen.x = connScreenPoint.x - pivotChangeScreenCoord.x;
            userobjPivotinScreen.y = connScreenPoint.y - pivotChangeScreenCoord.y;
            userobjPivotinScreen.z = 1;


        } else {
            userobjPivotinScreen.x = userObjBoxLT.x + userobjSize.x * spritePivot.x;
            userobjPivotinScreen.y = userObjBoxRB.y + userobjSize.y * spritePivot.y;
            userobjPivotinScreen.z = 1;
        }
        if (FBSModel.activeFBSInstance.getModelsVirtualPosType(model) == VirtualPosType.Stableasfixed)
        {
            userobjPivotinScreen = FBSModel.activeFBSInstance.getClosestModelsTruthScreenPos(model);
        }
        Vector3 scale = go.transform.localScale;
     //   Debug.Log("[DEBUG SIMULATION] BEFORE scale " + sc + "\t" + scaleRatio + "\t" + scale);
        scale.x = scale.x  * scaleRatio.x;
        scale.y = scale.y * scaleRatio.y;
     //   Debug.Log("[DEBUG SIMULATION] AFTER scale " + sc + "\t" + scaleRatio + "\t" + scale);
     //   Debug.Log("[DEBUG SIMULATION] AFTER scale " + sc + "\t" + scaleRatio.y + "\t" + scale.y);
    //    Debug.Log("[DEBUG simulation] type : " + Content.getOrganName(model.modelType) + " go size " + goSize + "\t use size" + userobjSize);
        go.transform.localScale = scale;


        //go.transform.position = Camera.main.ScreenToWorldPoint(userobjPivotinScreen);
        go.transform.position = getGameObject(alignModel,true).transform.position;
        //scale and move using shape-evaluation result

        //move model to the object. has to exactly fit with object guideline

        //adjust 'connectivity'
        
        SetSpriteAlphato(go, 0f);
        SimulationActiveObject.Add(go);
        
    }
    private static void SimulateStaticModel(ModelDef model, bool fitAspect, bool adjustPivottoConnect)
    {
        if (model == null) return;
        if (InactiveGameObjectDict[model.modelType] == null)
        {
            Debug.Log("[ERROR] Cannot find GameObject in SimulateStaticModel");
            return;
        }
        GameObject go = getGameObject(model.modelType,true);
        Vector3 goCenter = new Vector3();
        Vector3 goSize = new Vector3();
        SceneObjectManager.MeasureObjectInfoinScreenCoord(go, ref goCenter, ref goSize);

        //find game object
        SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
        if (sr == null) return;
        //find bounding rect size
        ObjectShape2DBuilder userobjGeometry = model.getShapeBuilder();
        if (userobjGeometry == null) return;
        CvRect userobjBox = userobjGeometry.bBox;
        Vector3 userObjBoxLT = SceneObjectManager.RegiontoScreen(userobjBox.TopLeft);
        Vector3 userObjBoxRB = SceneObjectManager.RegiontoScreen(userobjBox.BottomRight);
        Vector3 userobjSize = userObjBoxRB - userObjBoxLT;
        userobjSize.z = 0;
        float userobjDiagLength = userobjSize.magnitude;
        userobjSize.y = userObjBoxLT.y - userObjBoxRB.y;
        //   Debug.Log("[DEBUG simulation] type : "+Content.getOrganName(model.modelType)+" go size " + goSize + "\t use size" + userobjSize);
        Vector3 scaleRatio = new Vector3();
        float sc = 0;
        if (fitAspect)
        {
            scaleRatio.x = Mathf.Abs(userobjSize.x / goSize.x);
            scaleRatio.y = Mathf.Abs(userobjSize.y / goSize.y);
            scaleRatio.z = 1;

        }
        else
        {
           scaleRatio.x = (Mathf.Abs(userobjSize.x / goSize.x) + Mathf.Abs(userobjSize.y / goSize.y)) / 2.0f;
             scaleRatio.y = scaleRatio.x;
             sc = (Mathf.Abs(userobjSize.x / goSize.x) + Mathf.Abs(userobjSize.y / goSize.y)) / 2.0f;

        }

        Vector2 spritePivot = sr.sprite.pivot;
        UnityEngine.Rect spriteRect = sr.sprite.rect;
        spritePivot.x = spritePivot.x / spriteRect.width;
        spritePivot.y = spritePivot.y / spriteRect.height;
        Vector3 userobjPivotinScreen;
        Vector2 pivotChangeScreenCoord = new Vector2(0, 0);
        Debug.Log("Model Conn point: " + model.ConnPoint);
        if (adjustPivottoConnect && (model.ConnPoint.X != 0 && model.ConnPoint.Y != 0))
        {
            CvPoint objBboxCenter = (model.getShapeBuilder().bBox.TopLeft + model.getShapeBuilder().bBox.BottomRight);
            objBboxCenter.X = objBboxCenter.X / 2;
            objBboxCenter.Y = objBboxCenter.Y / 2;
            CvPoint connVector = model.ConnPoint - objBboxCenter;
            Vector2 connVectorNormal = new Vector2(connVector.X, connVector.Y * -1);
            Vector2 originalPivot = spritePivot;
            Vector2 connPivot = SceneObjectManager.MeasureObjectPivotwithDirectionVector(go, connVectorNormal);
            Vector2 PivotChange = connPivot - originalPivot;

            Debug.Log("Pivot Change to :"+ PivotChange);
            Vector3 adjustedGoSize = goSize;
            adjustedGoSize.x = adjustedGoSize.x * scaleRatio.x;
            adjustedGoSize.y = adjustedGoSize.y * scaleRatio.y;
            pivotChangeScreenCoord.x = adjustedGoSize.x * PivotChange.x;
            pivotChangeScreenCoord.y = adjustedGoSize.y * PivotChange.y;
            Vector3 connScreenPoint = SceneObjectManager.RegiontoScreen(model.ConnPoint);
            userobjPivotinScreen.x = connScreenPoint.x - pivotChangeScreenCoord.x;
            userobjPivotinScreen.y = connScreenPoint.y - pivotChangeScreenCoord.y;
            userobjPivotinScreen.z = 1;


        }
        else
        {
            userobjPivotinScreen.x = userObjBoxLT.x + userobjSize.x * spritePivot.x;
            userobjPivotinScreen.y = userObjBoxRB.y + userobjSize.y * spritePivot.y;
            userobjPivotinScreen.z = 1;
        }
      
        Vector3 scale = go.transform.localScale;
        //   Debug.Log("[DEBUG SIMULATION] BEFORE scale " + sc + "\t" + scaleRatio + "\t" + scale);
        scale.x = scale.x * scaleRatio.x;
        scale.y = scale.y * scaleRatio.y;
        //   Debug.Log("[DEBUG SIMULATION] AFTER scale " + sc + "\t" + scaleRatio + "\t" + scale);
        //   Debug.Log("[DEBUG SIMULATION] AFTER scale " + sc + "\t" + scaleRatio.y + "\t" + scale.y);
        //    Debug.Log("[DEBUG simulation] type : " + Content.getOrganName(model.modelType) + " go size " + goSize + "\t use size" + userobjSize);
        go.transform.localScale = scale;


        go.transform.position = Camera.main.ScreenToWorldPoint(userobjPivotinScreen);
        //scale and move using shape-evaluation result

        //move model to the object. has to exactly fit with object guideline

        //adjust 'connectivity'
        Color spriteColor = sr.color;
        spriteColor.a = 0;
        sr.color = spriteColor;
        SimulationActiveObject.Add(go);

    }
    public static GameObject getSimulationGameobject(ModelCategory modeltype)
    {
        
        if (InactiveGameObjectDict.ContainsKey(modeltype)) return InactiveGameObjectDict[modeltype];
        return null;
    }
    private static GameObject getGameObject(ModelCategory type, bool activate)
    {
        if (!InactiveGameObjectDict.ContainsKey(type)) return null;
        InactiveGameObjectDict[type].SetActive(activate);
        return InactiveGameObjectDict[type];
    }
    private static void initGameObjectPool()
    {
        if (InactiveGameObjectDict == null) InactiveGameObjectDict = new Dictionary<ModelCategory, GameObject>();
        else InactiveGameObjectDict.Clear();
        InactiveGameObjectDict[ModelCategory.LungLeft] = GameObject.Find("c1_leftlung");
        InactiveGameObjectDict[ModelCategory.LungRight] = GameObject.Find("c1_rightlung");
        InactiveGameObjectDict[ModelCategory.Diaphragm] = GameObject.Find("c1_diaphragm");
        InactiveGameObjectDict[ModelCategory.AirParticleLeft] = GameObject.Find("c1_airPLeft");
        InactiveGameObjectDict[ModelCategory.AirParticleRight] = GameObject.Find("c1_airPRight");
        
            
        InactiveGameObjectDict[ModelCategory.RearSprocket] = GameObject.Find("c2_FreeWheel");
        InactiveGameObjectDict[ModelCategory.FrontChainring] = GameObject.Find("c2_FrontChainring");
        InactiveGameObjectDict[ModelCategory.UpperChain] = GameObject.Find("c2_UpperChain");
        InactiveGameObjectDict[ModelCategory.LowerChain] = GameObject.Find("c2_LowerChain");
        InactiveGameObjectDict[ModelCategory.PedalCrank] = GameObject.Find("c2_PedalCrank");

        foreach (ModelCategory val in System.Enum.GetValues(typeof(ModelCategory)))
        {
            if (!InactiveGameObjectDict.ContainsKey(val)) InactiveGameObjectDict[val] = null;
          //  else InactiveGameObjectDict[val].SetActive(false);
        }
        SimulationActiveObject = new List<GameObject>();
    }
    private static void initSimulationConfig()
    {
        SimulationTypeDict = new Dictionary<ModelCategory, SimulationObjectType>();
        SimulationTypeDict[ModelCategory.LungLeft] = SimulationObjectType.LoadStaticModelConnect;
        SimulationTypeDict[ModelCategory.LungRight] = SimulationObjectType.LoadStaticModelConnect;
        SimulationTypeDict[ModelCategory.Diaphragm] = SimulationObjectType.LoadStaticModelAdaptAspectNoConnect;
        SimulationTypeDict[ModelCategory.Airways] = SimulationObjectType.TextureTransfer;

        SimulationTypeDict[ModelCategory.RearSprocket] = SimulationObjectType.LoadStaticModelMultiChild;
        SimulationTypeDict[ModelCategory.FrontChainring] = SimulationObjectType.LoadStaticModelMultiChild;
        SimulationTypeDict[ModelCategory.UpperChain] = SimulationObjectType.LoadCustomArtifact;
        SimulationTypeDict[ModelCategory.LowerChain] = SimulationObjectType.LoadCustomArtifact;
        SimulationTypeDict[ModelCategory.PedalCrank] = SimulationObjectType.LoadStaticModelFixedPosNoConnect;



        foreach (ModelCategory val in System.Enum.GetValues(typeof(ModelCategory)))
        {
            if (!SimulationTypeDict.ContainsKey(val)) SimulationTypeDict[val] = SimulationObjectType.None;
        }
    }
}


public enum SimulationObjectType
{
    None,
    LoadStaticModelConnect,
    LoadStaticModelAdaptAspectNoConnect,
    LoadStaticModelMultiChild,
    LoadStaticModelFixedPosNoConnect,
    TextureTransfer,
    LoadCustomArtifact,
    LoadDynamicModel,

}

public class SimulationState
{
    public float timeElapsed = 0;
    public float c1_speedBaseline = 1;
    public float c1_speedCurrent = 1;
    public float c1_halfPeriod = 3;


}