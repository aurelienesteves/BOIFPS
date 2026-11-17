using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace BOIFPS.Editor
{
    public static class DungeonSetup
    {
        [MenuItem("BOIFPS/Créer ou ouvrir le donjon")]
        public static void CreateScene()
        {
            const string path = "Assets/BOIFPS/Scenes/FirstDungeon.unity";
            if (System.IO.File.Exists(path)) { EditorSceneManager.OpenScene(path); return; }
            System.IO.Directory.CreateDirectory("Assets/BOIFPS/Scenes");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var game = new GameObject("First Dungeon").AddComponent<DungeonGame>();
            var serialized = new SerializedObject(game); serialized.FindProperty("dungeonShader").objectReferenceValue = Shader.Find("Universal Render Pipeline/Lit"); serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorSceneManager.SaveScene(scene, path);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(path, true) };
            AssetDatabase.SaveAssets();
            Debug.Log("BOIFPS prêt : ouvrir FirstDungeon et lancer Play.");
        }
    }
}
