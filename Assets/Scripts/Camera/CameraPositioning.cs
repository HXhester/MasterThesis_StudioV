using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPositioning : MonoBehaviour {
	public Transform positionTo;
	// Use this for initialization
	void Start () {
		transform.position = positionTo.position;
		transform.rotation = positionTo.rotation;
		transform.SetParent (positionTo);
	}
}
