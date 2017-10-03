using UnityEngine;
using System.Collections;
using OpenCvSharp;
using OpenCvSharp.Blob;
using OpenCvSharp.CPlusPlus;
using System.Collections.Generic;

using System.IO;
using System.Text;

using System.Runtime.Serialization.Formatters.Binary;
using System;

public class SceneObjectManager : MonoBehaviour {

    private static SceneObjectManager activeSOMgr = null;
    private Dictionary<PreLoadedObjects, GameObject> SceneObjectPool;
    
    // Use this for initialization
    void Start () {
        activeSOMgr = this;
        initSceneObject();

    }
	
	// Update is called once per frame
	void Update () {
	
	}
    public static SceneObjectManager getActiveInstance()
    {
        return activeSOMgr;
    }
    public void initSceneObject()
    {
        SceneObjectPool = new Dictionary<PreLoadedObjects, GameObject>();
        SceneObjectPool.Add(PreLoadedObjects.BEH_BV_missing_contract, GameObject.Find("BV_Missing_Contract_0"));
        SceneObjectPool.Add(PreLoadedObjects.BEH_BV_missing_pedal, GameObject.Find("BV_missing_Pedal"));
        SceneObjectPool.Add(PreLoadedObjects.BEH_BV_missing_reduce, GameObject.Find("BV_missing_Reduce"));
        SceneObjectPool.Add(PreLoadedObjects.STR_missing_c1_air, GameObject.Find("STR_MS_airways"));
        SceneObjectPool.Add(PreLoadedObjects.STR_missing_c1_lung, GameObject.Find("STR_MS_lung"));
        SceneObjectPool.Add(PreLoadedObjects.STR_missing_c1_dia, GameObject.Find("STR_MS_diaphragm"));
        SceneObjectPool.Add(PreLoadedObjects.STR_EXTRA_left, GameObject.Find("STR_EXTRA_left"));
        SceneObjectPool.Add(PreLoadedObjects.STR_EXTRA_down, GameObject.Find("STR_EXTRA_down"));
        SceneObjectPool.Add(PreLoadedObjects.STR_EXTRA_right, GameObject.Find("STR_EXTRA_up"));
        SceneObjectPool.Add(PreLoadedObjects.STR_EXTRA_up, GameObject.Find("STR_EXTRA_right"));
        SceneObjectPool.Add(PreLoadedObjects.STR_SHAPE_left, GameObject.Find("STR_SHAPE_left"));
        SceneObjectPool.Add(PreLoadedObjects.STR_SHAPE_right, GameObject.Find("STR_SHAPE_right"));
        SceneObjectPool.Add(PreLoadedObjects.STR_POS_dialog, GameObject.Find("STR_POS"));
        SceneObjectPool.Add(PreLoadedObjects.STR_CONN_missing_Dialogue, GameObject.Find("STR_CONN_missing"));
        SceneObjectPool.Add(PreLoadedObjects.STR_CONN_incorrect_Dialogue, GameObject.Find("STR_CONN_incorrect"));
        SceneObjectPool.Add(PreLoadedObjects.BEH_BL_missing, GameObject.Find("BEH_BL_missing"));
        SceneObjectPool.Add(PreLoadedObjects.BEH_BL_unnecessary_down, GameObject.Find("BEH_BL_unneces_down"));
        SceneObjectPool.Add(PreLoadedObjects.BEH_BL_unnecessary_left, GameObject.Find("BEH_BL_unneces_left"));
        SceneObjectPool.Add(PreLoadedObjects.BEH_BL_unnecessary_right, GameObject.Find("BEH_BL_unneces_right"));
        SceneObjectPool.Add(PreLoadedObjects.BEH_BL_unnecessary_up, GameObject.Find("BEH_BL_unneces_up"));
        SceneObjectPool.Add(PreLoadedObjects.BEH_BL_remap, GameObject.Find("BEH_BL_remap"));

        SceneObjectPool.Add(PreLoadedObjects.Content1_BGPartial, GameObject.Find("c1_bgpartial"));
        SceneObjectPool.Add(PreLoadedObjects.Content1_BGFull, GameObject.Find("c1_bgfull"));
        SceneObjectPool.Add(PreLoadedObjects.Content2_BGPartial, GameObject.Find("c2_bgpartial"));
        SceneObjectPool.Add(PreLoadedObjects.Content2_BGFull, GameObject.Find("c2_bgfull"));




        foreach (var item in SceneObjectPool)
        { 

            item.Value.SetActive(false);
        }


        /*  GameObject obj = GameObject.Find("test2d");
            Vector3 pos = Camera.main.ScreenToWorldPoint(new Vector3(x*2*Screen.width/1920, y*2*Screen.height/1080,1));
            obj.transform.position = pos;*/
    }
    public void activateObject(PreLoadedObjects objType,bool activate)
    {
        if (!SceneObjectPool.ContainsKey(objType)) return;

        GameObject obj = SceneObjectPool[objType];
        
        
        obj.SetActive(activate);
        Debug.Log("[DEBUG] Scene Object activated:"+obj.name);

    }
    public void adjustAlphaSpriteRendere(PreLoadedObjects objType,float value)
    {
        if (!SceneObjectPool.ContainsKey(objType)) return;

        GameObject obj = SceneObjectPool[objType];
        obj.SetActive(true);
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        if (sr == null) return;
        Color spriteColor = sr.color;
        if (Mathf.Abs(spriteColor.a - value / 255f)>0.005f)
        {
     //       Debug.Log("alpha : " + spriteColor.a + "---->" + value / 255f);
            spriteColor.a = spriteColor.a + Math.Max(Mathf.Abs((value / 255f - spriteColor.a) * 0.003f), 0.003f)*Mathf.Sign(value/255f - spriteColor.a);
        }
        else
        {
        
            spriteColor.a = value / 255f;
        }
        sr.color = spriteColor;
        return;
    }
    public void adjustAlphaSpriteRendere(PreLoadedObjects objType, float value, float mindiff)
    {
        if (!SceneObjectPool.ContainsKey(objType)) return;

        GameObject obj = SceneObjectPool[objType];
        obj.SetActive(true);
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        if (sr == null) return;
        Color spriteColor = sr.color;
        if (Mathf.Abs(spriteColor.a - value / 255f) > mindiff*1.5f)
        {
            //       Debug.Log("alpha : " + spriteColor.a + "---->" + value / 255f);
            spriteColor.a = spriteColor.a + Math.Max(Mathf.Abs((value / 255f - spriteColor.a) * mindiff), mindiff) * Mathf.Sign(value / 255f - spriteColor.a);
        }
        else
        {

            spriteColor.a = value / 255f;
        }
        sr.color = spriteColor;
        return;
    }
    public void adjustAlphaSpriteRenderInstant(PreLoadedObjects objType, float value)
    {
        if (!SceneObjectPool.ContainsKey(objType)) return;

        GameObject obj = SceneObjectPool[objType];
        obj.SetActive(true);
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        if (sr == null) return;
        Color spriteColor = sr.color;
        

        spriteColor.a = value / 255f;
        
        sr.color = spriteColor;
        return;
    }
    public static void adjustAlphaSpriteRendere(GameObject obj, float value,float mindiff)
    {
        if (obj==null) return;
        obj.SetActive(true);
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        if (sr == null) return;
        Color spriteColor = sr.color;
        if (spriteColor.a < value / 255f)
        {
            //       Debug.Log("alpha : " + spriteColor.a + "---->" + value / 255f);
            spriteColor.a = spriteColor.a + Math.Max((value / 255f - spriteColor.a) * mindiff, mindiff);
        }
        else
        {

            spriteColor.a = value / 255f;
        }
        sr.color = spriteColor;
        return;
    }

