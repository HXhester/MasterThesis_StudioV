﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tobii.Research.Unity.CodeExamples;

public class EyeRaycaster : MonoBehaviour {

    //public float m_RayLength;
    //public RaycastHit eyeHit;
    public KeyCode ToggleVisualizerKey = KeyCode.F12;

    private bool rayToggler;

    void Update()
    {
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
            Debug.DrawRay(transform.position, SubscribingToHMDGazeData.SubscribingInstance.GazeDirection, Color.blue);
        }        
    }

    void ToggleRay()
    {
        rayToggler = !rayToggler;
    }

}