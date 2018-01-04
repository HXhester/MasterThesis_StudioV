using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class AvatarAndSceneManagerScript : Photon.MonoBehaviour {

	public List<SpawnedResource> spawnedResources;
	public delegate void SceneAsyncDone();
	public static event SceneAsyncDone OnSceneAsyncDone;

	private GameManager gameManager;
	void Awake(){
		gameManager = GetComponent<GameManager> ();
	}

	[Header("Menu")]
	public GameObject menu;
	public Dropdown avatarDropdown;
	GameObject[] avatars;
	void Update(){
		if (Input.GetKeyDown (KeyCode.Escape)) {
			menu.SetActive (!menu.activeInHierarchy);
			RefreshAvatarList ();
			Cursor.visible = menu.activeInHierarchy;
		}
	}
	void RefreshAvatarList(){
		avatars = GameObject.FindGameObjectsWithTag ("Avatar");
		avatarDropdown.options.Clear ();
		foreach (GameObject avatar in avatars) {
			avatarDropdown.options.Add (new Dropdown.OptionData (){ text = avatar.name });
		}
		avatarDropdown.RefreshShownValue ();
	}
	public void ReplaceAvatar(Text newAvatar){
		GameObject oldAvatar = Array.Find (avatars, g => g.name == avatarDropdown.options [avatarDropdown.value].text);
		string skeletonName = oldAvatar.GetComponent<PlayerManager> ().localOptitrackAnimator.SkeletonAssetName;
		photonView.RPC ("RPC_ReplaceAvatar", PhotonTargets.AllBuffered, new object[]{ oldAvatar.name, newAvatar.text, skeletonName });
		Debug.Log ("Replacing avatar");

	}
	[PunRPC]
	void RPC_ReplaceAvatar(string oldAvatar, string newAvatar, string skeletonName){
		GameObject oldGO = GameObject.Find (oldAvatar);
		if (oldGO.GetPhotonView ().isMine) {
			try{
				gameManager.SpawnResource (newAvatar, skeletonName);
			}catch(System.Exception exc){
				Debug.LogWarning ("Cant spawn new avatar due to issues with optitrack");
			}
		}
		oldGO.SetActive (false);
		PhotonNetwork.Destroy (oldGO);
		if (oldGO != null) {
			Destroy (oldGO);
		}
	}
	public void LoadScene(Text sceneName){
		photonView.RPC ("RPC_LoadScene", PhotonTargets.AllBuffered, new object[]{ sceneName.text });
	}
	[PunRPC]
	void RPC_LoadScene(string sceneName){
		if (SceneManager.GetSceneByName (sceneName) == SceneManager.GetActiveScene () || SceneManager.GetActiveScene().name == "Launcher") {
			return;
		}
		StartCoroutine (CheckLoadSceneAsync (sceneName));
		//PhotonNetwork.LoadLevel	(sceneName);	
	}
	IEnumerator CheckLoadSceneAsync(string sceneName){
		yield return SceneManager.LoadSceneAsync(sceneName);
		if (OnSceneAsyncDone != null) {
			//OnSceneAsyncDone ();
		}
	}
}
[System.Serializable]
public class SpawnedResource{
	public string motiveName;
	public string gameObjectName;
	public SpawnedResource(string motiveName, string gameObjectName){
		this.motiveName = motiveName;
		this.gameObjectName = gameObjectName;
	}
}	