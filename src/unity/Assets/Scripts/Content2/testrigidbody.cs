using UnityEngine;
using System.Collections;

public class testrigidbody : MonoBehaviour {

	// Use this for initialization
	void Start () {
        Rigidbody2D t = this.GetComponent<Rigidbody2D>();
        t.AddForce(new Vector2(3, 0), ForceMode2D.Force);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
