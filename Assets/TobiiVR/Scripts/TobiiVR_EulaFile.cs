namespace Tobii.VR
{
    using UnityEngine;
    using System.IO;
#if UNITY_EDITOR
    using UnityEditor;
#endif

    public class TobiiVR_EulaFile : ScriptableObject
    {
#if UNITY_EDITOR
#else
        private static readonly string ResourcePath = "EULA_accepted";
#endif

        private static TobiiVR_EulaFile _instance;
        public static TobiiVR_EulaFile Instance
        {
            get
            {
                if (_instance != null)
                {
                    return _instance;
                }

                _instance = CreateInstance<TobiiVR_EulaFile>();
                return _instance;
            }
        }

        public bool IsEulaAccepted()
        {
#if UNITY_EDITOR
            var resourcePath = GetResourcePath() + "Resources/";
            if (Directory.Exists(resourcePath) == false)
            {
                Directory.CreateDirectory(resourcePath);
            }

            return File.Exists(resourcePath + "EULA_accepted.json");
#else
            TextAsset settings = Resources.Load<TextAsset>(ResourcePath);

            if (null == settings)
            {
                return false;
            }
            return true;
#endif
        }

#if UNITY_EDITOR
        public void SetEulaAccepted()
        {
            var resourcePath = GetResourcePath() + "Resources/";
            if (Directory.Exists(resourcePath) == false)
            {
                Directory.CreateDirectory(resourcePath);
            }

            File.WriteAllText(resourcePath + "EULA_accepted.json", "1");
        }

        string GetResourcePath()
        {
            var ms = MonoScript.FromScriptableObject(this);
            var path = AssetDatabase.GetAssetPath(ms);
            path = Path.GetDirectoryName(path);
            return path.Substring(0, path.Length - "Scripts".Length);
        }
#endif
    }
}