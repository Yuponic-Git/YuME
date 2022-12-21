using UnityEngine;
using UnityEditor;

class YuME_tileImporter : EditorWindow
{
	// TODO:
	// Add help buttons next to fields
	// Advanced split by type allowing the user to choose and create types and assign the string identifier, i.e. Wall
	// then associate it with the directory
	// This can be a list in the editorAttributeClass

	public static YuME_editorData editorData;

    public static YuME_importerSettings userSettings;
    static public YuME_tilesetData tilesetData;

    static GameObject tileParentObject;
    static GameObject tileMeshDataObject;

    static MeshFilter tileMeshFilter;
    static MeshRenderer tileMeshRenderer;
    static MeshCollider tileMeshCollider;

    static int updatedPrefabsCounter = 0;
    static int newPrefabsCounter = 0;
    static float tileImportProgress = 1f;
    static float totalMeshesInSourceObject = 0f;
    static float lowestVertex = 0f;

    static Vector2 _scrollPosition;

    [MenuItem("Window/Yuponic/YuME: Tile Importer")]
    static void Initialize()
    {
        YuME_tileImporter tileImporterWindow = EditorWindow.GetWindow<YuME_tileImporter>(false, "Tile Importer");
        tileImporterWindow.titleContent.text = "Tile Importer";
    }

