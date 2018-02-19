using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Com.MTGTech.MyGame{

	[RequireComponent(typeof(Dropdown))]
	public class PhotonRoleDropdown : MonoBehaviour {

		Dropdown _dropdown;
		public Dropdown sceneDropdown;

		public GameObject avatarName;
		public GameObject skeletonName;
		public GameObject actorHeight;
	    public GameObject dyadType;
	    public GameObject VRTogge;

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
                dyadType.SetActive(false);
                VRTogge.SetActive(false);

                if (HMDPlacementCanvas != null)
                    HMDPlacementCanvas.SetActive (true);
			} else if (_dropdown.options [_dropdown.value].text == "Master") {
				avatarName.SetActive (false);
				actorHeight.SetActive (false);
				skeletonName.SetActive (false);
				sceneDropdown.gameObject.SetActive (true);
                dyadType.SetActive(true);
                VRTogge.SetActive(true);
            } else {
				avatarName.SetActive (false);
				actorHeight.SetActive (false);
				skeletonName.SetActive (false);
				sceneDropdown.gameObject.SetActive (false);
                dyadType.SetActive(false);
                VRTogge.SetActive(false);
            }

		}
		#endregion
	}
}