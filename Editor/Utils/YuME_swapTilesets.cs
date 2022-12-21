using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class YuME_swapTilesets : EditorWindow
{
    static bool doSwap = false;
    static int swapTileSetIndex = 0;
    static GameObject[] swapTileSetObjects;

    [MenuItem("Window/Yuponic/Utils/Swap Tilesets")]
    static void Initialize()
    {
        YuME_swapTilesets swapTIlesetsEditorWindow = EditorWindow.GetWindow<YuME_swapTilesets>(true, "Swap Tilesets");
        swapTIlesetsEditorWindow.titleContent.text = "Swap Tilesets";
    }

    void OnEnable()
    {
        YuME_mapEditor.importTileSets(false);
    }

    void OnGUI()
    {
        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.LabelField("Pick The Tile Set To Swap To", EditorStyles.boldLabel);
        swapTileSetIndex = EditorGUILayout.Popup("Choose Tileset", swapTileSetIndex, YuME_mapEditor.tileSetNames);

        EditorGUILayout.EndVertical();

        doSwap = GUILayout.Toggle(doSwap, "Swap Tileset", "Button", GUILayout.Height(30));

        if (doSwap)
        {
            swapTileSet();
            SceneView.RepaintAll();
            EditorUtility.ClearProgressBar();
        }

        doSwap = false;
    }

    static void swapTileSet()
    {
        string path = YuTools_Utils.getAssetPath(YuME_mapEditor.availableTileSets[swapTileSetIndex]);
        swapTileSetObjects = YuTools_Utils.loadDirectoryContents(path, "*.prefab");

        List<GameObject> layerTiles = new List<GameObject>();

        if (swapTileSetObjects != null)
        {
            GameObject swapTile;

            if (YuME_mapEditor.findTileMapParent())
            {
                Undo.RegisterFullObjectHierarchyUndo(YuME_mapEditor.tileMapParent, "Swap Tiles");

                foreach (Transform layer in YuME_mapEditor.tileMapParent.transform)
                {
                    if (layer.gameObject.name.Contains("layer"))
                    {
                        layerTiles.Clear();

                        foreach (Transform tile in layer)
                        {
                            layerTiles.Add(tile.gameObject);
                        }

                        for(int i = 0; i < layerTiles.Count; i++)
                        {
                            for(int swap = 0; swap < swapTileSetObjects.Length; swap++)
                            {
                                if(layerTiles[i].name == swapTileSetObjects[swap].name)
                                {
                                    EditorUtility.DisplayProgressBar("Swapping Tileset", layerTiles[i].name, (float)i / (float)layerTiles.Count);
                                    swapTile = PrefabUtility.InstantiatePrefab(swapTileSetObjects[swap] as GameObject) as GameObject;
                                    swapTile.transform.parent = layer;
                                    swapTile.transform.position = layerTiles[i].transform.position;
                                    swapTile.transform.eulerAngles = layerTiles[i].transform.eulerAngles;
                                    swapTile.transform.GetChild(0).transform.position = layerTiles[i].transform.GetChild(0).transform.position;
                                    DestroyImmediate(layerTiles[i]);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
