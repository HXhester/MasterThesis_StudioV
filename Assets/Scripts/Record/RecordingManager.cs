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
    public StreamWriter sw_otherLogForEyes;
    public StreamWriter sw_otherLogForHeads;
    public StreamWriter sw_audio;

    public GameObject[]  Eyes;            // Left eye gameobjects on avatars
    public GameObject[] Heads;
    private WorldTimer _worldTimer;
    private bool _isMutualGazeInLastFrame;
    private bool _isMutualGazeInLastFrame_Head;
    public bool AvatarsReady;
    private bool _wasAGazingBLastFrame_Eyes;
    private bool _wasBGazingALastFrame_Eyes;
    private bool _wasAGazingBLastFrame_Heads;
    private bool _wasBGazingALastFrame_Heads;
    private bool _wasTalkingLastFrame;

    [HideInInspector]
    public bool IsRecording = false;

    // Use this for initialization
    void Start () {
        _worldTimer = GetComponent<WorldTimer>();
        if (PhotonNetwork.isMasterClient)
        {
            Eyes = new GameObject[2];
            Heads = new GameObject[2];
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
                    sw_mutualGaze.WriteLine(_worldTimer.ElapsedTimeSinceStart.TotalSeconds + ",eyes,Mutual gaze starts");
                    Debug.Log(_worldTimer.ElapsedTimeSinceStart.TotalSeconds + ",eyes,Mutual gaze starts");
                }
            }
            else if(_isMutualGazeInLastFrame && !IsMutualGaze(Eyes[0], Eyes[1]))
            {
                if (IsRecording)
                {
                    sw_mutualGaze.WriteLine(_worldTimer.ElapsedTimeSinceStart.TotalSeconds + ",eyes,Mutual gaze ends");
                    Debug.Log(_worldTimer.ElapsedTimeSinceStart.TotalSeconds + ",eyes,Mutual gaze ends");
                }
            }

            _isMutualGazeInLastFrame = IsMutualGaze(Eyes[0], Eyes[1]);

            // ==========================Recording for one-way gaze=====================================
            LogOtherBehavioursEyes(sw_otherLogForEyes,Eyes);

            // ==========================Recording for head in vr mode==================================
            if (!GameManager.Instance.UsingVR)
                return;

            if (!_isMutualGazeInLastFrame_Head && IsMutualGaze(Heads[0], Heads[1])) {
                if (IsRecording) {
                    sw_mutualGaze.WriteLine(_worldTimer.ElapsedTimeSinceStart.TotalSeconds + ",heads,Mutual gaze starts");
                    Debug.Log(_worldTimer.ElapsedTimeSinceStart.TotalSeconds + ",heads,Mutual gaze starts");
                }
            } else if (_isMutualGazeInLastFrame_Head && !IsMutualGaze(Heads[0], Heads[1])) {
                if (IsRecording) {
                    sw_mutualGaze.WriteLine(_worldTimer.ElapsedTimeSinceStart.TotalSeconds + ",heads,Mutual gaze ends");
                    Debug.Log(_worldTimer.ElapsedTimeSinceStart.TotalSeconds + ",heads,Mutual gaze ends");
                }
            }

            _isMutualGazeInLastFrame_Head = IsMutualGaze(Heads[0], Heads[1]);

            LogOtherBehavioursHeads(sw_otherLogForHeads, Heads);
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
        string _otherLogEyes = path + "OtherLogEyes_" + nameBase + ".txt";
        string _otherLogHeads = path + "OtherLogHeads_" + nameBase + ".txt";
        string _audioFile = path + "audio_" + nameBase + ".txt";
        sw_mutualGaze = new StreamWriter(_eyeFilename);
        sw_otherLogForEyes = new StreamWriter(_otherLogEyes);
        sw_otherLogForHeads = new StreamWriter(_otherLogHeads);
        sw_audio = new StreamWriter(_audioFile);

        IsRecording = true;
        //_worldTimer.StartTimer();

        // ===============================Deal with at the begining if there is any mutual gaze=======================
        if (IsMutualGaze(Eyes[0], Eyes[1])) {
            sw_mutualGaze.WriteLine(_worldTimer.ElapsedTimeSinceStart.TotalSeconds + ",eyes,Mutual gaze starts");
            _isMutualGazeInLastFrame = true;
        }
        if (IsAGazingB(Eyes[0], Eyes[1])) {
            sw_otherLogForEyes.WriteLine(_worldTimer.ElapsedTimeSinceStart.TotalSeconds +",eyes," + Eyes[0].name + ",starts gazing at " + Eyes[1].name);
            _wasAGazingBLastFrame_Eyes = true;
        }

        if (IsAGazingB(Eyes[1], Eyes[0])) {
            sw_otherLogForEyes.WriteLine(_worldTimer.ElapsedTimeSinceStart.TotalSeconds + ",eyes," + Eyes[1].name + ",starts gazing at " + Eyes[0].name);
            _wasBGazingALastFrame_Eyes = true;
        }

        if (IsAGazingB(Heads[0], Heads[1])) {
            sw_otherLogForHeads.WriteLine(_worldTimer.ElapsedTimeSinceStart.TotalSeconds + ",eyes," + Heads[0].name + ",starts gazing at " + Heads[1].name);
            _wasAGazingBLastFrame_Heads = true;
        }

        if (IsAGazingB(Heads[1], Heads[0])) {
            sw_otherLogForHeads.WriteLine(_worldTimer.ElapsedTimeSinceStart.TotalSeconds + ",eyes," + Heads[1].name + ",starts gazing at " + Heads[0].name);
            _wasBGazingALastFrame_Heads = true;
        }

        if (GameManager.Instance.IsTalking)
        {
            sw_audio.WriteLine(_worldTimer.ElapsedTimeSinceStart.TotalSeconds + ",start talking");
            _wasTalkingLastFrame = true;
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
        if (sw_otherLogForEyes != null)
        {
            sw_otherLogForEyes.WriteLine(_worldTimer.ElapsedTimeSinceStart.TotalSeconds + ",Stop recording");
            sw_otherLogForEyes.Close();
        }
        if (sw_otherLogForHeads != null) {
            sw_otherLogForHeads.WriteLine(_worldTimer.ElapsedTimeSinceStart.TotalSeconds + ",Stop recording");
            sw_otherLogForHeads.Close();
        }

        if (sw_audio != null)
        {
            sw_audio.Close();
        }

        //GetComponent<WorldTimer>().StopTimer();
        //GetComponent<WorldTimer>().ResetTimer();
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

    void LogOtherBehavioursEyes(StreamWriter sw, GameObject[] eyes)
    {
        if (!_wasAGazingBLastFrame_Eyes && IsAGazingB(eyes[0], eyes[1]))
        {
            if (IsRecording)
            {
                sw.WriteLine(_worldTimer.ElapsedTimeSinceStart.TotalSeconds + "," + eyes[0].name + ",starts gazing at " + eyes[1].name);
                Debug.Log(_worldTimer.ElapsedTimeSinceStart.TotalSeconds + eyes[0].name + ",One-way gaze starts");
            }
        }
        else if (_wasAGazingBLastFrame_Eyes && !IsAGazingB(eyes[0], eyes[1]))
        {
            if (IsRecording)
            {
                sw.WriteLine(_worldTimer.ElapsedTimeSinceStart.TotalSeconds + "," + eyes[0].name + ",One-way gaze ends");
                Debug.Log(_worldTimer.ElapsedTimeSinceStart.TotalSeconds + eyes[0].name + ",One-way gaze ends");
            }
        }
        _wasAGazingBLastFrame_Eyes = IsAGazingB(eyes[0], eyes[1]);

        // Another way around
        if (!_wasBGazingALastFrame_Eyes && IsAGazingB(eyes[1], eyes[0]))
        {
            if (IsRecording)
            {
                sw.WriteLine(_worldTimer.ElapsedTimeSinceStart.TotalSeconds + "," + eyes[1].name + ",starts gazing at " + eyes[0].name);
                Debug.Log(_worldTimer.ElapsedTimeSinceStart.TotalSeconds + eyes[1].name + ",One-way gaze starts");
            }
        }
        else if (_wasBGazingALastFrame_Eyes && !IsAGazingB(eyes[1], eyes[0]))
        {
            if (IsRecording)
            {
                sw.WriteLine(_worldTimer.ElapsedTimeSinceStart.TotalSeconds + "," + eyes[1].name + ",One-way gaze ends");
                Debug.Log(_worldTimer.ElapsedTimeSinceStart.TotalSeconds + eyes[1].name + ",One-way gaze ends");
            }
        }
        _wasBGazingALastFrame_Eyes = IsAGazingB(eyes[1], eyes[0]);
    }

    void LogOtherBehavioursHeads(StreamWriter sw, GameObject[] heads) {
        if (!_wasAGazingBLastFrame_Heads && IsAGazingB(heads[0], heads[1])) {
            if (IsRecording) {
                sw.WriteLine(_worldTimer.ElapsedTimeSinceStart.TotalSeconds + "," + heads[0].name + ",starts gazing at " + heads[1].name);
                Debug.Log(_worldTimer.ElapsedTimeSinceStart.TotalSeconds + heads[0].name + ",One-way gaze starts");
            }
        }
        else if (_wasAGazingBLastFrame_Heads && !IsAGazingB(heads[0], heads[1])) {
            if (IsRecording) {
                sw.WriteLine(_worldTimer.ElapsedTimeSinceStart.TotalSeconds + "," + heads[0].name + ",One-way gaze ends");
                Debug.Log(_worldTimer.ElapsedTimeSinceStart.TotalSeconds + heads[0].name + ",One-way gaze ends");
            }
        }
        _wasAGazingBLastFrame_Heads = IsAGazingB(heads[0], heads[1]);

        // Another way around
        if (!_wasBGazingALastFrame_Heads && IsAGazingB(heads[1], heads[0])) {
            if (IsRecording) {
                sw.WriteLine(_worldTimer.ElapsedTimeSinceStart.TotalSeconds + "," + heads[1].name + ",starts gazing at " + heads[0].name);
                Debug.Log(_worldTimer.ElapsedTimeSinceStart.TotalSeconds + heads[1].name + ",One-way gaze starts");
            }
        }
        else if (_wasBGazingALastFrame_Heads && !IsAGazingB(heads[1], heads[0])) {
            if (IsRecording) {
                sw.WriteLine(_worldTimer.ElapsedTimeSinceStart.TotalSeconds + "," + heads[1].name + ",One-way gaze ends");
                Debug.Log(_worldTimer.ElapsedTimeSinceStart.TotalSeconds + heads[1].name + ",One-way gaze ends");
            }
        }
        _wasBGazingALastFrame_Heads = IsAGazingB(heads[1], heads[0]);
    }
}
