using UnityEngine;
using UnityEditor;

class YuME_tileConverter : EditorWindow
{
    public static YuME_editorData editorData;

    public static YuME_importerSettings userSettings;
    public static YuME_tilesetData tilesetData;

    static GameObject tileParentObject;

    static int updatedPrefabsCounter = 0;
    static int newPrefabsCounter = 0;
    static float tileImportProgress = 1f;

    static Vector2 _scrollPosition;

    public enum importSettings { newTileset, reimport, added };

    [MenuItem("Window/Yuponic/YuME: Tile Converter")]
    static void Initialize()
    {
        YuME_tileConverter tileConverterWindow = EditorWindow.GetWindow<YuME_tileConverter>(false, "Tile Converter");
        tileConverterWindow.titleContent.text = "Tile Converter";
    }

    void OnEnable()
    {
		EditorUtility.ClearProgressBar();
        editorData = ScriptableObject.CreateInstance<YuME_editorData>();

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
        // ------ Tile Converter Data
        // ----------------------------------------------------------------------------------------------------

		EditorGUILayout.Space();
		GUILayout.Label(editorData.tileconverterHeader);

        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

        EditorGUILayout.LabelField("Tile Converter Settings", EditorStyles.boldLabel);

        // ----------------------------------------------------------------------------------------------------
        // ------ Folder Settings
        // ----------------------------------------------------------------------------------------------------

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Select Tile Prefab Destination Folder", EditorStyles.boldLabel);

        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.LabelField("Select Source Prefab Folders");

        if (GUILayout.Button("Prefab Source Folder", GUILayout.Height(30)))
        {
            userSettings.c_sourceFolder = EditorUtility.OpenFolderPanel("Tile Prefab Source Folder", "", "");

            if (userSettings.c_sourceFolder == "")
            {
                userSettings.c_sourceFolder = "Assets/";
            }
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Select Tile Destination Folder");

        if (GUILayout.Button("Tile Destination", GUILayout.Height(30)))
        {
            userSettings.c_destinationFolder = EditorUtility.OpenFolderPanel("Tile Prefab Destination Folder", "", "");

            if (userSettings.c_destinationFolder == "")
            {
                userSettings.c_destinationFolder = "Assets/";
            }
        }

        userSettings.c_destinationFolder = YuTools_Utils.shortenAssetPath(userSettings.c_destinationFolder);

        EditorGUILayout.Space();
        EditorGUILayout.EndVertical();

        userSettings.c_sourceFolder = YuTools_Utils.shortenAssetPath(userSettings.c_sourceFolder);

        // ----------------------------------------------------------------------------------------------------
        // ------ Name Settings
        // ----------------------------------------------------------------------------------------------------

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Tile Set Name", EditorStyles.boldLabel);

        EditorGUILayout.BeginVertical("box");

        userSettings.c_name = EditorGUILayout.TextField("Set Tile Set Name", userSettings.c_name);

        EditorGUILayout.EndVertical();

        // ----------------------------------------------------------------------------------------------------
        // ------ Scale Settings
        // ----------------------------------------------------------------------------------------------------

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Scale Adjustment", EditorStyles.boldLabel);

        EditorGUILayout.BeginVertical("box");

        userSettings.c_scale = EditorGUILayout.FloatField("Rescale Factor", userSettings.c_scale);

        EditorGUILayout.EndVertical();

        // ----------------------------------------------------------------------------------------------------
        // ------ Offset Settings
        // ----------------------------------------------------------------------------------------------------

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Offset Adjustment", EditorStyles.boldLabel);

        EditorGUILayout.BeginVertical("box");

        userSettings.c_offset = EditorGUILayout.Vector3Field("Offset Values", userSettings.c_offset);

        EditorGUILayout.EndVertical();

        // ----------------------------------------------------------------------------------------------------
        // ------ Append Settings
        // ----------------------------------------------------------------------------------------------------

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Custom Append Label", EditorStyles.boldLabel);

		EditorGUILayout.BeginVertical("box");

        userSettings.c_appendName = EditorGUILayout.TextField("Append to Tile Name", userSettings.c_appendName);

		EditorGUILayout.EndVertical();

        // ----------------------------------------------------------------------------------------------------
        // ------ Import Button 
        // ----------------------------------------------------------------------------------------------------

        EditorGUILayout.Space();

        if (userSettings.c_sourceFolder != "" && userSettings.c_destinationFolder != "")
		{
			if (GUILayout.Button("Convert Tileset", GUILayout.Height(40)))
            {
				bool tileDestinationFolderWarning = true;

				if(userSettings.c_destinationFolder == "Assets/")
				{
					tileDestinationFolderWarning = EditorUtility.DisplayDialog("Tile Prefabs will be created in the root Assets Folder",
						"Are you sure you want create the tile prefabs in the root Asset/ folder?", "OK", "Cancel");
				}

				if(tileDestinationFolderWarning)
				{
					createTileSet(importSettings.newTileset);
                    recreateCustomBrushes(userSettings.c_destinationFolder);
					cleanUpImport();
				}
            }
		}
		else
		{
			EditorGUILayout.HelpBox("Set a source and destination directories to start conversion", MessageType.Warning);			
		}

        // ----------------------------------------------------------------------------------------------------
        // ------ Reimport Button 
        // ----------------------------------------------------------------------------------------------------

        //EditorGUILayout.Space();

        if (userSettings.c_sourceFolder != "" && userSettings.c_destinationFolder != "")
        {
            EditorGUILayout.HelpBox("Use reimport if you are happy with your scale settings but wish to add new tiles to the exisiting set.", MessageType.Info);
            if (GUILayout.Button("Reimport Tileset", GUILayout.Height(40)))
            {
                bool tileDestinationFolderWarning = true;

                if (userSettings.c_destinationFolder == "Assets/")
                {
                    tileDestinationFolderWarning = EditorUtility.DisplayDialog("Tile Prefabs will be created in the root Assets Folder",
                        "Are you sure you want create the tile prefabs in the root Asset/ folder?", "OK", "Cancel");
                }

                if (tileDestinationFolderWarning)
                {
                    createTileSet(importSettings.reimport);
                    recreateCustomBrushes(userSettings.c_destinationFolder);
                    cleanUpImport();
                }
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Set a source and destination directories to start conversion", MessageType.Warning);
        }

        // ----------------------------------------------------------------------------------------------------
        // ------ Just Add Button 
        // ----------------------------------------------------------------------------------------------------

        //EditorGUILayout.Space();

        if (userSettings.c_sourceFolder != "" && userSettings.c_destinationFolder != "")
        {
            EditorGUILayout.HelpBox("Use to only add new tiles to the Tile Set.", MessageType.Info);
            if (GUILayout.Button("Add to Tileset", GUILayout.Height(40)))
            {
                bool tileDestinationFolderWarning = true;

                if (userSettings.c_destinationFolder == "Assets/")
                {
                    tileDestinationFolderWarning = EditorUtility.DisplayDialog("Tile Prefabs will be created in the root Assets Folder",
                        "Are you sure you want create the tile prefabs in the root Asset/ folder?", "OK", "Cancel");
                }

                if (tileDestinationFolderWarning)
                {
                    createTileSet(importSettings.added);
                    recreateCustomBrushes(userSettings.c_destinationFolder);
                    cleanUpImport();
                }
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Set a source and destination directories to start conversion", MessageType.Warning);
        }


        // ----------------------------------------------------------------------------------------------------
        // ------ Output Folder Information
        // ----------------------------------------------------------------------------------------------------

        EditorGUILayout.Space();

        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.LabelField("Source Prefab Folder:", EditorStyles.boldLabel);
        EditorGUILayout.LabelField(userSettings.c_sourceFolder);
        EditorGUILayout.LabelField("Tile Output Folder:", EditorStyles.boldLabel);
        EditorGUILayout.LabelField(userSettings.c_destinationFolder);
        
		EditorGUILayout.EndVertical();

        EditorGUILayout.EndScrollView();

		// ----------------------------------------------------------------------------------------------------
		// ------ GUI Changed
		// ----------------------------------------------------------------------------------------------------

        if (GUI.changed)
        {
			EditorUtility.SetDirty(userSettings);
        }
    }

    static void createTileSet(importSettings import)
    {
        updatedPrefabsCounter = 0;
        newPrefabsCounter = 0;
        tileImportProgress = 0f;

        tilesetData = ScriptableObject.CreateInstance<YuME_tilesetData>();
        tilesetData.tileSetName = userSettings.c_name;

        GameObject[] sourcePrefabs;

        sourcePrefabs = YuTools_Utils.loadDirectoryContents(userSettings.c_sourceFolder, "*.prefab");

        if (sourcePrefabs.Length == 0)
        {
            sourcePrefabs = YuTools_Utils.loadDirectoryContents(userSettings.c_sourceFolder, "*.obj");
        }

        if (sourcePrefabs.Length == 0)
        {
            sourcePrefabs = YuTools_Utils.loadDirectoryContents(userSettings.c_sourceFolder, "*.fbx");
        }

        bool validTile = false;
        
		foreach (GameObject child in sourcePrefabs)
        {
            // display the conversion progress based on the number of tiles found on the source tile set
            EditorUtility.DisplayProgressBar("Building Tile Set", "Tile " + tileImportProgress + " / " + sourcePrefabs.Length + " being exported", (float)(tileImportProgress / sourcePrefabs.Length));

			validTile = convertPrefabNew(child.gameObject, child.name, import);

            if (!validTile)
            {
                Debug.Log("---- invalid Tile");
            }

            tileImportProgress++;
        }

        if (!AssetDatabase.IsValidFolder(userSettings.c_destinationFolder + "/CustomBrushes"))
        {
            AssetDatabase.CreateFolder(userSettings.c_destinationFolder, "CustomBrushes");
        }

        try
        {
            tilesetData.customBrushDestinationFolder = userSettings.c_destinationFolder + "/CustomBrushes";
            AssetDatabase.CreateAsset(tilesetData, userSettings.c_destinationFolder + "/" + userSettings.c_name + "_tileSet.asset");
        }
        catch
        {
            Debug.Log("Destination folder is invalid. Please select a different folder.");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh(); // refesh the asset database to tell unity changes have been made

        EditorUtility.ClearProgressBar();
    }

    static bool convertPrefabNew(GameObject tile, string name, importSettings importType)
    {
        GameObject pivotGameObject = new GameObject();
        GameObject newTile = (GameObject)Object.Instantiate(tile);

        tileParentObject = new GameObject("Temporary Tile Parent Object");
        tileParentObject.isStatic = userSettings.c_tileStatic;
        tileParentObject.name = userSettings.c_appendName + name;
        tileParentObject.AddComponent<YuME_tileGizmo>();

        newTile.transform.parent = pivotGameObject.transform;

        pivotGameObject.name = "pivotObject";
        pivotGameObject.transform.parent = tileParentObject.transform;
        pivotGameObject.transform.position = Vector3.zero + userSettings.c_offset;
        pivotGameObject.isStatic = userSettings.c_tileStatic;
        Vector3 newScale = new Vector3(userSettings.c_scale, userSettings.c_scale, userSettings.c_scale);
        pivotGameObject.transform.localScale = newScale;

        // Try and load an exisiting version of the prefab
        GameObject prefabAlreadyCreated = AssetDatabase.LoadAssetAtPath(userSettings.c_destinationFolder + "/" + userSettings.c_appendName + name + "_YuME.prefab", typeof(GameObject)) as GameObject;


        // Check if the prefab already exisits and update it - or create a new prefab
        if (prefabAlreadyCreated != null)
        {
            if(importType == importSettings.reimport)
            {
                GameObject restorePivot = prefabAlreadyCreated.transform.GetChild(0).gameObject;
                pivotGameObject.transform.position = restorePivot.transform.localPosition;
                pivotGameObject.transform.eulerAngles = restorePivot.transform.eulerAngles;
                pivotGameObject.transform.localScale = restorePivot.transform.localScale;
            }
            updatedPrefabsCounter++; // increment the counter of updated prefabs for the finished log
#if UNITY_2018_3_OR_NEWER
            PrefabUtility.SaveAsPrefabAssetAndConnect(tileParentObject, userSettings.c_destinationFolder + "/" + userSettings.c_appendName + name + "_YuME.prefab", InteractionMode.AutomatedAction);
#else
            PrefabUtility.ReplacePrefab(tileParentObject, prefabAlreadyCreated, ReplacePrefabOptions.ReplaceNameBased); // replace the existing prefab with the updated data
#endif
        }
        else
        {
            newPrefabsCounter++; // increment the counter of new prefabs for the finished log
#if UNITY_2018_3_OR_NEWER
            PrefabUtility.SaveAsPrefabAsset(tileParentObject, userSettings.c_destinationFolder + "/" + tileParentObject.name + "_YuME.prefab");
#else
            PrefabUtility.CreatePrefab(userSettings.c_destinationFolder + "/" + tileParentObject.name + "_YuME.prefab", tileParentObject);
#endif
        }

        DestroyImmediate(tileParentObject);

        return true;
    }

    static void revertScenePrefabs()
    {
        if(YuME_mapEditor.findTileMapParent())
        {
            foreach(Transform child in YuME_mapEditor.tileMapParent.transform)
            {
#if UNITY_2018_3_OR_NEWER
                PrefabUtility.RevertPrefabInstance(child.gameObject, InteractionMode.AutomatedAction);
#else
                PrefabUtility.RevertPrefabInstance(child.gameObject);
#endif
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

    public static void recreateCustomBrushes(string path)
    {
        float customBrushCounter = 0f;

        GameObject[] customBrushes = YuTools_Utils.loadDirectoryContents(path + "/CustomBrushes", "*_YuME.prefab");

        if (customBrushes != null)
        {
            foreach (GameObject brush in customBrushes)
            {
                EditorUtility.DisplayProgressBar("Rebuilding Custom Brushes", "Brush " + customBrushCounter + " / " + customBrushes.Length + " being exported", (float)(customBrushCounter / customBrushes.Length));

                Object prefabAlreadyCreated = AssetDatabase.LoadAssetAtPath(path + "/CustomBrushes/" + brush.name + ".prefab", typeof(GameObject));

                if (prefabAlreadyCreated != null)
                {
                    GameObject newTileParent = new GameObject();
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
                    PrefabUtility.SaveAsPrefabAssetAndConnect(newTileParent, path + "/CustomBrushes/" + brush.name + ".prefab", InteractionMode.AutomatedAction);
#else
                    PrefabUtility.ReplacePrefab(newTileParent, prefabAlreadyCreated, ReplacePrefabOptions.ReplaceNameBased); // replace the existing prefab with the updated data
#endif
                    DestroyImmediate(newTileParent);
                    customBrushCounter++;
                }

                EditorUtility.ClearProgressBar();
            }
        }
    }


    static void cleanUpImport()
    {
        Debug.Log("Yuponic Tile Converter finished: Updated " + updatedPrefabsCounter + " prefabs and Created " + newPrefabsCounter + " prefabs in " + userSettings.c_destinationFolder);
    }
}
