using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class YuTools_Utils : EditorWindow
{
    // ----------------------------------------------------------------------------------------------------
    // ----- Hidden Editor Functionality
    // ----------------------------------------------------------------------------------------------------

    public static void showUnityGrid(bool show)  // based on a solution found in Unity Answers
    {
        Assembly editorAssembly = Assembly.GetAssembly(typeof(Editor));
        Type annotationUtility = editorAssembly.GetType("UnityEditor.AnnotationUtility");
        var property = annotationUtility.GetProperty("showGrid", BindingFlags.Static | BindingFlags.NonPublic);
        property.SetValue(null, show, null);
    }

    public static void disableTileGizmo(bool show) // based on a solution found in Unity Answers
    {
        var Annotation = Type.GetType("UnityEditor.Annotation, UnityEditor");
        var ClassId = Annotation.GetField("classID");
        var ScriptClass = Annotation.GetField("scriptClass");

        Type AnnotationUtility = Type.GetType("UnityEditor.AnnotationUtility, UnityEditor");
        var GetAnnotations = AnnotationUtility.GetMethod("GetAnnotations", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
        var SetGizmoEnabled = AnnotationUtility.GetMethod("SetGizmoEnabled", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);

        Array annotations = (Array)GetAnnotations.Invoke(null, null);

        foreach (var a in annotations)
        {
            int classId = (int)ClassId.GetValue(a);
            string scriptClass = (string)ScriptClass.GetValue(a);

            if (scriptClass == "YuME_tileGizmo")
            {
#if UNITY_2019_1_OR_NEWER
                SetGizmoEnabled.Invoke(null, new object[] { classId, scriptClass, Convert.ToInt32(show), false });
#else
                SetGizmoEnabled.Invoke(null, new object[] { classId, scriptClass, Convert.ToInt32(show)});
#endif
            }
        }
    }
    
    // ----------------------------------------------------------------------------------------------------
    // ----- File System Helpers
    // ----------------------------------------------------------------------------------------------------

    public static GameObject[] loadDirectoryContents(string path, string patternSearch)
    {
        if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(patternSearch))
        {
            Debug.LogWarning("YuME: Invalid path or pattern search provided to loadDirectoryContents");
            return new GameObject[0];
        }

        try
        {
            string fullPath = Application.dataPath.Replace("/Assets", "") + "/" + path; // Build a true path for the system functions to read the directory contents
            
            if (!Directory.Exists(fullPath))
            {
                Debug.LogWarning($"YuME: Directory does not exist: {fullPath}");
                return new GameObject[0];
            }

            string[] folderContents = Directory.GetFiles(fullPath, searchPattern: patternSearch); // Read the contents of the directory using the provided pattern search
            GameObject[] returnGameObjects = new GameObject[folderContents.Length]; // create a new array of GameObjects matching the directory contents

            for (int i = 0; i < folderContents.Length; i++)
            {
                int findAssetRoot = folderContents[i].IndexOf("Assets"); // Find the start of Unity's internal path for loading assets

                if (findAssetRoot >= 0)
                {
                    string loadPath = folderContents[i].Substring(findAssetRoot, folderContents[i].Length - findAssetRoot); // create a path that Unity can use
                    returnGameObjects[i] = AssetDatabase.LoadAssetAtPath(loadPath, typeof(GameObject)) as GameObject; // store the loaded asset in the return array
                }
                else
                {
                    Debug.LogWarning($"YuME: Invalid asset path found: {folderContents[i]}");
                }
            }

            return returnGameObjects;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"YuME: Failed to load directory contents: {e.Message}");
            return new GameObject[0];
        }
    }

    public static string[] getDirectoryContents(string path, string patternSearch)
    {
        if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(patternSearch))
        {
            Debug.LogWarning("YuME: Invalid path or pattern search provided to getDirectoryContents");
            return new string[0];
        }

        try
        {
            string fullPath = Application.dataPath.Replace("/Assets", "") + "/" + path; // Build a true path for the system functions to read the directory contents
            
            if (!Directory.Exists(fullPath))
            {
                Debug.LogWarning($"YuME: Directory does not exist: {fullPath}");
                return new string[0];
            }

            string[] folderContents = Directory.GetFiles(fullPath, searchPattern: patternSearch); // Read the contents of the directory using the provided pattern search
            
            for(int i = 0; i < folderContents.Length; i++)
            {
                folderContents[i] = folderContents[i].Replace(fullPath, "");
            }
            return folderContents;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"YuME: Failed to get directory contents: {e.Message}");
            return new string[0];
        }
    }

    public static string[] getFullPathFolderContents(string path, string patternSearch)
    {
        string fullPath = Application.dataPath.Replace("/Assets", "") + "/" + path;
        string[] folderContents = Directory.GetFiles(fullPath, searchPattern: patternSearch);

        return folderContents;
    }

    public static int numberOfFilesInFolder(string path, string patternSearch) // returns an array of files contained in a folder
    {
        if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(patternSearch))
        {
            Debug.LogWarning("YuME: Invalid path or pattern search provided to numberOfFilesInFolder");
            return 0;
        }

        try
        {
            string fullPath = Application.dataPath.Replace("/Assets", "") + "/" + path;
            
            if (!Directory.Exists(fullPath))
            {
                Debug.LogWarning($"YuME: Directory does not exist: {fullPath}");
                return 0;
            }

            string[] folderContents = Directory.GetFiles(fullPath, searchPattern: patternSearch);
            return folderContents.Length;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"YuME: Failed to count files in folder: {e.Message}");
            return 0;
        }
    }

    public static string getAssetPath(UnityEngine.Object sourceAsset)
    {
        if (sourceAsset == null)
        {
            Debug.LogWarning("YuME: Cannot get asset path for null object");
            return string.Empty;
        }

        try
        {
            string path = AssetDatabase.GetAssetPath(sourceAsset);
            if (!string.IsNullOrEmpty(path) && !string.IsNullOrEmpty(sourceAsset.name))
            {
                return path.Replace(sourceAsset.name + ".asset", "");
            }
            return string.Empty;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"YuME: Failed to get asset path: {e.Message}");
            return string.Empty;
        }
    }

    public static string getAssetPath(GameObject sourceAsset)
    {
        if (sourceAsset == null)
        {
            Debug.LogWarning("YuME: Cannot get asset path for null GameObject");
            return null;
        }

        try
        {
            string path = AssetDatabase.GetAssetPath(sourceAsset);
            if (!string.IsNullOrEmpty(path) && !string.IsNullOrEmpty(sourceAsset.name))
            {
                path = path.Replace(sourceAsset.name + ".asset", "");
                return !string.IsNullOrEmpty(path) ? path : null;
            }
            return null;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"YuME: Failed to get GameObject asset path: {e.Message}");
            return null;
        }
    }

    public static string shortenAssetPath(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return "Assets/";
        }
        
        if (!path.StartsWith("Assets/"))
        {
            try
            {
                int assetsIndex = path.IndexOf("Assets/");
                if (assetsIndex >= 0)
                {
                    path = path.Substring(assetsIndex);
                }
                else
                {
                    path = "Assets/";
                }
            }
            catch
            {
                path = "Assets/";
            }
        }

        return path;
    }

    public static string stripAssetPath(string path)
    {
        if (path.StartsWith("Assets/"))
        {
            try
            {
                path = path.Replace("Assets/","");
            }
            catch
            {
                path = "";
            }
        }

        return path;
    }

    public static string removeLastFolderSlash(string path)
    {
        return path.Substring(0, path.Length - 1);
    }


    // ----------------------------------------------------------------------------------------------------
    // ----- Editor Tag and Layer Helpers
    // ----------------------------------------------------------------------------------------------------

    public static void addLayer(string layerName)
    {
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty layersProp = tagManager.FindProperty("layers");

        for (int i = 8; i < layersProp.arraySize; i++)
        {
            SerializedProperty t = layersProp.GetArrayElementAtIndex(i);
            if (t.stringValue == layerName)
            {
                return;
            }
        }

        for (int i = 8; i < layersProp.arraySize; i++)
        {
            SerializedProperty t = layersProp.GetArrayElementAtIndex(i);
            if (t.stringValue == "")
            {
                SerializedProperty sp = layersProp.GetArrayElementAtIndex(i);
                if (sp != null) sp.stringValue = layerName;
                tagManager.ApplyModifiedProperties();
                return;
            }
        }
    }

    public static void addTag(string tagName)
    {
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProp = tagManager.FindProperty("tags");

        for (int i = 0; i < tagsProp.arraySize; i++)
        {
            SerializedProperty t = tagsProp.GetArrayElementAtIndex(i);
            if (t.stringValue.Equals(tagName))
            {
                return;
            }
        }

        tagsProp.InsertArrayElementAtIndex(0);
        SerializedProperty n = tagsProp.GetArrayElementAtIndex(0);
        n.stringValue = tagName;

        tagManager.ApplyModifiedProperties();
    }

    public static void ensureFolderExists(string folderPath)
    {
        if (string.IsNullOrEmpty(folderPath) || !folderPath.StartsWith("Assets/"))
        {
            return;
        }
        
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            string[] folders = folderPath.Split('/');
            string currentPath = "";
            for (int i = 0; i < folders.Length; i++)
            {
                if (i == 0)
                {
                    currentPath = folders[i];
                    continue;
                }
                
                string parentPath = currentPath;
                currentPath += "/" + folders[i];
                
                if (!AssetDatabase.IsValidFolder(currentPath))
                {
                    AssetDatabase.CreateFolder(parentPath, folders[i]);
                }
            }
        }
    }

    // ----------------------------------------------------------------------------------------------------
    // ----- Validation Helpers
    // ----------------------------------------------------------------------------------------------------
    
    public static bool isValidIndex<T>(T[] array, int index)
    {
        return array != null && index >= 0 && index < array.Length;
    }
    
    public static bool isValidIndex<T>(List<T> list, int index)
    {
        return list != null && index >= 0 && index < list.Count;
    }
    
    public static bool isValidGameObject(GameObject obj)
    {
        return obj != null && !obj.Equals(null);
    }
    
    public static bool isValidComponent<T>(T component) where T : Component
    {
        return component != null && !component.Equals(null);
    }
}
