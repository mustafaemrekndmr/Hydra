using UnityEngine;
using UnityEditor;
using System.Linq;

/// <summary>
/// Editor utility to find and remove all missing (broken) script components
/// from every GameObject in the currently open scene.
/// </summary>
public class MissingScriptCleaner : EditorWindow
{
    [MenuItem("Tools/Clean Missing Scripts")]
    static void CleanMissingScripts()
    {
        int totalRemoved = 0;
        
        // Get ALL objects, including inactive ones
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>()
            .Where(go => go.scene.isLoaded)
            .ToArray();
        
        foreach (GameObject go in allObjects)
        {
            int count = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(go);
            if (count > 0)
            {
                Debug.Log($"Removing {count} missing script(s) from: {GetFullPath(go)}");
                Undo.RegisterCompleteObjectUndo(go, "Remove Missing Scripts");
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
                EditorUtility.SetDirty(go);
                totalRemoved += count;
            }
        }
        
        if (totalRemoved > 0)
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
            Debug.Log($"<color=green>Cleaned {totalRemoved} missing script(s) total.</color>");
        }
        else
        {
            Debug.Log("<color=cyan>No missing scripts found.</color>");
        }
    }
    
    static string GetFullPath(GameObject go)
    {
        string path = go.name;
        Transform parent = go.transform.parent;
        while (parent != null)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }
        return path;
    }
}
