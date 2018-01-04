// Copyright © 2015 – Property of Tobii AB (publ) - All Rights Reserved

namespace Tobii.VR
{
    using Tobii.StreamEngine;
    using System;
    using Vector3 = UnityEngine.Vector3;
    using Vector2 = UnityEngine.Vector2;

    public static class TobiiVR_Util
    {
        // Credit
        // http://wiki.unity3d.com/index.php/3d_Math_functions
        // http://creativecommons.org/licenses/by-sa/3.0/
        public static bool ClosestPointsOnTwoLines(out Vector3 closestPointLine1, out Vector3 closestPointLine2, Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2)
        {
            closestPointLine1 = Vector3.zero;
            closestPointLine2 = Vector3.zero;

            float a = Vector3.Dot(lineVec1, lineVec1);
            float b = Vector3.Dot(lineVec1, lineVec2);
            float e = Vector3.Dot(lineVec2, lineVec2);

            float d = a * e - b * b;

            //lines are not parallel
            if (d != 0.0f)
            {
                Vector3 r = linePoint1 - linePoint2;
                float c = Vector3.Dot(lineVec1, r);
                float f = Vector3.Dot(lineVec2, r);

                float s = (b * f - c * e) / d;
                float t = (a * f - c * b) / d;

                closestPointLine1 = linePoint1 + lineVec1 * s;
                closestPointLine2 = linePoint2 + lineVec2 * t;

                return true;
            }
            else
            {
                return false;
            }
        }

        // Credit
        // http://wiki.unity3d.com/index.php/3d_Math_functions
        // http://creativecommons.org/licenses/by-sa/3.0/
        //This function returns a point which is a projection from a point to a line.
        //The line is regarded infinite. If the line is finite, use ProjectPointOnLineSegment() instead.
        public static Vector3 ProjectPointOnLine(Vector3 linePoint, Vector3 lineVec, Vector3 point)
        {
            //get vector from point on line to point in space
            Vector3 linePointToPoint = point - linePoint;

            float t = Vector3.Dot(linePointToPoint, lineVec);

            return linePoint + lineVec * t;
        }

        // Credit
        // http://wiki.unity3d.com/index.php/3d_Math_functions
        // http://creativecommons.org/licenses/by-sa/3.0/
        //This function returns a point which is a projection from a point to a line segment.
        //If the projected point lies outside of the line segment, the projected point will
        //be clamped to the appropriate line edge.
        //If the line is infinite instead of a segment, use ProjectPointOnLine() instead.
        public static Vector3 ProjectPointOnLineSegment(Vector3 linePoint1, Vector3 linePoint2, Vector3 point)
        {
            Vector3 vector = linePoint2 - linePoint1;

            Vector3 projectedPoint = ProjectPointOnLine(linePoint1, vector.normalized, point);

            int side = PointOnWhichSideOfLineSegment(linePoint1, linePoint2, projectedPoint);

            //The projected point is on the line segment
            if (side == 0)
            {
                return projectedPoint;
            }

            if (side == 1)
            {
                return linePoint1;
            }

            if (side == 2)
            {
                return linePoint2;
            }

            //output is invalid
            return Vector3.zero;
        }

        // Credit
        // http://wiki.unity3d.com/index.php/3d_Math_functions
        // http://creativecommons.org/licenses/by-sa/3.0/
        //This function finds out on which side of a line segment the point is located.
        //The point is assumed to be on a line created by linePoint1 and linePoint2. If the point is not on
        //the line segment, project it on the line using ProjectPointOnLine() first.
        //Returns 0 if point is on the line segment.
        //Returns 1 if point is outside of the line segment and located on the side of linePoint1.
        //Returns 2 if point is outside of the line segment and located on the side of linePoint2.
        public static int PointOnWhichSideOfLineSegment(Vector3 linePoint1, Vector3 linePoint2, Vector3 point)
        {
            Vector3 lineVec = linePoint2 - linePoint1;
            Vector3 pointVec = point - linePoint1;

            float dot = Vector3.Dot(pointVec, lineVec);

            //point is on side of linePoint2, compared to linePoint1
            if (dot > 0)
            {
                //point is on the line segment
                if (pointVec.magnitude <= lineVec.magnitude)
                {
                    return 0;
                }

                //point is not on the line segment and it is on the side of linePoint2
                else
                {
                    return 2;
                }
            }

            //Point is not on side of linePoint2, compared to linePoint1.
            //Point is not on the line segment and it is on the side of linePoint1.
            else
            {
                return 1;
            }
        }

