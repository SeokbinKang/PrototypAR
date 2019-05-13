using UnityEngine;
using System.Collections;

public class SimulateGameBike : MonoBehaviour {

    public GameObject game_camera;
    public GameObject[] bike_object;
    public GameObject finishLine;
    public GameObject UIStats;
    public GameObject UINames;
    public GameObject MultiviewUIInstance;
    public GameObject UIRoot;
    public float baseForce=9;
    private float DragAfterFinish = 10;
    private float defaultDrag;
    // Use this for initialization
    private PedalMode SimMode=PedalMode.None;
    private int RevOnceBufferCnt = 0;
    private int lastFinishRank ;
    private double timerPedalOnce;
	void Start () {
        
        Debug.Log("Bike app Start");
        //this will determine drag
      /*  bike_object[0].GetComponent<Content2_AppBikeSim>().SetGearSize(150, 300);
        bike_object[1].GetComponent<Content2_AppBikeSim>().SetGearSize(314, 476);
        bike_object[2].GetComponent<Content2_AppBikeSim>().SetGearSize(250, 150);*/
        defaultDrag = bike_object[0].GetComponent<Rigidbody2D>().drag;
        SimMode = PedalMode.None;
        lastFinishRank = 0;
       

    }
	void OnEnable()
    {
        reset();
        defaultDrag = bike_object[0].GetComponent<Rigidbody2D>().drag;
        UIRoot.GetComponent<Content2UI>().ModeTest();
        Debug.Log("Awake");
    }
	// Update is called once per frame
	void Update () {
        if(SimMode==PedalMode.Once) {
            RevOnceBufferCnt++;
        } else if(SimMode == PedalMode.Run)
        {
            PedalAllBikesRandomForce();
        } 
        //applyRandomForcetoAllBikes();
        CheckBikeStatus();
        //KeyInput();
        
    }
    private void reset()
    {
        this.SimMode = PedalMode.None;
        
        foreach(var t in bike_object)
        {
            t.GetComponent<Content2_AppBikeSim>().reset();
        }
        //ResetBikesDrag();
        lastFinishRank = 0;
        UIStats.GetComponent<FinishStats>().resetLabels();
        
        LoadBikeInfo();
        enablePrototypeImage(true);
        resetTimers();

    }
    public void LoadSimulationData()
    {
        //load bike info
        reset();
        
        //reset 
    }
    private void LoadBikeInfo()
    {
        Debug.Log("[DEBUG] loaindg bikes..");
        float fSize = 200, rSize = 200;
        string name = "?";
        UINames.GetComponent<DesignNames>().resetLabels();
        for (int i = 0; i < bike_object.Length; i++)
        {
            this.MultiviewUIInstance.GetComponent<PrototypeInstanceManager>().GetPrototypeProperties_Content2(i, ref fSize, ref rSize, ref name);
            if (name == "?" || name == "")
            {
                bike_object[i].SetActive(false);
                continue;
            }
            bike_object[i].SetActive(true);
            bike_object[i].GetComponent<Content2_AppBikeSim>().SetGearSize(fSize, rSize);
            bool IsCompletePrototype = this.MultiviewUIInstance.GetComponent<PrototypeInstanceManager>().CheckPrototypeError_Content2(i);
            bike_object[i].GetComponent<Content2_AppBikeSim>().mErrorStatus = !IsCompletePrototype;
            UINames.GetComponent<DesignNames>().SetName(i, name);

            //load the physical prototype image for a bike

            Texture2D txt = this.MultiviewUIInstance.GetComponent<PrototypeInstanceManager>().GetPrototypePhysicalImage(i);
                GameObject go_UIProto = Get_UI_prototypeimages();
                if(go_UIProto!=null)
                {
                    Content2UI_Prototypeimages t = go_UIProto.GetComponent<Content2UI_Prototypeimages>();
                    if (t != null)
                    {
                        t.LoadBikeImages(i, txt);
                    }
                }
            
        }
    }
    private void resetTimers()
    {
        timerPedalOnce = 0;
    }
    private void enablePrototypeImage(bool BikeisontheLeft)
    {
        if (UIRoot == null) return ;
        Content2UI_Prototypeimages UIProtoImage = UIRoot.GetComponentInChildren<Content2UI_Prototypeimages>();
        if (UIProtoImage == null) return ;
        UIProtoImage.EnablePanes(BikeisontheLeft);

    }
    private void disablePrototypeImage()
    {
        if (UIRoot == null) return;
        Content2UI_Prototypeimages UIProtoImage = UIRoot.GetComponentInChildren<Content2UI_Prototypeimages>();
        if (UIProtoImage == null) return;
        UIProtoImage.disablePanes();

    }
    public GameObject Get_UI_prototypeimages()
    {
        if (UIRoot == null) return null;
        Content2UI_Prototypeimages UIProtoImage = UIRoot.GetComponentInChildren<Content2UI_Prototypeimages>();
        if (UIProtoImage == null) return null;
        return UIProtoImage.gameObject;       

    }
    private void CheckBikeStatus()
    {
        int idx = 0;
        Vector3 Pos1st = new Vector3(0, 0, 0);
        int NBikesinRace = 0;
        int NFinishedBikes = 0;
        foreach (GameObject bike in bike_object)
          {
            if (!bike.activeSelf) continue;
            if(bike.GetComponent<Content2_AppBikeSim>().mRearGearSize > 0 ) NBikesinRace++;
            Rigidbody2D rb = bike.GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                Debug.Log("No Rigid Body1!!");
                continue;
            }

            //check the 1st runner
            if (bike.transform.position.x > Pos1st.x)
            {
                Pos1st = bike.transform.position;                
            }
            //     Debug.Log("Bike #" + idx + " :" + rb.velocity.x);
            //idx++;

            //Check the 1 revolution status
             if(SimMode==PedalMode.Once && bike.GetComponent<Content2_AppBikeSim>().mRearGearSize>0 && !bike.GetComponent<Content2_AppBikeSim>().mErrorStatus && RevOnceBufferCnt > 3 && rb.velocity.x==0 && bike.transform.position.x <this.finishLine.transform.position.x)
              {
                AllBikesStop();
                SimMode = PedalMode.None;
                RevOnceBufferCnt = 0;
                break;
              }

             // Check the finish line

            if(bike.transform.position.x > this.finishLine.transform.position.x && bike.GetComponent<Rigidbody2D>().drag<1)
            {
                BrakeAfterFinish(bike);
                FinishStats fs = this.UIStats.GetComponent<FinishStats>();
                lastFinishRank++;
                string placementlabel = "...";
                if (lastFinishRank == 1) placementlabel = "1st";
                else if (lastFinishRank == 2) placementlabel = "2nd";
                else
                {
                    if (lastFinishRank == 3) placementlabel = "3rd";
                }

                float gearRatio = bike.GetComponent<Content2_AppBikeSim>().mRearGearSize / bike.GetComponent<Content2_AppBikeSim>().mFrontGearSize;
                if (fs!=null)
                {
                    fs.SetStat(idx, placementlabel, "1:" + gearRatio.ToString("#.##"));
                }
             
            }
            if (bike.transform.position.x > this.finishLine.transform.position.x)
            {
                NFinishedBikes++;
            }
                idx++;
          }
        
