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
    private GameObject[]  _eyes;            // Left eye gameobjects on avatars    
    private WorldTimer _worldTimer;
    private bool _isMutualGazeInLastFrame;
    private bool _2Avatars;

    [HideInInspector]
    public bool IsRecording = false;

    // Use this for initialization
    void Start () {
        _worldTimer = GetComponent<WorldTimer>();
        if (PhotonNetwork.isMasterClient)
        {
            _eyes = new GameObject[2];
        }       
    }

    // Update is called once per frame
    void Update () {
        if (PhotonNetwork.isMasterClient && _2Avatars)
        {
            if (!_isMutualGazeInLastFrame && IsMutualGaze(_eyes[0], _eyes[1]))
            {
                if (IsRecording)
                {
                    sw_eye.WriteLine("Mutual gaze starts," + _worldTimer.ElapsedTimeSinceStart.TotalSeconds);
                    Debug.Log("Mutual gaze starts," + _worldTimer.ElapsedTimeSinceStart.TotalSeconds);
                }
            }
            else if(_isMutualGazeInLastFrame && !IsMutualGaze(_eyes[0], _eyes[1]))
            {
                if (IsRecording)
                {
                    sw_eye.WriteLine("Mutual gaze ends," + _worldTimer.ElapsedTimeSinceStart.TotalSeconds);
                }
            }

            _isMutualGazeInLastFrame = IsMutualGaze(_eyes[0], _eyes[1]);
        }
    }

    void OnApplicationQuit()
    {
        //Stop data recording when Unity quit if it's still writing
        if (IsRecording)
            StopRecording();    
    }

    public override void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
    {
        if (PhotonNetwork.isMasterClient)
        {
            Debug.Log("is master client, a player is connected");
            var avatars = GameObject.FindGameObjectsWithTag("Avatar");
            if (avatars.Length == 2)
            {
                _2Avatars = true;
                if (GameManager.Instance.UsingVR)
                {
                    _eyes = GameObject.FindGameObjectsWithTag("EyeMesh");                   
                }
                else
                {
                    _eyes = GameObject.FindGameObjectsWithTag("OptitrackHead");
                }

                foreach (GameObject eye in _eyes) {
                    eye.AddComponent<EyeRaycaster>();
                }
            }           
        }
    }

    public override void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer)
    {
        _2Avatars = false;
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
        GetComponent<WorldTimer>().StartTimer();
    }


    public void StopRecording()
    {     
        IsRecording = false;
        GetComponent<WorldTimer>().StopTimer();
        GetComponent<WorldTimer>().ResetTimer();

        if (sw_eye != null)
        {
            sw_eye.Close();
        }       
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
