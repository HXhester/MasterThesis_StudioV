namespace Tobii.VR
{
    using UnityEngine;
    using UnityEditor;
    using System.IO;

    [InitializeOnLoad]
    public class TobiiVR_EulaCheck : EditorWindow
    {
        private const string _eulaUrl = "https://developer.tobii.com/license-agreement/";
        private Vector2 _scrollPosition;
        private static TobiiVR_EulaCheck _window;

        static TobiiVR_EulaCheck()
        {
            if (TobiiVR_EulaFile.Instance.IsEulaAccepted() == false)
            {
                EditorApplication.update += Update;
                EditorApplication.playmodeStateChanged += HandleOnPlayModeChanged;
            }
        }

        private static void HandleOnPlayModeChanged()
        {
            if (EditorApplication.isPlaying && TobiiVR_EulaFile.Instance.IsEulaAccepted() == false)
            {
                ShowWindow();
            }
        }

        private static void Update()
        {
            ShowWindow();
            EditorApplication.update -= Update;
        }

        private static void ShowWindow()
        {
            _window = GetWindow<TobiiVR_EulaCheck>(true);
            _window.titleContent = new GUIContent("Tobii EULA");
            _window.minSize = new Vector2(400, 290);
            _window.position = new Rect(100, 75, 400, 290);
        }

        public void OnGUI()
        {
            EditorGUILayout.BeginVertical(EditorStyles.label);

            var logo = AssetDatabase.LoadAssetAtPath<Texture2D>(GetResourcePath() + "Textures/tobii_logo.png");
            var rect = GUILayoutUtility.GetRect(position.width, 150, GUI.skin.box);
            if (logo)
            {
                GUI.DrawTexture(rect, logo, ScaleMode.ScaleToFit);
            }

            EditorGUILayout.HelpBox("To use this package please read and accept the EULA.", MessageType.Info);

            if (GUILayout.Button("Read the EULA"))
            {
                Application.OpenURL(_eulaUrl);
            }

            EditorGUILayout.LabelField("");
            EditorGUILayout.LabelField("");

            EditorGUILayout.BeginHorizontal(EditorStyles.label);

            if (GUILayout.Button("Accept", EditorStyles.miniButtonRight))
            {
                EditorApplication.playmodeStateChanged -= HandleOnPlayModeChanged;
                TobiiVR_EulaFile.Instance.SetEulaAccepted();
                _window.Close();
            }

            GUILayout.Button("", EditorStyles.miniBoldLabel);

            if (GUILayout.Button("Decline", EditorStyles.miniButtonLeft))
            {
                _window.Close();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        string GetResourcePath()
        {
            var ms = MonoScript.FromScriptableObject(this);
            var path = AssetDatabase.GetAssetPath(ms);
            path = Path.GetDirectoryName(path);
            return path.Substring(0, path.Length - "Editor".Length);
        }
    }
}