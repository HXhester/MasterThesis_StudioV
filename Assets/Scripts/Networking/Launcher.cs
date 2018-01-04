using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Com.MTGTech.MyGame{
	
	public class Launcher : Photon.PunBehaviour {
		#region Public Variables



		/// <summary>
		/// The PUN loglevel. 
		/// </summary>
		public PhotonLogLevel Loglevel = PhotonLogLevel.Informational;
		[Tooltip("The Ui Panel to let the user enter name, connect and play")]
		public GameObject controlPanel;

		[Tooltip("The Ui Text to inform the user about the connection progress")]
		public GameObject progressLable;
		#endregion

//		private string sceneToLaunch;

		#region Private Variables
		/// <summary>
		/// Keep track of the current process. Since connection is asynchronous and is based on several callbacks from Photon, 
		/// we need to keep track of this to properly adjust the behavior when we receive call back by Photon.
		/// Typically this is used for the OnConnectedToMaster() callback.
		/// </summary>
		bool isConnecting;
		public Text spawnAsText;
		public Text sceneToLoadText;

		/// <summary>
		/// This client's version number. Users are separated from each other by gameversion (which allows you to make breaking changes).
		/// </summary>
		string _gameVersion = "1";


		#endregion


		#region MonoBehaviour CallBacks
		/// <summary>
		/// MonoBehaviour method called on GameObject by Unity during early initialization phase.
		/// </summary>
		void Awake()
		{

			// #Critical
			// we don't join the lobby. There is no need to join a lobby to get the list of rooms.
			PhotonNetwork.autoJoinLobby = false;


			// #Critical
			// this makes sure we can use PhotonNetwork.LoadLevel() on the master client and all clients in the same room sync their level automatically
			PhotonNetwork.automaticallySyncScene = true;

			// #NotImportant
			// Force LogLevel
			PhotonNetwork.logLevel = Loglevel;
		}

		// Use this for initialization
		void Start () {
			//Connect ();

			//dropdownText = controlPanel.transform.Find ("Dropdown").GetComponent<Dropdown> ().captionText;
			//nameText = controlPanel.transform.transform.Find ("Name InputField").Find ("Text").GetComponent<Text> ();
			controlPanel.SetActive(true);
			progressLable.SetActive (false);
		}
		#endregion


		#region Public Methods		
		/// <summary>
		/// Start the connection process. 
		/// - If already connected, we attempt joining a random room
		/// - if not yet connected, Connect this application instance to Photon Cloud Network
		/// </summary>
		public void Connect()
		{
			isConnecting = true;


			// we check if we are connected or not, we join if we are , else we initiate the connection to the server.
			if (PhotonNetwork.connected) {
			    ManageMasterOnJoin();
                
				
			} else {

				// #Critical: we failed to join a random room, maybe none exists or they are all full. No worries, we create a new room.

				PhotonNetwork.ConnectUsingSettings (_gameVersion);
			}
		}
		#endregion


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
		public override void OnPhotonJoinRoomFailed (object[] codeAndMsg)
		{
			Debug.Log ("Launcher:OnPhotonJoinRoomFailed() was called by PUN. Room studiov is not available, so we create one.");

			// Only master and CameraManger can create a room
			string dropdownString = spawnAsText.text;

			if (dropdownString == "Master" || dropdownString == "CameraManager") {
                Debug.Log("Creating room studiov");
				PhotonNetwork.CreateRoom ("studiov");
				controlPanel.SetActive(false);
				progressLable.SetActive (true);

			} else {
				Debug.LogWarning ("You must be a Master or CameraManager to host a game.");
			}
		}


		public override void OnJoinedRoom ()
		{
			PhotonNetwork.LoadLevel (sceneToLoadText.text);

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
//		public InputField avatarName;
		public InputField skeletonName;
		public InputField actorHeight;

		void Update(){
			if (spawnAsText.text == "Avatar" && Input.GetKeyDown (KeyCode.Tab)) {
//				if (avatarName.isFocused) {
//					EventSystem.current.SetSelectedGameObject (skeletonName.gameObject, null);
//					skeletonName.OnPointerClick (new PointerEventData (EventSystem.current));
//				}
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
