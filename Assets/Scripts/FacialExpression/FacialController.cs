using System.Collections;
using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class FacialController : Photon.MonoBehaviour {

	FaceExpressionController faceExpressionController;
	float[] blendShapes;

	float[] expressionWeights =
	{
		1.0f, // "JawOpen"s
		1.2f, // "JawLeft"
		1.2f, // "JawWRight"
		1.0f, // "JawFwd"
		2f, // "LipsUpperUp_L"
		2f, // "LipsUpperUp_R"
		2f, // "LipsLowerDown_L"
		2f, // "LipsLowerDown_R"
		1.2f, // "LipsUpperClose"
		1.2f, // "LipsLowerClose"
		2f, // "MouthSmile_L"
		2f, // "MouthSmile_R"
		2.2f, // "LipStretch_L"
		2.2f, // "LipStretch_R"
		3.2f, // "MouthFrown_L"
		3.2f, // "MouthFrown_R"
		1.3f, // "LipsPucker"
		2.2f, // "LipsFunnel"
		1.2f, // "MouthLeft"
		1.2f, // "MouthRight"
		1.0f  // "Puff"
	};

	[Tooltip("This contains 21 elements, which are corresponding to blendshapes this model has. It should has the same sequence as what ")]
	[SerializeField]
	[ExpressionTargets (new string[]{
		".JawOpen",
		".JawLeft",
		".JawRight",
		".JawFwd",
		".LipsUpperUp_L",
		".LipsUpperUp_R",
		".LipsLowerDown_L",
		".LipsLowerDown_R",
		".LipsUpperClose",
		".LipsLowerClose",
		".MouthSmile_L",
		".MouthSmile_R",
		".LipsStretch_L",
		".LipsStretch_R",
		".MouthFrown_L",
		".MouthFrown_R",
		".LipsPucker",
		".LipsFunnel",
		".MouthLeft",
		".MouthRight",
		".Puff"
	})]

	public string[] expressionTargets;

	private StreamWriter sw;
	private String filename;

	public bool isRecording = false;

	void Start()
	{
		faceExpressionController = FindObjectOfType<FaceExpressionController>();
    }

	void UpdateBlendShapes(Transform gameObject)
	{
        //  Update this
	    SkinnedMeshRenderer renderer = gameObject.GetComponent<SkinnedMeshRenderer>();
		if (renderer != null)
		{
			var mesh = renderer.sharedMesh;
			for (int i = 0; i < expressionTargets.Length; i++)
			{
				var expressionName = expressionTargets[i];
				var expressionWeight = expressionWeights[i];

				for (int j = 0; j < mesh.blendShapeCount; j++)
				{
					var blendShapeName = mesh.GetBlendShapeName(j);
					if (blendShapeName.Contains(expressionName))
					{
						renderer.SetBlendShapeWeight(j, blendShapes[i] * 100.0f * expressionWeight);
                    }
				}
			}
		}
		foreach(Transform children in gameObject)
		{
			UpdateBlendShapes(children);
		}
	}

	void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{


		if (stream.isWriting )
		{
			if (blendShapes == null)
				return;
			
			// create a byte array and copy the floats into it...
			var byteArray = new byte[blendShapes.Length * 4];
			Buffer.BlockCopy(blendShapes, 0, byteArray, 0, byteArray.Length);
			stream.SendNext (byteArray);
		}
		else
		{
			//recive float array in byte
			var byteArray = (byte[])stream.ReceiveNext();
			// create a second float array and copy the bytes into it...
			blendShapes = new float[byteArray.Length / 4];
			Buffer.BlockCopy(byteArray, 0, blendShapes, 0, byteArray.Length);

		}
	}

	// Update is called once per frame
	void Update()
	{
		if (photonView.isMine) {
			if (faceExpressionController == null)
				return;
			blendShapes = faceExpressionController.BlendShapeWeights;

			if (isRecording){
				for (int i = 0; i < expressionTargets.Length; i++) {
					sw.WriteLine (expressionTargets [i] + "," + Time.time + "," + blendShapes [i]);
				}
			}
		}
		if (blendShapes != null && blendShapes.Length == expressionTargets.Length) {
			UpdateBlendShapes (transform);
		}
	}
	#if UNITY_EDITOR
	[CustomPropertyDrawer (typeof(ExpressionTargets))]
	public class NamedArrayDrawer : PropertyDrawer{
		public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label){
			try {
				int pos = int.Parse(property.propertyPath.Split('[',']')[1]);
				EditorGUI.PropertyField(rect, property, new GUIContent(((ExpressionTargets)attribute).names[pos]));

			} catch {
				EditorGUI.PropertyField (rect, property, label);
			}
		}
	}
	#endif

	void OnApplicationQuit(){
		if (sw != null) {
			sw.Close ();
		}

	}
}
