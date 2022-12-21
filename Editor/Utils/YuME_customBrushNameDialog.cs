using UnityEngine;
using UnityEditor;
 
class YuME_customBrushNameDialog : EditorWindow
{
    string customBrushName;

    void OnGUI()
    {
        customBrushName = EditorGUILayout.TextField("Custom Brush Name", customBrushName);

        if (GUILayout.Button("Save Custom Brush"))
        {
            OnClickSavePrefab();
            GUIUtility.ExitGUI();
        }
    }

    void OnClickSavePrefab()
    {
        customBrushName = customBrushName.Trim();

        if (string.IsNullOrEmpty(customBrushName))
        {
            EditorUtility.DisplayDialog("Unable to save custom Brush", "Please specify a valid custom brush name.", "Close");
            return;
        }

        YuME_customBrushFunctions.createCustomBrush(customBrushName);

        Close();
    }
}