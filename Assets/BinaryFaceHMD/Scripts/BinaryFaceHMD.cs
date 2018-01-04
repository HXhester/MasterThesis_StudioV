/**************************************************************************************************
BINARYVR, INC. PROPRIETARY INFORMATION
This software is supplied under the terms of a license agreement or nondisclosure
agreement with BinaryVR, Inc. and may not be copied or disclosed except in
accordance with the terms of that agreement
Copyright(c) 2016 BinaryVR, Inc. All Rights Reserved.
**************************************************************************************************/

using System.Runtime.InteropServices;
using System;
using System.Text;

namespace BinaryFaceHMD
{
  using Context = System.Int32;
  using DeviceID = System.Int32;

  public enum ReturnValue
  {
    BINARYFACEHMD_OK = 0,                   /// Success
    BINARYFACEHMD_SDK_VERSION_MISMATCH = 1, /// binaryfacehmd.h is not up-to-date
    BINARYFACEHMD_FILE_NOT_FOUND = 2,       /// Data file is not found
    BINARYFACEHMD_FILE_CORRUPTED = 3,       /// Api key is not valid
    BINARYFACEHMD_INVALID_PARAMETER = 4,    /// Passed parameters are not valid

    BINARYFACEHMD_DEVICE_NOT_CONNECTED = 100, /// Failed to find a connected camera
    BINARYFACEHMD_DEVICE_OPEN_FAILED = 101,   /// Failed to open the camera device 
    BINARYFACEHMD_DEVICE_NOT_OPENED = 102,    /// The camera device is not open yet
    BINARYFACEHMD_FRAME_NOT_CAPTURED = 103,   /// Image frame is not captured 

    BINARYFACEHMD_PROCESSOR_READY = 200,          /// Processor is ready to start calibration or tracking
    BINARYFACEHMD_ON_CALIBRATION = 201,           /// Processor is running calibration
    BINARYFACEHMD_ON_TRACKING = 202,              /// Processor is tracking a user's facial expression 
    BINARYFACEHMD_TRACKING_START_FAILED = 203,    /// Failed to start tracking
    BINARYFACEHMD_TRACKING_NOT_STARTED_YET = 204, /// Tracking is not started yet

    BINARYFACEHMD_TRACKING_FAILED = 301,          /// Failed to track a user's facial expression for the current frame 

    BINARYFACEHMD_USER_MODEL_LOADED = 400,      /// User model is found in the cache directory
    BINARYFACEHMD_USER_MODEL_NOT_FOUND = 401,  /// User model is not found. Calibration should be done for accurate tracking

