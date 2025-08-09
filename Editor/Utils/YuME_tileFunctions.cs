using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using System.Linq;

public class YuME_tileFunctions : EditorWindow 
{
	// ----------------------------------------------------------------------------------------------------
	// ----- Scene Editor Tools
	// ----------------------------------------------------------------------------------------------------

	public static void addTile(Vector3 position)
	{
		GameObject placedTile;

		if (YuME_mapEditor.findTileMapParent())
		{
            if (YuME_mapEditor.useAltTiles == true)
            {
                YuME_mapEditor.s_AltTiles checkAltTiles = new YuME_mapEditor.s_AltTiles();

                try
                {
                    checkAltTiles = YuME_mapEditor.altTiles.Single(s => s.masterTile == YuME_mapEditor.currentTile.name);
                }
                catch
                {
                }

                if (checkAltTiles.masterTile != null)
                {
                    int randomTile = UnityEngine.Random.Range(0, checkAltTiles.altTileObjects.Length + 1);
                    if (randomTile < checkAltTiles.altTileObjects.Length)
                    {
                        placedTile = PrefabUtility.InstantiatePrefab(checkAltTiles.altTileObjects[randomTile] as GameObject) as GameObject;
                    }
                    else
                    {
                        placedTile = PrefabUtility.InstantiatePrefab(YuME_mapEditor.currentTile as GameObject) as GameObject;
                    }
                }
                else
                {
                    placedTile = PrefabUtility.InstantiatePrefab(YuME_mapEditor.currentTile as GameObject) as GameObject;
                }
            }
            else
            {
                placedTile = PrefabUtility.InstantiatePrefab(YuME_mapEditor.currentTile as GameObject) as GameObject;
            }

            Undo.RegisterCreatedObjectUndo(placedTile, "Placed Tile");
            if(YuME_mapEditor.randomRotationMode)
            {
                placedTile.transform.eulerAngles = new Vector3(UnityEngine.Random.Range(0,4) * 90f, UnityEngine.Random.Range(0, 4) * 90f, UnityEngine.Random.Range(0, 4) * 90f);
            }
            else
            {
                placedTile.transform.eulerAngles = new Vector3(YuME_mapEditor.tileRotationX, YuME_mapEditor.tileRotation, 0f);
            }

            placedTile.transform.position = position;
            placedTile.transform.localScale = YuME_mapEditor.brushTile.transform.localScale;
			placedTile.transform.parent = YuME_mapEditor.mapLayers[YuME_mapEditor.currentLayer-1].transform;
		}

		EditorSceneManager.MarkAllScenesDirty();
	}

