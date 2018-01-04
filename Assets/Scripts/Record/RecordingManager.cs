using System;
using System.Diagnostics;
using System.IO;
using System.Security.Authentication;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

public class RecordingManager : Photon.MonoBehaviour {

    private DataRecording _recordingThread;
    private WorldTimer _worldTimer;

    private GameObject[] _ratingButtons;

    private String _eyeFilename;

    [HideInInspector]
    public bool IsRecording = false;

    [HideInInspector]
    public int RecordingFPS;

    [HideInInspector]
    public int FrameOnAvatarChange;

    // Use this for initialization
    void Start () {

        _worldTimer = GetComponent<WorldTimer>();

        _ratingButtons = GameObject.FindGameObjectsWithTag("Option");
        foreach (GameObject option in _ratingButtons)
        {
            option.GetComponent<Button>().onClick.AddListener(delegate { OnRatingClick(option); });
        }
    }

    // Update is called once per frame
    void Update () {

    }
    // TODO: change RPC call back
    public void StartRecording()
    {
        RPC_StartRecord();
        //photonView.RPC("RPC_StartRecord", PhotonTargets.All);
    }


    public void StopRecording()
    {
        RPC_StopRecord();
//        photonView.RPC("RPC_StopRecord", PhotonTargets.All);
    }


    [PunRPC]
    public void RPC_StartRecord()
    {
        GameObject localPlayer = null;
        localPlayer = GameManager.Instance.localAvatar;
        
        if (localPlayer != null)
        {
            string path = "DataRecording/";
            _eyeFilename = localPlayer.name + "_" + PlayerPrefs.GetString("SkeletonName") + "_fps" + RecordingFPS;

            string nameBase = String.Format("{0}_{1:yyyy-MM-dd_HH-mm-ss}", _eyeFilename, DateTime.Now);
            _eyeFilename = path + "EyeData_" + nameBase + ".txt";

            _recordingThread = localPlayer.GetComponent<EyeController>().NewRecord;
            Debug.Log("thread" + _recordingThread);
            _recordingThread.sw_eye = new StreamWriter(_eyeFilename);

            // New a thread and start this thread
            _recordingThread.Start();

            IsRecording = true;
            GetComponent<WorldTimer>().StartTimer();

        }
    }

    [PunRPC]
    public void RPC_StopRecord()
    {
		if (GameManager.Instance.localAvatar != null)
        {
            IsRecording = false;
            GetComponent<WorldTimer>().StopTimer();
			GetComponent<WorldTimer> ().ResetTimer ();

            if (_recordingThread == null)
                return;
			if (_recordingThread.sw_eye != null) {
				_recordingThread.sw_eye.Close ();
			}

            _recordingThread.Abort();
            _recordingThread.MicroTimer.Enabled = false;
            _recordingThread.MicroTimer.Stop();
            _recordingThread.MicroTimer.Abort();
        }

    }


    void OnApplicationQuit()
    {
        //Stop data recording when Unity quit if it's still writing
        if (IsRecording)
            StopRecording();
	
    }

    //TODO: write in files
    private void OnRatingClick(GameObject clickedButton)
    {
        Debug.Log("click on rating: " + clickedButton.GetComponentInChildren<Text>().text);
        //_recordingThread.sw_eye.WriteLine("Selected option is " + clickedButton.GetComponentInChildren<Text>().text);

        FindObjectOfType<SceneManager_Game>().OnScreenRating[0].transform.parent.parent.gameObject.SetActive(false);
        FindObjectOfType<SceneManager_Game>().InGameRating[0].transform.parent.parent.gameObject.SetActive(false);
    }
}
