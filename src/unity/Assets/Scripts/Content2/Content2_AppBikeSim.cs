using UnityEngine;
using System.Collections;
using OpenCvSharp;
using OpenCvSharp.Blob;
using OpenCvSharp.CPlusPlus;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System;
using System.Runtime.InteropServices;
using Uk.Org.Adcock.Parallel;

public class Content2_AppBikeSim : MonoBehaviour {

    public GameObject part_frontWheel;
    public GameObject part_rearWheel;
    public GameObject part_frontGear;
    public GameObject part_rearGear;
    public GameObject part_pedal;
    public GameObject part_rider;
    public GameObject msg_bikeproblems;
    public Vector3 minitialGearScale;


    public float config_baseXVelocityToAngularSpeed; //1 to rotation rate of the rearwheel and gear    
    public float referenceGearSize = 430;    
    public float mFrontGearSize;
    public float mRearGearSize;
    public float mGearRatio;
    public bool mErrorStatus;

    private List<KeyValuePair<GameObject, TransformInstance>> initTransformProperties;
    private float initialDrag;
    private bool initialized = false;

    // Use this for initialization
    void Start () {
        if (part_frontWheel == null || part_rearGear == null || part_frontGear == null || part_rearGear == null)
        {
            Debug.Log("[ERROR] the bike object's sub-objects are null");
            return;
        }
       
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
    private void init()
    {
        if (!initialized)
        {
            Debug.Log("Saving Bike's transform");
            saveObjectTransforms();
            initialDrag = this.GetComponent<Rigidbody2D>().drag;
            initialized = true;
        }
    }
    public void reset()
    {
        init();
        //load the initial transforms
        restoreObjectTransforms();
        //init the variables
        this.mFrontGearSize = 0;
        this.mRearGearSize = 0;
        this.mGearRatio = 0;
        mErrorStatus = false;

        //set rigidbody properties
        resetRigidProperties();
        resetMsgs();


    }
    public void showMsgBikeProblems()
    {
        this.msg_bikeproblems.SetActive(true);
    }
    private void resetMsgs()
    {
        this.msg_bikeproblems.SetActive(false);
    }
    private void resetRigidProperties()
    {
        Rigidbody2D r = this.GetComponent<Rigidbody2D>();
        if (r == null) return;
        r.velocity = new Vector2(0, 0);
        r.angularVelocity = 0;
        this.GetComponent<Rigidbody2D>().drag = initialDrag;


    }
    private void restoreObjectTransforms()
    {
        if (initTransformProperties == null)
        {
            return;
        }
        Debug.Log("restoring object transforms.....");
        foreach(KeyValuePair<GameObject, TransformInstance> t in initTransformProperties)
        {
            t.Key.transform.position = t.Value.position;
            t.Key.transform.localScale = t.Value.localScale;
            t.Key.transform.rotation = t.Value.rotation;
        }
    }
    private void saveObjectTransforms()
    {
        if (initTransformProperties == null)
        {
            initTransformProperties = new List<KeyValuePair<GameObject, TransformInstance>>();
        }
        

        initTransformProperties.Clear();
        initTransformProperties.Add(new KeyValuePair<GameObject, TransformInstance>(this.gameObject, new TransformInstance(this.transform)));
        initTransformProperties.Add(new KeyValuePair<GameObject, TransformInstance>(part_frontGear, new TransformInstance(part_frontGear.transform)));
        initTransformProperties.Add(new KeyValuePair<GameObject, TransformInstance>(part_frontWheel, new TransformInstance(part_frontWheel.transform)));
        initTransformProperties.Add(new KeyValuePair<GameObject, TransformInstance>(part_pedal, new TransformInstance(part_pedal.transform)));
        initTransformProperties.Add(new KeyValuePair<GameObject, TransformInstance>(part_rearGear, new TransformInstance(part_rearGear.transform)));
        initTransformProperties.Add(new KeyValuePair<GameObject, TransformInstance>(part_rearWheel, new TransformInstance(part_rearWheel.transform)));

    }
    private void Roll()
    {
        Rigidbody2D rb = this.GetComponent<Rigidbody2D>();        
        if (rb == null) return;
        float GearRatio_FtoR;
        if (mFrontGearSize <= 0 || mRearGearSize <= 0) GearRatio_FtoR = 0;
            else GearRatio_FtoR= mRearGearSize / mFrontGearSize;
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

        ani = part_rider.GetComponent<Animator>();
        if (ani == null) return;
      //  Debug.Log("speed param" + frontGearAngularSpeed);
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
         Debug.Log("Set gear: " + front + " : " + rear);
        if (front < 0) front = 0;
        if (rear < 0) rear = 0;
        mFrontGearSize = front;
        mRearGearSize = rear;
        if (front != 0) mGearRatio = mRearGearSize / mFrontGearSize;
        else mGearRatio = 0;
        Vector3 newScale = minitialGearScale;
        newScale.x = newScale.x * (front / referenceGearSize);
        newScale.y = newScale.y * (front / referenceGearSize);
        part_frontGear.transform.localScale = newScale;
     //   Debug.Log("Set gear: " + newScale.x+" "+newScale.y);
        newScale = minitialGearScale;
        newScale.x = newScale.x * (rear / referenceGearSize);
        newScale.y = newScale.y * (rear / referenceGearSize);
        part_rearGear.transform.localScale = newScale;

    }


}
public class TransformInstance
{
    public Vector3 position;
    public Vector3 localScale;
    public Quaternion rotation;
    public TransformInstance(Transform t)
    {
        this.position = t.position;
        this.localScale = t.localScale;
        this.rotation = t.rotation;
    }
}