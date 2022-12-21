using UnityEngine;
using UnityEditor;

public class YuME_editorConfig : EditorWindow
{
    static float previousGlobalScale;

    static void Initialize()
    {
        YuME_editorConfig editorConfig = EditorWindow.GetWindow<YuME_editorConfig>(true, "Editor Config");
        editorConfig.titleContent.text = "Editor Config";
    }

    void OnEnable()
    {
        previousGlobalScale = YuME_mapEditor.editorPreferences.gridScaleFactor;
        checkForValidArrays();
    }

    void checkForValidArrays()
    {
        if(YuME_mapEditor.editorPreferences.layerNames.Count < 8)
        {
            YuME_mapEditor.editorPreferences.layerNames.Clear();
            for(int i = 1; i < 9; i++)
                YuME_mapEditor.editorPreferences.layerNames.Add("layer" + i);
        }

        if (YuME_mapEditor.editorPreferences.layerStatic.Count < 8)
        {
            YuME_mapEditor.editorPreferences.layerStatic.Clear();
            for (int i = 1; i < 9; i++)
                YuME_mapEditor.editorPreferences.layerStatic.Add(true);
        }

        if (YuME_mapEditor.editorPreferences.layerFreeze.Count < 8)
        {
            YuME_mapEditor.editorPreferences.layerFreeze.Clear();
            for (int i = 1; i < 9; i++)
                YuME_mapEditor.editorPreferences.layerFreeze.Add(true);
        }
    }

    Vector2 _scrollPosition;

    void OnGUI()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Editor Settings", EditorStyles.boldLabel);

        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.LabelField("Cursor Color Settings", EditorStyles.boldLabel);

