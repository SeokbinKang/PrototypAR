using UnityEngine;
using System.Collections;

public class BeamUnit : MonoBehaviour {

    public Vector2 BoundaryPos;

    public Vector2 InactivePos;

    public float BaseVelocity;

    private float[] hsvColor;
    private bool IsDecaying;
	// Use this for initialization
	void Start () {
        
        BaseVelocity = 0;
        Color c = this.GetComponent<SpriteRenderer>().color;
        hsvColor = new float[3];
        Color.RGBToHSV(c, out hsvColor[0], out hsvColor[1], out hsvColor[2]);
    }
	
	// Update is called once per frame
	void Update () {
        checkIfDead();
        UpdateDecay();
      //  changeColor();
    }
   /* void OnCollisionEnter2D(Collision2D col)
    {
        Debug.Log("OnCollision!");
        this.GetComponent<Rigidbody2D>().velocity = new Vector2(10, 10);
        if(col.gameObject.tag == "C4_lens")
        {
            //check the pos of the focal point

            //change velocity toward the focal point

            //do something
        }


    }*/
    void OnTriggerEnter2D(Collider2D col)
    {

        if (IsDecaying) return;
        if (col.gameObject.tag == "C4_sensor")
        {            
            col.gameObject.GetComponent<C4_sensor>().OnLightReceived();
            Decay();
        }
        if (col.gameObject.tag == "C4_shutter")
        {
            AnimatorStateInfo i = col.gameObject.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0);
            if(!i.IsName("open"))    Decay();
        }

    }
    void OnTriggerExit2D(Collider2D col)
    {
        // Debug.Log("OnTrigger!");
        if (IsDecaying) return;
        if (col.gameObject.tag == "C4_lens")
        {
            //this.GetComponent<Rigidbody2D>().velocity = new Vector2(10, 10);
            //check the pos of the focal point
            Vector3 focus = col.gameObject.GetComponent<C4_lens>().getFocusPos();
            //change velocity toward the focal point
            Vector3 direction = focus - this.transform.position;
            Vector2 direction2 = (Vector2)focus - this.GetComponent<Rigidbody2D>().position;
            Vector3 newVel = direction.normalized * BaseVelocity;
            this.GetComponent<Rigidbody2D>().velocity = newVel;
          //  Debug.Log("Light beam direction " + direction + "\t" + direction2);
            //change rotation
            float rotateZRad = Mathf.Atan2(direction.y, direction.x);
            float rotateZ = Vector2.Angle(Vector2.zero, (Vector2)direction);
            //   Debug.Log("direction: " + direction+" angle: "+rotateZ);

            this.transform.Rotate(new Vector3(0, 0, rotateZRad * Mathf.Rad2Deg));
            //this.transform.Rotate(new Vector3(0, 0, rotateZ));

        }
    }
    private void changeColor()
    {
        Color cPrev = this.GetComponent<SpriteRenderer>().color;
        hsvColor[0] = hsvColor[0] + 0.02f;
        if (hsvColor[0] >= 1) hsvColor[0] = 0;
        Color c = Color.HSVToRGB(hsvColor[0], hsvColor[1], hsvColor[2]);
        c.a = cPrev.a;
        this.GetComponent<SpriteRenderer>().color = c;
    }
    public void Revive()
    {
        IsDecaying = false;
        Color c = this.GetComponent<SpriteRenderer>().color;
        c.a =1f;
        this.GetComponent<SpriteRenderer>().color = c;
        
    }
    public void Decay()
    {
        IsDecaying = true;
    }
    private void UpdateDecay()
    {
        if (IsDecaying)
        {
            Color c = this.GetComponent<SpriteRenderer>().color;
            c.a = c.a - 0.2f;
            this.GetComponent<SpriteRenderer>().color = c;
            if (c.a <= 0) dead();
        }
    }
    private void TargetTo()
    {

    }
    public bool isDead()
    {
        if (this.GetComponent<Rigidbody2D>().velocity == Vector2.zero) return true;
        return false;
    }
    private void dead()
    {
        this.GetComponent<Rigidbody2D>().velocity = new Vector2(0, 0);
        this.transform.rotation = Quaternion.identity;
        this.transform.localPosition = InactivePos;
        this.GetComponent<TrailRenderer>().enabled = false;
        Revive();
    }

    private void checkIfDead()
    {

        if(this.transform.position.x>this.BoundaryPos.x || Mathf.Abs(this.transform.position.y) > Mathf.Abs(this.BoundaryPos.y))
        {
            this.dead();
            return;
        }
    }
    public void SetVelocity(Vector2 v)
    {
        this.GetComponent<Rigidbody2D>().velocity = v;
        this.BaseVelocity = v.magnitude;
        
    }

}
