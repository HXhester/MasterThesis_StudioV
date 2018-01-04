// This class is Auto-Generated

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Assets.Editor
{
    public static class GeneratedSceneMenuItems {

        [MenuItem("OpenScene/Complete/SchoolGymDay")]
        private static void OpenSceneSchoolGymDay() {
            Debug.Log("Selected item: SchoolGymDay");
            OpenCompletedScene("SchoolGymDay");
        }


        [MenuItem("OpenScene/Complete/Tropical")]
        private static void OpenSceneTropical() {
            Debug.Log("Selected item: Tropical");
            OpenCompletedScene("Tropical");
        }

        [MenuItem("OpenScene/Launcher")]
        private static void OpenSceneLauncher() {
            Debug.Log("Selected item: Launcher");
            OpenUnfinishedScene("Launcher");
        }


        static void OpenCompletedScene(string scene){
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) {
                EditorSceneManager.OpenScene("Assets/Scene/Complete/" + scene + ".unity");
            }
        }
        static void OpenUnfinishedScene(string scene){
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) {
                EditorSceneManager.OpenScene("Assets/Scene/" + scene + ".unity");
            }
        }
    }
}
