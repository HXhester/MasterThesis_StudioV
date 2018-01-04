using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleAdjust : MonoBehaviour {
	
	
	public Transform eyeLevel;
	public PlayerManager playerManager;

    private Transform CameraRig;

    private float targetHeight;    			// Height of actor
    private float originalHeight;  			// Original height of avatar
	private float headTipToEye = 0.108f;    // Dimenstion for average head
    private PhotonView _photonView;
    private Transform HMDHead;

	public float manualTargetHeight;

    void Awake()
    {
        originalHeight = eyeLevel.position.y;

        HMDHead = transform.parent.Find("mixamorig:Head").transform;

        if (_photonView == null)
        {
            _photonView = playerManager.GetComponent<PhotonView>();
        }       
       
    }

    void Start()
    {
        CameraRig = transform.parent.Find("[CameraRig]").transform;
        targetHeight = PlayerPrefs.GetFloat("ActorHeight") - headTipToEye;

        if (_photonView.isMine) {

            // Set both HMDBody and OptitrackBody size locally		
            transform.localScale *= targetHeight / originalHeight;

            // Set HMDHead size locally
            HMDHead.localScale = Vector3.zero;

            // Set both HMDBody and OptitrackBody size remotely
            playerManager.SetScale (transform.localScale);
            //		GetComponent<PhotonView> ().RPC ("RPC_SetScale", PhotonTargets.All, new object[]{playerManager.gameObject.name, transform.localScale});
        }
        else
        {
            //Set HMDhead size remotely
            HMDHead.localScale *= targetHeight / originalHeight;
        }
    }

	public void SetHeight(){
		transform.localScale = Vector3.one;
	    CameraRig.localScale = Vector3.one;
        manualTargetHeight = manualTargetHeight - headTipToEye;

	    float scale = manualTargetHeight / originalHeight;

        // Set body size locally	
        transform.localScale *= scale;

        // Set Vive area locally
        CameraRig.localScale *= scale;

        // Scale HeadToEyeVector on HMDHead locally
	    transform.parent.Find("mixamorig: Head").GetComponent<SetHeadPos>().origHeadToEyeVector *= scale;

        // Scale HeadToEyeVector on HMDHead remotely
        // TODO...


        playerManager.SetScale(transform.localScale);
        //		GetComponent<PhotonView> ().RPC ("RPC_SetScale", PhotonTargets.All, new object[]{playerManager.gameObject.name, transform.localScale});
    }

    // Set Optitrack head scale
    public void SetHeadScale(float scale)
    {
        Vector3 s = new Vector3(scale, scale, scale);
		playerManager.OptitrackHead.localScale = s;
		playerManager.SetHeadScale(s);
    }
}
