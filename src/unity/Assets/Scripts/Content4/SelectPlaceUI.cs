using UnityEngine;
using System.Collections;

public class SelectPlaceUI : MonoBehaviour {
    public GameObject[] MenuItems;
    public GameObject[] MenuDesignGoals;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
    void OnEnable()
    {
        if (MenuItems != null) {
            for (int i = 0;i<MenuItems.Length;i++)
            {
                MenuItems[i].SetActive(true);
            }
        }
        if (MenuDesignGoals != null)
        {
            for (int i = 0; i < MenuDesignGoals.Length; i++)
            {
                MenuDesignGoals[i].SetActive(false);
            }
        }

    }
    public void OnMenuSelected(int k) {
        if (MenuItems != null)
        {
            for (int i = 0; i < MenuItems.Length; i++)
            {
               MenuItems[i].SetActive(false);
            }
        }
        if (MenuDesignGoals != null)
        {
            for (int i = 0; i < MenuDesignGoals.Length; i++)
            {
                if (i != k) MenuDesignGoals[i].SetActive(false);
                    else MenuDesignGoals[i].SetActive(true);
            }
        }


    }
}
