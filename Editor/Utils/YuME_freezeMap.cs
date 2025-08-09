using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class YuME_freezeMap : EditorWindow
{
    static GameObject frozenMap;

    public static void combineTiles()
    {
        if (YuME_mapEditor.tileMapParent)
        {
            frozenMap = new GameObject();
            frozenMap.transform.parent = YuME_mapEditor.tileMapParent.transform;
            frozenMap.name = "frozenMap";

            List<GameObject> tilesToCombine = new List<GameObject>();
            List<GameObject> lightsToCreate = new List<GameObject>();
            List<Transform> freezeTiles = new List<Transform>();
            List<Transform> _freezeTiles = new List<Transform>();

            EditorUtility.DisplayProgressBar("Building Frozen Map", "Finding Tiles to Freeze", 0f);

            for(int i = 0; i < 8; i++)
            {
                if (YuME_mapEditor.editorPreferences.layerFreeze[i])
                {
                    YuME_mapEditor.mapLayers[i].GetComponentsInChildren<Transform>(false, _freezeTiles);
                    freezeTiles.AddRange(_freezeTiles);
                }
                else
                {
                    GameObject layerCopy = Instantiate(YuME_mapEditor.mapLayers[i], YuME_mapEditor.mapLayers[i].transform.position, Quaternion.identity) as GameObject;
                    layerCopy.transform.parent = frozenMap.transform;
                    if(!YuME_mapEditor.editorPreferences.layerStatic[i])
                    {
                        Transform[] tempTiles = layerCopy.GetComponentsInChildren<Transform>();
                        foreach (Transform child in tempTiles)
                        {
                            child.gameObject.isStatic = false;
                        }
                    }
                }
            }

            foreach (Transform tile in freezeTiles)
            {
                if(tile.GetComponent<MeshRenderer>())
                {
                    if(YuME_mapEditor.editorPreferences.convexCollision)
                    {
                        MeshCollider _tempCol = tile.GetComponent<MeshCollider>();
                        if (_tempCol != null)
                        {
                            _tempCol.convex = true;
                        }
                    }
                    tilesToCombine.Add(tile.gameObject);
                }

                if(tile.GetComponent<Light>())
                {
                    lightsToCreate.Add(tile.gameObject);
                }
            }

            foreach(GameObject light in lightsToCreate)
            {
                GameObject newLight = GameObject.Instantiate(light);
                newLight.isStatic = true;
                newLight.transform.position = light.transform.position;
                newLight.transform.eulerAngles = light.transform.eulerAngles;
                newLight.transform.parent = frozenMap.transform;
            }

            tilesToCombine = tilesToCombine.OrderBy(x => x.GetComponent<MeshRenderer>().sharedMaterial.name).ToList();

            Material previousMaterial = tilesToCombine[0].GetComponent<MeshRenderer>().sharedMaterial;

            List<CombineInstance> combine = new List<CombineInstance>();
            CombineInstance tempCombine = new CombineInstance();
            int vertexCount = 0;

            foreach (GameObject mesh in tilesToCombine)
            {
                vertexCount += mesh.GetComponent<MeshFilter>().sharedMesh.vertexCount;

                if (vertexCount > 60000)
                {
                    vertexCount = 0;
                    newSubMesh(combine, mesh.GetComponent<MeshRenderer>().sharedMaterial);
                    combine = new List<CombineInstance>();
                }

                if (mesh.GetComponent<MeshRenderer>().sharedMaterial.name != previousMaterial.name)
                {
                    newSubMesh(combine, previousMaterial);
                    combine = new List<CombineInstance>();
                }

                tempCombine.mesh = mesh.GetComponent<MeshFilter>().sharedMesh;
                tempCombine.transform = mesh.GetComponent<MeshFilter>().transform.localToWorldMatrix;
                combine.Add(tempCombine);
                previousMaterial = mesh.GetComponent<MeshRenderer>().sharedMaterial;
            }

            newSubMesh(combine, previousMaterial);

            foreach (Transform layer in YuME_mapEditor.tileMapParent.transform)
            {
                if (layer.name.Contains("layer"))
                {
                    layer.gameObject.SetActive(false);
                }
            }

            YuME_brushFunctions.cleanSceneOfBrushObjects();

            EditorUtility.ClearProgressBar();
            YuME_brushFunctions.destroyBrushTile();
            YuME_brushFunctions.cleanSceneOfBrushObjects();
            YuME_mapEditor.selectedTool = YuME_mapEditor.toolIcons.defaultTools;
        }
    }

    static void newSubMesh(List<CombineInstance> combine, Material mat)
    {
        GameObject subMesh = new GameObject();
        subMesh.transform.parent = frozenMap.transform;
        subMesh.name = "subMesh";

        MeshFilter subMeshFilter = subMesh.AddComponent<MeshFilter>();
        subMeshFilter.sharedMesh = new Mesh();
        subMeshFilter.sharedMesh.name = "subMesh";
        subMesh.isStatic = true;
        subMeshFilter.sharedMesh.CombineMeshes(combine.ToArray());
        MeshRenderer meshRenderer = subMesh.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = mat;
        subMesh.AddComponent<MeshCollider>().sharedMesh = subMeshFilter.sharedMesh;
        if(YuME_mapEditor.editorPreferences.convexCollision)
        {
            subMesh.GetComponent<MeshCollider>().convex = true;
        }

        subMesh.SetActive(true);

    }

    public static void saveFrozenMesh(string path)
    {
        path = YuTools_Utils.shortenAssetPath(path);
        
        // Ensure path is never empty or invalid
        if (string.IsNullOrEmpty(path) || !path.StartsWith("Assets/"))
        {
            path = "Assets/";
        }
        
        int counter = 1;

        if (YuME_mapEditor.findTileMapParent())
        {
            foreach (Transform child in YuME_mapEditor.tileMapParent.transform)
            {
                if (child.gameObject.name == "frozenMap")
                {
                    GameObject saveMap = Instantiate(child.gameObject);
                    saveMap.name = YuME_mapEditor.tileMapParent.name;

                    if (!AssetDatabase.IsValidFolder(path + "/" + saveMap.name + "Meshes"))
                    {
                        AssetDatabase.CreateFolder(path, saveMap.name + "Meshes");
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();

                    }

                    EditorUtility.ClearProgressBar();

                    foreach (Transform frozenMesh in saveMap.transform)
                    {
                        EditorUtility.DisplayProgressBar("Saving Meshes", "Saving Mesh " + (counter) + ". This might take some time", 1);
                        Mesh saveMesh = Object.Instantiate(frozenMesh.GetComponent<MeshFilter>().sharedMesh) as Mesh;
                        //Unwrapping.GenerateSecondaryUVSet(saveMesh);
                        try
                        {
                            AssetDatabase.CreateAsset(saveMesh, path + "/" + saveMap.name + "Meshes/" + frozenMesh.name + counter + ".asset");
                        }
                        catch
                        {
                            Debug.LogWarning("Failed to create saved map. This is likely due to a new folder being created and Unity not refreshing the asset database. Please retry saving the map.");
                            EditorUtility.ClearProgressBar();
                            return;
                        }
                        frozenMesh.GetComponent<MeshFilter>().mesh = saveMesh;
                        counter++;
                    }

                    EditorUtility.ClearProgressBar();

                    Object prefabAlreadyCreated = AssetDatabase.LoadAssetAtPath(path + "/" + saveMap.name + ".prefab", typeof(GameObject));

                    if (prefabAlreadyCreated != null)
                        PrefabUtility.SaveAsPrefabAssetAndConnect(saveMap, path + "/" + saveMap.name + ".prefab", InteractionMode.AutomatedAction);
                    else
                        PrefabUtility.SaveAsPrefabAsset(saveMap, path + "/" + saveMap.name + ".prefab");

                    AssetDatabase.SaveAssets();

                    if (saveMap != null)
                        DestroyImmediate(saveMap);
                }
            }

            AssetDatabase.Refresh();
        }

    }
}