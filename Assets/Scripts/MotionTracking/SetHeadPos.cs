using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetHeadPos : NetworkedPositionSmoothening {
    [HideInInspector] public PhotonView _playerPhotonView;

    private Transform CameraEye;
    //	public Transform remoteCameraEye;
    //	public float lerpRate = 42.0f;

    private Vector3 headToEyeVector;
    public Vector3 origHeadToEyeVector;
    private Vector3 origRotation;

    void Awake()
    {
        _playerPhotonView = transform.parent.GetComponent<PhotonView>();
    }

    void Start()
    {
        if (_playerPhotonView.isMine)
        {
            CameraEye = transform.parent.GetComponent<PlayerManager>().Camera.transform;
            origHeadToEyeVector = -transform.position;
            //origHeadToEyeVector = new Vector3(0, 0.118f, 0.15f);
            headToEyeVector = origHeadToEyeVector;
            origRotation = transform.localEulerAngles;

            //Scale HMD Head to 0 on local Avatars
            transform.localScale = new Vector3(0, 0, 0);
            foreach (Transform t in transform)
            {
                t.gameObject.SetActive(false);
            }
        }

    }

    public override void Update()
    {
        if (_playerPhotonView.isMine)
        {
            //			Vector3 newPos;
            Vector3 rot = CameraEye.localEulerAngles;
            headToEyeVector = Quaternion.Euler(rot.x, rot.y, rot.z) * headToEyeVector;

            newPos = CameraEye.position - headToEyeVector;

            transform.position = Vector3.Lerp(transform.position, newPos, Time.deltaTime * lerpRate);

            transform.localEulerAngles = origRotation;
            transform.Rotate(CameraEye.localEulerAngles);

            headToEyeVector = origHeadToEyeVector;
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, newPos, Time.deltaTime * lerpRate);
            transform.rotation = Quaternion.Lerp(transform.rotation, newRot, Time.deltaTime * lerpRate);
        }
    }
}
