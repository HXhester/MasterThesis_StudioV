using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HTC.UnityPlugin.StereoRendering;

public class PlayerManager : Photon.PunBehaviour
{
    private SceneManager_Game _sceneManager;
    private Transform expressionController;
    [HideInInspector]
    public GameObject Camera;

    [Header("Local")]
    public OptitrackSkeletonAnimator localOptitrackAnimator; //used in gamemanager to set skeleton name on all clients

    [Header("Remote")]
    public OptitrackSkeletonAnimator remoteOptitrackAnimator; //used in gamemanager to set skeleton name on all clients
    public Transform OptitrackHead;

    private GameObject[] eyes;
    private GameObject[] eyeOrigs;

    void OnEnable() {
        gameObject.name += photonView.viewID.ToString();
        _sceneManager = FindObjectOfType<SceneManager_Game>();
        if (GameObject.FindGameObjectsWithTag("Avatar").Length == 2)
            _sceneManager.Has2Avatars = true;

        expressionController = GameObject.FindGameObjectWithTag("ExpressionController").transform;
        GameManager.VRModeChangeDelegate += DealWithVRmodeChange;

        if (photonView.isMine && !PhotonNetwork.isMasterClient) {
            var cameraRig = GameObject.Find("[CameraRig]").transform;
            try {
                Camera = cameraRig.Find("Camera (head)").Find("Camera (eye)").gameObject;
                //listener = cameraRig.Find("Camera (head)").Find("Camera (ears)").GetComponent<AudioListener>();
            } catch (System.Exception) {
                Camera = cameraRig.Find("Camera (eye)").gameObject;
                //listener = cameraRig.Find("Camera (eye)").Find("Camera (ears)").GetComponent<AudioListener>();
            }
        }

    }

    void OnDisable() {
        _sceneManager.Has2Avatars = false;
        GameManager.VRModeChangeDelegate -= DealWithVRmodeChange;
    }


    void DealWithVRmodeChange() {
        
        ToggleAvatarUI();

        //If the spawned avatar is mine, only deal with avatar client
        if (photonView.isMine && !PhotonNetwork.isMasterClient)
        {
            // if using vr, then only see local avatar
            if (GameManager.Instance.UsingVR)
            {
                Debug.Log("is using vr");
                localOptitrackAnimator.gameObject.SetActive(true);
                remoteOptitrackAnimator.gameObject.SetActive(false);

                Camera.GetComponent<SteamVR_Camera>().enabled = true;
                Camera.GetComponent<Camera>().enabled = true;
                // Don't render the remote layer
                Camera.GetComponent<Camera>().cullingMask &= ~(1 << LayerMask.NameToLayer("Remote"));
            }
            // if not using vr, then see the remote one
            else
            {
                localOptitrackAnimator.gameObject.SetActive(false);
                remoteOptitrackAnimator.gameObject.SetActive(true);

                Camera.GetComponent<SteamVR_Camera>().enabled = false;
                Camera.GetComponent<Camera>().enabled = false;

                // Don't render hmd layer
                var headCam = OptitrackHead.gameObject.AddComponent<Camera>();
                headCam.nearClipPlane = 0.01f;
                headCam.cullingMask &= ~(1 << LayerMask.NameToLayer("HMD"));
            }
            

            localOptitrackAnimator.enabled = false;
            remoteOptitrackAnimator.enabled = false;

            //Not necessary if remoteavatar is already set to layer Remote in the prefab..
            //SetLayerTo(this, "Remote");

            var overviewcam = GameObject.FindGameObjectWithTag("CameraClientPickup");
            if (overviewcam != null)
                overviewcam.GetComponent<Camera>().enabled = false;
        }
        else
        {
            //Turn LocalAvatar off and RemoteAvatar on for all the non-avatar clients
            
            //Find the other avatar clients and tell them to set the newly spawned RemoteAvatar to Remote Layer - this is done because every avatar should see the local avatars in order to have synced positions
            GameObject[] avatars = GameObject.FindGameObjectsWithTag("Avatar");

            bool usingVR = GameManager.Instance.UsingVR;
            foreach (GameObject g in avatars)
            {
                if (g.GetPhotonView().isMine)
                {
					g.GetComponent<PlayerManager>().localOptitrackAnimator.gameObject.SetActive(usingVR);
					g.GetComponent<PlayerManager>().remoteOptitrackAnimator.gameObject.SetActive(!usingVR);
					//if the other avatar is mine then return. if no avatar is mine continue to show only remote/optitrack gameObject.
					return;
                }
            }
            //localOptitrackAnimator.gameObject.SetActive(false);
            //remoteOptitrackAnimator.gameObject.SetActive(true);
        }

        // Master client should have HMD avatar enable to collect correct data
        if (PhotonNetwork.isMasterClient)
        {
            if (GameManager.Instance.UsingVR)
            {
                localOptitrackAnimator.gameObject.SetActive(true);
                remoteOptitrackAnimator.gameObject.SetActive(false);

                eyes = GameObject.FindGameObjectsWithTag("EyeMesh");
                //TODO: only for testing, need to remove
                //eyes = new GameObject[2] { eyes[1], eyes[2] };         
            }
            else
            {
                localOptitrackAnimator.gameObject.SetActive(false);
                remoteOptitrackAnimator.gameObject.SetActive(true);

                GetComponentInChildren<SetHeadPos>().gameObject.SetActive(false);
                GetComponent <FacialController>().enabled = false;
                GetComponent<EyeController>().enabled = false;

                eyes = GameObject.FindGameObjectsWithTag("OptitrackHead");               
            }

            if (eyes.Length == 2) {
                Debug.Log("find eyes");

                eyeOrigs = new GameObject[2];
                for(int i=0; i<eyes.Length;i++) {

                    var eyeOrig = new GameObject("EyeOrig");
                    eyeOrig.transform.SetParent(eyes[i].transform.parent);
                    StartCoroutine(ChangeEyeOrigName(eyeOrig));

                    if (GameManager.Instance.UsingVR) {
                        eyeOrig.transform.localPosition = new Vector3(0, eyes[i].transform.localPosition.y, eyes[i].transform.localPosition.z);
                    } else {
                        eyeOrig.transform.localPosition = eyes[i].transform.localPosition;
                    }
                        
                    eyeOrigs[i] = eyeOrig;

                    var raycaster = eyes[i].AddComponent<EyeRaycaster>();
                    if(raycaster!=null)
                        raycaster.eyeOrig = eyeOrig;
                }

                FindObjectOfType<RecordingManager>().Eyes = eyeOrigs;                
                FindObjectOfType<RecordingManager>().AvatarsReady = true;
                var heads = GameObject.FindGameObjectsWithTag("HMDHead");
                foreach (GameObject head in heads) {
                    head.name = head.transform.parent.name;
                }
                FindObjectOfType<RecordingManager>().Heads = heads;
            }             
        }  
    }

