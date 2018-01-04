using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HTC.UnityPlugin.StereoRendering;

public class PlayerManager : Photon.PunBehaviour
{

    private Transform expressionController;

    public GameObject _camera;

    [Header("Local")]
    public OptitrackSkeletonAnimator localOptitrackAnimator; //used in gamemanager to set skeleton name on all clients

    public ScaleAdjust localScaleAdjust;
    public AudioListener listener;

    [Header("Remote")]
    public OptitrackSkeletonAnimator remoteOptitrackAnimator; //used in gamemanager to set skeleton name on all clients

    public ScaleAdjust remoteScaleAdjust;
    public Transform OptitrackHead;

    void Awake()
    {

        gameObject.name += photonView.viewID.ToString();
        expressionController = GameObject.FindGameObjectWithTag("ExpressionController").transform;
        //If the spawned avatar is mine
        if (photonView.isMine)
        {
			localOptitrackAnimator.gameObject.SetActive(true);
			remoteOptitrackAnimator.gameObject.SetActive(false);
            _camera.GetComponent<SteamVR_Camera>().enabled = true;
            _camera.GetComponent<Camera>().enabled = true;
            _camera.GetComponent<Camera>().cullingMask &= ~(1 << LayerMask.NameToLayer("Remote"));
            listener.enabled = true;
            localScaleAdjust.enabled = true;

            localOptitrackAnimator.enabled = false;
            remoteOptitrackAnimator.enabled = false;

            //Not necessary if remoteavatar is already set to layer Remote in the prefab..
            SetLayerTo(this, "Remote");
        }
        else
        {
            //Turn LocalAvatar off and RemoteAvatar on for all the non-avatar clients
            
            //Find the other avatar clients and tell them to set the newly spawned RemoteAvatar to Remote Layer - this is done because every avatar should see the local avatars in order to have synced positions
            GameObject[] avatars = GameObject.FindGameObjectsWithTag("Avatar");
            foreach (GameObject g in avatars)
            {
                if (g.GetPhotonView().isMine)
                {
//                    g.GetComponent<PlayerManager>().SetLayerTo(this, "Remote");
					g.GetComponent<PlayerManager>().localOptitrackAnimator.gameObject.SetActive(true);
					g.GetComponent<PlayerManager>().remoteOptitrackAnimator.gameObject.SetActive(false);
					//if the other avatar is mine then return. if no avatar is mine continue to show only remote/optitrack gameObject.
					return;
                }
            }
			localOptitrackAnimator.gameObject.SetActive(false);
			remoteOptitrackAnimator.gameObject.SetActive(true);
        }
    }

    //This method is only called by Avatars
    public void SetLayerTo(PlayerManager playerManager, string layer)
    {
        playerManager.localOptitrackAnimator.gameObject.SetActive(true);
        playerManager.remoteOptitrackAnimator.gameObject.SetActive(false);
        foreach (Transform t in playerManager.remoteOptitrackAnimator.transform)
        {
            if (t.gameObject.activeInHierarchy)
            {
                t.gameObject.layer = LayerMask.NameToLayer(layer);
            }
        }
    }

    public void SetSkeletonName()
    {
        if (photonView.isMine)
        {
            photonView.RPC("RPC_SetSkeletonName", PhotonTargets.AllBuffered, new object[]
            {
                gameObject.name,
                PlayerPrefs.GetString("SkeletonName")
            });
        }
    }

    public void SetSkeletonName(string skeletonName)
    {
        if (photonView.isMine)
        {
            photonView.RPC("RPC_SetSkeletonName", PhotonTargets.AllBuffered, new object[]
            {
                gameObject.name,
                skeletonName
            });
        }
    }

    [PunRPC]
    public void RPC_SetSkeletonName(string gameObjectName, string skeletonName)
    {
        PlayerManager pm = GameObject.Find(gameObjectName).GetComponent<PlayerManager>();
        pm.localOptitrackAnimator.SkeletonAssetName = skeletonName;
        pm.remoteOptitrackAnimator.SkeletonAssetName = skeletonName;
        //Enable the components in case setting the skeletonName was too late
        pm.localOptitrackAnimator.enabled = true;
        pm.remoteOptitrackAnimator.enabled = true;

        GameObject.FindGameObjectWithTag("AvatarAndSceneManager")
            .GetComponent<AvatarAndSceneManagerScript>()
            .spawnedResources.Add(new SpawnedResource(skeletonName, gameObjectName));
    }

    [PunRPC]
    void SetParent(string playerName)
    {

        GameObject player = GameObject.Find(playerName);
        player.transform.parent = expressionController.transform;
    }

    public void SetScale(Vector3 localScale)
    {
        photonView.RPC("RPC_SetScale", PhotonTargets.OthersBuffered, new object[] {gameObject.name, localScale});
    }

    [PunRPC]
    public void RPC_SetScale(string playerName, Vector3 localScale)
    {
        Debug.Log("Setting scale of " + playerName + " on all clients");
        //Debug.Log (GameObject.FindGameObjectWithTag ("ExpressionController").transform.Find(playerName));
        foreach (Transform t in GameObject.FindGameObjectWithTag("ExpressionController").transform)
        {
            Debug.Log(t.name);
        }
        GameObject.Find(playerName).GetComponent<PlayerManager>().remoteScaleAdjust.transform.localScale = localScale;
        GameObject.Find(playerName).GetComponent<PlayerManager>().localScaleAdjust.transform.localScale = localScale;
    }

    public void SetHeadScale(Vector3 localScale)
    {
        photonView.RPC("RPC_SetHeadScale", PhotonTargets.OthersBuffered, new object[] {gameObject.name, localScale});
    }

    [PunRPC]
    public void RPC_SetHeadScale(string playerName, Vector3 localScale)
    {
        GameObject.Find(playerName).GetComponent<PlayerManager>().OptitrackHead.localScale = localScale;
    }

}
