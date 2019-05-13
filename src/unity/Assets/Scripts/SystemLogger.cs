using UnityEngine;
using System.Collections;
using System.IO;
using System;
using System.Collections.Generic;

public class SystemLogger : MonoBehaviour
{


    public string UserActionLog = "./log/useraction/log.txt";
    // Use this for initialization
    public static SystemLogger mInstance;
    private int mCounter = 0;
    public static SystemLogger activeInstance;

    private float AccDelta;
    private List<float>[] performanceLogger;
    
    void Start()
    {
        mInstance = this;
        activeInstance = this;
        AccDelta = 0;
        Application.targetFrameRate = 60;
        performanceLogger = new List<float>[4];
        for (int i = 0; i < 4; i++)
            performanceLogger[i] = new List<float>();
              
    }

    // Update is called once per frame
    void Update()
    {
        AccDelta += Time.deltaTime;
        if (mCounter++ % 60 == 0) CheckFPS();
    }
    void OnDestroy()
    {
        File.AppendAllText(UserActionLog, "******PERFORMANCE SUMMARY******\r\n");
        File.AppendAllText(UserActionLog, "FPS: " + 1f / (AccDelta / (float)mCounter)+"\r\n");

        CheckFPS();
        for (int i = 0; i < 4; i++) {
            float sum = 0;
            foreach (var p in performanceLogger[i])
                sum += p;
            if (performanceLogger[i].Count > 0) sum = sum / performanceLogger[i].Count;
            File.AppendAllText(UserActionLog, "Performance " + i + ": " + sum + "\r\n");
        }

            
    }
    public void AddPerformance(int idx, float time)
    {
        performanceLogger[idx].Add(time);
    }
    private void CheckFPS()
    {
        Debug.Log("FPS: " + 1f / (AccDelta / (float)mCounter));
        //  AccDelta = 0;
    }
    public void WriteUserEvent(string txt)
    {
        //string t = DateTime.Now.ToLongTimeString() + "\t" + txt+"\r\n";
        string t = System.DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss") + "\t" + txt + "\r\n";
        File.AppendAllText(UserActionLog, t);
    }

}