        if(NBikesinRace>0 && NFinishedBikes== NBikesinRace)
        {
            SimMode = PedalMode.RaceFinished;
            enablePrototypeImage(false);
        }
           Vector3 camPos = game_camera.transform.position;
            camPos.x = Pos1st.x + 0.2f;
            game_camera.transform.position = camPos;
       
    }
    
    private void AllBikesStop()
    {
        Debug.Log("[DEBUG] All Bikes Stop!");
        foreach (GameObject bike in bike_object)
        {
            if (!bike.activeSelf) continue;
            Rigidbody2D rb = bike.GetComponent<Rigidbody2D>();
            if (rb == null) continue;
            //rb.drag = 30;
            rb.velocity = new Vector2(0, 0);         
        }
    }
    private void BrakeAfterFinish(GameObject go)
    {

        Rigidbody2D rb = go.GetComponent<Rigidbody2D>();
        if (rb == null) return;
        rb.drag = 8;
    }
    private void ResetBikesDrag()
    {
        foreach (GameObject bike in bike_object)
        {
            Rigidbody2D rb = bike.GetComponent<Rigidbody2D>();
            if (rb == null) continue;
            rb.drag = defaultDrag;
           
        }
    }
    public void RunBikes()
    {
        SimMode = PedalMode.Run;
        ResetBikesDrag();
        lastFinishRank=0;
        disablePrototypeImage();
    }
    public void PedalAllBikesRandomForce()
    {

        if (bike_object == null || bike_object.Length < 1) return;
        System.Random rnd = new System.Random();
        int k = 0;
        Vector3 Pos1st = new Vector3(0, 0, 0);   
        
        foreach (GameObject bike in bike_object)
        {
            if (!bike.activeSelf) continue;
            if (bike.transform.position.x < this.finishLine.transform.position.x)
            {
                //running
                
                Rigidbody2D rb = bike.GetComponent<Rigidbody2D>();
                if (rb == null) continue;
               // Debug.Log(rb.velocity.x);
                

                //rb.AddForce(new Vector2(rnd.Next(4, 5), 0), ForceMode2D.Force);
                float gearRatio = bike.GetComponent<Content2_AppBikeSim>().mGearRatio;
                // if (rb.velocity.x > 6) continue;
                if(gearRatio==0 || bike.GetComponent<Content2_AppBikeSim>().mErrorStatus) bike.GetComponent<Content2_AppBikeSim>().showMsgBikeProblems();
                else if (gearRatio > 0) rb.AddForce(new Vector2(((float)rnd.Next(2, 3)) / Mathf.Sqrt(Mathf.Sqrt(gearRatio)), 0), ForceMode2D.Force);
                else bike.GetComponent<Content2_AppBikeSim>().showMsgBikeProblems();


            }
            else
            {
                //apply high drag to stop the bike
                
            }
        }
       
        //rb2.velocity = new Vector2(0.f, 1);



    }
    public void PedalAllBikesOnce()
    {
    
        if (bike_object == null || bike_object.Length <3) return;
        if (Time.time - timerPedalOnce <= 3.0) return;
        timerPedalOnce = Time.time;
        System.Random rnd = new System.Random();        
        int k = 0;
        Vector3 Pos1st=new Vector3(0,0,0);
        //float[] ff = new float[3];
        //ff[0] = 1.5f;
        //ff[1] = 3f;
        //ff[2] = 5f;
        foreach (GameObject bike in bike_object)
        {
            if (!bike.activeSelf) continue;
            if (bike.transform.position.x < this.finishLine.transform.position.x)
            {
                //running
                Rigidbody2D rb = bike.GetComponent<Rigidbody2D>();
                if (rb == null) continue;
                //rb.AddForce(new Vector2(rnd.Next(4, 5), 0), ForceMode2D.Force);
                float gearRatio = bike.GetComponent<Content2_AppBikeSim>().mGearRatio;
                if (gearRatio == 0 || bike.GetComponent<Content2_AppBikeSim>().mErrorStatus) bike.GetComponent<Content2_AppBikeSim>().showMsgBikeProblems();
                else if (gearRatio>0)  rb.AddForce(new Vector2(baseForce*3/gearRatio, 0), ForceMode2D.Force);
                  else bike.GetComponent<Content2_AppBikeSim>().showMsgBikeProblems();
                //   Debug.Log("Force " + ff[k] + "    " + 3 / gearRatio);
                //rb.AddForce(new Vector2(ff[k++]*baseForce, 0), ForceMode2D.Force);
                Pos1st = bike.transform.position;
                
                
            } else
            {
                
            }
        }

        Rigidbody2D rb2 = game_camera.GetComponent<Rigidbody2D>();
        if (rb2 == null) return;
       
        //rb2.velocity = new Vector2(0.f, 1);
        ResetBikesDrag();
        RevOnceBufferCnt = 0;
        SimMode = PedalMode.Once;

        disablePrototypeImage();

    }

}

public enum PedalMode
{
    None,
    Once,
    Run,
    RaceFinished
}
