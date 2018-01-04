using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkedCameraPositionSmoothening : Photon.MonoBehaviour {
	PhotonView cameraEyePhotonView;
	// Use this for initialization
	void Start () {
		if (gameObject.tag == "CameraClient")
			return;
		
		cameraEyePhotonView = transform.parent.parent.GetComponent<PhotonView>();

		if (cameraEyePhotonView == null)
		{
			cameraEyePhotonView = transform.parent.parent.parent.GetComponent<PhotonView>();
		}
	}
	
	// Update is called once per frame
	void Update () {
		//Photon smoothening
	    if (gameObject.tag == "CameraClient")
	    {
	        if (!photonView.isMine)
	        {
	            transform.position = Vector3.Lerp(transform.position, newPos, Time.deltaTime * lerpRate);
	            transform.rotation = Quaternion.Lerp(transform.rotation, newRot, Time.deltaTime * lerpRate);
            }
        }
	    else
	    {
            if (!cameraEyePhotonView.isMine)
	        {
	            transform.position = Vector3.Lerp(transform.position, newPos, Time.deltaTime * lerpRate);
	            transform.rotation = Quaternion.Lerp(transform.rotation, newRot, Time.deltaTime * lerpRate);
	        }
        }
	    
	}
	Vector3 newPos;
	Quaternion newRot;
	public float lerpRate = 15f;

	void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.isWriting )
		{
			stream.SendNext(transform.position);
			stream.SendNext(transform.rotation);
		}
		else
		{
			newPos = (Vector3)stream.ReceiveNext();
			newRot = (Quaternion)stream.ReceiveNext();
		}
	}
}