	public static void eraseTile(Vector3 position)
	{
        if (!YuME_mapEditor.findTileMapParent())
        {
            Debug.LogWarning("YuME: Cannot erase tile - no tile map parent found");
            return;
        }

        if (YuME_mapEditor.mapLayers == null || YuME_mapEditor.currentLayer < 1 || YuME_mapEditor.currentLayer > YuME_mapEditor.mapLayers.Length)
        {
            Debug.LogWarning("YuME: Cannot erase tile - invalid layer selection");
            return;
        }

        GameObject currentLayer = YuME_mapEditor.mapLayers[YuME_mapEditor.currentLayer - 1];
        if (currentLayer == null)
        {
            Debug.LogWarning("YuME: Cannot erase tile - current layer is null");
            return;
        }

        Vector3 tempVec3;

        for (int i = 0; i < currentLayer.transform.childCount; ++i)
        {
            var child = currentLayer.transform.GetChild(i);
            if (child == null) continue;

            tempVec3 = child.transform.position;

            if (tempVec3.x == position.x && tempVec3.z == position.z && tempVec3.y >= position.y && tempVec3.y < position.y + 1f)
            {
                try
                {
                    Undo.DestroyObjectImmediate(child.gameObject);
                    EditorSceneManager.MarkAllScenesDirty();
                    return;
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"YuME: Failed to erase tile: {e.Message}");
                }
            }
        }
	}

	public static void pickTile(Vector3 position)
	{
        if (YuME_mapEditor.findTileMapParent())
        {
            GameObject currentLayer = YuME_mapEditor.mapLayers[YuME_mapEditor.currentLayer - 1];
            Vector3 tempVec3;

            for (int i = 0; i < currentLayer.transform.childCount; ++i)
            {
                tempVec3 = currentLayer.transform.GetChild(i).transform.position;

                if (tempVec3.x == position.x && tempVec3.z == position.z && tempVec3.y >= position.y && tempVec3.y < position.y + 1f)
                {
                    YuME_mapEditor.currentTile = AssetDatabase.LoadAssetAtPath(AssetDatabase.GetAssetPath(PrefabUtility.GetCorrespondingObjectFromSource(currentLayer.transform.GetChild(i).gameObject)), typeof(GameObject)) as GameObject;

                    float pickRotation = 0;
                    float pickRotationX = 0;

                    Transform pickTileTransform = currentLayer.transform.GetChild(i).transform;

                    if (pickTileTransform.eulerAngles.y > 0)
                    {
                        pickRotation = pickTileTransform.eulerAngles.y;
                    }

                    if (pickTileTransform.eulerAngles.x > 0)
                    {
                        pickRotationX = pickTileTransform.eulerAngles.x;
                    }

                    YuME_mapEditor.currentBrushIndex = Array.IndexOf(YuME_mapEditor.currentTileSetObjects, YuME_mapEditor.currentTile);
                    YuME_mapEditor.tileRotation = pickRotation;
                    YuME_mapEditor.tileRotationX = pickRotationX;

                    tempVec3 = pickTileTransform.localScale;
                    tempVec3.x /= YuME_mapEditor.globalScale;
                    tempVec3.y /= YuME_mapEditor.globalScale;
                    tempVec3.z /= YuME_mapEditor.globalScale;
                    YuME_mapEditor.tileScale = tempVec3;

                    YuME_brushFunctions.updateBrushTile(pickTileTransform.localScale);
                    YuME_mapEditor.currentBrushType = YuME_mapEditor.brushTypes.standardBrush;
                    YuME_mapEditor.selectedTool = YuME_mapEditor.toolIcons.brushTool;

                    return;
                }
            }
        }
	}

    public static void flipHorizontal()
    {
        Undo.RecordObject(YuME_mapEditor.brushTile.transform, "Flip Horizontal");

        if (YuME_mapEditor.tileScale.x == 1f)
        {
            YuME_mapEditor.tileScale.x = -1f;
        }
        else
        {
            YuME_mapEditor.tileScale.x = 1f;
        }
    }

    public static void flipVertical()
    {
        Undo.RecordObject(YuME_mapEditor.brushTile.transform, "Flip Vertical");

        if (YuME_mapEditor.tileScale.z == 1f)
        {
            YuME_mapEditor.tileScale.z = -1f;
        }
        else
        {
            YuME_mapEditor.tileScale.z = 1f;
        }
    }

    public static void checkTileSelectionStatus()
    {
        if (YuME_mapEditor.selectedTiles == null)
        {
            YuME_mapEditor.selectedTiles = new List<GameObject>();
        }

        if (YuME_mapEditor.selectedTiles.Count == 0 && Selection.gameObjects != null && Selection.gameObjects.Length > 0)
        {
            YuME_mapEditor.selectedTiles.Clear();

            foreach(GameObject tile in Selection.gameObjects)
            {
                if (tile != null && tile.GetComponent<YuME_tileGizmo>() != null)
                {
                    YuME_mapEditor.selectedTiles.Add(tile);
                }
            }
        }
    }

    public static void selectTile(Vector3 position)
    {
        if (!YuME_mapEditor.findTileMapParent())
        {
            Debug.LogWarning("YuME: Cannot select tile - no tile map parent found");
            return;
        }

        if (YuME_mapEditor.mapLayers == null || YuME_mapEditor.currentLayer < 1 || YuME_mapEditor.currentLayer > YuME_mapEditor.mapLayers.Length)
        {
            Debug.LogWarning("YuME: Cannot select tile - invalid layer selection");
            return;
        }

        GameObject currentLayer = YuME_mapEditor.mapLayers[YuME_mapEditor.currentLayer - 1];
        if (currentLayer == null)
        {
            Debug.LogWarning("YuME: Cannot select tile - current layer is null");
            return;
        }

        if (YuME_mapEditor.selectedTiles == null)
        {
            YuME_mapEditor.selectedTiles = new List<GameObject>();
        }

        for (int i = 0; i < currentLayer.transform.childCount; ++i)
        {
            var child = currentLayer.transform.GetChild(i);
            if (child == null) continue;

            float distanceToTile = Vector3.Distance(child.transform.position, position);

            if (distanceToTile < 0.1f && child.name != "YuME_brushTile")
            {
                // Check if already selected
                for (int t = 0; t < YuME_mapEditor.selectedTiles.Count; t++)
                {
                    if (YuME_mapEditor.selectedTiles[t] != null && 
                        child.gameObject.transform.position == YuME_mapEditor.selectedTiles[t].transform.position)
                    {
                        return;
                    }
                }

                YuME_mapEditor.selectedTiles.Add(child.gameObject);
                return;
            }
        }
    }

    public static void deSelectTile(Vector3 position)
    {
        if (YuME_mapEditor.findTileMapParent())
        {
            GameObject currentLayer = YuME_mapEditor.mapLayers[YuME_mapEditor.currentLayer - 1];

            for (int i = 0; i < currentLayer.transform.childCount; ++i)
            {
                float distanceToTile = Vector3.Distance(currentLayer.transform.GetChild(i).transform.position, position);

                if (distanceToTile < 0.1f && currentLayer.transform.GetChild(i).name != "YuME_brushTile")
                {
                    for (int t = 0; t < YuME_mapEditor.selectedTiles.Count; t++)
                    {
                        if (currentLayer.transform.GetChild(i).gameObject.transform.position == YuME_mapEditor.selectedTiles[t].transform.position)
                        {
                            YuME_mapEditor.selectedTiles.RemoveAt(t);
                            return;
                        }
                    }
                }
            }
        }
    }

    public static void selectAllTiles()
    {
        if (YuME_mapEditor.findTileMapParent())
        {
            GameObject currentLayer = YuME_mapEditor.mapLayers[YuME_mapEditor.currentLayer - 1];

            if (YuME_mapEditor.selectedTiles.Count > 0)
            {
                YuME_mapEditor.selectedTiles.Clear();
            }
            else
            {
                for (int i = 0; i < currentLayer.transform.childCount; ++i)
                {
                    YuME_mapEditor.selectedTiles.Add(currentLayer.transform.GetChild(i).gameObject);
                }
            }
        }
    }

    public static void trashTiles()
    {
        Undo.RegisterFullObjectHierarchyUndo(YuME_mapEditor.tileMapParent, "Trash Tiles");

        checkTileSelectionStatus();

        foreach (GameObject tile in YuME_mapEditor.selectedTiles)
        {
            DestroyImmediate(tile);
        }

        EditorSceneManager.MarkAllScenesDirty();
        YuME_mapEditor.selectedTiles.Clear();
    }

    public static void isolateTilesToggle()
    {
        if (YuME_mapEditor.isolateTiles == false)
        {
            isolateGridTiles();
            YuME_mapEditor.isolateTiles = true;
        }
        else
        {
            restoreIsolatedGridTiles();
            YuME_mapEditor.isolateTiles = false;
        }
    }

    public static void isolateLayerToggle()
    {
        if (YuME_mapEditor.isolateLayer == false)
        {
            isolateLayerTiles();
            YuME_mapEditor.isolateLayer = true;
        }
        else
        {
            restoreIsolatedLayerTiles();
            YuME_mapEditor.isolateLayer = false;
        }
    }

    public static void isolateGridTiles()
    {
        restoreIsolatedGridTiles();

        if (YuME_mapEditor.findTileMapParent())
        {
            foreach (Transform layer in YuME_mapEditor.tileMapParent.transform)
            {
                foreach (Transform tile in layer)
                {
                    if (!YuME_mapEditor.editorPreferences.twoPointFiveDMode)
                    {
                        if (tile.gameObject.transform.position.y != YuME_mapEditor.gridHeight)
                        {
                            tile.gameObject.SetActive(false);
                            YuME_mapEditor.isolatedGridObjects.Add(tile.gameObject);
                        }
                    }
                    else
                    {
                        if (tile.gameObject.transform.position.z != YuME_mapEditor.gridHeight)
                        {
                            tile.gameObject.SetActive(false);
                            YuME_mapEditor.isolatedGridObjects.Add(tile.gameObject);
                        }
                    }
                }
            }
        }
    }

    public static void restoreIsolatedGridTiles()
    {
        if (YuME_mapEditor.isolatedGridObjects != null && YuME_mapEditor.isolatedGridObjects.Count > 0)
        {
            foreach (GameObject tile in YuME_mapEditor.isolatedGridObjects)
            {
                if (tile != null)
                {
                    try
                    {
                        tile.SetActive(true);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogWarning($"YuME: Failed to restore isolated tile: {e.Message}");
                    }
                }
            }
        }

        if (YuME_mapEditor.isolatedGridObjects != null)
        {
            YuME_mapEditor.isolatedGridObjects.Clear();
        }
    }

    public static void isolateLayerTiles()
    {
        restoreIsolatedLayerTiles();

        if (!YuME_mapEditor.findTileMapParent())
        {
            Debug.LogWarning("YuME: Cannot isolate layer tiles - no tile map parent found");
            return;
        }

        if (YuME_mapEditor.tileMapParent == null)
        {
            Debug.LogWarning("YuME: Cannot isolate layer tiles - tile map parent is null");
            return;
        }

        if (YuME_mapEditor.isolatedLayerObjects == null)
        {
            YuME_mapEditor.isolatedLayerObjects = new List<GameObject>();
        }

        foreach (Transform tile in YuME_mapEditor.tileMapParent.transform)
        {
            if (tile != null && tile.name != "layer" + YuME_mapEditor.currentLayer)
            {
                try
                {
                    tile.gameObject.SetActive(false);
                    YuME_mapEditor.isolatedLayerObjects.Add(tile.gameObject);
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"YuME: Failed to isolate layer tile: {e.Message}");
                }
            }
        }
    }

    public static void restoreIsolatedLayerTiles()
    {
        if (YuME_mapEditor.isolatedLayerObjects.Count > 0)
        {
            foreach (GameObject tile in YuME_mapEditor.isolatedLayerObjects)
            {
                if (tile != null)
                {
                    tile.SetActive(true);
                }
            }
        }

        YuME_mapEditor.isolatedLayerObjects.Clear();
    }
}
