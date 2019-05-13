using UnityEngine;
using System.Collections;

public class SpriteDrag : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        //Checks whether the mouse is over the sprite
        bool overSprite = this.GetComponent<SpriteRenderer>().bounds.Contains(mousePosition);
        Debug.Log(mousePosition+"  "+overSprite);
        //If it's over the sprite
        if (overSprite)
        {
            //If we've pressed down on the mouse (or touched on the iphone)
            if (Input.GetMouseButtonDown(0))
            {
                Debug.Log("key down");
                //Set the position to the mouse position
                this.transform.position = new Vector3(Camera.main.ScreenToWorldPoint(Input.mousePosition).x,
                                                    Camera.main.ScreenToWorldPoint(Input.mousePosition).y,
                                                      0.0f);
            }
        }
    }
}
