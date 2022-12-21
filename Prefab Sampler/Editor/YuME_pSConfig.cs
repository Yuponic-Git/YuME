using UnityEngine;
using UnityEditor;

public class YuME_pSConfig : EditorWindow
{
    public static YuME_prefabSamplerData editorData;

    [MenuItem("Window/Yuponic/Utils/YuME: Prefab Sampler Config")]
    static void Initialize()
    {
        YuME_pSConfig editorConfig = EditorWindow.GetWindow<YuME_pSConfig>(true, "Prefab Sampler Config");
        editorConfig.titleContent.text = "Prefab Sampler Config";
    }

    void OnEnable()
    {
        editorData = ScriptableObject.CreateInstance<YuME_prefabSamplerData>();
        string[] guids = AssetDatabase.FindAssets("YuME_prefabSamplerSettings");

        if (guids.Length == 0)
        {
            // create a new setting file
            Debug.Log("Yuponic Prefab Sampler: No settings file found. Creating new settings file");
            editorData = ScriptableObject.CreateInstance<YuME_prefabSamplerData>();

            AssetDatabase.CreateAsset(editorData, "Assets/Yuponic/YuME_prefabSamplerSettings.asset");
            AssetDatabase.SaveAssets();

            editorData = AssetDatabase.LoadAssetAtPath("Assets/Yuponic/YuME_prefabSamplerSettings.asset", typeof(YuME_prefabSamplerData)) as YuME_prefabSamplerData;
        }
        else
        {
            // load the settings
            editorData = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guids[0]), typeof(YuME_prefabSamplerData)) as YuME_prefabSamplerData;
        }

        checkForValidSettings();
    }

    void checkForValidSettings()
    {
        if (editorData.yPivotTypes.Length < 3)
        {
            editorData.yPivotTypes = new string[3];
            editorData.yPivotTypes[0] = "Zero Y";
            editorData.yPivotTypes[1] = "Selection Ground";
            editorData.yPivotTypes[2] = "Selection Middle";
        }

        if(editorData.destinationFolder == "")
        {
            editorData.destinationFolder = "Assets/";
        }

        if (editorData.destinationFolder != "Assets/")
        {
            if (!AssetDatabase.IsValidFolder(editorData.destinationFolder))
            {
                editorData.destinationFolder = "Assets/";
            }
        }
    }

    void OnGUI()
    {
        EditorGUILayout.Space();
        
        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.LabelField("Select Prefab Destination Folder", EditorStyles.boldLabel);

        if (GUILayout.Button("Prefab Destination Folder", GUILayout.Height(30)))
        {
            editorData.destinationFolder = EditorUtility.OpenFolderPanel("Prefab Destination Folder", "", "");
            editorData.destinationFolder = YuTools_Utils.shortenAssetPath(editorData.destinationFolder);
            if (editorData.destinationFolder == "")
            {
                editorData.destinationFolder = "Assets/";
            }
        }

        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.LabelField("Prefab Folder:", EditorStyles.boldLabel);
        EditorGUILayout.LabelField(editorData.destinationFolder);

        EditorGUILayout.EndVertical();
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.LabelField("Prefab Pivot Setting:", EditorStyles.boldLabel);
        editorData.yPivotType = EditorGUILayout.Popup("Pivot Setting", editorData.yPivotType, editorData.yPivotTypes);
        EditorGUILayout.EndVertical();

        // ----------------------------------------------------------------------------------------------------
        // ------ Append Settings
        // ----------------------------------------------------------------------------------------------------

        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.LabelField("Custom Append Label", EditorStyles.boldLabel);

        EditorGUILayout.BeginVertical("box");

        editorData.appendName = EditorGUILayout.TextField("Append to Prefab Name", editorData.appendName);

        EditorGUILayout.EndVertical();
        EditorGUILayout.EndVertical();

        if (GUI.changed)
        {
            SceneView.RepaintAll();
        }

    }
}
