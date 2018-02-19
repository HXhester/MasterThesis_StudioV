// This class is Auto-Generated
using UnityEngine;
using UnityEditor;

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

    [MenuItem("OpenScene/NatnetSampleTest")]
    private static void OpenSceneNatnetSampleTest() {
        Debug.Log("Selected item: NatnetSampleTest");
        OpenUnfinishedScene("NatnetSampleTest");
    }

static void OpenCompletedScene(string scene){
    if (EditorApplication.SaveCurrentSceneIfUserWantsTo ()) {
		EditorApplication.OpenScene("Assets/Scene/Complete/" + scene + ".unity");
   }
}
static void OpenUnfinishedScene(string scene){
    if (EditorApplication.SaveCurrentSceneIfUserWantsTo ()) {
		EditorApplication.OpenScene("Assets/Scene/" + scene + ".unity");
   }
}
}