    void OnEnable()
    {
		EditorUtility.ClearProgressBar();

        editorData = ScriptableObject.CreateInstance<YuME_editorData>();

        // ----------------------------------------------------------------------------------------------------
        // ----- Load Editor Settings
        // ----------------------------------------------------------------------------------------------------

        string[] guids;
		guids = AssetDatabase.FindAssets("YuME_editorSetupData");
		editorData = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guids[0]), typeof(YuME_editorData)) as YuME_editorData;

        guids = AssetDatabase.FindAssets("YuME_importSettings");

        if (guids.Length == 0)
        {
            // create a new setting file
            Debug.Log("Yuponic Tile Editor: No settings file found. Creating new settings file");
            YuME_importerSettings asset = ScriptableObject.CreateInstance<YuME_importerSettings>();

            AssetDatabase.CreateAsset(asset, "Assets/YuME_importSettings.asset");
            AssetDatabase.SaveAssets();

			userSettings = AssetDatabase.LoadAssetAtPath("Assets/YuME_importSettings.asset", typeof(YuME_importerSettings)) as YuME_importerSettings;
        }
        else
        {
            // load the settings
            userSettings = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guids[0]), typeof(YuME_importerSettings)) as YuME_importerSettings;
        }
    }

    void OnGUI()
    {
        // ----------------------------------------------------------------------------------------------------
        // ------ Tile Import Data
        // ----------------------------------------------------------------------------------------------------

        EditorGUILayout.Space();
		GUILayout.Label(editorData.tileImporterHeader);

		_scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

		EditorGUILayout.LabelField("Tile Import Settings", EditorStyles.boldLabel);

		EditorGUILayout.BeginVertical("box");

        if (userSettings.sourceTiles == null)
        {
            userSettings.sourceTiles = (GameObject)EditorGUILayout.ObjectField("Source Tiles Object", null, typeof(GameObject), false);
        }
        else
        {
            userSettings.sourceTiles = (GameObject)EditorGUILayout.ObjectField("Source Tiles Object", userSettings.sourceTiles, typeof(GameObject), false);
        }

        if (userSettings.sourceMaterial == null)
        {
            userSettings.sourceMaterial = (Material)EditorGUILayout.ObjectField("Source Tile Material", null, typeof(Material), false);
        }
        else
        {
            userSettings.sourceMaterial = (Material)EditorGUILayout.ObjectField("Source Tile Material", userSettings.sourceMaterial, typeof(Material), false);
        }

        if (userSettings.sourceTexture == null)
        {
            userSettings.sourceTexture = (Texture)EditorGUILayout.ObjectField("Source Tile Texture", null, typeof(Texture), false);
        }
        else
        {
            userSettings.sourceTexture = (Texture)EditorGUILayout.ObjectField("Source Tile Texture", userSettings.sourceTexture, typeof(Texture), false);
        }

		EditorGUILayout.EndVertical();
        
        // ----------------------------------------------------------------------------------------------------
        // ------ Layer Settings
        // ----------------------------------------------------------------------------------------------------

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Layer Setup", EditorStyles.boldLabel);

        EditorGUILayout.BeginVertical("box");

        userSettings.layer = EditorGUILayout.LayerField("Tileset Layer", userSettings.layer);

        EditorGUILayout.EndVertical();

        // ----------------------------------------------------------------------------------------------------
        // ------ Static Settings
        // ----------------------------------------------------------------------------------------------------

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Prefab Static Setting", EditorStyles.boldLabel);

        EditorGUILayout.BeginVertical("box");

        userSettings.tileStatic = EditorGUILayout.Toggle("Set Tiles as Static", userSettings.tileStatic);

        EditorGUILayout.EndVertical();

        // ----------------------------------------------------------------------------------------------------
        // ------ Alt Settings
        // ----------------------------------------------------------------------------------------------------

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("ALT Tile Setting", EditorStyles.boldLabel);

        EditorGUILayout.BeginVertical("box");

        userSettings.altIdentifier = EditorGUILayout.TextField("Alternative Tile Identifier", userSettings.altIdentifier);

        EditorGUILayout.EndVertical();

        // ----------------------------------------------------------------------------------------------------
        // ------ Shadow Settings
        // ----------------------------------------------------------------------------------------------------

        EditorGUILayout.Space();

		EditorGUILayout.LabelField("Prefab Shadow Setup", EditorStyles.boldLabel);

		EditorGUILayout.BeginVertical("box");

        userSettings.shadowCastingMode = EditorGUILayout.Popup("Cast Shadows", userSettings.shadowCastingMode, new string[] { "Off", "On", "ShadowsOnly", "TwoSided" });

        userSettings.receiveShadows = EditorGUILayout.Toggle("Receive Shadows", userSettings.receiveShadows);

		EditorGUILayout.EndVertical();

        // ----------------------------------------------------------------------------------------------------
        // ------ Collision Settings
        // ----------------------------------------------------------------------------------------------------

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Tile Collision Setup", EditorStyles.boldLabel);

		EditorGUILayout.BeginVertical("box");

        userSettings.bypassCollisionSetup = EditorGUILayout.Toggle("Bypass Collision Setup", userSettings.bypassCollisionSetup);

        if (!userSettings.bypassCollisionSetup)
        {
            userSettings.importCollision = EditorGUILayout.Toggle("Custom Collision Mesh", userSettings.importCollision);

            if (userSettings.importCollision)
            {
                userSettings.collisionIdentifier = EditorGUILayout.TextField("Collision Mesh Identifier", userSettings.collisionIdentifier);
            }
        }

		EditorGUILayout.EndVertical();

        // ----------------------------------------------------------------------------------------------------
        // ------ Bounding Box Settings
        // ----------------------------------------------------------------------------------------------------

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Tile Bounding Setup", EditorStyles.boldLabel);

        EditorGUILayout.BeginVertical("box");

        userSettings.importCustomBounds= EditorGUILayout.Toggle("Custom Bounds Mesh", userSettings.importCustomBounds);

        if (userSettings.importCustomBounds)
        {
            userSettings.customBoundsIdentifier = EditorGUILayout.TextField("Custom Bounds Identifier", userSettings.customBoundsIdentifier);
        }

        EditorGUILayout.EndVertical();

        //data.userSettings.splitByType = EditorGUI.Toggle(new Rect(3, 295, position.width - 3, 20), 
        //    new GUIContent("Split Tiles by Type", "Checks the tile name for keywords such as 'wall', 'floor', 'corner' and puts them in seperate folders for easy navigation"),
        //    data.userSettings.splitByType);

        // ----------------------------------------------------------------------------------------------------
        // ------ Append Settings
        // ----------------------------------------------------------------------------------------------------

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Custom Append Label", EditorStyles.boldLabel);

		EditorGUILayout.BeginVertical("box");

        userSettings.appendName = EditorGUILayout.TextField("Append to Tile Name", userSettings.appendName);

		EditorGUILayout.EndVertical();

        // ----------------------------------------------------------------------------------------------------
        // ------ Folder Settings
        // ----------------------------------------------------------------------------------------------------

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Select Tile Prefab Destination Folder", EditorStyles.boldLabel);

		EditorGUILayout.BeginVertical("box");

		EditorGUILayout.LabelField("Set Destination Folders");

		if (GUILayout.Button("Tile Destination", GUILayout.Height(30)))
        {
            userSettings.destinationFolder = EditorUtility.OpenFolderPanel("Tile Prefab Destination Folder", "", "");

			if(userSettings.destinationFolder == "")
			{
				userSettings.destinationFolder = "Assets/";
			}
        }
        
        userSettings.destinationFolder = YuTools_Utils.shortenAssetPath(userSettings.destinationFolder);

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Select Mesh Destination Folder");

		if (GUILayout.Button("Mesh Destination", GUILayout.Height(30)))
        {
            userSettings.meshFolder = EditorUtility.OpenFolderPanel("Tile Mesh Destination Folder", "", "");

			if(userSettings.meshFolder == "")
			{
				userSettings.meshFolder = "Assets/";
			}
        }
			        
        EditorGUILayout.Space();

        EditorGUILayout.EndVertical();

        userSettings.meshFolder = YuTools_Utils.shortenAssetPath(userSettings.meshFolder);

        // ----------------------------------------------------------------------------------------------------
        // ------ Import Button 
        // ----------------------------------------------------------------------------------------------------

        EditorGUILayout.Space();

        if (userSettings.sourceTiles != null)
        {
			if (GUILayout.Button("Import Tileset", GUILayout.Height(40)))
            {
				bool tileDestinationFolderWarning = true;
				bool meshDestinationFolderWarning = true;

				if(userSettings.destinationFolder == "Assets/")
				{
					tileDestinationFolderWarning = EditorUtility.DisplayDialog("Tile Prefabs will be created in the root Assets Folder",
						"Are you sure you want create the tile prefabs in the root Asset/ folder?", "OK", "Cancel");
				}

				if(userSettings.meshFolder == "Assets/")
				{
					meshDestinationFolderWarning = EditorUtility.DisplayDialog("New mesh data will be created in the root Assets Folder",
						"Are you sure you want create the new mesh data in the root Asset/ folder?", "OK", "Cancel");
				}

				if(tileDestinationFolderWarning && meshDestinationFolderWarning)
				{
                    refreshSourceTileObject();
                    createTemplateMaterial();
	                createTemplateTileObject();
                    setTileShadowOptions();
					createTileSet();
                    revertScenePrefabs();
                    YuME_tileConverter.recreateCustomBrushes(userSettings.destinationFolder);
					cleanUpImport();
				}
            }
		}
		else
		{
			EditorGUILayout.HelpBox("Set a Source Tile Object To Create Tile Set", MessageType.Warning);			
		}

        // ----------------------------------------------------------------------------------------------------
        // ------ Output Folder Information
        // ----------------------------------------------------------------------------------------------------

        EditorGUILayout.Space();

        EditorGUILayout.BeginVertical("box");
        
		EditorGUILayout.LabelField("Tile Output Folder:", EditorStyles.boldLabel);
        EditorGUILayout.LabelField(userSettings.destinationFolder);
        
		EditorGUILayout.LabelField("Mesh Output Folder:", EditorStyles.boldLabel);
        EditorGUILayout.LabelField(userSettings.meshFolder);
        
		EditorGUILayout.EndVertical();

        EditorGUILayout.EndScrollView();

		// ----------------------------------------------------------------------------------------------------
		// ------ GUI Changed
		// ----------------------------------------------------------------------------------------------------

        if (GUI.changed)
        {
            if (userSettings.sourceMaterial != null)
            {
                userSettings.sourceTexture = userSettings.sourceMaterial.mainTexture;
            }
            
			EditorUtility.SetDirty(userSettings);
        }
    }
	
    static void refreshSourceTileObject()
    {
        // Force refresh the Tile Asset since bounds and pivots in the source file are effected by the conversion process
        EditorUtility.DisplayProgressBar("Refreshing " + userSettings.sourceTiles.name + " before conversion", "", 0f);
        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(userSettings.sourceTiles), ImportAssetOptions.ForceUpdate);
        EditorUtility.ClearProgressBar();
    }

    static void createTemplateTileObject()
    {
        updatedPrefabsCounter = 0;
        newPrefabsCounter = 0;

        tileParentObject = new GameObject("Temporary Tile Parent Object");
        tileParentObject.AddComponent<YuME_tileGizmo>();
        tileParentObject.isStatic = userSettings.tileStatic;

        tileMeshDataObject = new GameObject("Temporary Tile Mesh Data");
        tileMeshDataObject.isStatic = userSettings.tileStatic;

        tileMeshFilter = tileMeshDataObject.AddComponent<MeshFilter>();
        tileMeshRenderer = tileMeshDataObject.AddComponent<MeshRenderer>();

        if(!userSettings.bypassCollisionSetup)
        {
            tileMeshCollider = tileMeshDataObject.AddComponent<MeshCollider>();
        }

        tileMeshDataObject.transform.parent = tileParentObject.transform;
    }

    static void setTileShadowOptions()
    {
        tileMeshRenderer.material = userSettings.sourceMaterial;
        tileMeshRenderer.receiveShadows = userSettings.receiveShadows;

        switch (userSettings.shadowCastingMode)
        {
            case 0:
                tileMeshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                break;
            case 1:
                tileMeshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                break;
            case 2:
                tileMeshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                break;
            case 3:
                tileMeshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.TwoSided;
                break;
        }
    }

    static void createTemplateMaterial()
    {
        // create a new material if one hasn't been defined at import

        if (userSettings.sourceMaterial == null)
        {
            string materialName = "";

            userSettings.sourceMaterial = new Material(Shader.Find("Standard")); // choose the standard shader by default

            if (userSettings.sourceTexture != null) // check if the player has defined a texture to use
            {
                userSettings.sourceMaterial.mainTexture = userSettings.sourceTexture;
                materialName = userSettings.sourceTexture.name;
            }
            else
            {
                materialName = "YuponicMapEditorDefault";
            }

            userSettings.sourceMaterial.name = materialName;

            AssetDatabase.CreateAsset(userSettings.sourceMaterial, userSettings.destinationFolder + "/" + materialName + ".mat");
        }
    }

    static void createTileSet()
    {
        tilesetData = ScriptableObject.CreateInstance<YuME_tilesetData>();

        scanNumberOfMeshesToConvert();

        foreach (Transform child in userSettings.sourceTiles.transform)
        {
            if (child.GetComponent<MeshFilter>() != null)
            {
                if (child.GetComponent<MeshFilter>().sharedMesh != null)
                {
                    // display the conversion progress based on the number of tiles found on the source tile set
                    EditorUtility.DisplayProgressBar("Building Tile Set", "Tile " + tileImportProgress + " / " + totalMeshesInSourceObject + " being exported", (float)(tileImportProgress / totalMeshesInSourceObject));

                    if (!userSettings.bypassCollisionSetup)
                    {
                        tileMeshCollider.sharedMesh = null; // reset the collision mesh for each conversion
                    }

                    if (!(child.name.Contains(userSettings.collisionIdentifier) || child.name.Contains(userSettings.customBoundsIdentifier)))
                    {
                        tileMeshFilter.sharedMesh = child.GetComponent<MeshFilter>().sharedMesh;

                        // Set the template gameobject name to the destination prefab name based in the object being imported
                        tileParentObject.name = userSettings.appendName + child.name;
                        tileParentObject.layer = userSettings.layer;
                        tileParentObject.transform.localScale = child.transform.localScale;
                        tileMeshDataObject.name = userSettings.appendName + child.name + "Tile";

                        tileMeshFilter.sharedMesh.bounds = findCustomBoundsMesh(child.GetComponent<MeshFilter>().sharedMesh);

                        // recenter the source mesh so it's origin is 0,0.5,0
                        tileMeshFilter.sharedMesh = recenterMesh(child.GetComponent<MeshFilter>().sharedMesh);

                        tileMeshDataObject.transform.position = setTheTileMeshObjectOffsetFromTheParent(tileMeshFilter.sharedMesh.bounds.extents);
                        tileMeshDataObject.layer = userSettings.layer;

                        attachCustomCollisionMesh(); // check if the object has a custom collision mesh and perform setup

                        if (!tileParentObject.name.Contains(userSettings.altIdentifier))
                        {
                            // Try and load an exisiting version of the prefab
                            Object prefabAlreadyCreated = AssetDatabase.LoadAssetAtPath(userSettings.destinationFolder + "/" + userSettings.appendName + child.name + "_YuME.prefab", typeof(GameObject));

                            // Check if the prefab already exisits and update it - or create a new prefab
                            if (prefabAlreadyCreated != null)
                            {
                                updatedPrefabsCounter++; // increment the counter of updated prefabs for the finished log
//#if UNITY_2018_3_OR_NEWER
//                                PrefabUtility.SaveAsPrefabAssetAndConnect(tileParentObject, userSettings.destinationFolder + "/" + userSettings.appendName + child.name + "_YuME.prefab", InteractionMode.AutomatedAction);
//#else
                                PrefabUtility.ReplacePrefab(tileParentObject, prefabAlreadyCreated, ReplacePrefabOptions.ReplaceNameBased); // replace the existing prefab with the updated data
//#endif
                            }
                            else
                            {
                                newPrefabsCounter++; // increment the counter of new prefabs for the finished log
//#if UNITY_2018_3_OR_NEWER
//                                PrefabUtility.SaveAsPrefabAsset(tileParentObject, userSettings.destinationFolder + "/" + tileParentObject.name + "_YuME.prefab");
//#else
                                PrefabUtility.CreatePrefab(userSettings.destinationFolder + "/" + tileParentObject.name + "_YuME.prefab", tileParentObject);
//#endif
                            }

                            tilesetData.tileData.Add(userSettings.destinationFolder + "/" + tileParentObject.name + "_YuME.prefab");
                        }
                        else
                        {
                            string[] masterTileName = tileParentObject.name.Split(new string[] { userSettings.altIdentifier }, System.StringSplitOptions.None);
                            masterTileName[0] += "_YuME";

                            if (!AssetDatabase.IsValidFolder(userSettings.destinationFolder + "/" + masterTileName[0]))
                            {
                                AssetDatabase.CreateFolder(userSettings.destinationFolder, masterTileName[0]);
                            }

                            Object prefabAlreadyCreated = AssetDatabase.LoadAssetAtPath(userSettings.destinationFolder + "/" + masterTileName[0] + "/" + userSettings.appendName + child.name + "_YuME.prefab", typeof(GameObject));

                            // Check if the prefab already exisits and update it - or create a new prefab
                            if (prefabAlreadyCreated != null)
                            {
                                updatedPrefabsCounter++; // increment the counter of updated prefabs for the finished log
//#if UNITY_2018_3_OR_NEWER
//                                PrefabUtility.SaveAsPrefabAssetAndConnect(tileParentObject, userSettings.destinationFolder + "/" + masterTileName[0] + "/" + userSettings.appendName + child.name + "_YuME.prefab", InteractionMode.AutomatedAction);
//#else
                                PrefabUtility.ReplacePrefab(tileParentObject, prefabAlreadyCreated, ReplacePrefabOptions.ReplaceNameBased); // replace the existing prefab with the updated data
//#endif
                            }
                            else
                            {
                                newPrefabsCounter++; // increment the counter of new prefabs for the finished log
//#if UNITY_2018_3_OR_NEWER
//                                PrefabUtility.SaveAsPrefabAsset(tileParentObject, userSettings.destinationFolder + "/" + masterTileName[0] + "/" + tileParentObject.name + "_YuME.prefab");
//#else
                                PrefabUtility.CreatePrefab(userSettings.destinationFolder + "/" + masterTileName[0] + "/" + tileParentObject.name + "_YuME.prefab", tileParentObject);
//#endif
                            }

                            tilesetData.tileData.Add(userSettings.destinationFolder + "/" + masterTileName[0] + "/" + tileParentObject.name + "_YuME.prefab");
                        }

                        tileImportProgress++;
                    }
                }
            }
        }

        string[] tileSetDataName = userSettings.destinationFolder.Split('/');

        //Debug.Log(tileSetDataName[tileSetDataName.Length - 1]);

        tilesetData.customBrushDestinationFolder = userSettings.destinationFolder + "/CustomBrushes";

        if (!AssetDatabase.IsValidFolder(tilesetData.customBrushDestinationFolder))
        {
            AssetDatabase.CreateFolder(userSettings.destinationFolder, "CustomBrushes");
        }

        tilesetData.tileSetName = tileSetDataName[tileSetDataName.Length - 1];
        AssetDatabase.CreateAsset(tilesetData, userSettings.destinationFolder + "/" + tileSetDataName[tileSetDataName.Length-1] + "_tileSet.asset");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh(); // refesh the asset database to tell unity changes have been made
 
        EditorUtility.ClearProgressBar();
    }

    static Vector3 setTheTileMeshObjectOffsetFromTheParent(Vector3 tileBoundsExtents)
    {
		tileBoundsExtents.x = tileBoundsExtents.x >= 0.5f ? tileBoundsExtents.x -= 0.5f : tileBoundsExtents.x = 0f;
		tileBoundsExtents.y = tileBoundsExtents.y >= 0.5f ? tileBoundsExtents.y -= 0.5f : tileBoundsExtents.y = 0f;
		tileBoundsExtents.z = tileBoundsExtents.z >= 0.5f ? tileBoundsExtents.z -= 0.5f : tileBoundsExtents.z = 0f;
        tileBoundsExtents.y = -0.5f;
        
        return tileBoundsExtents;
    }

    static void scanNumberOfMeshesToConvert() // pre-calculate how many tiles are present in the source tile set
    {
        totalMeshesInSourceObject = 0f;
        tileImportProgress = 1f;

        foreach (Transform child in userSettings.sourceTiles.transform)
        {
            if (!(child.name.Contains(userSettings.collisionIdentifier) || child.name.Contains(userSettings.customBoundsIdentifier)))
            {
                if (child.GetComponent<MeshFilter>() != null)
                {
                    if (child.GetComponent<MeshFilter>().sharedMesh != null)
                    {
                        totalMeshesInSourceObject++;
                    }
                }
            }
        }
    }

    static void attachCustomCollisionMesh()
    {
        // Check if a custom collision mesh is being used. If no mesh is found the mesh collider defauls to the mesh unless collsion importing is bypassed
        if (!userSettings.bypassCollisionSetup && userSettings.importCollision)
        {
			tileMeshCollider.sharedMesh = tileMeshFilter.sharedMesh;

			foreach (Transform collisionMesh in userSettings.sourceTiles.transform) // search the source file for the associated collision mesh based the identifer setup in the tool
            {
                if (collisionMesh.name == tileParentObject.name + userSettings.collisionIdentifier) // check to see if we found the collision mesh
                {
                    tileMeshCollider.sharedMesh = recenterMesh(collisionMesh.GetComponent<MeshFilter>().sharedMesh); // recenter the collision mesh so it matches the source mesh
                    return;
                }
            }
        }
		else if (!userSettings.bypassCollisionSetup && !userSettings.importCollision)
		{
            tileMeshCollider.sharedMesh = tileMeshFilter.sharedMesh;
		}
    }

    static Bounds findCustomBoundsMesh(Mesh sourceMesh)
    {
        Bounds customBounds = sourceMesh.bounds;

        if (userSettings.importCustomBounds)
        {
            foreach (Transform boundsMesh in userSettings.sourceTiles.transform) // search the source file for the associated collision mesh based the identifer setup in the tool
            {
                if (boundsMesh.name == tileParentObject.name + userSettings.customBoundsIdentifier) // check to see if we found the collision mesh
                {
                    if (boundsMesh.GetComponent<MeshFilter>().sharedMesh != null  && boundsMesh.GetComponent<MeshFilter>().sharedMesh.bounds != sourceMesh.bounds)
                    {
                        customBounds = boundsMesh.GetComponent<MeshFilter>().sharedMesh.bounds;
                    }
                }
            }
        }

		return customBounds;
    }

    static Mesh recenterMesh(Mesh sourceMesh)
    {
        // this code will take an object that has been imported with an offset position and
        // reposition it at 0,0,0

        Vector3[] vertices = sourceMesh.vertices;

        Bounds bounds = sourceMesh.bounds;
        lowestVertex = 0f;

        for (int i = 0; i < vertices.Length; i++)
        {
            if (vertices[i].y < lowestVertex)
            {
                lowestVertex = vertices[i].y;
            }

            vertices[i].x += -bounds.center.x;
            //vertices[i].y += -bounds.center.y;
            vertices[i].z += -bounds.center.z;

        }

        Mesh recenteredMesh = Object.Instantiate(sourceMesh) as Mesh;

        recenteredMesh.vertices = vertices;
        recenteredMesh.RecalculateBounds();

        AssetDatabase.CreateAsset(recenteredMesh, userSettings.meshFolder + "/" + sourceMesh.name + ".asset");

        return recenteredMesh;
    }

    static void revertScenePrefabs()
    {
        if(YuME_mapEditor.findTileMapParent())
        {
            foreach(Transform child in YuME_mapEditor.tileMapParent.transform)
            {
//#if UNITY_2018_3_OR_NEWER
//                PrefabUtility.RevertPrefabInstance(child.gameObject, InteractionMode.AutomatedAction);
//#else
                PrefabUtility.RevertPrefabInstance(child.gameObject);
//#endif
            }
        }
    }

    static void recreateCustomBrushes()
    {
        float customBrushCounter = 0f;

        GameObject[] customBrushes = YuTools_Utils.loadDirectoryContents(userSettings.destinationFolder + "/CustomBrushes", "*_YuME.prefab");

        foreach(GameObject brush in customBrushes)
        {
            EditorUtility.DisplayProgressBar("Rebuilding Custom Brushes", "Brush " + customBrushCounter + " / " + customBrushes.Length + " being exported", (float)(customBrushCounter / customBrushes.Length));

            Object prefabAlreadyCreated = AssetDatabase.LoadAssetAtPath(userSettings.destinationFolder + "/CustomBrushes/" + brush.name + ".prefab", typeof(GameObject));

            if (prefabAlreadyCreated != null)
            {
                GameObject newTileParent = new GameObject() ;
                newTileParent.transform.position = Vector3.zero;
                newTileParent.name = brush.name;
                newTileParent.AddComponent<YuME_tileGizmo>();

                foreach (Transform tile in brush.transform)
                {
                    string[] guids = AssetDatabase.FindAssets(tile.name);

                    if (guids != null)
                    {
                        GameObject sourceTile = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guids[0]), typeof(GameObject)) as GameObject;

                        GameObject newTile = (GameObject)Object.Instantiate(sourceTile);
                        newTile.transform.position = tile.position;
                        newTile.transform.eulerAngles = tile.eulerAngles;
                        newTile.transform.parent = newTileParent.transform;
                        newTile.name = tile.name;
                    }
                }

#if UNITY_2018_3_OR_NEWER
                PrefabUtility.SaveAsPrefabAssetAndConnect(newTileParent, userSettings.destinationFolder + "/CustomBrushes/" + brush.name + ".prefab", InteractionMode.AutomatedAction);
#else
                PrefabUtility.ReplacePrefab(newTileParent, prefabAlreadyCreated, ReplacePrefabOptions.ReplaceNameBased); // replace the existing prefab with the updated data
#endif
                DestroyImmediate(newTileParent);
                customBrushCounter++;
            }

            EditorUtility.ClearProgressBar();
        }
    }


    static void cleanUpImport()
    {
        Debug.Log("Yuponic Tile Importer finished: Updated " + updatedPrefabsCounter + " prefabs and Created " + newPrefabsCounter + " prefabs in Resources/" + userSettings.destinationFolder);
        DestroyImmediate(tileParentObject);
    }
}
