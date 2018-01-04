using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Com.MTGTech.MyGame{

	[RequireComponent(typeof(InputField))]
	public class PlayerNameInputField : MonoBehaviour {

		#region Private Variables
		// Store the PlayerPref key to avoid typos
		static string playerNamePrefKey = "PlayerName";

		#endregion

		#region MonoBehavior CallBacks
		// Use this for initialization
		void Start () {
			string defaultName = "";
			InputField _inputField = this.GetComponent<InputField> ();
			if (_inputField != null){
				defaultName = PlayerPrefs.GetString(playerNamePrefKey);
			}

			PhotonNetwork.playerName = defaultName;
		}
		#endregion

		#region Public Methods
		public void SetPlayerName(string value){
			if (GetComponent<InputField> ().text != null) {
				PhotonNetwork.playerName = value + "";
				PlayerPrefs.SetString (playerNamePrefKey, value);
			}
		}
		#endregion
	}
}
