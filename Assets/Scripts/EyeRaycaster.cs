using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tobii.Research.Unity.CodeExamples;

[DisallowMultipleComponent]
public class EyeRaycaster : MonoBehaviour {

    //public float m_RayLength;
    //public RaycastHit eyeHit;
    public KeyCode ToggleVisualizerKey = KeyCode.F12;
    public GameObject eyeOrig;

    private bool rayToggler;
    

    void Update()
    {
        if(eyeOrig == null)
            return;

        //var ray = new Ray(transform.position, SubscribingToHMDGazeData.SubscribingInstance.GazeDirection);
        //RaycastHit info;
        //Physics.Raycast(ray, out info, m_RayLength);
        //eyeHit = info;
        if (Input.GetKeyDown(ToggleVisualizerKey))
        {
            ToggleRay();
        }

        if (rayToggler)
        {
            Debug.DrawRay(eyeOrig.transform.position, eyeOrig.transform.forward*10f, Color.blue);
        }        
    }

    void ToggleRay()
    {
        rayToggler = !rayToggler;
    }

}