    void ToggleAvatarUI() {
        GameObject[] avatarUi = GameObject.FindGameObjectsWithTag("AvatarUI");
        bool usingvr = GameManager.Instance.UsingVR;
        Debug.Log("is using vr: " + usingvr);
        foreach (GameObject go in avatarUi) {
            go.GetComponent<Canvas>().enabled = usingvr;
        }
    }

    void Update()
    {
        if (!PhotonNetwork.isMasterClient)
            return;

        for (int i = 0; i < eyes.Length; i++)
        {
            eyeOrigs[i].transform.rotation = eyes[i].transform.rotation;
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

        //GameObject.FindGameObjectWithTag("AvatarAndSceneManager")
        //    .GetComponent<AvatarAndSceneManagerScript>()
        //    .spawnedResources.Add(new SpawnedResource(skeletonName, gameObjectName));
    }

    [PunRPC]
    void SetParent(string playerName)
    {

        GameObject player = GameObject.Find(playerName);
        player.transform.parent = expressionController.transform;
    }

    public void SetScale(float scale)
    {
        photonView.RPC("RPC_SetScale", PhotonTargets.AllBuffered, new object[] {gameObject.name, scale});
    }

    [PunRPC]
    public void RPC_SetScale(string playerName, float scale)
    {
        Debug.Log("Setting scale of " + playerName + " on all clients");
        GameObject.Find(playerName).transform.localScale *= scale;
        if(GameManager.Instance.UsingVR)
            GameObject.Find(playerName).GetComponentInChildren<SetHeadPos>().origHeadToEyeVector *= scale;
    }

    IEnumerator ChangeEyeOrigName(GameObject eyeOrig)
    {
        Debug.Log("change eye orig name coroutin starts");
        Transform parent = eyeOrig.transform.parent;
        while (parent.tag != "Avatar")
        {
            parent = parent.parent;
            yield return new WaitForEndOfFrame();
        }

        eyeOrig.name = parent.name;
    }
}
