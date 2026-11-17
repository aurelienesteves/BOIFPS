using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace BOIFPS
{
    public static class DungeonVerification
    {
        public static void Run()
        {
            if(!Application.isBatchMode) { Debug.LogError("Run verification in a separate batch editor."); return; }
            EditorSceneManager.OpenScene("Assets/BOIFPS/Scenes/FirstDungeon.unity");
            SessionState.SetBool("BOIFPS.DepthCheck",true);
            EditorApplication.EnterPlaymode();
        }
        public static void Builds()
        {
            SessionState.SetBool("BOIFPS.BuildCheck",true); Run();
        }
        public static void Smoke()
        {
            SessionState.SetBool("BOIFPS.SmokeCheck",true); Run();
        }
        [InitializeOnLoadMethod]
        static void Resume()
        {
            EditorApplication.playModeStateChanged+=state=>
            {
                if(state==PlayModeStateChange.EnteredPlayMode && SessionState.GetBool("BOIFPS.DepthCheck",false))
                {
                    SessionState.SetBool("BOIFPS.DepthCheck",false);
                    if(SessionState.GetBool("BOIFPS.SmokeCheck",false))
                    {
                        SessionState.SetBool("BOIFPS.SmokeCheck",false);
                        new GameObject("Smoke verification").AddComponent<DungeonSmokeCheck>();
                        double started=EditorApplication.timeSinceStartup;
                        var reportStarted=System.DateTime.UtcNow;
                        EditorApplication.update+=()=>
                        {
                            const string report="Artifacts/smoke-check.txt";
                            if(System.IO.File.Exists(report) && System.IO.File.GetLastWriteTimeUtc(report)>=reportStarted) EditorApplication.Exit(System.IO.File.ReadAllText(report).Contains("FAIL")?1:0);
                            else if(EditorApplication.timeSinceStartup-started>120) EditorApplication.Exit(2);
                        };
                    }
                    else if(SessionState.GetBool("BOIFPS.BuildCheck",false))
                    {
                        SessionState.SetBool("BOIFPS.BuildCheck",false);
                        new GameObject("Build verification").AddComponent<DungeonBuildCheck>();
                    }
                    else new GameObject("Depth verification").AddComponent<DungeonDepthCheck>();
                }
            };
        }
    }
}
