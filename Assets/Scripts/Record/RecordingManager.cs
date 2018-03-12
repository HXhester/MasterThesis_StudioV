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
    private StreamWriter sw_eye;
    public GameObject[]  Eyes;            // Left eye gameobjects on avatars    
    private WorldTimer _worldTimer;
    private bool _isMutualGazeInLastFrame;
    public bool AvatarsReady;

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
            if (!_isMutualGazeInLastFrame && IsMutualGaze(Eyes[0], Eyes[1]))
            {
                if (IsRecording)
                {
                    sw_eye.WriteLine("Mutual gaze starts," + _worldTimer.ElapsedTimeSinceStart.TotalSeconds);
                    Debug.Log("Mutual gaze starts," + _worldTimer.ElapsedTimeSinceStart.TotalSeconds);
                    //Debug.Log("Mutual gaze starts," + Time.time);
                }
            }
            else if(_isMutualGazeInLastFrame && !IsMutualGaze(Eyes[0], Eyes[1]))
            {
                if (IsRecording)
                {
                    sw_eye.WriteLine("Mutual gaze ends," + _worldTimer.ElapsedTimeSinceStart.TotalSeconds);
                    Debug.Log("Mutual gaze ends," + _worldTimer.ElapsedTimeSinceStart.TotalSeconds);
                    //Debug.Log("Mutual gaze ends," + Time.time);
                }
            }

            _isMutualGazeInLastFrame = IsMutualGaze(Eyes[0], Eyes[1]);
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
        sw_eye = new StreamWriter(_eyeFilename);

        IsRecording = true;
        _worldTimer.StartTimer();
        sw_eye.WriteLine("Start recording," + _worldTimer.ElapsedTimeSinceStart);
    }


    public void StopRecording()
    {     
        IsRecording = false;
        
        if (sw_eye != null)
        {
            sw_eye.WriteLine("Stop recording," + _worldTimer.ElapsedTimeSinceStart);
            sw_eye.Close();
        }

        GetComponent<WorldTimer>().StopTimer();
        GetComponent<WorldTimer>().ResetTimer();
    }

    bool IsMutualGaze(GameObject eye1, GameObject eye2)
    {
        // if angle between vector eye.forward and eye-eye vector < 9°, then is gazing
        Vector3 gazeDirection1;
        Vector3 gazeDirection2;
        Vector3 eye1ToEye2Dir;

        gazeDirection1 = eye1.transform.forward;
        gazeDirection2 = eye2.transform.forward;
        eye1ToEye2Dir = eye2.transform.position - eye1.transform.position;
        float angle1 = Vector3.Angle(gazeDirection1, eye1ToEye2Dir);
        float angle2 = Vector3.Angle(gazeDirection2, -eye1ToEye2Dir);
       
        return (Mathf.Abs(angle1) < 9f) && (Mathf.Abs(angle2) < 9f);
    }
}
