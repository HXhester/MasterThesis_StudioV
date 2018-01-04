// Copyright © 2015 – Property of Tobii AB (publ) - All Rights Reserved

namespace Tobii.VR
{
    using System;
    using System.Collections;
    using Tobii.StreamEngine;
    using UnityEngine;
    using Vector3 = UnityEngine.Vector3;

    public class IntroAnimationArgs : EventArgs
    {
        public int CalibrationIndex { get; private set; }

        public IntroAnimationArgs(int calibrationIndex)
        {
            CalibrationIndex = calibrationIndex;
        }
    }

    public class CalibrationCompletedArgs : EventArgs
    {
        public CalibrationResults Result { get; private set; }

        public CalibrationCompletedArgs(CalibrationResults result)
        {
            Result = result;
        }
    }
    public enum CalibrationResults
    {
        Successfully,
        Failed
    };

    public class TobiiVR_CalibrationViewModel : MonoBehaviour
    {
        private TobiiVR_CalibrationHelper _calibrationHelper;
        private bool _hasAnimated;

        public event EventHandler<IntroAnimationArgs> WaitingOnIntroAnimation = delegate { };
        public event EventHandler WaitingOnOutroAnimation = delegate { };
        public event EventHandler<CalibrationCompletedArgs> OnCalibrationCompleted = delegate { };

        public bool HasCompletedAnimation { set { _hasAnimated = value; } }

        public bool TryLaunchCalibration(Vector3[] calibrationPoints)
        {           
            _calibrationHelper = TobiiVR_Host.Instance.CreateCalibrationHelper();
            if (_calibrationHelper == null)
            {
                return false;
            }

            StartCoroutine(CalibrationLoop(calibrationPoints));
            return true;
        }

        private IEnumerator CalibrationLoop(Vector3[] calibrationPoints)
        {
            TobiiVR_Logging.Log("Starting the calibration loop.");

            // Get and set the lens cup separation
            float hmdIpdInMeter;
            if (TobiiVR_Util.TryGetHmdLensCupSeparationInMeter(out hmdIpdInMeter) == false)
            {
                TobiiVR_Logging.LogWarning("Failed to get lens cup separation from HMD. Setting default lens cup separation.");
                hmdIpdInMeter = 0.0635f;
            }

            var result = TobiiVR_Util.SetLensCupSeparation(hmdIpdInMeter, TobiiVR_Host.Instance.DeviceContext);
            if (result != tobii_error_t.TOBII_ERROR_NO_ERROR)
            {
                TobiiVR_Logging.LogWarning("Failed to set lens configuration. " + result);
            }
            else
            {
                TobiiVR_Logging.Log("Successfully set lens configuration: " + hmdIpdInMeter +"m");
            }

            // Start calibration
            _calibrationHelper.StartCalibration();
            TobiiVR_CalibrationHelper.Command reply = null;

            while (reply == null)
            {
                yield return null;
                reply = _calibrationHelper.NextResult;
            }

            if (reply.Result != tobii_error_t.TOBII_ERROR_NO_ERROR)
            {
                TobiiVR_Logging.LogError(reply.GetCommandResultAndTimeString());
                yield return StartCoroutine(NotifyAndStopCalibration(new CalibrationCompletedArgs(CalibrationResults.Failed)));
                yield break;
            }
            TobiiVR_Logging.Log(reply.GetCommandResultAndTimeString());
            reply = null;

            // Clear calibration
            _calibrationHelper.ClearCalibration();

            while (reply == null)
            {
                yield return null;
                reply = _calibrationHelper.NextResult;
            }

            if (reply.Result != tobii_error_t.TOBII_ERROR_NO_ERROR)
            {
                TobiiVR_Logging.LogError(reply.GetCommandResultAndTimeString());
                yield return StartCoroutine(NotifyAndStopCalibration(new CalibrationCompletedArgs(CalibrationResults.Failed)));
                yield break;
            }
            TobiiVR_Logging.Log(reply.GetCommandResultAndTimeString());
            reply = null;

            var index = 0;
            var remainingPoints = true;
            while (remainingPoints)
            {
                // Play intro animation for the point
                _hasAnimated = false;
                WaitingOnIntroAnimation.Invoke(this, new IntroAnimationArgs(index));
                yield return StartCoroutine(WaitingForAnimation());

                TobiiVR_Logging.Log("Finished waiting for INTRO animation.");

                var point = calibrationPoints[index];

                // Transform the calibration point in x to the trackers coordinate system
                point.x *= -1;

                // Calibrating the point
                _calibrationHelper.CollectCalibrationDataForPoint(point * 1000.0f); // Needs to be specified in mm

                while (reply == null)
                {
                    yield return null;
                    reply = _calibrationHelper.NextResult;
                }

                if (reply.Result != tobii_error_t.TOBII_ERROR_NO_ERROR)
                {
                    TobiiVR_Logging.LogError(reply.GetCommandResultAndTimeString());
                    yield return StartCoroutine(NotifyAndStopCalibration(new CalibrationCompletedArgs(CalibrationResults.Failed)));
                    yield break;
                }
                TobiiVR_Logging.Log(reply.GetCommandResultAndTimeString());
                reply = null;

                index++;

                // Outro animation for the point
                _hasAnimated = false;
                WaitingOnOutroAnimation.Invoke(this, null);
                yield return StartCoroutine(WaitingForAnimation());

                TobiiVR_Logging.Log("Finished waiting for OUTRO animation.");

                // Check if there are more points remaining
                if (index > calibrationPoints.Length - 1)
                {
                    remainingPoints = false;
                }
            }

            // Compute and apply calibration
            _calibrationHelper.ComputeAndApplyCalibration();

            while (reply == null)
            {
                yield return null;
                reply = _calibrationHelper.NextResult;
            }

            if (reply.Result != tobii_error_t.TOBII_ERROR_NO_ERROR)
            {
                TobiiVR_Logging.LogError(reply.GetCommandResultAndTimeString());
                yield return StartCoroutine(NotifyAndStopCalibration(new CalibrationCompletedArgs(CalibrationResults.Failed)));
                yield break;
            }
            TobiiVR_Logging.Log(reply.GetCommandResultAndTimeString());
            reply = null;

            // Stop calibration
            yield return StartCoroutine(NotifyAndStopCalibration(new CalibrationCompletedArgs(CalibrationResults.Successfully)));

            TobiiVR_Logging.Log("Finished the calibration loop.");
        }

        private IEnumerator WaitingForAnimation()
        {
            while (!_hasAnimated)
            {
                yield return null;
            }
        }

        private IEnumerator NotifyAndStopCalibration(CalibrationCompletedArgs args)
        {
            TobiiVR_CalibrationHelper.Command result = null;
            _calibrationHelper.StopCalibration();

            while (result == null)
            {
                yield return null;
                result = _calibrationHelper.NextResult;
            }

            if (result.Result != tobii_error_t.TOBII_ERROR_NO_ERROR)
            {
                TobiiVR_Logging.LogError(result.GetCommandResultAndTimeString());
            }

            TobiiVR_Logging.Log(result.GetCommandResultAndTimeString());

            if (_calibrationHelper.KillJoin(10, 1))
            {
                TobiiVR_Logging.Log("Successfully stoped calibration thread.");
            }
            else
            {
                TobiiVR_Logging.LogError("Failed to stop calibration thread.");
            }
            _calibrationHelper = null;
            TobiiVR_Host.Instance.ReleaseCalibrationHelper();
            OnCalibrationCompleted.Invoke(this, args);
        }
    }
}