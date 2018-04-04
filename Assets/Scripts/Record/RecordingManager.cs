using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
/// <summary>
/// Record mutual gaze on host
/// </summary>
/// 
/// TODO: record head rotation in 2nd experiment
public class RecordingManager : Photon.PunBehaviour {

    private string _eyeFilename;
    private StreamWriter sw_mutualGaze;
    public StreamWriter sw_otherLog;

    public GameObject[]  Eyes;            // Left eye gameobjects on avatars    
    private WorldTimer _worldTimer;
    private bool _isMutualGazeInLastFrame;
    public bool AvatarsReady;
    private bool _wasAGazingBLastFrame;
    private bool _wasBGazingALastFrame;

    [HideInInspector]
    public bool IsRecording = false;

    // Use this for initialization
    void Start () {
        _worldTimer = GetComponent<WorldTimer>();
        if (PhotonNetwork.isMasterClient)
        {
            Eyes = new GameObject[2];
        }       
    }

    // Update is called once per frame
    void Update () {
        if (PhotonNetwork.isMasterClient && AvatarsReady)
        {
            // ==========================Recording for mutual gaze=====================================
            if (!_isMutualGazeInLastFrame && IsMutualGaze(Eyes[0], Eyes[1]))
            {
                if (IsRecording)
                {
                    sw_mutualGaze.WriteLine(_worldTimer.ElapsedTimeSinceStart.TotalSeconds + ",Mutual gaze starts");
                    Debug.Log(_worldTimer.ElapsedTimeSinceStart.TotalSeconds + ",Mutual gaze starts");
                }
            }
            else if(_isMutualGazeInLastFrame && !IsMutualGaze(Eyes[0], Eyes[1]))
            {
                if (IsRecording)
                {
                    sw_mutualGaze.WriteLine(_worldTimer.ElapsedTimeSinceStart.TotalSeconds + ",Mutual gaze ends");
                    Debug.Log(_worldTimer.ElapsedTimeSinceStart.TotalSeconds + ",Mutual gaze ends");
                }
            }

            _isMutualGazeInLastFrame = IsMutualGaze(Eyes[0], Eyes[1]);

            // ==========================Recording for one-way gaze=====================================
            LogOtherBehaviours(sw_otherLog);
        }
    }

    void OnApplicationQuit()
    {
        //Stop data recording when Unity quit if it's still writing
        if (IsRecording)
            StopRecording();    
    }

    public override void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer)
    {
        AvatarsReady = false;
    }


    public void StartRecording()
    {
        string dyadType = GameManager.Instance.DyadType;
        string distance = GameObject.FindObjectOfType<SceneManager_Game>().CurrentDistance.ToString();

        string path = "DataRecording/";
        Directory.CreateDirectory("DataRecording");

        _eyeFilename = GameManager.Instance.UsingVR ? "_VR" : "Real_";
        _eyeFilename += dyadType + "_" + distance;
        string nameBase = String.Format("{0}_{1:yyyy-MM-dd_HH-mm-ss}", _eyeFilename, DateTime.Now);
        _eyeFilename = path + "EyeData_" + nameBase + ".txt";
        string _otherLog = path + "OtherLog_" + nameBase + ".txt";
        sw_mutualGaze = new StreamWriter(_eyeFilename);
        sw_otherLog = new StreamWriter(_otherLog);

        IsRecording = true;
        _worldTimer.StartTimer();
        sw_mutualGaze.WriteLine(_worldTimer.ElapsedTimeSinceStart.TotalSeconds + ",Start recording");
        sw_otherLog.WriteLine(_worldTimer.ElapsedTimeSinceStart.TotalSeconds + ",Start recording");

        if (IsMutualGaze(Eyes[0], Eyes[1])) {
            sw_mutualGaze.WriteLine(_worldTimer.ElapsedTimeSinceStart.TotalSeconds + ",Mutual gaze starts");
        }
    }


    public void StopRecording()
    {     
        IsRecording = false;
        
        if (sw_mutualGaze != null)
        {
            sw_mutualGaze.WriteLine(_worldTimer.ElapsedTimeSinceStart.TotalSeconds + ",Stop recording");
            sw_mutualGaze.Close();
        }
        if (sw_otherLog != null)
        {
            sw_otherLog.WriteLine(_worldTimer.ElapsedTimeSinceStart.TotalSeconds + ",Stop recording");
            sw_otherLog.Close();
        }

        GetComponent<WorldTimer>().StopTimer();
        GetComponent<WorldTimer>().ResetTimer();
    }

    bool IsMutualGaze(GameObject eye1, GameObject eye2)
    {
        // if angle between vector eye.forward and eye-eye vector < 9°, then is gazing       
       
        return IsAGazingB(eye1,eye2) && IsAGazingB(eye2, eye1);
    }

    bool IsAGazingB(GameObject a, GameObject b)
    {
        Vector3 gazeDirection;
        Vector3 AToBDir;

        gazeDirection = a.transform.forward;
        AToBDir = b.transform.position - a.transform.position;
        float angle = Vector3.Angle(gazeDirection, AToBDir);

        return Mathf.Abs(angle) < 9f;
    }

    void LogOtherBehaviours(StreamWriter sw)
    {
        if (!_wasAGazingBLastFrame && IsAGazingB(Eyes[0], Eyes[1]))
        {
            if (IsRecording)
            {
                sw.WriteLine(_worldTimer.ElapsedTimeSinceStart.TotalSeconds + "," + Eyes[0].name + ",starts gazing at " + Eyes[1].name);
                Debug.Log(_worldTimer.ElapsedTimeSinceStart.TotalSeconds + Eyes[0].name + ",One-way gaze starts");
            }
        }
        else if (_wasAGazingBLastFrame && !IsAGazingB(Eyes[0], Eyes[1]))
        {
            if (IsRecording)
            {
                sw.WriteLine(_worldTimer.ElapsedTimeSinceStart.TotalSeconds + "," + Eyes[0].name + ",One-way gaze ends");
                Debug.Log(_worldTimer.ElapsedTimeSinceStart.TotalSeconds + Eyes[0].name + ",One-way gaze ends");
            }
        }
        _wasAGazingBLastFrame = IsAGazingB(Eyes[0], Eyes[1]);

        // Another way around
        if (!_wasBGazingALastFrame && IsAGazingB(Eyes[1], Eyes[0]))
        {
            if (IsRecording)
            {
                sw.WriteLine(_worldTimer.ElapsedTimeSinceStart.TotalSeconds + "," + Eyes[1].name + ",starts gazing at " + Eyes[0].name);
                Debug.Log(_worldTimer.ElapsedTimeSinceStart.TotalSeconds + Eyes[1].name + ",One-way gaze starts");
            }
        }
        else if (_wasBGazingALastFrame && !IsAGazingB(Eyes[1], Eyes[0]))
        {
            if (IsRecording)
            {
                sw.WriteLine(_worldTimer.ElapsedTimeSinceStart.TotalSeconds + "," + Eyes[1].name + ",One-way gaze ends");
                Debug.Log(_worldTimer.ElapsedTimeSinceStart.TotalSeconds + Eyes[1].name + ",One-way gaze ends");
            }
        }
        _wasBGazingALastFrame = IsAGazingB(Eyes[1], Eyes[0]);

    }
}
