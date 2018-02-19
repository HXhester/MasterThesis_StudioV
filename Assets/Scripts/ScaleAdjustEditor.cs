using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(ScaleAdjust))]
public class ScaleAdjustEditor : Editor
{

	public override void OnInspectorGUI ()
	{
		base.OnInspectorGUI ();

		ScaleAdjust script = (ScaleAdjust)target;
		if (GUILayout.Button ("SetHeight")) {
			script.SetHeight ();
		}

    }

}
#endif