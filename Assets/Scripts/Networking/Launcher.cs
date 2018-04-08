using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Com.MTGTech.MyGame{
	
	public class Launcher : Photon.PunBehaviour {
        bool isConnecting;
        public PhotonLogLevel Loglevel = PhotonLogLevel.Informational;
	    public Text DyadTypeText;
		public Text spawnAsText;
		public Text sceneToLoadText;
	    public Toggle isUsingVR;

		/// <summary>
		/// This client's version number. Users are separated from each other by gameversion (which allows you to make breaking changes).
		/// </summary>
		string _gameVersion = "1";

		void Awake()
		{
			PhotonNetwork.autoJoinLobby = false;
			PhotonNetwork.automaticallySyncScene = true;
			PhotonNetwork.logLevel = Loglevel;
		}

		// Use this for initialization
		void Start () {
			//Connect ();
		}	

		public void Connect()
		{
			isConnecting = true;

			// we check if we are connected or not, we join if we are , else we initiate the connection to the server.
			if (PhotonNetwork.connected) {
			    ManageMasterOnJoin();				
			} else {
				PhotonNetwork.ConnectUsingSettings (_gameVersion);
			}
		}

		#region Photon.PunBehavior CallBacks
        // OnConnectedToMaster Server
		public override void OnConnectedToMaster ()
		{
			Debug.Log ("Laucher: OnConnectedToMaster() is called by PUN");
			if (isConnecting) {
                // #Critical we need at this point to attempt joining a Random Room. If it fails, we'll get notified in OnPhotonRandomJoinFailed() and we'll create one.
                ManageMasterOnJoin();			    
			}
		}

	    public override void OnDisconnectedFromPhoton ()
		{
			Debug.LogWarning("Laucher: OnDisconnectedFromMaster() is called by PUN");
		}

		public override void OnJoinedRoom ()
		{
			PhotonNetwork.LoadLevel (sceneToLoadText.text);
		    GameManager.Instance.DyadType = DyadTypeText.text;
            if (PhotonNetwork.isMasterClient) {
                GameManager.Instance.GetComponent<PhotonView>().RPC("RPC_SetVRMode", PhotonTargets.AllBuffered, isUsingVR.isOn);
                if (isUsingVR.isOn) {
                    MicInput.Instance.StopMicrophone();
                } else {
                    MicInput.Instance.MinTalkLoudness = 0.0006f;
                }
                    
            } else {
                if (!isUsingVR.isOn)
                    MicInput.Instance.StopMicrophone();
            }
            
            if (!string.IsNullOrEmpty(skeletonName.text)) {
                PlayerPrefs.SetString("SkeletonName", skeletonName.text);
            }
            if (!string.IsNullOrEmpty(actorHeight.text))
            {
                PlayerPrefs.SetFloat("ActorHeight", float.Parse(actorHeight.text));
            }
        }
        #endregion

	    void ManageMasterOnJoin()
	    {
			if (spawnAsText.text == "Master")
	        {
	            Debug.Log("Creating room studiov");
                PhotonNetwork.CreateRoom("studiov");
	        }
	        else
	        {
				//If joining room fails, a new room will be created if dropdown is set to CameraManager or Master
	            PhotonNetwork.JoinRoom("studiov");
	        }
        }

		[Header("Tab-able Inputfields")]
		public InputField skeletonName;
		public InputField actorHeight;

		void Update(){
			if (spawnAsText.text == "Avatar" && Input.GetKeyDown (KeyCode.Tab)) {
				if (skeletonName.isFocused) {
					EventSystem.current.SetSelectedGameObject (actorHeight.gameObject, null);
					actorHeight.OnPointerClick (new PointerEventData (EventSystem.current));
				}else if (actorHeight.isFocused) {
					EventSystem.current.SetSelectedGameObject (skeletonName.gameObject, null);
					skeletonName.OnPointerClick (new PointerEventData (EventSystem.current));
				}
			}
		}
	}
}
