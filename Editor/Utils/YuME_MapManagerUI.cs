using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class YuME_MapManagerUI : EditorWindow
{
    Vector2 _scrollPosition;
    string newMapName;

    static void Initialize()
    {
        YuME_MapManagerUI mapManagerUI = EditorWindow.GetWindow<YuME_MapManagerUI>(true, "Map Manager");
        mapManagerUI.titleContent.text = "Map Manager";
    }

    void OnGUI()
    {
        EditorGUILayout.Space();
        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.LabelField("New Map Name", EditorStyles.boldLabel);
        newMapName = EditorGUILayout.TextField(newMapName);

        if (GUILayout.Button("Add New Map", GUILayout.Height(20)))
        {
            YuME_mapManagerFunctions.buildNewMap(newMapName);
        }

        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical("box");

        for (int i = 0; i < YuME_mapEditor.ref_MapManager.mapList.Count; i++)
        {
            if (YuME_mapEditor.ref_MapManager.mapList[i] != null)
            {
                EditorGUILayout.BeginHorizontal();
                YuME_mapEditor.ref_MapManager.mapList[i].name = EditorGUILayout.TextField(YuME_mapEditor.ref_MapManager.mapList[i].name, GUILayout.Width(200));

                if (i != 0)
                {
                    if (GUILayout.Button("Clone Map", GUILayout.Height(20), GUILayout.Width(75)))
                    {
                        YuME_mapEditor.cloneMap(YuME_mapEditor.ref_MapManager.mapList[i]);
                        EditorSceneManager.MarkAllScenesDirty();
                    }
                    if (GUILayout.Button("Delete Map", GUILayout.Height(20), GUILayout.Width(75)))
                    {
                        DestroyImmediate(YuME_mapEditor.ref_MapManager.mapList[i]);
                        YuME_mapEditor.ref_MapManager.mapList.RemoveAt(i);
                        YuME_mapEditor.currentMapIndex = 0;
                        EditorSceneManager.MarkAllScenesDirty();
                    }
                }
                else
                {
                    if (GUILayout.Button("Clone Map", GUILayout.Height(20), GUILayout.Width(154)))
                    {
                        YuME_mapEditor.cloneMap(YuME_mapEditor.ref_MapManager.mapList[i]);
                        EditorSceneManager.MarkAllScenesDirty();
                    }
                }

                EditorGUILayout.EndHorizontal();
            }
        }
        
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.EndScrollView();
        
    }
}
