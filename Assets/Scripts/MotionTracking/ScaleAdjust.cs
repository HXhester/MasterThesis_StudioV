using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleAdjust : MonoBehaviour {
	
	
	public Transform eyeLevel;
	public PlayerManager playerManager;

    //private Transform CameraRig;

    private float targetHeight;    			// Height of actor
    private float originalHeight;  			// Original height of avatar
	private float headTipToEye = 0.108f;    // Dimenstion for average head
    private PhotonView _photonView;
    private Transform HMDHead;

	public float manualTargetHeight;

    void Awake()
    {
        var headEnd = transform.Find("HeadEnd");
        originalHeight = GameManager.Instance.UsingVR ? eyeLevel.position.y : headEnd.position.y;


        HMDHead = transform.Find("mixamorig:Head").transform;

        if (_photonView == null)
        {
            _photonView = playerManager.GetComponent<PhotonView>();
        }       
       
    }

    void Start()
    {
        //CameraRig = GameObject.Find("[CameraRig]").transform;
        if (GameManager.Instance.UsingVR)
        {
            targetHeight = PlayerPrefs.GetFloat("ActorHeight") - headTipToEye;
        }
        else
        {
            targetHeight = PlayerPrefs.GetFloat("ActorHeight");
        }
        float scale = targetHeight / originalHeight;

        if (_photonView.isMine) {

            // Set both HMDBody and OptitrackBody size locally and remotely
            playerManager.SetScale (scale);
                
        }
    }

	public void SetHeight(){
		transform.localScale = Vector3.one;
	    //CameraRig.localScale = Vector3.one;
        manualTargetHeight = manualTargetHeight - headTipToEye;

	    float scale = manualTargetHeight / originalHeight;

        playerManager.SetScale(scale);
        //		GetComponent<PhotonView> ().RPC ("RPC_SetScale", PhotonTargets.All, new object[]{playerManager.gameObject.name, transform.localScale});
    }

}