        public static tobii_error_t SetLensCupSeparation(float hmdIpdInMeter, IntPtr instance)
        {

            var newLensCupConfig = new tobii_lens_configuration_t
            {
                left_xyz = { x = (hmdIpdInMeter / 2f) * 1000f, y = 0, z = 0},
                right_xyz = { x = (-hmdIpdInMeter / 2F) * 1000f, y = 0, z = 0 }
            };

            return Interop.tobii_set_lens_configuration(instance, newLensCupConfig);
        }

        public static bool TryGetHmdLensCupSeparationInMeter(out float hmdIpdInMeter)
        {
            hmdIpdInMeter = 0;

            var steamVR = Type.GetType("SteamVR");
            if (steamVR == null) return false;

            var instance = steamVR.GetProperty("instance").GetValue(null, null);
            if (instance == null) return false;

            var steamVRActive = steamVR.GetProperty("active").GetValue(null, null);
            if (steamVRActive == null || (bool)steamVRActive == false)
            {
                return false;
            }

            var error = 0;
            var hmd = instance.GetType().GetProperty("hmd").GetValue(instance, null);
            var method = hmd.GetType().GetMethod("GetFloatTrackedDeviceProperty");
            var props = method.GetParameters()[1].ParameterType;

            var arguments = new object[] { (uint)0, Enum.Parse(props, "Prop_UserIpdMeters_Float"), error };

            hmdIpdInMeter = (float)method.Invoke(hmd, arguments);
            error = (int)arguments[2];

            if (error != 0)
            {
                return false;
            }

            return true;
        }
    }

    public struct EyeData
    {
        public bool IsGazeOriginValid;
        public Vector3 GazeOrigin;

        public bool IsGazeDirectionValid;
        public Vector3 GazeDirectionNormalized;

        public bool PupilValidity;
        public float PupilDiameter;

        public bool EyeOpennessValidity;
        public float EyeOpenness;

        public bool PupilPositionInSensorAreaValidity;
        public Vector2 PupilPositionInSensorAreaXy;

        public void CopyFromWearableEyeData(ref tobii_wearable_eye_t eyeData)
        {
            IsGazeOriginValid = (int)eyeData.gaze_direction_validity == 1;
            GazeOrigin.x = eyeData.gaze_origin_mm_xyz.x * -1 / 1000f;
            GazeOrigin.y = eyeData.gaze_origin_mm_xyz.y / 1000f;
            GazeOrigin.z = eyeData.gaze_origin_mm_xyz.z / 1000f;

            IsGazeDirectionValid = (int)eyeData.gaze_direction_validity == 1;
            GazeDirectionNormalized.x = eyeData.gaze_direction_normalized_xyz.x * -1;
            GazeDirectionNormalized.y = eyeData.gaze_direction_normalized_xyz.y;
            GazeDirectionNormalized.z = eyeData.gaze_direction_normalized_xyz.z;

            PupilValidity = (int)eyeData.pupil_diameter_validity == 1;
            PupilDiameter = eyeData.pupil_diameter_mm / 1000f;

            EyeOpennessValidity = (int)eyeData.eye_openness_validity == 1;
            EyeOpenness = eyeData.eye_openness;

            PupilPositionInSensorAreaValidity = (int)eyeData.pupil_position_in_sensor_area_validity == 1;
            PupilPositionInSensorAreaXy.x = eyeData.pupil_position_in_sensor_area_xy.x;
            PupilPositionInSensorAreaXy.y = eyeData.pupil_position_in_sensor_area_xy.y;
        }
    }

    /// <summary>
    /// This struct converts from tracker types to Unity types as well as transforms from tracker coordinate space 
    /// to Unity coordinate space. Please note that the API and or other documentations may refer to the tracker 
    /// coordinate space.
    /// </summary>
    public struct TrackerData
    {
        public long TimestampTrackerUs;
        public long TimestampSystemUs;
        public uint FrameCounter;
        public uint LedMode;
        public EyeData Left;
        public EyeData Right;

        public void CopyFromWearableData(ref tobii_wearable_data_t data)
        {
            TimestampTrackerUs = data.timestamp_tracker_us;
            TimestampSystemUs = data.timestamp_system_us;
            FrameCounter = data.frame_counter;
            LedMode = data.led_mode;

            Left.CopyFromWearableEyeData(ref data.left);
            Right.CopyFromWearableEyeData(ref data.right);
        }
    }
}