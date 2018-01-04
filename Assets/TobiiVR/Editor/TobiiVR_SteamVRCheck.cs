// Copyright © 2015 – Property of Tobii AB (publ) - All Rights Reserved

namespace Tobii.VR
{
    using System.Reflection;
    using UnityEditor;
    using UnityEngine;

    [InitializeOnLoad]
    public class TobiiVR_SteamVRCheck : EditorWindow
    {
        private const string SteamVrPluginUrl = "http://u3d.as/content/valve-corporation/steam-vr-plugin";
        private const string IgnoreSteamVrMissing = "TobiiVR.IgnoreSteamVrMissing";

        static TobiiVR_SteamVRCheck()
        {
            EditorApplication.update += Update;
        }

        private static void Update()
        {
            var val = EditorPrefs.GetBool(IgnoreSteamVrMissing, false);
            if (!val && !SteamVRExists())
            {
                var window = GetWindow<TobiiVR_SteamVRCheck>(true);
                window.titleContent = new GUIContent("Tobii VR could not find SteamVR");
                window.minSize = new Vector2(400, 110);
                window.position = new Rect(100, 500, 400, 110);
            }

            EditorApplication.update -= Update;
        }

        private static bool SteamVRExists()
        {
            Assembly _assembly = Assembly.Load("Assembly-CSharp");
            var steamVR = _assembly.GetType("SteamVR");
            return steamVR != null;
        }

        bool _toggleState = false;

        public void OnGUI()
        {
            EditorGUILayout.BeginVertical(EditorStyles.label);

            EditorGUILayout.HelpBox("Some aspects of TobiiVR requires SteamVR to work properly.", MessageType.Warning);

            if (GUILayout.Button("Import SteamVR"))
            {
                Application.OpenURL(SteamVrPluginUrl);
            }

            EditorGUILayout.LabelField("");

            EditorGUI.BeginChangeCheck();
            var doNotShow = GUILayout.Toggle(_toggleState, "Do not check for SteamVR again.");
            if (EditorGUI.EndChangeCheck())
            {
                _toggleState = doNotShow;
                if (_toggleState)
                {
                    EditorPrefs.SetBool(IgnoreSteamVrMissing, true);
                }
                else
                {
                    EditorPrefs.DeleteKey(IgnoreSteamVrMissing);
                }
            }

            EditorGUILayout.EndVertical();
        }
    }
}