    BINARYFACEHMD_UNKNOWN_ERROR = 1000      /// Unknown error
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct ContextInfo
  {
    public int SdkVersion;
    public int DataFileVersion;
    public int NumBlendShapes;
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct ImageInfo
  {
    public int Width;
    public int Height;
    public int NumBytesPerPixel;
    public float FocalLength;
    public float PrincipalPointX;
    public float PrincipalPointY;
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct FaceInfo
  {
    System.Single Confidence;
  }

  //  Wrapped C# API
  public static class API
  {
    public const string DEFAULT_API_KEY = "api_key";
    public const int SDK_VERSION = 0x0001;

    //  Native interfaces
    [DllImport("binaryfacehmd", EntryPoint = "binaryfacehmd_open_context")]
    static extern int binaryfacehmd_open_context(
      IntPtr modelFilePath,
      IntPtr cacheDirPath,
      IntPtr apiKey,
      int sdkVersion,
      ref int contextOut,
      ref ContextInfo infoOut);

    [DllImport("binaryfacehmd")]
    static extern int binaryfacehmd_close_context(
      int contextId);

    [DllImport("binaryfacehmd")]
    static extern int binaryfacehmd_open_device(
      int contextId,
      int deviceId,
      ref ImageInfo imageInfoOut);

    [DllImport("binaryfacehmd")]
    static extern int binaryfacehmd_open_device_file(
      int contextId,
      IntPtr deviceFilePath,
      ref ImageInfo imageInfoOut);

    [DllImport("binaryfacehmd")]
    static extern int binaryfacehmd_close_device(
      int contextId);

    [DllImport("binaryfacehmd")]
    static extern int binaryfacehmd_set_user(
      int contextId,
      IntPtr userId);

    [DllImport("binaryfacehmd")]
    static extern int binaryfacehmd_start_calibration_and_tracking(
      int contextId);

    [DllImport("binaryfacehmd")]
    static extern int binaryfacehmd_start_tracking(
      int contextId);

    [DllImport("binaryfacehmd")]
    static extern int binaryfacehmd_get_image(
      int contextId,
      byte[] image_out);

    [DllImport("binaryfacehmd")]
    static extern int binaryfacehmd_get_face_info(
      int contextId,
      ref FaceInfo faceInfoOut,
      float[] blendShapeWeightsOut);

    [DllImport("binaryfacehmd")]
    static extern int binaryfacehmd_get_processing_status(
      int contextId);

    //  C# wrappers
    static IntPtr StringToUTF8(string managedString)
    {
      int len = Encoding.UTF8.GetByteCount(managedString);
      byte[] buffer = new byte[len + 1];
      Encoding.UTF8.GetBytes(managedString, 0, managedString.Length, buffer, 0);
      IntPtr nativeUtf8 = Marshal.AllocHGlobal(buffer.Length);
      Marshal.Copy(buffer, 0, nativeUtf8, buffer.Length);
      return nativeUtf8;
    }

    public static ReturnValue OpenContext(
      string modelFilePath,
      string cacheDirPath,
      string apiKey,
      int sdkVersion,
      ref Context contextOut,
      ref ContextInfo contextInfoOut)
    {
      contextOut = new Context();
      contextInfoOut = new ContextInfo();

      var ret = (ReturnValue)binaryfacehmd_open_context(
        StringToUTF8(modelFilePath),
        StringToUTF8(cacheDirPath),
        StringToUTF8(apiKey),
        sdkVersion,
        ref contextOut,
        ref contextInfoOut);

      return ret;
    }

    public static ReturnValue CloseContext(
      Context context)
    {
      var ret = (ReturnValue)binaryfacehmd_close_context(context);
      return ret;
    }

    public static ReturnValue OpenDevice(
      Context context,
      ref ImageInfo imageInfo)
    {
      return (ReturnValue)binaryfacehmd_open_device(
        context,
        0,
        ref imageInfo);
    }

    public static ReturnValue OpenDeviceFile(
      Context context,
      string filename,
      ref ImageInfo imageInfo)
    {
      return (ReturnValue)binaryfacehmd_open_device_file(
        context, StringToUTF8(filename), ref imageInfo);
    }

    public static ReturnValue CloseDevice(
      Context context)
    {
      return (ReturnValue)binaryfacehmd_close_device(context);
    }

    public static ReturnValue SetUser(
      Context context,
      string userId)
    {
      return (ReturnValue)binaryfacehmd_set_user(
        context, StringToUTF8(userId));
    }

    public static ReturnValue StartCalibrationAndTracking(
      Context context)
    {
      return (ReturnValue)binaryfacehmd_start_calibration_and_tracking(
        context);
    }

    public static ReturnValue StartTracking(
      Context context)
    {
      return (ReturnValue)binaryfacehmd_start_tracking(
        context);
    }

    public static ReturnValue GetImage(
      Context context,
      byte[] image)
    {
      return (ReturnValue)binaryfacehmd_get_image(context, image);
    }

    public static ReturnValue GetFaceInfo(
      Context context,
      ref FaceInfo faceInfo,
      float[] blendshapeWeightsOut)
    {
      return (ReturnValue)binaryfacehmd_get_face_info(context, ref faceInfo, blendshapeWeightsOut);
    }

    public static ReturnValue GetProcessingStatus(
      Context context)
    {
      return (ReturnValue)binaryfacehmd_get_processing_status(context);
    }
  }
}
