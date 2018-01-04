/**************************************************************************************************
BINARYVR, INC. PROPRIETARY INFORMATION
This software is supplied under the terms of a license agreement or nondisclosure
agreement with BinaryVR, Inc. and may not be copied or disclosed except in
accordance with the terms of that agreement
Copyright(c) 2016 BinaryVR, Inc. All Rights Reserved.
**************************************************************************************************/


using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.IO;
using UnityEngine.UI;

public class FaceExpressionController : MonoBehaviour
{
  byte[] imageOutBuffer = null;

  public RawImage capturedIrImage;
  Texture2D capturedIrImageTex;
  byte[] imageTexBuffer = null;
  static int imageTexWidth = 256;  // power of 2 > image_out.width
  static int imageTexHeight = 256; // power of 2 > image_out.height

  int context = 0;
  bool deviceOpened = false;

  BinaryFaceHMD.ContextInfo contextInfo = new BinaryFaceHMD.ContextInfo();
  BinaryFaceHMD.ImageInfo imageInfo = new BinaryFaceHMD.ImageInfo();
  BinaryFaceHMD.FaceInfo faceInfo = new BinaryFaceHMD.FaceInfo();
  public BinaryFaceHMD.FaceInfo FaceInfo
  {
    get
    {
      return faceInfo;
    }
  }

  float[] blendShapeWeights = null;
  public float[] BlendShapeWeights
  {
    get
    {
      return blendShapeWeights;
    }
  }

  // Use this for initialization
  void Start()
  {
    Debug.Assert(context == 0);
    Debug.Assert(!deviceOpened);

    string modelFile = Application.dataPath + "/StreamingAssets/model.bfh";
    string userDataDir = Application.temporaryCachePath + "/BinaryFaceHMD";

    Directory.CreateDirectory(userDataDir);

    var ret = BinaryFaceHMD.API.OpenContext(
      modelFile,
      userDataDir,
      BinaryFaceHMD.API.DEFAULT_API_KEY,
      BinaryFaceHMD.API.SDK_VERSION,
      ref context,
      ref contextInfo);

    if (ret != BinaryFaceHMD.ReturnValue.BINARYFACEHMD_OK)
    {
      Debug.LogError("BinaryFaceHMD.API.OpenContext:" + ret);
      return;
    }

    blendShapeWeights = new float[contextInfo.NumBlendShapes];

    //  Set the user name 'Demo'.
    ret = BinaryFaceHMD.API.SetUser(context, "Demo");
  }

  void OnDestroy()
  {
    if (context != 0)
    {
      BinaryFaceHMD.ReturnValue ret;
      if (deviceOpened)
      {
        ret = BinaryFaceHMD.API.CloseDevice(context);
        if (ret != BinaryFaceHMD.ReturnValue.BINARYFACEHMD_OK)
        {
          Debug.LogError("BinaryFaceHMD.API.CloseDevice:" + ret);
          return;
        }
        deviceOpened = false;
      }

      ret = BinaryFaceHMD.API.CloseContext(context);
      if (ret != BinaryFaceHMD.ReturnValue.BINARYFACEHMD_OK)
      {
        Debug.LogError("BinaryFaceHMD.API.CloseContext:" + ret);
      }
      context = 0;
    }
  }

  void Update()
  {
    if (!deviceOpened)
    {
      return;
    }
    var ret = BinaryFaceHMD.API.GetImage(context, imageOutBuffer);

    int imageOutBytesPerRow = imageInfo.Width * imageInfo.NumBytesPerPixel;
    int imageTexBytesPerRow = imageTexWidth * imageInfo.NumBytesPerPixel;
    for (int i = 0; i < imageInfo.Height; ++i)
    {
      System.Array.Copy(imageOutBuffer, (imageInfo.Height - i - 1) * imageOutBytesPerRow, // flip image upside down. 
                        imageTexBuffer, i * imageTexBytesPerRow,
                        imageOutBytesPerRow);
    }

    capturedIrImageTex.LoadRawTextureData(imageTexBuffer);
    capturedIrImageTex.Apply();
    capturedIrImage.texture = capturedIrImageTex;

    ret = BinaryFaceHMD.API.GetProcessingStatus(context);
    if (ret == BinaryFaceHMD.ReturnValue.BINARYFACEHMD_ON_TRACKING)
    {
      ret = BinaryFaceHMD.API.GetFaceInfo(context, ref faceInfo, blendShapeWeights);
    }
  }

  //  Button Controls
  public void OpenDevice()
  {
    if (deviceOpened)
    {
      if (context != 0)
      {
        BinaryFaceHMD.API.CloseDevice(context);
        deviceOpened = false;
      }
    }

    var ret = BinaryFaceHMD.API.OpenDevice(
      context,
      ref imageInfo);

    //string recorded_file = "recorded_file.rrf";
    //var ret = BinaryFaceHMD.API.OpenDeviceFile(
    //  context,
    //  recorded_file,
    //  ref imageInfo);

    if (ret != BinaryFaceHMD.ReturnValue.BINARYFACEHMD_OK)
    {
      Debug.LogError("BinaryFaceHMD.API.OpenContext:" + ret);
      return;
    }

    imageOutBuffer = new byte[imageInfo.NumBytesPerPixel * imageInfo.Width * imageInfo.Height];

    capturedIrImageTex = new Texture2D(imageTexWidth, imageTexHeight, TextureFormat.RGB24, false);
    imageTexBuffer = new byte[imageInfo.NumBytesPerPixel * imageTexWidth * imageTexHeight];

    deviceOpened = true;
  }

  public void StartCalibrateAndTracking()
  {
    if (!deviceOpened) return;
    BinaryFaceHMD.API.StartCalibrationAndTracking(context);
  }

  public void StartTracking()
  {
    if (!deviceOpened) return;
    BinaryFaceHMD.API.StartTracking(context);
  }
}
