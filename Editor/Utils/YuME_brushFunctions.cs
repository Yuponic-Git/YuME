using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class YuME_brushFunctions : EditorWindow 
{
    static List<GameObject> brushTilesInParent = new List<GameObject>();

	public static void updateBrushPosition()
	{
		if (YuME_mapEditor.brushTile != null)
		{
			YuME_mapEditor.brushTile.transform.position = YuME_mapEditor.tilePosition;
			YuME_mapEditor.brushTile.transform.eulerAngles = new Vector3(YuME_mapEditor.tileRotationX, YuME_mapEditor.tileRotation, 0f);
            if (YuME_mapEditor.currentBrushType == YuME_mapEditor.brushTypes.standardBrush)
            {
                YuME_mapEditor.brushTile.transform.localScale = Vector3.Scale(new Vector3(YuME_mapEditor.globalScale, YuME_mapEditor.globalScale, YuME_mapEditor.globalScale), YuME_mapEditor.tileScale);
            }
            else
            {
                YuME_mapEditor.brushTile.transform.localScale = Vector3.Scale(Vector3.one, YuME_mapEditor.tileScale);
            }

            if (YuME_mapEditor.eraseToolOverride || YuME_mapEditor.pickToolOverride)
            {
                foreach (GameObject child in YuME_mapEditor.tileChildObjects)
                {
                    child.SetActive(false);
                }
            }
            else
            {
                foreach (GameObject child in YuME_mapEditor.tileChildObjects)
                {
                    child.SetActive(true);
                }
            }
        }
	}

	public static void displayBrush(bool display)
	{
        if (YuME_mapEditor.selectedTool != YuME_mapEditor.previousSelectedTool)
        {
            foreach (Transform child in YuME_mapEditor.brushTile.transform)
            {
                YuME_mapEditor.showWireBrush = !display;

                child.gameObject.SetActive(display);
            }
        }
	}

	public static void createBrushTile()
	{
		if (YuME_mapEditor.selectedTool != YuME_mapEditor.previousSelectedTool)
		{
			updateBrushTile();
		}
	}

    public static void updateBrushTile()
    {
        YuME_mapEditor.pickTileScale = Vector3.zero;

        if (YuME_mapEditor.currentTile != null)
            updateBrushTile(YuME_mapEditor.currentTile.transform.localScale);
    }

	public static void updateBrushTile(Vector3 tileScale)
	{
        if (YuME_mapEditor.findTileMapParent() && YuME_mapEditor.toolEnabled)
        {
            destroyBrushTile();

            YuME_mapEditor.brushTile = Instantiate(YuME_mapEditor.currentTile, Vector3.zero, Quaternion.identity) as GameObject;
            YuME_mapEditor.brushTile.transform.eulerAngles = new Vector3(YuME_mapEditor.tileRotationX, YuME_mapEditor.tileRotation, 0f);
            YuME_mapEditor.brushTile.transform.parent = YuME_mapEditor.tileMapParent.transform;
            YuME_mapEditor.brushTile.transform.localScale = tileScale;
            YuME_mapEditor.brushTile.name = "YuME_brushTile";
            YuME_mapEditor.brushTile.hideFlags = HideFlags.HideAndDontSave;
            YuME_mapEditor.brushTile.isStatic = false;

            foreach (Transform child in YuME_mapEditor.brushTile.transform)
            {
                child.gameObject.isStatic = false;
            }

            YuME_mapEditor.tileChildObjects.Clear();

            foreach (Transform child in YuME_mapEditor.brushTile.transform)
            {
                YuME_mapEditor.tileChildObjects.Add(child.gameObject);
            }

            YuME_mapEditor.showWireBrush = false;
        }
    }

    public static void destroyBrushTile()
    {
        brushTilesInParent.Clear();

        foreach (Transform child in YuME_mapEditor.tileMapParent.transform)
        {
            if (child.name == "YuME_brushTile")
            {
                brushTilesInParent.Add(child.gameObject);
            }
        }

        for (int i = 0; i < brushTilesInParent.Count; i++)
        {
            DestroyImmediate(brushTilesInParent[i]);
        }

        SceneView.RepaintAll();
        YuME_mapEditor.showWireBrush = true;
    }

    public static void cleanSceneOfBrushObjects()
	{
        if (YuME_mapEditor.findTileMapParent())
        {
            List<GameObject> destroyList = new List<GameObject>();

            foreach (Transform child in YuME_mapEditor.tileMapParent.transform)
            {
                if (child.gameObject.name == "YuME_brushTile")
                {
                    destroyList.Add(child.gameObject);
                }
            }

            foreach (GameObject tileToDestory in destroyList)
            {
                DestroyImmediate(tileToDestory);
            }
        }
	}
}

