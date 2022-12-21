using System;
using System.IO;
using System.Reflection;
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
        try
        {
            string fullPath = Application.dataPath.Replace("/Assets", "") + "/" + path; // Build a true path for the system functions to read the directory contents
            string[] folderContents = Directory.GetFiles(fullPath, searchPattern: patternSearch); // Read the contents of the directory using the provided pattern search
            GameObject[] returnGameObjects = new GameObject[folderContents.Length]; // create a new array of GameObjects matching the directory contents

            for (int i = 0; i < folderContents.Length; i++)
            {
                int findAssetRoot = folderContents[i].IndexOf("Assets"); // Find the start of Unity's internal path for loading assets

                string loadPath = folderContents[i].Substring(findAssetRoot, folderContents[i].Length - findAssetRoot); // create a path that Unity can use
                returnGameObjects[i] = AssetDatabase.LoadAssetAtPath(loadPath, typeof(GameObject)) as GameObject; // store the loaded asset in the return array
            }

            return returnGameObjects;
        }
        catch
        {
            return null;
        }
    }

    public static string[] getDirectoryContents(string path, string patternSearch)
    {
        try
        {
            string fullPath = Application.dataPath.Replace("/Assets", "") + "/" + path; // Build a true path for the system functions to read the directory contents
            string[] folderContents = Directory.GetFiles(fullPath, searchPattern: patternSearch); // Read the contents of the directory using the provided pattern search
            
            for(int i = 0; i < folderContents.Length; i++)
            {
                folderContents[i] = folderContents[i].Replace(fullPath, "");
            }
            return folderContents;
        }
        catch
        {
            return null;
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
        string fullPath = Application.dataPath.Replace("/Assets", "") + "/" + path;
        string[] folderContents = Directory.GetFiles(fullPath, searchPattern: patternSearch);
        return folderContents.Length;
    }

    public static string getAssetPath(UnityEngine.Object sourceAsset)
    {
        string path = "";

        try
        {
            path = AssetDatabase.GetAssetPath(sourceAsset).Replace(sourceAsset.name + ".asset", "");
        }
        catch
        {
            path = "";
        }

        return path;
    }

    public static string getAssetPath(GameObject sourceAsset)
    {
        string path = AssetDatabase.GetAssetPath(sourceAsset).Replace(sourceAsset.name + ".asset", "");

        if (path != null)
		{
        	return path;
		}
		else
		{
			return null;
		}
    }

    public static string shortenAssetPath(string path)
    {
        if (!path.StartsWith("Assets/"))
        {
            try
            {
                path = path.Substring(path.IndexOf("Assets/"));
            }
            catch
            {
                path = "";
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
}
