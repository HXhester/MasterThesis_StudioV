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
/// TODO: manage different file on different client
public class RecordingManager : Photon.PunBehaviour {

    private string _eyeFilename;

    // File record on Host
    //private StreamWriter sw_mutualEyeGaze;              // Only eye mutual gaze, in vr
    //private StreamWriter sw_mutualEyeGaze_HitMesh;    // Together with sw_mutualGaze, record on server, in vr
    //private StreamWriter sw_mutualHeadGaze;           // Only head gaze, both in vr and real

    // File record on Client
    private StreamWriter sw_individualEyeGaze;          // VR, cone
    private StreamWriter sw_individualEyeGaze_HitMesh;  // VR, hit mesh
    private StreamWriter sw_individualHeadGaze;         // Both VR and real, cone
    public StreamWriter sw_audio;                       // Record the loudness each frame; May be do it manually
    
    private WorldTimer _worldTimer;
    private bool _isMutualGazeInLastFrame;
    private bool _isMutualGazeInLastFrame_Head;
    private bool _wasAGazingBLastFrame_Eyes;
    private bool _wasAGazingBLastFrame_Heads;
    private bool _wasTalkingLastFrame;

    [HideInInspector]
    public bool IsRecording = false;

    // Use this for initialization
    void Start () {
        _worldTimer = GetComponent<WorldTimer>();       
    }

    // Update is called once per frame
    void Update () {
        if (!PhotonNetwork.isMasterClient && photonView.isMine && IsRecording)
        {
            // TODO: Log eye gaze behavior using hitmesh method         
            // ==========================Recording for one-way gaze=====================================
            LogOtherBehavioursEyes(sw_individualEyeGaze, GameManager.Instance.localEye, GameManager.Instance.remoteEye);

            // ==========================Recording for head in vr mode==================================
            if (GameManager.Instance.UsingVR)
            {
                LogOtherBehavioursHeads(sw_individualHeadGaze, GameManager.Instance.localHead, GameManager.Instance.remoteHead);
            }
            
            sw_audio.WriteLine(_worldTimer.ElapsedTimeSinceStart.TotalSeconds + "," + MicInput.Instance.MicLoudness);           
        }
    }

    void OnApplicationQuit()
    {
        //Stop data recording when Unity quit if it's still writing
        if (IsRecording)
            StopRecording();    
    }

    public void StartRecording()
    {
        string dyadType = GameManager.Instance.DyadType;
        string distance = GameObject.FindObjectOfType<SceneManager_Game>().CurrentDistance.ToString();

        string path = "DataRecording/";
        Directory.CreateDirectory("DataRecording");

        _eyeFilename = GameManager.Instance.localAvatar.name + (GameManager.Instance.UsingVR ? "VR_" : "Real_");
        _eyeFilename += dyadType + "_" + distance;
        string nameBase = String.Format("{0}_{1:yyyy-MM-dd_HH-mm-ss}", _eyeFilename, DateTime.Now);

        string _individualEyes = path + "IndividualEyes_" + nameBase + ".txt";
        string _individualEyesHitMesh = path + "IndividualEyes(hitMesh)_" + nameBase + ".txt";
        string _individualHeads = path + "IndividualHeads_" + nameBase + ".txt";
        string _audioFile = path + "Audio_" + nameBase + ".txt";
       
        if (GameManager.Instance.UsingVR)
        {
            sw_individualEyeGaze = new StreamWriter(_individualEyes);
            sw_individualEyeGaze_HitMesh = new StreamWriter(_individualEyesHitMesh);
        }
        sw_individualHeadGaze = new StreamWriter(_individualHeads);
        sw_audio = new StreamWriter(_audioFile);

        IsRecording = true;

        // ===============================Deal with at the begining of recording=======================
        if (IsAGazingB(GameManager.Instance.localEye, GameManager.Instance.remoteEye)) {
            sw_individualEyeGaze.WriteLine(_worldTimer.ElapsedTimeSinceStart.TotalSeconds + ",starts gazing");
            _wasAGazingBLastFrame_Eyes = true;
        }

        if(!GameManager.Instance.UsingVR)
            return;
        
        if (IsAGazingB(GameManager.Instance.localHead, GameManager.Instance.remoteHead)) {
            sw_individualHeadGaze.WriteLine(_worldTimer.ElapsedTimeSinceStart.TotalSeconds + ",starts gazing");
            _wasAGazingBLastFrame_Heads = true;
        }
    }

    public void StopRecording()
    {     
        IsRecording = false;
        
        if (sw_individualEyeGaze != null)
        {
            sw_individualEyeGaze.WriteLine(_worldTimer.ElapsedTimeSinceStart.TotalSeconds + ",Stop recording");
            sw_individualEyeGaze.Close();
        }
        if (sw_individualHeadGaze != null) {
            sw_individualHeadGaze.WriteLine(_worldTimer.ElapsedTimeSinceStart.TotalSeconds + ",Stop recording");
            sw_individualHeadGaze.Close();
        }

        if (sw_audio != null)
        {
            sw_audio.Close();
        }
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

    void LogOtherBehavioursEyes(StreamWriter sw, GameObject localEye, GameObject remoteEye)
    {
        if (!_wasAGazingBLastFrame_Eyes && IsAGazingB(localEye, remoteEye))
        {
            
            sw.WriteLine(_worldTimer.ElapsedTimeSinceStart.TotalSeconds + ",starts gazing");
            Debug.Log(_worldTimer.ElapsedTimeSinceStart.TotalSeconds +",One-way gaze starts");
            
        }
        else if (_wasAGazingBLastFrame_Eyes && !IsAGazingB(localEye, remoteEye))
        {
            
            sw.WriteLine(_worldTimer.ElapsedTimeSinceStart.TotalSeconds + ",One-way gaze ends");
            Debug.Log(_worldTimer.ElapsedTimeSinceStart.TotalSeconds + ",One-way gaze ends");
            
        }
        _wasAGazingBLastFrame_Eyes = IsAGazingB(localEye, remoteEye);
    }

    void LogOtherBehavioursHeads(StreamWriter sw, GameObject localHead, GameObject remoteHead) {
        if (!_wasAGazingBLastFrame_Heads && IsAGazingB(localHead, remoteHead)) {
            
            sw.WriteLine(_worldTimer.ElapsedTimeSinceStart.TotalSeconds + ",starts gazing");
            Debug.Log(_worldTimer.ElapsedTimeSinceStart.TotalSeconds + ",One-way gaze starts");
            
        }
        else if (_wasAGazingBLastFrame_Heads && !IsAGazingB(localHead, remoteHead)) {
            
            sw.WriteLine(_worldTimer.ElapsedTimeSinceStart.TotalSeconds + ",One-way gaze ends");
            Debug.Log(_worldTimer.ElapsedTimeSinceStart.TotalSeconds + ",One-way gaze ends");
            
        }
        _wasAGazingBLastFrame_Heads = IsAGazingB(localHead, remoteHead);
    }

    public void writeStartTalking(string playername) {
        sw_audio.WriteLine(_worldTimer.ElapsedTimeSinceStart.TotalSeconds +","+ playername + ",start talking");
    }

    public void writeEndTalking(string playername) {
        sw_audio.WriteLine(_worldTimer.ElapsedTimeSinceStart.TotalSeconds +","+ playername +  ",end talking");
    }

}
