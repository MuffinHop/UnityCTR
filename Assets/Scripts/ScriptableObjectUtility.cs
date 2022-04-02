using UnityEngine;
using UnityEditor;
using System.IO;
 
public static class ScriptableObjectUtility
{
    /// <summary>
    //  This makes it easy to create, name and place unique new ScriptableObject asset files.
    /// </summary>
    public static void CreateAsset<T> (string path) where T : ScriptableObject
    {
        T asset = ScriptableObject.CreateInstance<T> ();
        string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath (path);
 
        AssetDatabase.CreateAsset (asset, assetPathAndName);
 
        AssetDatabase.SaveAssets ();
        AssetDatabase.Refresh();
        EditorUtility.FocusProjectWindow ();
        Selection.activeObject = asset;
 
    }
 
    public static void OnDestroy()
    {
 
        Debug.Log("SOU on Destory");
        AssetDatabase.SaveAssets();
 
    }
 
    public static void OnApplicationQuit()
    {
        Debug.Log("SOU on Quit");
        AssetDatabase.SaveAssets();
    }
}