    public void MoveToScreenRelativePos(PreLoadedObjects objType, Vector2 screenPosition)
    {

        if (!SceneObjectPool.ContainsKey(objType)) return;

        GameObject obj = SceneObjectPool[objType];
        if (obj == null) return;
        obj.SetActive(true);
        Vector3 worldpos = Camera.main.ScreenToWorldPoint(new Vector3(screenPosition.x *Screen.width, screenPosition.y*Screen.height, 1));
        obj.transform.position = worldpos;
       
    }
    public void activateObject(PreLoadedObjects objType, CvPoint posinRegionBox)
    {
        if (!SceneObjectPool.ContainsKey(objType) || objType==PreLoadedObjects.None) return;
        
        GameObject obj = SceneObjectPool[objType];
        CvRect regionBox = GlobalRepo.GetRegionBox(false);
        if (regionBox.Width == 0) return;
        Vector3 pos = Camera.main.ScreenToWorldPoint(new Vector3(posinRegionBox.X * Screen.width / regionBox.Width, (Screen.height - posinRegionBox.Y * Screen.height / regionBox.Height), 1));
        obj.SetActive(true);
        obj.transform.position = pos;
        
        Debug.Log("[DEBUG] Scene Object activated at " + DebugTrans.ToString(posinRegionBox));
        
    }
    public static CvPoint ScreentoRegion(Vector3 screenp)
    {
        CvPoint ret = new CvPoint();
        ret.X = ((int)screenp.x) * GlobalRepo.GetRegionBox(false).Width / Screen.width;
        ret.Y = GlobalRepo.GetRegionBox(false).Height - ((int)screenp.y) * GlobalRepo.GetRegionBox(false).Height / Screen.height;
        
        return ret;
    }
    public static Vector3 RegiontoScreen(CvPoint p)
    {
        CvRect regionBox = GlobalRepo.GetRegionBox(false);
        if (regionBox.Width == 0) return Vector3.zero;
        Vector3 ret = new Vector3(p.X * Screen.width / regionBox.Width, Screen.height - p.Y * Screen.height / regionBox.Height, 1);
        return ret;
    }
    public static Vector3 regionToWorld(CvPoint p)
    {
        CvRect regionBox = GlobalRepo.GetRegionBox(false);
        if (regionBox.Width == 0) return Vector3.zero;
        Vector3 ret = new Vector3(p.X * Screen.width / regionBox.Width, Screen.height - p.Y * Screen.height / regionBox.Height, 1);

        return Camera.main.ScreenToWorldPoint(ret);
    }
    public static void MeasureObjectInfoinScreenCoord(GameObject go, ref Vector3 center, ref Vector3 size)
    {        
        if (go == null) return;
        SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
        if (sr == null) return ;

        center = sr.bounds.center;
        center = Camera.main.WorldToScreenPoint(center);

        Vector3 minp = sr.bounds.min;
        Vector3 maxp = sr.bounds.max;
        //   Debug.Log("[DEBUG SImulation]  GO bound center : " + sr.bounds.center + "   extend : " + sr.bounds.extents);
        //  Debug.Log("[DEBUG SImulation]  GO bound world min pos : " + minp + "   max pos : " + maxp);
        minp.z = sr.bounds.center.z;
        maxp.z = sr.bounds.center.z;
        minp = Camera.main.WorldToScreenPoint(minp);
        maxp = Camera.main.WorldToScreenPoint(maxp);
      //  Debug.Log("[DEBUG SImulation]  GO bound Screem min pos : " + minp + "   max pos : " + maxp);
        size = maxp - minp;
    }
    public static void MeasureObjectPointinScreen(PreLoadedObjects preObjType, Vector2 pointPivot, ref Vector3 screenCoord)
    {
        if (activeSOMgr==null || !activeSOMgr.SceneObjectPool.ContainsKey(preObjType)) return;

        GameObject obj = activeSOMgr.SceneObjectPool[preObjType];
        if (obj == null) return;
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        if (sr == null) return;
        Vector3 minp = sr.bounds.min;
        Vector3 maxp = sr.bounds.max;
        Vector3 centerp = sr.bounds.center;
        Vector3 size;
        
        minp = Camera.main.WorldToScreenPoint(minp);
        maxp = Camera.main.WorldToScreenPoint(maxp);
        size = maxp - minp;
       // Debug.Log("[DEBUG MeasureObjectPointinScreen]  min : " + minp + "   max : " + maxp);
        screenCoord.x = minp.x + size.x * pointPivot.x;
        screenCoord.y = minp.y + size.y * pointPivot.y;
        screenCoord.z = centerp.z;


    }
    public static Vector2 MeasureObjectPivotwithDirectionVector(GameObject go, Vector2 directionVector)
    {  //assume directionVector is unit vector. // vecotr has to be x flipped
        Vector2 ret=new Vector2(0.5f,0.5f);
        SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
        if (sr == null) return ret;
        Sprite sp = sr.sprite;
        if (sp == null) return ret;
        directionVector.Normalize();
        float width = sp.texture.width;
        float height = sp.texture.height;
        Vector2 pivot = new Vector2(0.5f, 0.5f); // assume the center not CM
        Vector2 curPoint = new Vector2(width * pivot.x, height * pivot.y);
        while (curPoint.x<=width && curPoint.y<=height)
        {
            Color p = sp.texture.GetPixel((int)curPoint.x, (int)curPoint.y);
            if (p.a == 0) break;
            curPoint += directionVector;
        }
        ret.x = curPoint.x / width;
        ret.y = curPoint.y / height;
        return ret;
    }
  
}

public enum PreLoadedObjects
{
    None,
    STR_missing_c1_lung,
    STR_missing_c1_dia,
    STR_missing_c1_air,
    STR_EXTRA_left,
    STR_EXTRA_down,
    STR_EXTRA_right,
    STR_EXTRA_up,
    STR_SHAPE_left,
    STR_SHAPE_right,
    STR_POS_dialog,
    STR_CONN_missing_Dialogue,
    STR_CONN_incorrect_Dialogue,
    BEH_BL_missing,
    BEH_BL_unnecessary_down,
    BEH_BL_unnecessary_left,
    BEH_BL_unnecessary_right,
    BEH_BL_unnecessary_up,
    BEH_BL_remap,
    BEH_BV_missing_contract,
    BEH_BV_missing_pedal,
    BEH_BV_missing_reduce,
    Content1_BGPartial,
    Content1_BGFull,
    Content2_BGPartial,
    Content2_BGFull,
    TotalNofObjects
}
