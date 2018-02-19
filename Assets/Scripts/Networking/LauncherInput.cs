using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LauncherInput : MonoBehaviour {

	public Dropdown resourceDropdown;

	public void SetResourceName(){
		PhotonNetwork.playerName = resourceDropdown.options [resourceDropdown.value].text;
		Debug.Log ("playerName: " + PhotonNetwork.playerName);
	}

	public void SetAvatarName(Text value){
		PhotonNetwork.playerName = value.text;
	}
	public void SetSkeletonName(Text value){
		PlayerPrefs.SetString ("SkeletonName", value.text);
	}
	public void SetActorHeight(Text value){
		if (!string.IsNullOrEmpty (value.text)) {
			PlayerPrefs.SetFloat ("ActorHeight", float.Parse (value.text));
		} else {
			PlayerPrefs.SetFloat ("ActorHeight", 1.6f);
		}
	}
}
