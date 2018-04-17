using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Tobii.Research.Unity.CodeExamples;
using Debug = UnityEngine.Debug;

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
    private StreamWriter sw_distance;
    
    private WorldTimer _worldTimer;
    private bool _wasAGazingBLastFrame_Eyes;
    private bool _wasAGazingBLastFrame_Heads;
    private bool _wasAGazingBLastFrame_EyeHitMesh;
    private bool _wasTalkingLastFrame;
    private bool _wasBlinkingLastFrame;

    [HideInInspector]
    public bool IsRecording = false;

    // Use this for initialization
    void Start () {
        _worldTimer = GetComponent<WorldTimer>();       
    }

    // Update is called once per frame
    void Update () {
        if (!PhotonNetwork.isMasterClient && IsRecording)
        {
            if (GameManager.Instance.UsingVR)
            {
                LogOtherBehavioursEyes(sw_individualEyeGaze, GameManager.Instance.localEye,
                    GameManager.Instance.remoteEye);
                LogOtherBehavioursHeads(sw_individualHeadGaze, GameManager.Instance.localHead, GameManager.Instance.remoteHead);
            }
            else
            {
                LogOtherBehavioursHeads(sw_individualHeadGaze, GameManager.Instance.localHead, GameManager.Instance.remoteHead);

            }

            sw_audio.WriteLine(_worldTimer.ElapsedTimeSinceStart.TotalSeconds + "," + MicInput.Instance.MicLoudness);

            var distVector = GameManager.Instance.remoteHead.transform.position-GameManager.Instance.localHead.transform.position;
            sw_distance.WriteLine(_worldTimer.ElapsedTimeSinceStart.TotalSeconds + "," + distVector.x + "," +
                                  distVector.y + "," + distVector.z);
        }
    }

    void FixedUpdate()
    {
        // Use raycast hit for checking if there is any eye mesh hit
        if (!PhotonNetwork.isMasterClient && IsRecording)
        {
            if (GameManager.Instance.UsingVR)
                LogEyeGazeHitMesh(sw_individualEyeGaze_HitMesh,GameManager.Instance.localEye);
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
        if (PhotonNetwork.isMasterClient)
            return;

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
        string _distanceFile = path + "Distance_" + nameBase + ".txt";
       
        if (GameManager.Instance.UsingVR)
        {
            sw_individualEyeGaze = new StreamWriter(_individualEyes);
            sw_individualEyeGaze_HitMesh = new StreamWriter(_individualEyesHitMesh);
        }
        sw_individualHeadGaze = new StreamWriter(_individualHeads);
        sw_audio = new StreamWriter(_audioFile);
        sw_distance = new StreamWriter(_distanceFile);

        IsRecording = true;

        // ===============================Deal with at the begining of recording=======================
        if (IsAGazingB(GameManager.Instance.localHead, GameManager.Instance.remoteHead)) {
            sw_individualHeadGaze.WriteLine(_worldTimer.ElapsedTimeSinceStart.TotalSeconds + ",starts head gazing");
            _wasAGazingBLastFrame_Heads = true;
        }
        

        if(!GameManager.Instance.UsingVR)
            return;

        if (IsAGazingB(GameManager.Instance.localEye, GameManager.Instance.remoteEye)) {
            sw_individualEyeGaze.WriteLine(_worldTimer.ElapsedTimeSinceStart.TotalSeconds + ",starts eye gazing");
            _wasAGazingBLastFrame_Eyes = true;
        }

        if (IsAGazingB_hitmesh(GameManager.Instance.localEye.transform))
        {
            sw_individualEyeGaze_HitMesh.WriteLine(_worldTimer.ElapsedTimeSinceStart.TotalSeconds + ",starts gazing on eye mesh");
            _wasAGazingBLastFrame_EyeHitMesh = true;
        }
    }

    public void StopRecording()
    {
        if (PhotonNetwork.isMasterClient)
            return;

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

        if (sw_individualEyeGaze_HitMesh != null) {
            sw_individualEyeGaze_HitMesh.WriteLine(_worldTimer.ElapsedTimeSinceStart.TotalSeconds + ",Stop recording");
            sw_individualEyeGaze_HitMesh.Close();
        }

        if (sw_audio != null)
        {
            sw_audio.WriteLine(_worldTimer.ElapsedTimeSinceStart.TotalSeconds + ",Stop recording");
            sw_audio.Close();
        }

        if (sw_distance != null)
        {
            sw_distance.WriteLine(_worldTimer.ElapsedTimeSinceStart.TotalSeconds + ",Stop recording");
            sw_distance.Close();
        }
    }

    void LogOtherBehavioursEyes(StreamWriter sw, GameObject localEye, GameObject remoteEye)
    {
        if (!_wasAGazingBLastFrame_Eyes && IsAGazingB(localEye, remoteEye))
        {
            
            sw.WriteLine(_worldTimer.ElapsedTimeSinceStart.TotalSeconds + ",starts eye gazing");
            Debug.Log(_worldTimer.ElapsedTimeSinceStart.TotalSeconds + ",starts eye gazing");


        }
        else if (_wasAGazingBLastFrame_Eyes && !IsAGazingB(localEye, remoteEye))
        {
            
            sw.WriteLine(_worldTimer.ElapsedTimeSinceStart.TotalSeconds + ",eye gaze ends");
            Debug.Log(_worldTimer.ElapsedTimeSinceStart.TotalSeconds + ",eye gaze ends");
            
        }
        _wasAGazingBLastFrame_Eyes = IsAGazingB(localEye, remoteEye);
        

        if (!_wasBlinkingLastFrame && SubscribingToHMDGazeData.SubscribingInstance.LeftEyeOpenness == 0) {
            sw.WriteLine(_worldTimer.ElapsedTimeSinceStart.TotalSeconds + ",blink start");
        } else if (_wasBlinkingLastFrame && SubscribingToHMDGazeData.SubscribingInstance.LeftEyeOpenness != 0) {
            sw.WriteLine(_worldTimer.ElapsedTimeSinceStart.TotalSeconds + ",blink end");
        }
        _wasBlinkingLastFrame = (SubscribingToHMDGazeData.SubscribingInstance.LeftEyeOpenness == 0);
    }

    void LogOtherBehavioursHeads(StreamWriter sw, GameObject localHead, GameObject remoteHead) {
        if (!_wasAGazingBLastFrame_Heads && IsAGazingB(localHead, remoteHead)) {
            
            sw.WriteLine(_worldTimer.ElapsedTimeSinceStart.TotalSeconds + ",starts head gazing");
            Debug.Log(_worldTimer.ElapsedTimeSinceStart.TotalSeconds + ",starts head gazing");
            
        }
        else if (_wasAGazingBLastFrame_Heads && !IsAGazingB(localHead, remoteHead)) {
            
            sw.WriteLine(_worldTimer.ElapsedTimeSinceStart.TotalSeconds + ",head gaze ends");
            Debug.Log(_worldTimer.ElapsedTimeSinceStart.TotalSeconds + ",head gaze ends");
            
        }
        _wasAGazingBLastFrame_Heads = IsAGazingB(localHead, remoteHead);
    }

    void LogEyeGazeHitMesh(StreamWriter sw, GameObject localEye)
    {
        if (!_wasAGazingBLastFrame_EyeHitMesh && IsAGazingB_hitmesh(localEye.transform))
        {

            sw.WriteLine(_worldTimer.ElapsedTimeSinceStart.TotalSeconds + ",starts gazing on eye mesh");
            Debug.Log(_worldTimer.ElapsedTimeSinceStart.TotalSeconds + ",starts gazing on eye mesh");

        }
        else if (_wasAGazingBLastFrame_EyeHitMesh && !IsAGazingB_hitmesh(localEye.transform))
        {

            sw.WriteLine(_worldTimer.ElapsedTimeSinceStart.TotalSeconds + ",ends gazing on eye mesh");
            Debug.Log(_worldTimer.ElapsedTimeSinceStart.TotalSeconds + ",ends gazing on eye mesh");

        }
        _wasAGazingBLastFrame_EyeHitMesh = IsAGazingB_hitmesh(localEye.transform);
    }

    bool IsMutualGaze(GameObject eye1, GameObject eye2)
    {
        // if angle between vector eye.forward and eye-eye vector < 9°, then is gazing       

        return IsAGazingB(eye1, eye2) && IsAGazingB(eye2, eye1);
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

    bool IsAGazingB_hitmesh(Transform eye)
    {
        RaycastHit hit;

        if (Physics.Raycast(eye.position, eye.forward, out hit))
        {
            if (hit.transform.name == "EyeBox")
            {
                Debug.Log("hit on eye!");
                return true;
            }
                
            return false;
        }
        return false;
    }
}
