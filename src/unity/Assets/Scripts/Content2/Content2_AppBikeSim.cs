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
    public Vector3 minitialGearScale;


    public float config_baseXVelocityToAngularSpeed; //1 to rotation rate of the rearwheel and gear    
    public float referenceGearSize = 430;    
    public float mFrontGearSize;
    public float mRearGearSize;

    private List<KeyValuePair<GameObject, Transform>> initTransformProperties;
    private float initialDrag;
    

    // Use this for initialization
    void Start () {
        if (part_frontWheel == null || part_rearGear == null || part_frontGear == null || part_rearGear == null)
        {
            Debug.Log("[ERROR] the bike object's sub-objects are null");
            return;
        }
        saveObjectTransforms();
        initialDrag = this.GetComponent<Rigidbody2D>().drag;
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
    public void reset()
    {
        //load the initial transforms
        restoreObjectTransforms();
        //init the variables
        this.mFrontGearSize = 0;
        this.mRearGearSize = 0;

        //set rigidbody properties
        resetRigidProperties();


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
        foreach(var t in initTransformProperties)
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
            initTransformProperties = new List<KeyValuePair<GameObject, Transform>>();
        }
        initTransformProperties.Clear();
        initTransformProperties.Add(new KeyValuePair<GameObject, Transform>(this.gameObject, this.transform));
        initTransformProperties.Add(new KeyValuePair<GameObject, Transform>(part_frontGear, part_frontGear.transform));
        initTransformProperties.Add(new KeyValuePair<GameObject, Transform>(part_frontWheel, part_frontWheel.transform));
        initTransformProperties.Add(new KeyValuePair<GameObject, Transform>(part_pedal, part_pedal.transform));
        initTransformProperties.Add(new KeyValuePair<GameObject, Transform>(part_rearGear, part_rearGear.transform));
        initTransformProperties.Add(new KeyValuePair<GameObject, Transform>(part_rearWheel, part_rearWheel.transform));

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
        Debug.Log("Set gear: " + front + " : " + rear);
        mFrontGearSize = front;
        mRearGearSize = rear;
        Vector3 newScale = minitialGearScale;
        newScale.x = newScale.x * (front / referenceGearSize);
        newScale.y = newScale.y * (front / referenceGearSize);
        part_frontGear.transform.localScale = newScale;
        Debug.Log("Set gear: " + newScale.x+" "+newScale.y);
        newScale = minitialGearScale;
        newScale.x = newScale.x * (rear / referenceGearSize);
        newScale.y = newScale.y * (rear / referenceGearSize);
        part_rearGear.transform.localScale = newScale;

    }


}
