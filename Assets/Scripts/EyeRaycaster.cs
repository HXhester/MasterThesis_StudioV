using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tobii.VR;

public class EyeRaycaster : MonoBehaviour {

	public float m_RayLength;
	public RaycastHit eyeHit;

	protected void OnEnable(){
		TobiiVR_Host.Instance.ValidTrackerData += OnValidData;
	}

	protected void OnDisable(){
		TobiiVR_Host.Instance.ValidTrackerData -= OnValidData;
	}

	protected void OnValidData(object sender, EventArgs e)
	{
		var ray = new Ray(transform.position, TobiiVR_Host.Instance.GazeDirection);

		RaycastHit info;
		Physics.Raycast (ray, out info, m_RayLength);
		eyeHit = info;
	}
}
