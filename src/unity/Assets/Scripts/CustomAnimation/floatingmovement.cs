using UnityEngine;
using System.Collections;

public class floatingmovement : MonoBehaviour {

    public float maxSpeed = 0.2f;
    public float maxZRotSpeed = 0.0f;
    private Vector3 WorldTankLB;
    private Vector3 WorldTankRT;
    private float speed=0.02f;
    private Vector3 RotSpeed = new Vector3(0f,0f,0f);
    private Vector3 direction;
    private Vector3 lastTrans;
    private Vector3 GoalPos;
    private Vector3 GoalAngle;
    
    
    private float TouchDistance= 0.1f;
    // Use this for initialization
    void Start () {
        updateWorldTank();
        setRandomGoal();
    }
	
	// Update is called once per frame
	void Update () {
      //  checkTankBoundary();
        movementUpdate();
        

    }
    private void speedUpdate()
    {

    }
    private void movementUpdate()
    {
        Vector3 distanceToGoal = GoalPos - this.transform.position;
   
        if(distanceToGoal.x*direction.x < 0 || distanceToGoal.y * direction.y <  0)
        {
         
            setRandomGoal();
            return;
        }
        Vector3 trans = direction * speed * Time.deltaTime;
        this.lastTrans = trans;
        this.transform.Translate(trans);
     
        if(RotSpeed.z> 0)
        {
            
            this.transform.Rotate(RotSpeed);

        }

    }
    private void checkTankBoundary()
    {
        float dist = (GoalPos - this.transform.position).magnitude;
        
        if (this.transform.position.x < WorldTankLB.x || this.transform.position.x > WorldTankRT.x) setRandomGoal();
            else if (this.transform.position.y < WorldTankLB.y || this.transform.position.y > WorldTankRT.y) setRandomGoal();
    }
    private void setRandomGoal()
    {
        float dist = 0;
        while (dist < 0.5f)
        {
            GoalPos = new Vector3(Random.Range(WorldTankLB.x, WorldTankRT.x),
                Random.Range(WorldTankLB.y, WorldTankRT.y),
                1);
            dist = (GoalPos - this.transform.position).magnitude;

        }
        direction = GoalPos - this.transform.position;
        direction.Normalize();
        speed = Random.Range(0.01f, maxSpeed);
        
        Vector3 rot = this.transform.localScale;
        rot.x = Mathf.Abs(rot.x);
        if (direction.x < 0) rot.x = rot.x * -1f;
        this.transform.localScale = rot;

        if(maxZRotSpeed>0)
        {
            RotSpeed.z = Random.Range(-maxZRotSpeed, maxZRotSpeed);
        }

        setAnimationSpeed(Mathf.Lerp(0.5f, 2.5f, speed / maxSpeed));
        
    }
    private void setAnimationSpeed(float param)
    {
        Animator ani = this.GetComponent<Animator>();
        if (ani == null) return;
        ani.SetFloat("speedparam", param);
    }
    private void updateWorldTank()
    {
        WorldTankLB = Camera.main.ScreenToWorldPoint(new Vector3(30, 30, 1));
        WorldTankRT = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width-30, Screen.height-30, 1));
    }

}
