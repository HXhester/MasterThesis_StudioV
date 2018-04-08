using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR;

public class GameManager : Photon.PunBehaviour
{
    public static GameManager Instance;
    public delegate void OnVRModeChangeDelegate();
    public static OnVRModeChangeDelegate VRModeChangeDelegate;

    [HideInInspector]
    public GameObject localAvatar;
    public string DyadType;
    public bool UsingVR;
    public bool IsTalking;

    private void OnEnable() {
        MicInput.StartTalkingDelegate += startTalking;
        MicInput.EndTalkingDelegate += endTalking;
    }
    private void OnDisable() {
        MicInput.StartTalkingDelegate -= startTalking;
        MicInput.EndTalkingDelegate -= endTalking;
    }

    private void Awake()
    {
        //Check if instance already exists
        if (Instance == null)
            Instance = this;

        //If instance already exists and it's not this:
        else if (Instance != this)
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);

    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene(0);
    }

    public void leaveRoom() {
        PhotonNetwork.LeaveRoom();
    }

	void Start(){
		DontDestroyOnLoad (gameObject);    
	}
	void Update()
	{
        // TODO: subscribe delegate and Send rpc to sync game manager
	    IsTalking = MicInput.Instance.isTalking;
	}

    public override void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
    {
        if (PhotonNetwork.isMasterClient)
        {
            Debug.Log("is master client, a player is connected");
        }
    }

    public override void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer)
    {
        if (PhotonNetwork.isMasterClient)
        {
			Debug.Log("a player disconnected");
        }

		GameObject obj = GameObject.Find ("OptiTrack Skeleton - " + otherPlayer.NickName);
		if (obj != null) {
			Destroy (obj);
		}
       
    }
	public override void OnJoinedRoom (){
		SpawnResource (PhotonNetwork.playerName);

        var avatars = GameObject.FindGameObjectsWithTag("Avatar");
        foreach(GameObject avatar in avatars)
        {
            if (avatar.GetPhotonView().isMine)
            {
                localAvatar = avatar;
            }
        }        
    }
	public void SpawnResource(string resourceName){
		Debug.Log ("Launcher: OnJoinedRoom() called by PUN. Now this client is in a room.");
		//Maybe remove this if statment since OnJoinedRoom doesnt ever seem to be running on masterclient
		if (!PhotonNetwork.isMasterClient) 
		{
			GameObject player = PhotonNetwork.Instantiate (resourceName, new Vector3 (0, 0, 0), Quaternion.identity, 0);

			// Handle camera when the scene is in Main
			if (SceneManager.GetActiveScene ().name == "Main") {
				Camera.main.gameObject.SetActive (false);
			}

			if (player.tag == "Avatar") {
				SetParent (player);
				if (player.GetPhotonView ().isMine) {
					var listener = player.GetComponent<PlayerManager>().Camera.GetComponent<AudioListener> ();
					if (listener != null) {
						listener.enabled = true;
					}
					
					player.GetComponent<PlayerManager> ().SetSkeletonName ();
					player.GetComponent<PlayerManager> ().localOptitrackAnimator.enabled = true;
					player.GetComponent<PlayerManager> ().remoteOptitrackAnimator.enabled = true;
				}
			}
		} 
	}

    // This is only used for replacing avatar
	public void SpawnResource(string resourceName, string skeletonName){
		Debug.Log ("Launcher: OnJoinedRoom() called by PUN. Now this client is in a room.");
		//Maybe remove this if statment since OnJoinedRoom doesnt ever seem to be running on masterclient

		GameObject player = PhotonNetwork.Instantiate (resourceName, new Vector3 (0, 0, 0), Quaternion.identity, 0);

		// Handle camera when the scene is in Main
		if (SceneManager.GetActiveScene ().name == "Main") {
			Camera.main.gameObject.SetActive (false);
		}

		if (player.tag == "Avatar") {
			SetParent (player);
			if (player.GetPhotonView ().isMine) {
				var listener = player.GetComponent<PlayerManager>().Camera.GetComponent<AudioListener> ();
				if (listener != null) {
					listener.enabled = true;
				}
				GameObject[] avatarUi = GameObject.FindGameObjectsWithTag ("AvatarUI");
                bool usingvr = UsingVR;
				foreach (GameObject go in avatarUi) {
					go.GetComponent<Canvas> ().enabled = UsingVR;
				}
				player.GetComponent<PlayerManager> ().SetSkeletonName (skeletonName);
			}
		}

	}
	void SetParent(GameObject player){
		PhotonView pv = player.GetPhotonView ();
		pv.RPC ("SetParent", PhotonTargets.AllBuffered, player.name);
	}

    [PunRPC]
    void RPC_SetVRMode(bool isUsingVR) {
        GameManager.Instance.UsingVR = isUsingVR;
        Debug.Log("vr mode is: " + isUsingVR);
        XRSettings.enabled = isUsingVR;

        if (VRModeChangeDelegate != null)
            VRModeChangeDelegate();
    }

    void startTalking() {
        if (!PhotonNetwork.inRoom)
            return;
        var name = localAvatar.name;
        photonView.RPC("RPC_WriteTalkingStatus", PhotonTargets.MasterClient, name, true);
    }
    void endTalking() {
        if (!PhotonNetwork.inRoom)
            return;
        var name = localAvatar.name;
        photonView.RPC("RPC_WriteTalkingStatus", PhotonTargets.MasterClient, name, false);
    }

    [PunRPC]
    void RPC_WriteTalkingStatus(string playerName, bool istalking) {
        var recordingManager = FindObjectOfType<RecordingManager>();

        if (!recordingManager.IsRecording)
            return;

        if (istalking) {
            recordingManager.writeStartTalking(playerName);
        } else {
            recordingManager.writeEndTalking(playerName);
        }
    }
}