        YuME_mapEditor.editorPreferences.brushCursorColor = EditorGUILayout.ColorField("Brush Cursor Color", YuME_mapEditor.editorPreferences.brushCursorColor);
        YuME_mapEditor.editorPreferences.pickCursorColor = EditorGUILayout.ColorField("Brush Picker Color", YuME_mapEditor.editorPreferences.pickCursorColor);
        YuME_mapEditor.editorPreferences.eraseCursorColor = EditorGUILayout.ColorField("Brush Erase Color", YuME_mapEditor.editorPreferences.eraseCursorColor);

        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.LabelField("Icon Size", EditorStyles.boldLabel);
        YuME_mapEditor.editorPreferences.iconWidth = (int)EditorGUILayout.Slider("Icon Size", YuME_mapEditor.editorPreferences.iconWidth, 16f, 64f);

        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.LabelField("Invert Mouse wheel scrolling", EditorStyles.boldLabel);
        YuME_mapEditor.editorPreferences.invertMouseWheel = EditorGUILayout.Toggle("Invert Mouse Wheel", YuME_mapEditor.editorPreferences.invertMouseWheel);

        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.LabelField("Centre Grid", EditorStyles.boldLabel);
        YuME_mapEditor.editorPreferences.centreGrid = EditorGUILayout.Toggle("Centre Grid", YuME_mapEditor.editorPreferences.centreGrid);

        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.LabelField("Global Scale Setting", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Warning. Changing the grid scale will have a knock on effect to all your maps. The default scale is 1. If you are seeing issues with your maps, please reset the scale.", MessageType.Warning);
        YuME_mapEditor.editorPreferences.gridScaleFactor = EditorGUILayout.Slider("Grid Size", YuME_mapEditor.editorPreferences.gridScaleFactor, 1f, 10f);
        YuME_mapEditor.editorPreferences.gridLayerHeightScaler = EditorGUILayout.FloatField("Grid Layer Height Scaler", YuME_mapEditor.editorPreferences.gridLayerHeightScaler);

        if (previousGlobalScale != YuME_mapEditor.editorPreferences.gridScaleFactor)
        {
            YuME_mapEditor.gridHeight = 0;
            YuME_mapEditor.editorPreferences.gridScaleFactor = YuME_mapEditor.editorPreferences.gridScaleFactor * 2;
            YuME_mapEditor.editorPreferences.gridScaleFactor = Mathf.Round(YuME_mapEditor.editorPreferences.gridScaleFactor) / 2;
            if (YuME_mapEditor.editorPreferences.gridScaleFactor < 1) { YuME_mapEditor.editorPreferences.gridScaleFactor = 1; }
            if (YuME_mapEditor.editorPreferences.gridScaleFactor > 10) { YuME_mapEditor.editorPreferences.gridScaleFactor = 10; }
            previousGlobalScale = YuME_mapEditor.editorPreferences.gridScaleFactor;
        }
        
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.LabelField("Grid Color Settings", EditorStyles.boldLabel);

        YuME_mapEditor.editorPreferences.gridColorNormal = EditorGUILayout.ColorField("Grid Color", YuME_mapEditor.editorPreferences.gridColorNormal);
        YuME_mapEditor.editorPreferences.gridColorFill = EditorGUILayout.ColorField("Fill Color", YuME_mapEditor.editorPreferences.gridColorFill);
        YuME_mapEditor.editorPreferences.gridColorBorder = EditorGUILayout.ColorField("Border Color", YuME_mapEditor.editorPreferences.gridColorBorder);

        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Grid Spawn Position", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        YuME_mapEditor.editorPreferences.initialOffset = EditorGUILayout.Vector3Field("", YuME_mapEditor.editorPreferences.initialOffset);
        YuME_mapEditor.editorPreferences.initialOffset.x = YuME_mapEditor.editorPreferences.initialOffset.x * 2;
        YuME_mapEditor.editorPreferences.initialOffset.x = Mathf.Round(YuME_mapEditor.editorPreferences.initialOffset.x) / 2;
        YuME_mapEditor.editorPreferences.initialOffset.y = YuME_mapEditor.editorPreferences.initialOffset.y * 2;
        YuME_mapEditor.editorPreferences.initialOffset.y = Mathf.Round(YuME_mapEditor.editorPreferences.initialOffset.y) / 2;
        YuME_mapEditor.editorPreferences.initialOffset.z = YuME_mapEditor.editorPreferences.initialOffset.z * 2;
        YuME_mapEditor.editorPreferences.initialOffset.z = Mathf.Round(YuME_mapEditor.editorPreferences.initialOffset.z) / 2;
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Grid Offset", EditorStyles.boldLabel);
        YuME_mapEditor.gridOffset = (float)EditorGUILayout.Slider("Grid Offset", YuME_mapEditor.gridOffset, -0.25f, 0.25f);
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Alternative shortcut keys (ASD -> GHJ). Use for Unreal style editor navigation", EditorStyles.boldLabel);
        YuME_mapEditor.editorPreferences.useAlternativeKeyShortcuts = EditorGUILayout.Toggle("Alternative Keys", YuME_mapEditor.editorPreferences.useAlternativeKeyShortcuts);
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Hide Unity UI objects while tool is enabled (EXPERIMENTAL!)", EditorStyles.boldLabel);
        YuME_mapEditor.editorPreferences.hideUIObjects = EditorGUILayout.Toggle("Hide Unity UI Objects", YuME_mapEditor.editorPreferences.hideUIObjects);
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Layer Name", EditorStyles.boldLabel, GUILayout.Width(125));
        EditorGUILayout.LabelField("Freeze", EditorStyles.boldLabel, GUILayout.Width(75));
        EditorGUILayout.LabelField("Static", EditorStyles.boldLabel, GUILayout.Width(75));
        EditorGUILayout.EndHorizontal();

        for (int i = 0; i < YuME_mapEditor.editorPreferences.layerNames.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            YuME_mapEditor.editorPreferences.layerNames[i] = EditorGUILayout.TextField(YuME_mapEditor.editorPreferences.layerNames[i], GUILayout.Width(125));
            YuME_mapEditor.editorPreferences.layerFreeze[i] = EditorGUILayout.Toggle(YuME_mapEditor.editorPreferences.layerFreeze[i], GUILayout.Width(75));
            YuME_mapEditor.editorPreferences.layerStatic[i] = EditorGUILayout.Toggle(YuME_mapEditor.editorPreferences.layerStatic[i], GUILayout.Width(75));
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndScrollView();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(YuME_mapEditor.editorPreferences);
            SceneView.RepaintAll();
        }
    }
}
