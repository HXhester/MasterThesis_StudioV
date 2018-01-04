// Copyright © 2015 – Property of Tobii AB (publ) - All Rights Reserved

namespace Tobii.VR
{
    using System;
    using UnityEngine;

    public static class TobiiVR_Logging
    {
        public static void Log(string msg)
        {
            Debug.Log(msg + " Time since startup " + Time.realtimeSinceStartup);
        }
        public static void LogWarning(string msg)
        {
            Debug.LogWarning(msg + " Time since startup " + Time.realtimeSinceStartup);
        }

        public static void LogError(string msg)
        {
            Debug.LogError(msg + " Time since startup " + Time.realtimeSinceStartup);
        }

        public static void LogException(Exception e)
        {
            Debug.LogException(e);
        }
    }
}