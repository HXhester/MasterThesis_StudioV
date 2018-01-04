using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Tobii.VR;

namespace Com.MTGTech.MyGame{

	[RequireComponent(typeof(Dropdown))]
	public class PhotonRoleDropdown : MonoBehaviour {

		#region Private Variables
		// Store the PlayerPref key to avoid typos
		Dropdown _dropdown;
		public Dropdown sceneDropdown;

		#endregion

		public GameObject avatarName;
		public GameObject skeletonName;
		public GameObject actorHeight;

		private GameObject HMDPlacementCanvas;

		#region MonoBehavior CallBacks
		// Use this for initialization
		void Start () {
			_dropdown = GetComponent<Dropdown> ();
			PhotonNetwork.playerName = _dropdown.options [_dropdown.value].text;

			HMDPlacementCanvas = GameObject.Find ("Placement_Canvas");
			if (HMDPlacementCanvas != null) {
				HMDPlacementCanvas.SetActive (false);
			}
		
		}

		public void OnDropdownChange(){
			
			if (_dropdown.options [_dropdown.value].text == "Avatar") {
				avatarName.SetActive (true);
				actorHeight.SetActive (true);
				skeletonName.SetActive (true);
				sceneDropdown.gameObject.SetActive (false);

				HMDPlacementCanvas.SetActive (true);
			} else if (_dropdown.options [_dropdown.value].text == "Master" || _dropdown.options [_dropdown.value].text == "CameraManager") {
				avatarName.SetActive (false);
				actorHeight.SetActive (false);
				skeletonName.SetActive (false);
				sceneDropdown.gameObject.SetActive (true);
			} else {
				avatarName.SetActive (false);
				actorHeight.SetActive (false);
				skeletonName.SetActive (false);
				sceneDropdown.gameObject.SetActive (false);
			}

		}
		#endregion
	}
}