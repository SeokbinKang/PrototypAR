using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using OpenCvSharp;
using OpenCvSharp.Blob;
using OpenCvSharp.CPlusPlus;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

public class designARManager : MonoBehaviour {

    public ConceptModelManager pConceptModelManager;    
    // Use this for initialization
    public bool showArtifact = true;
    public int pMaxPrototype = 1;
    public bool enableAnimation = true;
    public static bool ControlBackground = true;
    
    // internal
    private GlobalRepo.UserPhase lastUserPhase = GlobalRepo.UserPhase.none;

    private FBSModel mDesignContentModel = null;
    void Start()
    {
        GlobalRepo.initRepo();
        pConceptModelManager = new ConceptModelManager();
        if (mDesignContentModel == null)
        {
            GameObject appControl = GameObject.Find("ApplicationControl");
            if(appControl==null)
            {
                Debug.Log("[ERROR] Could not find ApplicationControl obejct");
                return;
            }
            ApplicationControl appControlInstance = appControl.GetComponent<ApplicationControl>();
            if (appControlInstance == null)
            {
                Debug.Log("[ERROR] Could not find ApplicationControl instance");
                return;
            }
            initDesignContent(appControlInstance.getContentType());
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        pConceptModelManager.ConceptModelTick();
        if (showArtifact) pConceptModelManager.showDebugImage();
        //animate
        if (enableAnimation)
        {
         //   pConceptModelManager.animateVisual3DModelTick();
        }
        ContentBackgroundControl();
    }
    void Ondestory()
    {
        GlobalRepo.closeDebugWindow();
    }
    public void evaluateandVisualize()
    {
       
    }
    public void addPrototype(prototypeDef t)
    {
        if (pConceptModelManager == null || t==null) return;
        if (pConceptModelManager.mPrototypeList.Count >= pMaxPrototype) return;
        if (mDesignContentModel == null)
        {
            GameObject appControl = GameObject.Find("ApplicationControl");
            if (appControl == null)
            {
                Debug.Log("[ERROR] Could not find ApplicationControl obejct");
                return;
            }
            ApplicationControl appControlInstance = appControl.GetComponent<ApplicationControl>();
            if (appControlInstance == null)
            {
                Debug.Log("[ERROR] Could not find ApplicationControl instance");
                return;
            }
            initDesignContent(appControlInstance.getContentType());
        }
        //add the prototype to artifact manager
        pConceptModelManager.addPrototype(t);
        pConceptModelManager.setVisual2DMgr(this.GetComponentInParent<Visual2DModelManager>());
        //construct 3d model for the prototype and link
        //   this.GetComponentInParent<Visual3DModelManager>().create3DModel(t);        
        //  t.initAnimation();




        //this.GetComponentInParent<Visual2DModelManager>().createVisual2DModel(t); 



    }
    public void resetScene()
    {
        GlobalRepo.SetUserPhas(GlobalRepo.UserPhase.design);
        pConceptModelManager.reset();
        this.GetComponentInParent<BehaviorDetector>().reset();
        this.GetComponentInParent<ColorDetector>().reset();
        this.GetComponentInParent<Visual2DModelManager>().reset();

    }
    private void initDesignContent(DesignContent contentType)
    {
        if(contentType==DesignContent.HumanRespiratorySystem)
        {
            mDesignContentModel = new FBSModel(contentType);
            StructurePosVariable pos;
            //pos = new StructurePosVariable(PosEvalType.ProximitytoPoint,new OpenCvSharp.CvPoint(40,50),150);
            pos = new StructurePosVariable(PosEvalType.CentralProximitytoPoint,PreLoadedObjects.Content1_BGPartial, new Vector2(0.53f,0.72f), 150);
            StructureEntity lung_left = new StructureEntity(ModelCategory.LungLeft, pos);
            pos = new StructurePosVariable(PosEvalType.CentralProximitytoPoint, PreLoadedObjects.Content1_BGPartial, new Vector2(0.69f, 0.72f), 150);
            StructureEntity lung_right = new StructureEntity(ModelCategory.LungRight, pos);
            pos = new StructurePosVariable(PosEvalType.CentralProximitytoPoint, PreLoadedObjects.Content1_BGPartial, new Vector2(0.61f, 0.71f), 150);
            StructureEntity airways = new StructureEntity(ModelCategory.Airways, pos);
            pos = new StructurePosVariable(PosEvalType.CentralProximitytoPoint, PreLoadedObjects.Content1_BGPartial, new Vector2(0.61f, 0.64f), 150);
            StructureEntity diaphragm = new StructureEntity(ModelCategory.Diaphragm, pos);

            mDesignContentModel.AddStructureEntity(lung_left);
            mDesignContentModel.AddStructureEntity(lung_right);
            mDesignContentModel.AddStructureEntity(airways);
            mDesignContentModel.AddStructureEntity(diaphragm);

            mDesignContentModel.AddConnectivity(lung_left, airways);
            mDesignContentModel.AddConnectivity(lung_right, airways);

            BehaviorEntity be;
            be = new BehaviorEntity(BehaviorCategory.CONTRACT, diaphragm, new BehaviorVariableEntity(BehaviorVariableType.Numeric, new KeyValuePair<float, float>(1, 40)));
            mDesignContentModel.AddBehaviorEntity(be);
            be = new BehaviorEntity(BehaviorCategory.DIFFUSE, lung_left, null);
            mDesignContentModel.AddBehaviorEntity(be);
            be = new BehaviorEntity(BehaviorCategory.DIFFUSE, lung_right, null);
            mDesignContentModel.AddBehaviorEntity(be);
            be = new BehaviorEntity(BehaviorCategory.PASS, airways, null);
            mDesignContentModel.AddBehaviorEntity(be);
            //set color coding


            this.pConceptModelManager.setFBSModel(mDesignContentModel);

        }
        if (contentType == DesignContent.BicycleGearSystem)
        {
            mDesignContentModel = new FBSModel(contentType);
            StructurePosVariable pos;            
            pos = new StructurePosVariable(PosEvalType.CentralProximitytoPoint, PreLoadedObjects.Content2_BGPartial, new Vector2(0.265f, 0.4f), 150);
            StructureEntity freeWheel = new StructureEntity(ModelCategory.RearSprocket, pos);
            freeWheel.v6_VirtualPositionType = VirtualPosType.AlignWithVirtualBG;
            pos = new StructurePosVariable(PosEvalType.CentralProximitytoPoint, PreLoadedObjects.Content2_BGPartial, new Vector2(0.462f, 0.38f), 150);
            StructureEntity frontChainRing = new StructureEntity(ModelCategory.FrontChainring, pos);
            frontChainRing.v6_VirtualPositionType = VirtualPosType.AlignWithVirtualBG;

            pos = new StructurePosVariable(PosEvalType.None, PreLoadedObjects.Content2_BGPartial, new Vector2(0.61f, 0.71f), 150);
            StructureEntity Upperchain = new StructureEntity(ModelCategory.UpperChain, pos);
            Upperchain.v6_VirtualPositionType = VirtualPosType.AlignWithVirtualBG;

            pos = new StructurePosVariable(PosEvalType.None, PreLoadedObjects.Content2_BGPartial, new Vector2(0.61f, 0.71f), 150);
            StructureEntity Lowerchain = new StructureEntity(ModelCategory.LowerChain, pos);
            Lowerchain.v6_VirtualPositionType = VirtualPosType.AlignWithVirtualBG;

            pos = new StructurePosVariable(PosEvalType.ContourProximitytoPoint, PreLoadedObjects.Content2_BGPartial, new Vector2(0.462f, 0.38f), 100);
            StructureEntity PedalCrank = new StructureEntity(ModelCategory.PedalCrank, pos);
            PedalCrank.v6_VirtualPositionType = VirtualPosType.AlignWithVirtualBG;

            mDesignContentModel.AddStructureEntity(freeWheel);
            mDesignContentModel.AddStructureEntity(frontChainRing);
            mDesignContentModel.AddStructureEntity(Upperchain);
            mDesignContentModel.AddStructureEntity(Lowerchain);
            mDesignContentModel.AddStructureEntity(PedalCrank);

            mDesignContentModel.AddConnectivity(frontChainRing, PedalCrank);            
            mDesignContentModel.AddConnectivity(frontChainRing, Upperchain);
            mDesignContentModel.AddConnectivity(frontChainRing, Lowerchain);
            mDesignContentModel.AddConnectivity(freeWheel, Upperchain);
            mDesignContentModel.AddConnectivity(freeWheel, Lowerchain);



            BehaviorEntity be;
            be = new BehaviorEntity(BehaviorCategory.PEDAL, PedalCrank, new BehaviorVariableEntity(BehaviorVariableType.Numeric, new KeyValuePair<float, float>(1, 40)));
            mDesignContentModel.AddBehaviorEntity(be);
            be = new BehaviorEntity(BehaviorCategory.ROTATE, freeWheel, null);
            mDesignContentModel.AddBehaviorEntity(be);
            be = new BehaviorEntity(BehaviorCategory.DRIVE, frontChainRing, null);
            mDesignContentModel.AddBehaviorEntity(be);
            be = new BehaviorEntity(BehaviorCategory.TRANSFER, Upperchain, null);
            mDesignContentModel.AddBehaviorEntity(be);
            be = new BehaviorEntity(BehaviorCategory.TRANSFER, Lowerchain, null);
            mDesignContentModel.AddBehaviorEntity(be);
            //set color coding


            this.pConceptModelManager.setFBSModel(mDesignContentModel);

        }
        if (contentType == DesignContent.AquariumEcology)
        {
            mDesignContentModel = new FBSModel(contentType);
            StructurePosVariable pos;
            pos = new StructurePosVariable(PosEvalType.None, PreLoadedObjects.Content2_BGPartial, new Vector2(0.215f, 0.348f), 500);
            StructureEntity Fish = new StructureEntity(ModelCategory.Fish, pos);
            pos = new StructurePosVariable(PosEvalType.CentralProximitytoPoint, PreLoadedObjects.Content2_BGPartial, new Vector2(0.412f, 0.3144f), 150);
            StructureEntity AirPump = new StructureEntity(ModelCategory.AirPump, pos);
            pos = new StructurePosVariable(PosEvalType.VerticalProximitytoPoint, PreLoadedObjects.Content2_BGPartial, new Vector2(0.61f, 0.31f), 150);
            StructureEntity Plant = new StructureEntity(ModelCategory.Plant, pos);
            pos = new StructurePosVariable(PosEvalType.None, PreLoadedObjects.Content2_BGPartial, new Vector2(0.61f, 0.71f), 150);
            StructureEntity Bacteria = new StructureEntity(ModelCategory.Bacteria, pos);
            

            mDesignContentModel.AddStructureEntity(Fish);
            mDesignContentModel.AddStructureEntity(AirPump);
            mDesignContentModel.AddStructureEntity(Plant);
            mDesignContentModel.AddStructureEntity(Bacteria);
            
            
            BehaviorEntity be;
            List<string> reducecategories = new List<string>();
            reducecategories.Add("nitrogen");
            reducecategories.Add("ammonia");
            reducecategories.Add("nitrate");
            reducecategories.Add("nitrite");
            be = new BehaviorEntity(BehaviorCategory.PRODUCE, Fish, null);
            mDesignContentModel.AddBehaviorEntity(be);
            be = new BehaviorEntity(BehaviorCategory.REDUCE, Bacteria, new BehaviorVariableEntity(BehaviorVariableType.Categorical, reducecategories));
            mDesignContentModel.AddBehaviorEntity(be);
            be = new BehaviorEntity(BehaviorCategory.CONSUME, Plant, null);
            mDesignContentModel.AddBehaviorEntity(be);
            be = new BehaviorEntity(BehaviorCategory.SUPPLY, AirPump, null);
            mDesignContentModel.AddBehaviorEntity(be);
            
            //set color coding


            this.pConceptModelManager.setFBSModel(mDesignContentModel);

        }
        if (contentType == DesignContent.CameraSystem)
        {
            mDesignContentModel = new FBSModel(contentType);
            StructurePosVariable pos;
            pos = new StructurePosVariable(PosEvalType.CentralProximitytoPoint, PreLoadedObjects.Content4_BGPartial, new Vector2(0.265f, 0.4f), 150);
            StructureEntity lens = new StructureEntity(ModelCategory.C4_lens, pos);
            lens.v6_VirtualPositionType = VirtualPosType.AlignWithVirtualBG;
            pos = new StructurePosVariable(PosEvalType.CentralProximitytoPoint, PreLoadedObjects.Content4_BGPartial, new Vector2(0.462f, 0.38f), 150);
            StructureEntity shutter = new StructureEntity(ModelCategory.C4_shutter, pos);
            shutter.v6_VirtualPositionType = VirtualPosType.AlignWithVirtualBG;
            //TODO structure virtual positions have to be updated

            pos = new StructurePosVariable(PosEvalType.None, PreLoadedObjects.Content4_BGPartial, new Vector2(0.62f, 0.38f), 150);
            StructureEntity sensor = new StructureEntity(ModelCategory.C4_sensor, pos);
            sensor.v6_VirtualPositionType = VirtualPosType.AlignWithVirtualBG;

            mDesignContentModel.AddStructureEntity(lens);
            mDesignContentModel.AddStructureEntity(shutter);
            mDesignContentModel.AddStructureEntity(sensor);

            BehaviorEntity be;
            be = new BehaviorEntity(BehaviorCategory.C4_FOCUS, lens, new BehaviorVariableEntity(BehaviorVariableType.Numeric, new KeyValuePair<float, float>(10, 200)));
            mDesignContentModel.AddBehaviorEntity(be);
            be = new BehaviorEntity(BehaviorCategory.C4_EXPOSE, shutter, new BehaviorVariableEntity(BehaviorVariableType.Numeric, new KeyValuePair<float, float>(1, 1000)));
            mDesignContentModel.AddBehaviorEntity(be);
            List<string> sensorBVdef = Content.getCategoricalBVdefinition(BehaviorCategory.C4_CAPTURE);
            if (sensorBVdef == null || sensorBVdef.Count==0) Debug.Log("sensor BV def is empty");
            be = new BehaviorEntity(BehaviorCategory.C4_CAPTURE, sensor, new BehaviorVariableEntity(BehaviorVariableType.Categorical, sensorBVdef));
            mDesignContentModel.AddBehaviorEntity(be);
            
            
            this.pConceptModelManager.setFBSModel(mDesignContentModel);

        }
    }
    private void ContentBackgroundControl()
    {
        if (!ControlBackground) return;
        if (SceneObjectManager.getActiveInstance()==null) return;
        lastUserPhase = GlobalRepo.UserMode;

        if(FBSModel.ContentType==DesignContent.HumanRespiratorySystem)
        {
            if(lastUserPhase==GlobalRepo.UserPhase.design)
            {
                SceneObjectManager.getActiveInstance().adjustAlphaSpriteRendere(PreLoadedObjects.Content1_BGPartial, 20);
                SceneObjectManager.getActiveInstance().adjustAlphaSpriteRendere(PreLoadedObjects.Content1_BGFull, 0,0.005f);
                SceneObjectManager.getActiveInstance().MoveToScreenRelativePos(PreLoadedObjects.Content1_BGPartial, new Vector2(0.5f, 0.5f));
            } else if (lastUserPhase == GlobalRepo.UserPhase.feedback)
            {
                SceneObjectManager.getActiveInstance().adjustAlphaSpriteRendere(PreLoadedObjects.Content1_BGPartial, 10);
                SceneObjectManager.getActiveInstance().adjustAlphaSpriteRendere(PreLoadedObjects.Content1_BGFull, 0);
            } else if (lastUserPhase == GlobalRepo.UserPhase.simulation)
            {
                
                SceneObjectManager.getActiveInstance().adjustAlphaSpriteRendere(PreLoadedObjects.Content1_BGPartial, 30);
                SceneObjectManager.getActiveInstance().adjustAlphaSpriteRendere(PreLoadedObjects.Content1_BGFull, 200);
            }
        }

        if (FBSModel.ContentType == DesignContent.BicycleGearSystem)
        {
            
            if (lastUserPhase == GlobalRepo.UserPhase.design)
            {

                SceneObjectManager.getActiveInstance().adjustAlphaSpriteRendereFadeIn(PreLoadedObjects.Content2_BGPartial,50);
                SceneObjectManager.getActiveInstance().adjustAlphaSpriteRendere(PreLoadedObjects.Content2_BGFull, 0);
                SceneObjectManager.getActiveInstance().MoveToScreenRelativePos(PreLoadedObjects.Content2_BGPartial, new Vector2(0.5f, 0.5f));
            }
            else if (lastUserPhase == GlobalRepo.UserPhase.feedback)
            {
                SceneObjectManager.getActiveInstance().adjustAlphaSpriteRendere(PreLoadedObjects.Content2_BGPartial, 10);
                SceneObjectManager.getActiveInstance().adjustAlphaSpriteRendere(PreLoadedObjects.Content2_BGFull, 0);
            }
            else if (lastUserPhase == GlobalRepo.UserPhase.simulation)
            {

                SceneObjectManager.getActiveInstance().adjustAlphaSpriteRendere(PreLoadedObjects.Content2_BGPartial, 20);
                SceneObjectManager.getActiveInstance().adjustAlphaSpriteRendere(PreLoadedObjects.Content2_BGFull, 200);
            }
        }
        if (FBSModel.ContentType == DesignContent.CameraSystem)
        {
            Debug.Log("designARManager, contenttype = canera");
            if (lastUserPhase == GlobalRepo.UserPhase.design)
            {

                SceneObjectManager.getActiveInstance().adjustAlphaSpriteRendereFadeIn(PreLoadedObjects.Content4_BGPartial, 50);
                SceneObjectManager.getActiveInstance().adjustAlphaSpriteRendere(PreLoadedObjects.Content4_BGFull, 0);
                SceneObjectManager.getActiveInstance().MoveToScreenRelativePos(PreLoadedObjects.Content4_BGPartial, new Vector2(0.5f, 0.5f));
            }
            else if (lastUserPhase == GlobalRepo.UserPhase.feedback)
            {
                SceneObjectManager.getActiveInstance().adjustAlphaSpriteRendere(PreLoadedObjects.Content4_BGPartial, 10);
                SceneObjectManager.getActiveInstance().adjustAlphaSpriteRendere(PreLoadedObjects.Content4_BGFull, 0);
            }
            else if (lastUserPhase == GlobalRepo.UserPhase.simulation)
            {
                SceneObjectManager.getActiveInstance().adjustAlphaSpriteRendere(PreLoadedObjects.Content4_BGPartial, 20);
                SceneObjectManager.getActiveInstance().adjustAlphaSpriteRendere(PreLoadedObjects.Content4_BGFull, 50);
            }
        }
    }
    
    void ResetAll ()
    {
        
    }
}



