using UnityEngine;
using System.Collections;

public class Content2_AppBikeSim : MonoBehaviour {

    public GameObject part_frontWheel;
    public GameObject part_rearWheel;
    public GameObject part_frontGear;
    public GameObject part_rearGear;
    public GameObject part_pedal;

    public float config_baseXVelocityToAngularSpeed; //1 to rotation rate of the rearwheel and gear    
    public float referenceGearSize = 200;    

    public float mFrontGearSize;
    public float mRearGearSize;
    private Vector3 minitialGearScale;
    // Use this for initialization
    void Start () {
        if (part_frontWheel == null || part_rearGear == null || part_frontGear == null || part_rearGear == null)
        {
            Debug.Log("[ERROR] the bike object's sub-objects are null");
            return;
        }
        minitialGearScale = part_frontGear.transform.localScale;
    }
	
	// Update is called once per frame
	void Update () {
        if(part_frontWheel==null || part_rearGear==null || part_frontGear==null || part_rearGear==null)
        {
            Debug.Log("[ERROR] the bike object's sub-objects are null");
            return;
        }
        Roll();

    }
    private void Roll()
    {
        Rigidbody2D rb = this.GetComponent<Rigidbody2D>();        
        if (rb == null) return;
        float GearRatio_FtoR = mRearGearSize / mFrontGearSize;
        float xVelocity = rb.velocity.x;                
        float wheelAngularSpeed = xVelocity* config_baseXVelocityToAngularSpeed;
        float frontGearAngularSpeed = wheelAngularSpeed * GearRatio_FtoR;

        
        //rotate the wheels by controlling the animation speed
        Animator ani = part_frontWheel.GetComponent<Animator>();
        if (ani == null) return;        
        ani.SetFloat("speedparam", wheelAngularSpeed);

        ani = part_rearWheel.GetComponent<Animator>();
        if (ani == null) return;
        ani.SetFloat("speedparam", wheelAngularSpeed);

        ani = part_rearGear.GetComponent<Animator>();
        if (ani == null) return;
        ani.SetFloat("speedparam", wheelAngularSpeed);

        ani = part_frontGear.GetComponent<Animator>();
        if (ani == null) return;
        ani.SetFloat("speedparam", frontGearAngularSpeed);
        ani = part_pedal.GetComponent<Animator>();
        if (ani == null) return;
        ani.SetFloat("speedparam", frontGearAngularSpeed);
    }

    //Set the gear size in the physical prototype
    public void SetGearSize(float front, float rear)
    {
        if (part_frontWheel == null || part_rearGear == null || part_frontGear == null || part_rearGear == null)
        {
            Debug.Log("[ERROR] the bike object's sub-objects are null");
            return;
        }
        mFrontGearSize = front;
        mRearGearSize = rear;
        Vector3 newScale = minitialGearScale;
        newScale.x = newScale.x * (front / referenceGearSize);
        newScale.y = newScale.y * (front / referenceGearSize);
        part_frontGear.transform.localScale = newScale;

        newScale = minitialGearScale;
        newScale.x = newScale.x * (rear / referenceGearSize);
        newScale.y = newScale.y * (rear / referenceGearSize);
        part_rearGear.transform.localScale = newScale;

    }


}
