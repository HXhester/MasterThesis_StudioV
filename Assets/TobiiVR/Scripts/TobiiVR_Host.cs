// Copyright © 2015 – Property of Tobii AB (publ) - All Rights Reserved

namespace Tobii.VR
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using System.Diagnostics;
    using Vector3 = UnityEngine.Vector3;
    using System.Collections;
    using System.Threading;
    using StreamEngine;

    public class TobiiVR_Host : MonoBehaviour
    {
        public enum TrackerState
        {
            NotConnected,   // Before init
            Connected,      // All is good
            Reconnecting,   // Do not expect anything to work during this state
            NotRecoverable, // Unexpected error, should never go here
            Fallback        // No eye tracker found, using nose as fallback
        }

        /// <summary>
        /// The gaze direction in local space relative to parents. Based of the latest valid data.
        /// </summary>
        public Vector3 LocalGazeDirection { get { return _localGazeDirection; } }

        /// <summary>
        /// The gaze direction in world space. Based of the latest valid data.
        /// </summary>
        public Vector3 GazeDirection { get; private set; }

        public IntPtr DeviceContext { get { return _deviceContext; } private set { _deviceContext = value; } }

        public IntPtr ApiContext { get; private set; }

        /// <summary>
        /// The transform used to change from eye tracking coordinate to world space. 
        /// Default is Camera.main.transform
        /// </summary>
        public Transform LocalToWorldTransform { get { return _localToWorldTransform; } }

        public bool IsCalibrating { get { return _calibrationHelper != null; } }

        /// <summary>
        /// Only called once per frame if the new data received is valid.
        /// </summary>
        public event EventHandler ValidTrackerData = delegate { };

        public TrackerData LatestData { get { return _latestData; } }

        /// <summary>
        /// This returns false if the internal state is for some reason using fallback.
        /// </summary>
        public bool IsDataComingFromTracker { get { return _state != TrackerState.Fallback; } }

        private static TobiiVR_Host _instance;

        private readonly Stopwatch _timer = new Stopwatch();
        private TrackerState _state = TrackerState.NotConnected;
        private tobii_wearable_data_callback_t _wearableDataCallback; // Needed to prevent GC from removing callback
        private TobiiVR_CalibrationHelper _calibrationHelper = null; // This should be null unless a calibrating is underway
        private TrackerData _latestData;
        private Vector3 _localGazeDirection = Vector3.forward;
        private Coroutine _reconnectCorutine;
        private volatile IntPtr _deviceContext;
        private Transform _localToWorldTransform;

        public TobiiVR_CalibrationHelper CreateCalibrationHelper()
        {
            if (_state != TrackerState.Connected)
            {
                TobiiVR_Logging.Log(string.Format("Cannot create a calibration when in state {0}.", _state));
                return null;
            }

            if (IsCalibrating)
            {
                TobiiVR_Logging.Log("Cannot create the calibration helper, calibration is already started.");
                return null;
            }

            _calibrationHelper = new TobiiVR_CalibrationHelper(DeviceContext);
            return _calibrationHelper;
        }

        public void ReleaseCalibrationHelper()
        {
            _calibrationHelper = null;
        }

        public void Init(Camera camera = null)
        {
            if (camera == null) return;

            SetCameraUsedToRender(camera);
        }

        /// <summary>
        /// SteamVR renders the left and right eye 15mm behind the HMD origin and
        /// eye tracking origin is between the lenses it has to be offseted.
        /// </summary>
        /// <param name="newCamera">Should be considered the "main" camera that renders the scene.</param>
        public void SetCameraUsedToRender(Camera newCamera)
        {
            if (newCamera == null) return;

            if (LocalToWorldTransform != null)
            {
                Destroy(LocalToWorldTransform.gameObject);
            }

            // Default offset.
            var zOffs = 0.015f;

            var error = -1;

            // Reflection is used to remove the dependency to have SteamVR in the package.
            var steamVR = Type.GetType("SteamVR");
            if (steamVR != null)
            {
                var instance = steamVR.GetProperty("instance").GetValue(null, null);
                if (instance != null)
                {
                    var steamVRActive = steamVR.GetProperty("active").GetValue(null, null);
                    if (steamVRActive != null && (bool)steamVRActive == true)
                    {
                        var hmd = instance.GetType().GetProperty("hmd").GetValue(instance, null);
                        var method = hmd.GetType().GetMethod("GetFloatTrackedDeviceProperty");
                        var props = method.GetParameters()[1].ParameterType;

                        var arguments = new object[] { (uint)0, Enum.Parse(props, "Prop_UserHeadToEyeDepthMeters_Float"), error };

                        var hmdHeadToEyeDepthMeters = (float)method.Invoke(hmd, arguments);
                        error = (int)arguments[2];

                        if (error == 0)
                        {
                            zOffs = hmdHeadToEyeDepthMeters;
                        }
                    }
                }
            }

            if (error != 0)
            {
                TobiiVR_Logging.LogWarning("Failed to get the offset from head to eye. Setting default " + zOffs + "m");
            }

            var eyeTrackerOrigin = new GameObject("TobiiVR_Origin");
            eyeTrackerOrigin.transform.parent = newCamera.transform;
            eyeTrackerOrigin.transform.localScale = Vector3.one;
            eyeTrackerOrigin.transform.localPosition = new Vector3(0, 0, zOffs);
            eyeTrackerOrigin.transform.localRotation = Quaternion.identity;

            _localToWorldTransform = eyeTrackerOrigin.transform;
        }

        private void Update()
        {
            switch (_state)
            {
                case TrackerState.NotConnected:
                    break;
                case TrackerState.Connected:
                    var previoustTimeStamp = LatestData.TimestampTrackerUs;

                    _timer.Start();
                    var result = Interop.tobii_process_callbacks(DeviceContext);
                    _timer.Stop();

                    if (_timer.ElapsedMilliseconds > 1)
                    {
                        TobiiVR_Logging.LogError(string.Format("Process callback took {0}ms to process.", _timer.ElapsedMilliseconds));
                    }

                    _timer.Reset();

                    if (result != tobii_error_t.TOBII_ERROR_NO_ERROR)
                    {
                        TobiiVR_Logging.LogError(string.Format("Process callback returned with error code: {0}.", result));
                        switch (result)
                        {
                            case tobii_error_t.TOBII_ERROR_CONNECTION_FAILED:
                            case tobii_error_t.TOBII_ERROR_FIRMWARE_NO_RESPONSE:
                                SetState(TrackerState.Reconnecting);
                                break;
                            default:
                                SetState(TrackerState.NotRecoverable);
                                break;
                        }

                        return;
                    }

                    if (LatestData.TimestampTrackerUs == previoustTimeStamp)
                    {
                        if (LocalToWorldTransform != null)
                        {
                            GazeDirection = LocalToWorldTransform.TransformDirection(_localGazeDirection);
                        }
                        return;
                    }

                    var isBoothEyesValid = LatestData.Left.IsGazeDirectionValid &&
                                           LatestData.Right.IsGazeDirectionValid;

                    if (isBoothEyesValid == false)
                    {
                        return;
                    }

                    _localGazeDirection.x = ((LatestData.Left.GazeDirectionNormalized.x) +
                                             (LatestData.Right.GazeDirectionNormalized.x)) / 2f;
                    _localGazeDirection.y = (LatestData.Left.GazeDirectionNormalized.y +
                                             LatestData.Right.GazeDirectionNormalized.y) / 2f;
                    _localGazeDirection.z = (LatestData.Left.GazeDirectionNormalized.z +
                                             LatestData.Right.GazeDirectionNormalized.z) / 2f;
                    _localGazeDirection.Normalize();

                    if (LocalToWorldTransform != null)
                    {
                        GazeDirection = LocalToWorldTransform.TransformDirection(_localGazeDirection);
                    }

                    ValidTrackerData.Invoke(this, null);
                    break;
                case TrackerState.Reconnecting:
                    break;
                case TrackerState.NotRecoverable:
                    break;
                case TrackerState.Fallback:
                    // TODO: Check if a tracker has been connected.

                    if (LocalToWorldTransform == null)
                    {
                        return;
                    }

                    GazeDirection = LocalToWorldTransform.TransformDirection(_localGazeDirection);
                    ValidTrackerData.Invoke(this, null);
                    break;
                default:
                    break;
            }
        }

        private void OnApplicationQuit()
        {
            if (_reconnectCorutine != null)
            {
                StopCoroutine(_reconnectCorutine);
            }

            var result = tobii_error_t.TOBII_ERROR_NO_ERROR;

            if (_state != TrackerState.Fallback)
            {
                result = Interop.tobii_wearable_data_unsubscribe(DeviceContext);
                if (result != tobii_error_t.TOBII_ERROR_NO_ERROR)
                {
                    TobiiVR_Logging.LogError(string.Format("Failed to unsubscribe from wearable stream. Error {0}", result));
                }

                result = Interop.tobii_device_destroy(DeviceContext);
                if (result != tobii_error_t.TOBII_ERROR_NO_ERROR)
                {
                    TobiiVR_Logging.LogError(string.Format("Failed to destroy device context. Error {0}", result));
                }
            }

            result = Interop.tobii_api_destroy(ApiContext);
            if (result != tobii_error_t.TOBII_ERROR_NO_ERROR)
            {
                TobiiVR_Logging.LogError(string.Format("Failed to destroy API context. Error {0}", result));
            }
        }

        private void Reconnect()
        {
            if (_state != TrackerState.Reconnecting)
            {
                TobiiVR_Logging.LogWarning(string.Format("Cannot reconnect while in state {0}", _state));
                return;
            }

            if (_reconnectCorutine != null)
            {
                TobiiVR_Logging.LogError(string.Format("The reconnect cortunine indicates its still running. This should not happen"));
                return;
            }

            _reconnectCorutine = StartCoroutine(Reconnecting());
        }

        private IEnumerator Reconnecting()
        {
            var hasReconnected = false;
            var reconnectThread = new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                var error = tobii_error_t.TOBII_ERROR_CONNECTION_FAILED;
                while (hasReconnected == false)
                {
                    error = Interop.tobii_reconnect(DeviceContext);

                    if (error == tobii_error_t.TOBII_ERROR_NO_ERROR)
                    {
                        hasReconnected = true;
                    }
                    else
                    {
                        Thread.Sleep(500);
                    }
                }
            });

            reconnectThread.Name = "TobiiVR Reconnect Thread";
            reconnectThread.Start();

            while (reconnectThread.IsAlive)
            {
                yield return null;
            }

            if (hasReconnected)
            {
                TobiiVR_Logging.Log("Successfully reconnected to the tracker.");
                SetState(TrackerState.Connected);
            }
            else
            {
                TobiiVR_Logging.LogError("Failed to reconnect to tracker.");
                SetState(TrackerState.NotRecoverable);
            }

            _reconnectCorutine = null;
        }

        private void OnWearableData(ref tobii_wearable_data_t data)
        {
            _latestData.CopyFromWearableData(ref data);
        }

        private bool TryConnectToTracker()
        {
            switch (_state)
            {
                case TrackerState.NotConnected:
                case TrackerState.Fallback:
                    break;
                case TrackerState.Connected:
                    TobiiVR_Logging.LogWarning("There is already a connection, cannot connect again.");
                    return false;
                case TrackerState.Reconnecting:
                    TobiiVR_Logging.LogWarning("Cannot connect while trying to reconnect.");
                    return false;
                case TrackerState.NotRecoverable:
                    TobiiVR_Logging.LogWarning("In unknown state. Will not try to connect to tracker.");
                    break;
                default:
                    TobiiVR_Logging.LogError("Unknown internal state: " + _state);
                    break;
            }

            var stopwatch = new Stopwatch();
            stopwatch.Start();


            IntPtr apiContext;
            var result = Interop.tobii_api_create(out apiContext, null);
            if (result != tobii_error_t.TOBII_ERROR_NO_ERROR)
            {
                TobiiVR_Logging.LogError("Failed to create api context. " + result);
                return false;
            }

            ApiContext = apiContext;

            List<string> urls;
            result = Interop.tobii_enumerate_local_device_urls(ApiContext, out urls);
            if (result != tobii_error_t.TOBII_ERROR_NO_ERROR)
            {
                TobiiVR_Logging.LogError("Failed to get list of devices. " + result);
                return false;
            }

            if (urls.Count < 1)
            {
                TobiiVR_Logging.LogError("No eye trackers found.");
                return false;
            }

            var index = -1;
            for (int i = 0; i < urls.Count; i++)
            {
                var deviceUrl = urls[i];
                if (deviceUrl.Contains("vr"))
                {
                    index = i;
                    break;
                }
            }

            if (index == -1)
            {
                TobiiVR_Logging.LogError("Failed to find any VR eye tracker.");
                return false;
            }

            IntPtr deviceContext;
            result = Interop.tobii_device_create(ApiContext, urls[index], out deviceContext);
            if (result != tobii_error_t.TOBII_ERROR_NO_ERROR)
            {
                if (deviceContext != IntPtr.Zero)
                {
                    Interop.tobii_device_destroy(deviceContext);
                    Interop.tobii_api_destroy(ApiContext);
                }
                TobiiVR_Logging.LogError("Failed to create context. " + result);
                return false;
            }

            DeviceContext = deviceContext;

            _wearableDataCallback = OnWearableData;
            result = Interop.tobii_wearable_data_subscribe(DeviceContext, _wearableDataCallback);
            if (result != tobii_error_t.TOBII_ERROR_NO_ERROR)
            {
                TobiiVR_Logging.LogError("Failed to subscribe to wearable stream." + result);
                return false;
            }

            stopwatch.Stop();
            TobiiVR_Logging.Log(string.Format("Connected to tracker: {0} and it took {1}ms", urls[index], stopwatch.ElapsedMilliseconds));

            SetState(TrackerState.Connected);
            return true;

        }

        private void SetState(TrackerState newState)
        {
            TobiiVR_Logging.Log(string.Format("Changing state from {0} to {1}", _state, newState));

            switch (_state)
            {
                case TrackerState.NotConnected:
                    break;
                case TrackerState.Connected:
                    break;
                case TrackerState.Reconnecting:
                    break;
                case TrackerState.NotRecoverable:
                    break;
                case TrackerState.Fallback:
                    break;
                default:
                    break;
            }

            _state = newState;

            switch (_state)
            {
                case TrackerState.NotConnected:
                    break;
                case TrackerState.Connected:
                    break;
                case TrackerState.Reconnecting:
                    Reconnect();
                    break;
                case TrackerState.NotRecoverable:
                    break;
                case TrackerState.Fallback:
                    break;
                default:
                    break;
            }
        }

        public static TobiiVR_Host Instance
        {
            get
            {
                if (_instance != null)
                {
                    return _instance;
                }

                if (TobiiVR_EulaFile.Instance.IsEulaAccepted() == false)
                {
                    TobiiVR_Logging.LogError("Tobii EULA not accepted, cannot create TobiiVR_Host instance.");
                    return null;
                }

                var newGameObject = new GameObject("TobiiVR_Host");
                DontDestroyOnLoad(newGameObject);
                _instance = newGameObject.AddComponent<TobiiVR_Host>();

                bool result = false;
                try
                {
                    result = _instance.TryConnectToTracker();
                }
                catch (DllNotFoundException e)
                {
                    TobiiVR_Logging.LogException(e);
                    result = false;
                }

                if (result == false)
                {
                    TobiiVR_Logging.LogError("Failed to create tracker.");
                    _instance.SetState(TrackerState.Fallback);
                }

                _instance.SetCameraUsedToRender(Camera.main);

                return _instance;
            }
        }
    }
}