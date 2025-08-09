using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YuME_mapManagerFunctions
{
    public static void setDefaultMap()
    {
        GameObject mainMap = GameObject.Find("YuME_MapData");

        if (mainMap == null)
        {
            Debug.LogWarning("YuME cannot find the default map. Please ensure you have a YuME Map named YuME_MapData");
        }
        else
        {
            try
            {
                getGridSceneObjectReference(mainMap);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"YuME: Failed to set default map: {e.Message}");
            }
        }
    }

    public static void setActiveMap()
    {
        if (YuME_mapEditor.ref_MapManager == null)
        {
            Debug.LogWarning("YuME: Cannot set active map - map manager reference is null");
            return;
        }

        if (YuME_mapEditor.ref_MapManager.mapList == null)
        {
            Debug.LogWarning("YuME: Cannot set active map - map list is null");
            return;
        }

        try
        {
            for (int i = 0; i < YuME_mapEditor.ref_MapManager.mapList.Count; i++)
            {
                if (YuME_mapEditor.ref_MapManager.mapList[i] != null)
                {
                    YuME_mapEditor.ref_MapManager.mapList[i].SetActive(false);
                }
            }

            if (YuME_mapEditor.currentMapIndex >= 0 && YuME_mapEditor.currentMapIndex < YuME_mapEditor.ref_MapManager.mapList.Count)
            {
                var activeMap = YuME_mapEditor.ref_MapManager.mapList[YuME_mapEditor.currentMapIndex];
                if (activeMap != null)
                {
                    activeMap.SetActive(true);
                    YuME_brushFunctions.updateBrushTile();
                }
                else
                {
                    Debug.LogWarning($"YuME: Map at index {YuME_mapEditor.currentMapIndex} is null");
                }
            }
            else
            {
                Debug.LogWarning($"YuME: Invalid map index {YuME_mapEditor.currentMapIndex}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"YuME: Failed to set active map: {e.Message}");
        }
    }

    public static GameObject buildNewMap(string mapName)
    {
        GameObject mainMap = new GameObject(mapName);

        for (int i = 1; i < 9; i++)
        {
            GameObject layer = new GameObject("layer" + i);
            layer.transform.parent = mainMap.transform;
            layer.transform.position = Vector3.zero;
        }

        getGridSceneObjectReference(mainMap);
        return mainMap;
    }

    public static void getGridSceneObjectReference()
    {
        if (YuME_mapEditor.gridSceneObject == null)
        {
            YuME_mapEditor.gridSceneObject = GameObject.Find("YuME_MapEditorObject");
        }

        if (YuME_mapEditor.gridSceneObject != null)
        {
            YuME_mapEditor.ref_MapManager = YuME_mapEditor.gridSceneObject.GetComponent<YuME_MapManager>();
        }
        else
        {
            Debug.Log("Unable to find a reference to the map manager.");
        }
    }

    public static void getGridSceneObjectReference(string mapName)
    {
        if (YuME_mapEditor.gridSceneObject == null)
        {
            YuME_mapEditor.gridSceneObject = GameObject.Find("YuME_MapEditorObject");
        }

        if (YuME_mapEditor.gridSceneObject != null)
        {
            YuME_mapEditor.ref_MapManager = YuME_mapEditor.gridSceneObject.GetComponent<YuME_MapManager>();

            if (YuME_mapEditor.ref_MapManager != null)
            {
                GameObject mainMap = new GameObject(mapName);

                if (mainMap != null)
                {
                    YuME_mapEditor.ref_MapManager.mapList.Add(mainMap);
                    setActiveMap();
                }
                else
                {
                    Debug.Log("Unable to find a reference to the selected map.");
                }
            }
            else
            {
                Debug.Log("Unable to find a reference to the map manager.");
            }
        }
        else
        {
            Debug.Log("Unable to find a reference to the YuME_MapEditorObject");
        }
    }

    public static void getGridSceneObjectReference(GameObject map)
    {
        if (YuME_mapEditor.gridSceneObject == null)
        {
            YuME_mapEditor.gridSceneObject = GameObject.Find("YuME_MapEditorObject");
        }

        if (YuME_mapEditor.gridSceneObject != null)
        {
            YuME_mapEditor.ref_MapManager = YuME_mapEditor.gridSceneObject.GetComponent<YuME_MapManager>();

            if (YuME_mapEditor.ref_MapManager != null)
            {
                if (map != null)
                {
                    YuME_mapEditor.ref_MapManager.mapList.Add(map);
                    setActiveMap();
                }
                else
                {
                    Debug.Log("Unable to find a reference to the selected map.");
                }
            }
            else
            {
                Debug.Log("Unable to find a reference to the map manager.");
            }
        }
        else
        {
            Debug.Log("Unable to find a reference to the YuME_MapEditorObject");
        }
    }

    public static void refreshMap()
    {
        Vector3 tempVec3;
        if(YuME_mapEditor.findTileMapParent())
        {
            foreach(Transform layer in YuME_mapEditor.tileMapParent.transform)
            {
                foreach(Transform tile in layer)
                {
                    tempVec3 = tile.localScale;

                    if(tempVec3.x != YuME_mapEditor.editorPreferences.gridScaleFactor)
                    {
                        if(tempVec3.x < 0)
                            tempVec3.x = YuME_mapEditor.editorPreferences.gridScaleFactor * -1;
                        else
                            tempVec3.x = YuME_mapEditor.editorPreferences.gridScaleFactor;
                    }

                    if (tempVec3.y != YuME_mapEditor.editorPreferences.gridScaleFactor)
                    {
                        if (tempVec3.y < 0)
                            tempVec3.y = YuME_mapEditor.editorPreferences.gridScaleFactor * -1;
                        else
                            tempVec3.y = YuME_mapEditor.editorPreferences.gridScaleFactor;
                    }

                    if (tempVec3.z != YuME_mapEditor.editorPreferences.gridScaleFactor)
                    {
                        if (tempVec3.z < 0)
                            tempVec3.z = YuME_mapEditor.editorPreferences.gridScaleFactor * -1;
                        else
                            tempVec3.z = YuME_mapEditor.editorPreferences.gridScaleFactor;
                    }

                    tile.localScale = tempVec3;
                }
            }
        }
    }
}
