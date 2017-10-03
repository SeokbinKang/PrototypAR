using UnityEngine;
using System.Collections;

public class HorizontalBar : MonoBehaviour {

    public GameObject[] levelBars;
    
    private float[] level;  // 0 ~ 100

    private bool[] Statusblink;
    private float[] Statusblink_change;
    // Use this for initialization
    void Start () {
        if (levelBars != null || levelBars.Length > 0)
        {
            level = new float[levelBars.Length];
            Statusblink = new bool[levelBars.Length];
            Statusblink_change = new float[levelBars.Length];
            for (int i = 0; i < levelBars.Length;i++)
                Statusblink_change[i] = 1f;
        }
	}
	
	// Update is called once per frame
	void Update () {
        if (levelBars == null) return;
        for(int i = 0; i < levelBars.Length;i++)
        {
            Vector3 scale = levelBars[i].transform.localScale;
            scale.x = level[i] / 100f;
            levelBars[i].transform.localScale = scale;
        }
        blink();

    }
    public void updateLevel(int index, float level_)
    {
        if (level == null || index > level.Length - 1) return;
        if (level_ < 0) level_ = 0;
        else if (level_ > 100) level_ = 100;
        level[index] = level_;
    }
    private void blink()
    {
        if (level == null || levelBars == null || Statusblink==null) return;
        for(int i = 0; i < Statusblink.Length; i++)
        {
            if (Statusblink[i] == true)
            {
                blinkLevel(i);
            }
        }
    }
    public void SetBlink(int idx)
    {
        if (level == null || levelBars == null || Statusblink == null || idx > level.Length - 1) return;
        Statusblink[idx] = true;
        
    }
    private void blinkLevel(int index)
    {
        SpriteRenderer sr = levelBars[index].GetComponent<SpriteRenderer>();
        if (sr == null) return;
        Color t = sr.color;
        t.a = t.a + Statusblink_change[index] * Time.deltaTime;
        Debug.Log("status blink change" + Statusblink_change[index]+"t.a"+t.a);
        if(t.a>=1f)
        {
            t.a = 1;
            Statusblink_change[index] = Statusblink_change[index] * -1;
        } else if (t.a <= 0)
        {
            t.a = 0;
            Statusblink_change[index] = Statusblink_change[index] * -1;
        }
        sr.color = t;
    }
}
