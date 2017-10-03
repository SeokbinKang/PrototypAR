using UnityEngine;
using System.Collections;
using UnityEditor;

[ExecuteInEditMode]
[RequireComponent(typeof(ParticleSystem))]

public class ParticleBubble : MonoBehaviour {

    public class PathParticleTracker
    {
        public ParticleSystem.Particle particle;
        public float distance;
        public float rotation;

        public PathParticleTracker()
        {

            this.particle = new ParticleSystem.Particle();
            this.particle.lifetime = 0.0f;

        }

        public void Revive(ParticleSystem systemRef)
        {
            
            this.distance = Random.Range(0.0f, 1.0f);
            this.rotation = Random.Range(0.0f, 360.0f);
            
            this.particle.position =  new Vector3(Random.Range(WorldTankLB.x,WorldTankRT.x ), Random.Range(WorldTankLB.y, WorldTankRT.y), 0);
            this.particle.startLifetime = systemRef.startLifetime;
            this.particle.lifetime = this.particle.startLifetime;
            this.particle.startColor = systemRef.startColor;
            this.particle.startSize = systemRef.startSize;
            this.particle.rotation = systemRef.startRotation;
        }
    }

    public float emissionRate = 25.0f;
    private float _emissionRateTracker = 0.0f;


    [Range(0.0f, 5.0f)]
    public float pathWidth = 0.0f;

    private int _particle_count;
    private PathParticleTracker[] _particle_trackerArray;
    private ParticleSystem.Particle[] _particle_array;
    private ParticleSystem _particle_system;


    private double _editorTimeDelta = 0.0f;
    private double _editorTimetracker = 0.0f;


    //private Path_Comp _path_comp;
    public static Vector3 WorldTankLB;
    public static Vector3 WorldTankRT;
    void OnEnable()
    {

        //  _path_comp = GetComponent<Path_Comp>();
        Debug.Log("init particle");
        _particle_system = GetComponent<ParticleSystem>();
        ParticleSystem.EmissionModule em = _particle_system.emission;
     //   em.enabled = false;

     
     
        updateWorldTank();

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            _editorTimetracker = EditorApplication.timeSinceStartup;
        }
#endif

    }

    void LateUpdate()
    {

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            _editorTimeDelta = EditorApplication.timeSinceStartup - _editorTimetracker;
            _editorTimetracker = EditorApplication.timeSinceStartup;
        }
#endif

     
        
        // move them
        ParticleSystem.Particle[] activeParticles=new ParticleSystem.Particle[_particle_system.particleCount];
        int pCount;
        pCount = _particle_system.GetParticles(activeParticles);
        for(int i=0;i<pCount;i++)
        
        {
            Vector3 p = activeParticles[i].position;
            p.z = 0;
            activeParticles[i].position = p;
            
        }        

      //  _particle_system.SetParticles(activeParticles, pCount);
        
    }

    void RenewOneDeadParticle()
    {

        for (int i = 0; i < _particle_trackerArray.Length; i++)
            if (_particle_trackerArray[i].particle.lifetime <= 0.0f)
            {
                _particle_trackerArray[i].Revive(_particle_system);
                break;
            }
    }
    private void updateWorldTank()
    {
        WorldTankLB = Camera.main.ScreenToWorldPoint(new Vector3(30, 30, 1));
        WorldTankRT = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width - 30, Screen.height - 30, 1));
    }

    private void OLDWAY()
    {

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            _editorTimeDelta = EditorApplication.timeSinceStartup - _editorTimetracker;
            _editorTimetracker = EditorApplication.timeSinceStartup;
        }
#endif



        // emision
        if (_emissionRateTracker <= 0.0f)
        {
            _emissionRateTracker += 1.0f / emissionRate;

            RenewOneDeadParticle();
        }
        _emissionRateTracker -= (Application.isPlaying ? Time.deltaTime : (float)_editorTimeDelta);

        // age them
        foreach (PathParticleTracker tracker in _particle_trackerArray)
            if (tracker.particle.lifetime > 0.0f)
            {
                tracker.particle.lifetime = Mathf.Max(tracker.particle.lifetime - (Application.isPlaying ? Time.deltaTime : (float)_editorTimeDelta), 0.0f);
            }


        float normLifetime = 0.0f;
        //   Path_Point Rpoint;

        // move them
        foreach (PathParticleTracker tracker in _particle_trackerArray)
            if (tracker.particle.lifetime > 0.0f)
            {

                normLifetime = tracker.particle.lifetime / tracker.particle.startLifetime;
                normLifetime = 1.0f - normLifetime;
                //  Rpoint = _path_comp.GetPathPoint(normLifetime * _path_comp.TotalDistance);

                Vector3 vel = new Vector3(0, 3f, 0);
                Vector3 pos = tracker.particle.position;
                pos += new Vector3(Random.Range(-0.05f, 0.05f), Random.Range(0f, 0.2f), 0) * Time.deltaTime;
                tracker.particle.position = pos;
                // rotate around Rpoint.direction

                //      Rpoint.point += (pathWidth * tracker.distance) * Math_Functions.Rotate_Direction(Rpoint.up, Rpoint.forward, tracker.rotation);

                //tracker.particle.position = pos;
                //tracker.particle.velocity = Rpoint.forward
                //  tracker.particle.velocity = vel;


            }

        _particle_count = 0;

        // set the given array
        foreach (PathParticleTracker tracker in _particle_trackerArray)
            if (tracker.particle.lifetime > 0.0f)
            { // it's alive
                _particle_array[_particle_count] = tracker.particle;
                _particle_count++;
            }

        _particle_system.SetParticles(_particle_array, _particle_count);
    }
}
