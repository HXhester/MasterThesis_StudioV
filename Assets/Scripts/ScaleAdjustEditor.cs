using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(ScaleAdjust))]
public class ScaleAdjustEditor : Editor
{
    private static float headScale = 1f;

	public override void OnInspectorGUI ()
	{
		base.OnInspectorGUI ();

		ScaleAdjust script = (ScaleAdjust)target;
		if (GUILayout.Button ("SetHeight")) {
			script.SetHeight ();
		}

	    headScale = EditorGUILayout.Slider("Set Head Scale", headScale, 0, 5);
        script.SetHeadScale(headScale);
    }

}
#endif