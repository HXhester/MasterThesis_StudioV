//======================================================================================================
// Copyright 2016, NaturalPoint Inc.
//======================================================================================================

using System;
using UnityEngine;


public class OptitrackRigidBody : MonoBehaviour
{
    public OptitrackStreamingClient StreamingClient;
    public Int32 RigidBodyId;


    void Start()
    {
        // If the user didn't explicitly associate a client, find a suitable default.
        if ( this.StreamingClient == null )
        {
            this.StreamingClient = OptitrackStreamingClient.FindDefaultClient();

            // If we still couldn't find one, disable this component.
            if ( this.StreamingClient == null )
            {
                Debug.LogError( GetType().FullName + ": Streaming client not set, and no " + typeof( OptitrackStreamingClient ).FullName + " components found in scene; disabling this component.", this );
                this.enabled = false;
                return;
            }
            
            this.StreamingClient.TriggerUpdateDefinitions();
            
        }
    }

	public float lerpSpeed = 5f;

#if UNITY_2017_1_OR_NEWER
    void OnEnable() {
        Application.onBeforeRender += OnBeforeRender;
    }


    void OnDisable() {
        Application.onBeforeRender -= OnBeforeRender;
    }


    void OnBeforeRender() {
        UpdatePose();
    }
#endif


    void Update()
    {
        UpdatePose();
    }

    void UpdatePose() {
        OptitrackRigidBodyState rbState = StreamingClient.GetLatestRigidBodyState(RigidBodyId);
        if (rbState != null) {
            this.transform.localPosition = Vector3.Lerp(transform.localPosition, rbState.Pose.Position.V3, Time.deltaTime * lerpSpeed);
            //			this.transform.localPosition = rbState.Pose.Position.V3;
            this.transform.localRotation = Quaternion.Lerp(transform.rotation, rbState.Pose.Orientation.Q, Time.deltaTime * lerpSpeed);
            //           this.transform.localRotation = rbState.Pose.Orientation.Q;
        }
    }
}
