using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsObjectInstantiator : MonoBehaviour {


	[SerializeField]float firePower;
	[SerializeField]GameObject prefab;
	[SerializeField]float fireRate; 
	// Use this for initialization
	void Start () {
		StartAutoFire ();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	void StartAutoFire(){
		if (prefab != null) {
			InvokeRepeating ("FireObject", 0, fireRate);
		} else {
			Debug.LogError ("no projectile added to object instantiator");
		}
	}

	void FireObject(){
		GameObject go = Instantiate (prefab, transform.position, transform.rotation, transform) as GameObject;
		if (go.GetComponent<Rigidbody> ()) {
			Rigidbody rb = go.GetComponent<Rigidbody> ();
			rb.AddForce (rb.transform.forward * firePower);
		} else {
			Debug.LogError (go.name + " dose not have a Rigidbody");
		}
	}
}
