using UnityEngine;
using System.Collections;

public class BeamMgr : MonoBehaviour {

    public GameObject beamPreFab;
    // Use this for initialization
    public GameObject[] beams;
    public GameObject desc;
    public int pBeamCount;
    public Vector2 baseVelocity;
    public int SpawningInterval;
    public Rect SpawningArea;
    public Vector2 BoundaryPos;
    void Start () {
        InitBeams();
        
    }
	
	// Update is called once per frame
	void Update () {
        //TEST
        if (Time.frameCount % 130==0 && GlobalRepo.getLearningCount()<=0) SpawnBeams(pBeamCount);
	}
    public void InitBeams()
    {
        beams = new GameObject[40];
        for (int i = 0; i < 40; i++)
        {
            UnityEngine.GameObject newBeam = Instantiate(beamPreFab, new Vector3(-5,0,1), Quaternion.identity) as GameObject;
            newBeam.transform.SetParent(this.gameObject.transform);
            newBeam.GetComponent<BeamUnit>().BoundaryPos = BoundaryPos;
            beams[i] = newBeam;


        }
    }
    public void SpawnBeams(int N)
    {
        int k = 0;
        for(int i = 0; i < beams.Length && N>k; i++)
        {
            //check available beam
            if(beams[i].GetComponent<BeamUnit>().isDead())
            {
                //spawn
                //Vector3 p= new Vector3(Random.value * SpawningArea.width- SpawningArea.width/2, Random.value * SpawningArea.height- SpawningArea.height/2, 1);
                //Vector3 p = new Vector3(-0.3f, Random.value * SpawningArea.height - SpawningArea.height / 2, 0);
                Vector3 p = new Vector3(-0.3f, CVProc.linearMap(k++,0,N, 0- SpawningArea.height / 2, SpawningArea.height / 2));
                beams[i].transform.localPosition = p;
                beams[i].GetComponent<BeamUnit>().SetVelocity(baseVelocity);
                //position
                
            }
            
        }
        // desc.GetComponent<Animator>().playbackTime = 0;
        //desc.GetComponent<Animator>().StartPlayback();
        desc.GetComponent<Animator>().Play("blinkTrigger");
    }
}
