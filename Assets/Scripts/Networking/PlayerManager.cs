﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HTC.UnityPlugin.StereoRendering;

public class PlayerManager : Photon.PunBehaviour
{
    private SceneManager_Game _sceneManager;
    private Transform expressionController;
    private GameObject GazeIndicator;
    [HideInInspector]
    public GameObject Camera;

    [Header("Local")]
    public OptitrackSkeletonAnimator localOptitrackAnimator; //used in gamemanager to set skeleton name on all clients

    [Header("Remote")]
    public OptitrackSkeletonAnimator remoteOptitrackAnimator; //used in gamemanager to set skeleton name on all clients
    public Transform OptitrackHead;
    public Transform eye;

    void OnEnable() {
        gameObject.name += photonView.viewID.ToString();
        _sceneManager = FindObjectOfType<SceneManager_Game>();
        if (GameObject.FindGameObjectsWithTag("Avatar").Length == 2)
        {
            _sceneManager.Has2Avatars = true;
            if (!PhotonNetwork.isMasterClient)
            {
                if (GameManager.Instance.UsingVR)
                {
                    var avatars = GameObject.FindGameObjectsWithTag("Avatar");
                    foreach (GameObject avatar in avatars)
                    {
                        if (!avatar.GetComponent<PhotonView>().isMine)
                        {
                            GameManager.Instance.remoteEye = avatar.GetComponent<PlayerManager>().eye.gameObject;
                            GameManager.Instance.remoteHead = avatar.GetComponent<PlayerManager>().transform.GetComponentInChildren<SetHeadPos>().gameObject;
                        }
                    }
                }
                else
                {
                    var avatars = GameObject.FindGameObjectsWithTag("Avatar");
                    foreach (GameObject avatar in avatars)
                    {
                        if (!avatar.GetComponent<PhotonView>().isMine)
                        {
                            GameManager.Instance.remoteHead = avatar.GetComponent<PlayerManager>().OptitrackHead.gameObject;
                        }
                    }
                }
            }       
        }
        

        expressionController = GameObject.FindGameObjectWithTag("ExpressionController").transform;
        GameManager.VRModeChangeDelegate += DealWithVRmodeChange;

        // Master client
        if (PhotonNetwork.isMasterClient)
        {
            if (GameManager.Instance.UsingVR)
            {
                localOptitrackAnimator.gameObject.SetActive(true);
                remoteOptitrackAnimator.gameObject.SetActive(false);
            }
            else
            {
                localOptitrackAnimator.gameObject.SetActive(false);
                remoteOptitrackAnimator.gameObject.SetActive(true);
            }
        }

        // For local avatar client
        if (photonView.isMine && !PhotonNetwork.isMasterClient) {
            var cameraRig = GameObject.Find("[CameraRig]").transform;
            try {
                Camera = cameraRig.Find("Camera (head)").Find("Camera (eye)").gameObject;
            } catch (System.Exception) {
                Camera = cameraRig.Find("Camera (eye)").gameObject;
            }

            // Disable hitmesh box on local avatar
            GetComponentInChildren<SetHeadPos>().transform.Find("EyeBox").gameObject.SetActive(false);
        }
    }

    void OnDisable() {
        _sceneManager.Has2Avatars = false;
        GameManager.VRModeChangeDelegate -= DealWithVRmodeChange;
    }

    void Start()
    {
        if (photonView.isMine)
            GazeIndicator = GameObject.Instantiate(Resources.Load("GazeIndicator", typeof(GameObject))) as GameObject;
    }

    void Update()
    {
        if (!photonView.isMine)
            return;

        var relativePos = GameManager.Instance.remoteEye.transform.position -
                          GameManager.Instance.localEye.transform.position;
        var dist = relativePos.magnitude;
        var pointInWorld = GameManager.Instance.localEye.transform.position +
                           dist * GameManager.Instance.localEye.transform.forward.normalized;
        var radius = dist* Mathf.Tan(9f * Mathf.PI / 180f);

        GazeIndicator.transform.localScale = Vector3.one*radius;
        GazeIndicator.transform.position = pointInWorld;
        Quaternion rotation = Quaternion.LookRotation(-GameManager.Instance.localEye.transform.forward);
        GazeIndicator.transform.rotation = rotation;
    }

    void DealWithVRmodeChange() {
        
        ToggleAvatarUI();

        //If the spawned avatar is mine, only deal with avatar client
        if (PhotonNetwork.isMasterClient)
            return;

        // if using vr, then only see local avatar
        if (GameManager.Instance.UsingVR)
        {           
            localOptitrackAnimator.gameObject.SetActive(true);
            remoteOptitrackAnimator.gameObject.SetActive(false);

            Camera.GetComponent<SteamVR_Camera>().enabled = true;
            Camera.GetComponent<Camera>().enabled = true;
            // Don't render the remote layer
            Camera.GetComponent<Camera>().cullingMask &= ~(1 << LayerMask.NameToLayer("Remote"));
            if (photonView.isMine)
            {

                // TODO: tests if skeleton animator works or not
                //localOptitrackAnimator.enabled = false;
                //remoteOptitrackAnimator.enabled = false;
                
                //should be camera(eye)
                GameManager.Instance.localEye = Camera;
                GameManager.Instance.localHead = GetComponentInChildren<SetHeadPos>().gameObject;

                var eyeOrig = new GameObject("EyeOrig");
                eyeOrig.transform.SetParent(Camera.transform);
                eyeOrig.name = GameManager.Instance.localAvatar.name;
                eyeOrig.transform.localPosition = Vector3.zero;
                var raycaster = eyeOrig.AddComponent<EyeRaycaster>();
                if (raycaster != null)
                {
                    raycaster.eyeOrig = eyeOrig;
                }
                

                var overviewcam = GameObject.FindGameObjectWithTag("CameraClientPickup");
                if (overviewcam != null)
                    overviewcam.GetComponent<Camera>().enabled = false;
            }

        }
        // if not using vr, then see the remote one
        else
        {
            localOptitrackAnimator.gameObject.SetActive(false);
            remoteOptitrackAnimator.gameObject.SetActive(true);

            Camera.GetComponent<SteamVR_Camera>().enabled = false;
            Camera.GetComponent<Camera>().enabled = false;

            GetComponentInChildren<SetHeadPos>().gameObject.SetActive(false);
            GetComponent<FacialController>().enabled = false;
            GetComponent<EyeController>().enabled = false;

            if (photonView.isMine)
            {
                // Don't render hmd layer
                var headCam = new GameObject("HeadCam");
                var headcamComponent = headCam.AddComponent<Camera>();
                var eyeOnHeadPos = OptitrackHead.Find("Eye_L").transform.localPosition;
                headCam.transform.parent = OptitrackHead;
                headCam.transform.localPosition = new Vector3(0, eyeOnHeadPos.y, eyeOnHeadPos.z);

                headcamComponent.nearClipPlane = 0.01f;
                headcamComponent.cullingMask &= ~(1 << LayerMask.NameToLayer("HMD"));

                var eyeOrig = new GameObject("EyeOrig");
                eyeOrig.transform.SetParent(OptitrackHead.transform.parent);
                eyeOrig.name = GameManager.Instance.localAvatar.name;
                eyeOrig.transform.localPosition = OptitrackHead.transform.localPosition;
                var raycaster = eyeOrig.AddComponent<EyeRaycaster>();
                if (raycaster != null)
                {
                    raycaster.eyeOrig = eyeOrig;
                }

                GameManager.Instance.localHead = OptitrackHead.gameObject;                
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
        expressionController = FindObjectOfType<FaceExpressionController>().transform;
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

}
