using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditorInternal;

class YuME_prefabSampler : EditorWindow
{
    List<GameObject> selected = new List<GameObject>();
    public static YuME_prefabSamplerData editorData;
    static bool openConfig = false;

    [MenuItem("Window/Yuponic/YuME: Prefab Sampler")]
    static void Initialize()
    {
        YuME_prefabSampler prefabSamplerWindow = EditorWindow.GetWindow<YuME_prefabSampler>(false, "YuME: Prefab Sampler");
        prefabSamplerWindow.titleContent.text = "YuME: Prefab Sampler";
    }

    void OnEnable()
    {
        editorData = ScriptableObject.CreateInstance<YuME_prefabSamplerData>();
        string[] guids = AssetDatabase.FindAssets("YuME_prefabSamplerSettings");
        editorData = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guids[0]), typeof(YuME_prefabSamplerData)) as YuME_prefabSamplerData;
    }

    void OnGUI()
    {
        EditorGUILayout.Space();

        if (editorData.destinationFolder != "" )
        {
            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("SAMPLE PREFAB", GUILayout.Height(30)))
            {
                if (selected.Count > 0)
                {
                    samplePrefab();
                }
            }
            openConfig = GUILayout.Toggle(openConfig, editorData.configButton, "Button", GUILayout.Width(30), GUILayout.Height(30));

            if (openConfig == true)
            {
                YuME_pSConfig editorConfig = EditorWindow.GetWindow<YuME_pSConfig>(true, "Prefab Sampler Config");
                editorConfig.titleContent.text = "Prefab Sampler Config";
            }

            openConfig = false;
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.LabelField("Prefab Folder:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(editorData.destinationFolder);

            EditorGUILayout.EndVertical();
        }
        else
        {
            EditorGUILayout.HelpBox("Set a destination directory to create prefabs", MessageType.Warning);
        }
    }

    void OnSelectionChange()
    {
        selected.Clear();

        foreach (GameObject selection in Selection.gameObjects)
        {
            selected.Add(selection);
        }
    }

    void samplePrefab()
    {
        GameObject prefabParent = new GameObject("Parent Object");
        var myBounds = InternalEditorUtility.CalculateSelectionBounds(false, false);
        Vector3 centerPoint = myBounds.center;

        switch(editorData.yPivotType)
        {
            case 0:
                centerPoint.y = 0;
                break;

            case 1:
                centerPoint.y = myBounds.min.y;
                break;

            case 2:
                centerPoint.y = myBounds.center.y;
                break;

            default:
                centerPoint.y = 0;
                break;
        }

        prefabParent.transform.position = centerPoint;

        foreach(GameObject copyObject in selected)
        {
            GameObject copy = Instantiate(copyObject) as GameObject;
            copy.transform.position = copyObject.transform.position;
            copy.transform.eulerAngles = copyObject.transform.eulerAngles;
            copy.transform.parent = prefabParent.transform;
        }

        prefabParent.name = editorData.appendName + selected[0].name;
        prefabParent.transform.position = Vector3.zero;

#if UNITY_2018_3_OR_NEWER
        PrefabUtility.SaveAsPrefabAsset(prefabParent, appendNumber(editorData.destinationFolder + "/" + prefabParent.name));
#else
        PrefabUtility.CreatePrefab(appendNumber(editorData.destinationFolder + "/" + prefabParent.name), prefabParent);
#endif

        DestroyImmediate(prefabParent);
    }

    string appendNumber(string asset)
    {
        int uniqueNumber = 0;
        bool foundUnique = false;
        while (!foundUnique)
        {
            Object prefabAlreadyCreated = AssetDatabase.LoadAssetAtPath(asset+uniqueNumber.ToString()+".prefab", typeof(GameObject));
            if (prefabAlreadyCreated == null)
            {
                foundUnique = true;
            }
            else
            {
                uniqueNumber++;
            }
        }

        return asset + uniqueNumber.ToString() + ".prefab";
    }
}
