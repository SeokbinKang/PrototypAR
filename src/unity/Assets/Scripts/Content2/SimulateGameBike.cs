using UnityEngine;
using System.Collections;

public class SimulateGameBike : MonoBehaviour {

    public GameObject game_camera;
    public GameObject[] bike_object;
    public GameObject finishLine;
    public float baseForce=9;
    private float DragAfterFinish = 10;
    private float defaultDrag;
    // Use this for initialization
    private bool OneRev = false;
	void Start () {
        

        //this will determine drag
        bike_object[0].GetComponent<Content2_AppBikeSim>().SetGearSize(150, 300);
        bike_object[1].GetComponent<Content2_AppBikeSim>().SetGearSize(200, 200);
        bike_object[2].GetComponent<Content2_AppBikeSim>().SetGearSize(250, 150);
        defaultDrag = bike_object[0].GetComponent<Rigidbody2D>().drag;
        OneRev = false;
    }
	
	// Update is called once per frame
	void Update () {
        //applyRandomForcetoAllBikes();
        DebugBikeSpeed();
        KeyInput();
        
    }
    private void KeyInput()
    {
        if (Input.GetMouseButtonDown(0))
        {

            applyRandomForcetoAllBikes();
        }
    }
    private void DebugBikeSpeed()
    {
        int idx = 0;
        foreach (GameObject bike in bike_object)
          {
              Rigidbody2D rb = bike.GetComponent<Rigidbody2D>();
              if (rb == null) continue;
              Debug.Log("Bike #" + idx + " :" + rb.velocity.x);
              idx++;
            if(OneRev && rb.velocity.x==0)
            {
                AllBikesStop();
                OneRev = false;
                break;
            }

          }
      /*  Rigidbody2D rb = bike_object[0].GetComponent<Rigidbody2D>();
        Rigidbody2D rb2 = bike_object[2].GetComponent<Rigidbody2D>();
        Debug.Log("Speed ratio: " + rb2.velocity.x / rb.velocity.x + " has to be: " + 5f / 1.5f);*/
    }
    private void AllBikesStop()
    {
        foreach (GameObject bike in bike_object)
        {
            Rigidbody2D rb = bike.GetComponent<Rigidbody2D>();
            if (rb == null) continue;
            //rb.drag = 30;
            rb.velocity = new Vector2(0, 0);
            //rb.velocity.x = 0;
        }
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
    private void applyRandomForcetoAllBikes()
    {
    
        if (bike_object == null || bike_object.Length < 1) return;
        System.Random rnd = new System.Random();        
        int k = 0;
        Vector3 Pos1st=new Vector3(0,0,0);
        float[] ff = new float[3];
        ff[0] = 1.5f;
        ff[1] = 3f;
        ff[2] = 5f;
        foreach (GameObject bike in bike_object)
        {
            if (bike.transform.position.x < this.finishLine.transform.position.x)
            {
                //running
                Rigidbody2D rb = bike.GetComponent<Rigidbody2D>();
                if (rb == null) continue;
                
                //rb.AddForce(new Vector2(rnd.Next(4, 5), 0), ForceMode2D.Force);
                float gearRatio = bike.GetComponent<Content2_AppBikeSim>().mRearGearSize / bike.GetComponent<Content2_AppBikeSim>().mFrontGearSize;
                 rb.AddForce(new Vector2(baseForce*3/gearRatio, 0), ForceMode2D.Force);

                Debug.Log("Force " + ff[k] + "    " + 3 / gearRatio);
                //rb.AddForce(new Vector2(ff[k++]*baseForce, 0), ForceMode2D.Force);
                Pos1st = bike.transform.position;
                
                
            } else
            {
                //apply high drag to stop the bike
                Rigidbody2D rb = bike.GetComponent<Rigidbody2D>();
                if (rb == null) continue;
                rb.AddForce(new Vector2(rnd.Next(4, 5), 0),ForceMode2D.Force);
                Pos1st = bike.transform.position;
            }
        }

        Rigidbody2D rb2 = game_camera.GetComponent<Rigidbody2D>();
        if (rb2 == null) return;
        Vector3 camPos = game_camera.transform.position;
        camPos.x = Pos1st.x + 1.5f;
        game_camera.transform.position = camPos;
        //rb2.velocity = new Vector2(0.f, 1);
        ResetBikesDrag();
        OneRev = true;

    }
}
