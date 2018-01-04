// Copyright © 2015 – Property of Tobii AB (publ) - All Rights Reserved

namespace Tobii.VR
{
    using UnityEngine;
    using System;

    [RequireComponent(typeof(LineRenderer))]
    public class TobiiVR_GazeVisualizer : MonoBehaviour {

        public KeyCode ToggleOnOffKey = KeyCode.F12;

        private float _rayLength = 25f;
        private LineRenderer _lr;

        void Start ()
        {
            _lr = GetComponent<LineRenderer>();

            TobiiVR_Host.Instance.Init();
            _lr.enabled = false;
        }

        void Update()
        {
            if (Input.GetKeyUp(ToggleOnOffKey))
            {
                ToggleGazeVisulization();
            }
        }

        public void ToggleGazeVisulization()
        {
            _lr.enabled = !_lr.enabled;

            if (_lr.enabled)
            {
                TobiiVR_Host.Instance.ValidTrackerData += OnValidData;
            }
            else
            {
                TobiiVR_Host.Instance.ValidTrackerData -= OnValidData;
            }
        }

        private void OnValidData(object sender, EventArgs e)
        {
            var ray = new Ray(transform.position, TobiiVR_Host.Instance.GazeDirection);

            RaycastHit info;
            if (Physics.Raycast(ray, out info, _rayLength))
            {
                _lr.SetPosition(1, transform.InverseTransformPoint(info.point));
            }
            else
            {
                _lr.SetPosition(1, transform.localPosition + TobiiVR_Host.Instance.LocalGazeDirection * _rayLength);
            }
        }
    }
}