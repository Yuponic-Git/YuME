using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class YuME_mapEditor : EditorWindow
{
    //private static bool testGridDepth = true;

    // ----------------------------------------------------------------------------------------------------
    // ----- Editor Helpers and Settings
    // ----------------------------------------------------------------------------------------------------

    public static YuME_editorData editorData;
    public static YuME_editorPreferences editorPreferences;
    public static YuME_importerSettings userSettings;
    public static GameObject editorGameObject;
    public static GameObject tileMapParent;
    public static GameObject[] mapLayers = new GameObject[8];
    public static List<YuME_tilesetData> availableTileSets = new List<YuME_tilesetData>();
    public static List<string> availableTileSetsPath = new List<string>();
    public static List<GameObject> selectedTiles = new List<GameObject>();

    // NEW
    public static string currentMapName = "YuME_MapData";
    public static YuME_MapManager ref_MapManager;
    public static int currentMapIndex = 0;
    static int oldCurrentMapIndex = -99;
    public static string[] mapNames;
    public static bool mouseDown = false;
    static bool oldCustomBrushWarning = false;

    // END NEW

    public static string[] tileSetNames;
    public static GameObject[] currentTileSetObjects;
    static GameObject[] currentCustomBrushes;
    static Vector2 _scrollPosition;
    static brushOptions brushPallete = brushOptions.tilesetBrush;
    public static GameObject gridSceneObject;

    static Color _gridColorNormal = Color.black;
    static Color _gridColorBorder = Color.black;
    static Color _gridColorFill = Color.black;

	static Vector3 _brushSize = Vector3.one;

    static bool setupScene = false;
    
    public static bool eraseToolOverride = false;
    public static bool pickToolOverride = false;

    public static int currentBrushIndex = 0; // note - custom brushes should not effect this. It's used for cycling through the standard tiles.

    public static brushTypes currentBrushType = brushTypes.standardBrush;

    public static bool showWireBrush = true;
    public static bool showGizmos = false;

    public static float globalScale = 1f;
    static float _globalScale = 1f;
    static int gridType = 0;

    static string meshFolder;
    static List<Canvas> uiObjects;
    static bool uiState = true;

    public enum toolIcons
    {
        defaultTools,
        brushTool,
        pickTool,
        eraseTool,
        selectTool,
        showGizmos,
        isolateTool,
        gridUpTool,
        gridDownTool,
        rotateTool,
        rotateXTool,
        flipVerticalTool,
        flipHorizontalTool,
        refreshMap,
        copyTool,
        moveTool,
        customBrushTool,
        trashTool,
        isolateLayerTool,
        layerUp,
        layerDown,
        none
    }

    public enum brushOptions
    {
        tilesetBrush,
        customBrush
    }

    public enum brushTypes
    {
        standardBrush,
        customBrush,
        copyBrush
    }

    public static bool controlHeld = false;
    public static bool shiftHeld = false;
    public static bool altHeld = false;

    public static bool randomRotationMode = false;

    public static List<GameObject> isolatedGridObjects = new List<GameObject>();
    public static bool isolateTiles = false;
    public static List<GameObject> isolatedLayerObjects = new List<GameObject>();
    public static bool isolateLayer = false;

    // ----------------------------------------------------------------------------------------------------
    // ----- Editor Tools Variables
    // ----------------------------------------------------------------------------------------------------

    public static toolIcons selectedTool = toolIcons.brushTool;
    public static toolIcons previousSelectedTool;
    static float _tileRotation = 0f;
    static float _tileRotationX = 0f;
    static bool allowTileRedraw = true;
    static string currentScene;
    static int _currentTileSetIndex;
    static int _currentLayer = 1;
	static bool openConfig = false;

    // ----------------------------------------------------------------------------------------------------
    // ----- Scene Tools Variables
    // ----------------------------------------------------------------------------------------------------

    bool _toolEnabled;
    static public Vector3 tilePosition = Vector3.zero;
    static public bool validTilePosition = false;
    static Vector3 oldTilePosition = Vector3.zero;
    static float quantizedGridHeight = 0f;

    // ----------------------------------------------------------------------------------------------------
    // ----- Brush and Current Tile Variables
    // ----------------------------------------------------------------------------------------------------

    public static GameObject brushTile;
    public static GameObject currentTile;
    public static List<GameObject> tileChildObjects = new List<GameObject>();

    // ----------------------------------------------------------------------------------------------------
    // ----- ALT Tile Variables
    // ----------------------------------------------------------------------------------------------------

    public static bool useAltTiles = false;
    public static List<s_AltTiles> altTiles = new List<s_AltTiles>();

    public struct s_AltTiles
    {
        public string masterTile;
        public GameObject[] altTileObjects;
    }

    static int controlId;

    // ----------------------------------------------------------------------------------------------------

    [MenuItem("Window/Yuponic/YuME: Map Editor")]
    static void Initialize()
    {
        YuME_mapEditor tileMapEditorWindow = EditorWindow.GetWindow<YuME_mapEditor>(false, "Map Editor");
        tileMapEditorWindow.titleContent.text = "Map Editor";
    }

    void OnEnable()
    {
        editorData = ScriptableObject.CreateInstance<YuME_editorData>();
        AssetPreview.SetPreviewTextureCacheSize(1000);
        YuTools_Utils.disableTileGizmo(showGizmos);
        YuTools_Utils.addLayer("YuME_TileMap");

        YuME_brushFunctions.cleanSceneOfBrushObjects();

        string[] guids;

        // ----------------------------------------------------------------------------------------------------
        // ----- Load Editor Settings
        // ----------------------------------------------------------------------------------------------------

        guids = AssetDatabase.FindAssets("YuME_editorSetupData");
        editorData = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guids[0]), typeof(YuME_editorData)) as YuME_editorData;

        guids = AssetDatabase.FindAssets("YuME_editorPreferenceData");

        if (guids.Length == 0)
        {
           if (!AssetDatabase.IsValidFolder(YuTools_Utils.getAssetPath(editorData)+"Preferences"))
                AssetDatabase.CreateFolder(YuTools_Utils.removeLastFolderSlash(YuTools_Utils.getAssetPath(editorData)), "Preferences");

            editorPreferences = CreateInstance("YuME_editorPreferences") as YuME_editorPreferences;

            for(int i = 1; i < 9; i++)
            {
                editorPreferences.layerNames.Add("layer" + i);
                editorPreferences.layerFreeze.Add(true);
                editorPreferences.layerStatic.Add(true);
            }


            AssetDatabase.CreateAsset(editorPreferences, YuTools_Utils.getAssetPath(editorData)+"Preferences/"+"YuME_editorPreferenceData.asset");
        }
        else
        {
            editorPreferences = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guids[0]), typeof(YuME_editorPreferences)) as YuME_editorPreferences;
        }

        guids = AssetDatabase.FindAssets("YuME_importSettings");
        userSettings = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guids[0]), typeof(YuME_importerSettings)) as YuME_importerSettings;

        globalScale = editorPreferences.gridScaleFactor;

        importTileSets(false);
        loadPreviewTiles();
        loadCustomBrushes();

        _toolEnabled = toolEnabled;

        if (editorPreferences.twoPointFiveDMode == false)
        {
            gridType = 0;
        }
        else
        {
            gridType = 1;
        }

        updateGridColors();
        updateGridScale();
        updateGridType();

        gridHeight = 0;

        //YuME_brushFunctions.createBrushTile();

        YuTools_Utils.showUnityGrid(true);
        SceneView.RepaintAll();

        // ----------------------------------------------------------------------------------------------------
        // ----- Setup Scene Delegates
        // ----------------------------------------------------------------------------------------------------

        currentScene = EditorSceneManager.GetActiveScene().name;
#if UNITY_2019_1_OR_NEWER
        SceneView.duringSceneGui -= OnSceneGUI;
        SceneView.duringSceneGui += OnSceneGUI;
#else
        SceneView.onSceneGUIDelegate -= OnSceneGUI;
        SceneView.onSceneGUIDelegate += OnSceneGUI;
#endif 
#if UNITY_2017
        EditorApplication.hierarchyWindowChanged -= OnSceneChanged;
        EditorApplication.hierarchyWindowChanged += OnSceneChanged;
#else
        EditorApplication.hierarchyChanged -= OnSceneChanged;
        EditorApplication.hierarchyChanged += OnSceneChanged;
#endif

        findUI();
        if(toolEnabled)
        {
            showUI(false);
        }
    }

	static void OnSceneChanged()
	{
		if (currentScene != EditorSceneManager.GetActiveScene().name)
		{
			toolEnabled = false;
            YuTools_Utils.showUnityGrid(true);
			currentScene = EditorSceneManager.GetActiveScene().name;
		}
        YuME_sceneGizmoFunctions.displayGizmoGrid();
	}

    void OnDestroy()
    {
#if UNITY_2019_1_OR_NEWER
        SceneView.duringSceneGui -= OnSceneGUI;
#else
        SceneView.onSceneGUIDelegate -= OnSceneGUI;
#endif
#if UNITY_2017
        EditorApplication.hierarchyWindowChanged -= OnSceneChanged;
#else
        EditorApplication.hierarchyChanged -= OnSceneChanged;
#endif
    }

    void OnSelectionChange()
    {
        if (Selection.gameObjects.Length > 0 && selectedTiles.Count > 0)
        {
            selectedTiles.Clear();
        }
    }

    void Update()
    {
        if (!EditorApplication.isPlaying)
        {
            Repaint();
        }
        if(EditorApplication.isPlayingOrWillChangePlaymode && uiState == false)
        {
            showUI(true);
        }
    }

    // ----------------------------------------------------------------------------------------------------
    // ----- Draw Editor Tool GUI
    // ----------------------------------------------------------------------------------------------------

    void OnGUI()
    {
        if (!Application.isPlaying)
        {
            if (GameObject.Find("YuME_MapEditorObject") == null)
            {
                setupGUI();
            }
            else
            {
                if (!checkForForzenMap())
                {
                    mainGUI();
                }
                else
                {
                    unFreezeMap();
                }
            }
            updateGridType();
            updateGridScale();
        }
    }

    void setupGUI()
    {
        toolEnabled = false;

        EditorGUILayout.Space();

        GUILayout.Label(editorData.mapEditorHeader);

        EditorGUILayout.BeginVertical("box");

        setupScene = GUILayout.Toggle(setupScene, "Add YuME Map Editor Objects", "Button", GUILayout.Height(30));

        if (setupScene)
        {
            string[] guids;

            // ----------------------------------------------------------------------------------------------------
            // ----- Load Editor Settings
            // ----------------------------------------------------------------------------------------------------

            guids = AssetDatabase.FindAssets("YuME_MapEditorObject");
            GameObject tileParentPrefab = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guids[0]), typeof(GameObject)) as GameObject;

            tileMapParent = PrefabUtility.InstantiatePrefab(tileParentPrefab as GameObject) as GameObject;
            tileMapParent.transform.position = editorPreferences.initialOffset;

            tileMapParent.layer = LayerMask.NameToLayer("YuME_TileMap");
            YuME_mapManagerFunctions.buildNewMap("YuME_MapData");
            
            EditorSceneManager.MarkAllScenesDirty();
        }

        setupScene = false;

        EditorGUILayout.EndVertical();
    }

    void mainGUI()
    {
        if (Event.current != null)
        {
            // ----------------------------------------------------------------------------------------------------
            // ----- Check Keyboard and Mouse Shortcuts
            // ----------------------------------------------------------------------------------------------------

            YuME_keyboardShortcuts.checkKeyboardShortcuts(Event.current);
            YuME_mouseShorcuts.checkMouseShortcuts(Event.current);

            SceneView.RepaintAll();
        }

        EditorGUILayout.Space();

        GUILayout.Label(editorData.mapEditorHeader);

        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.BeginHorizontal();

        toolEnabled = GUILayout.Toggle(toolEnabled, "Enable YuME", "Button", GUILayout.Height(30));

        if (_toolEnabled != toolEnabled)
        {
            if (!toolEnabled)
            {
                showUI(true);
                YuTools_Utils.showUnityGrid(true);
                YuME_tileFunctions.restoreIsolatedGridTiles();
                YuME_tileFunctions.restoreIsolatedLayerTiles();
                YuME_brushFunctions.cleanSceneOfBrushObjects();
            }
            else
            {
                showUI(false);

                setTileBrush(0);
                YuTools_Utils.showUnityGrid(false);
            }

            SceneView.RepaintAll();
        }

        _toolEnabled = toolEnabled;

        openConfig = GUILayout.Toggle(openConfig, editorData.configButton, "Button", GUILayout.Width(30), GUILayout.Height(30));

        if (openConfig == true)
        {
            YuME_editorConfig editorConfig = EditorWindow.GetWindow<YuME_editorConfig>(true, "Editor Config");
            editorConfig.titleContent.text = "Editor Config";
        }

        openConfig = false;

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();

        displayMapList();

        EditorGUILayout.BeginVertical("box");
        gridDimensions = EditorGUILayout.Vector2Field("Grid Dimensions", gridDimensions);
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical("box");

        string[] gridLayout = new string[] { "Flat Grid", "2.5D Grid" };

        gridType = GUILayout.SelectionGrid(
            gridType,
            gridLayout,
            2,
            EditorStyles.toolbarButton
            );

        if(gridType == 0)
        {
            editorPreferences.twoPointFiveDMode = false;
        }
        else
        {
            editorPreferences.twoPointFiveDMode = true;
        }

        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.BeginHorizontal();

        quantizedGridHeight = gridHeight / globalScale;
        GUILayout.Label("Grid Height: " + quantizedGridHeight.ToString());

        GUILayout.Label("Brush Size: (" + brushSize.x.ToString() + "," + brushSize.y.ToString() + "," + brushSize.z.ToString() + ")");
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Pick The Tile Set To Use", EditorStyles.boldLabel);
        currentTileSetIndex = EditorGUILayout.Popup("Choose Tileset", currentTileSetIndex, tileSetNames);

        if (currentTileSetIndex != _currentTileSetIndex)
        {
            loadPreviewTiles();
            loadCustomBrushes();
        }

        if (GUILayout.Button("Reload Available Tilesets", GUILayout.Height(30)))
        {
            reloadTileSets();
        }

        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical("box");

        string[] buttonLabels = new string[] { "Tileset Brushes", "Custom Brushes" };

        brushPallete = (brushOptions)GUILayout.SelectionGrid(
            (int)brushPallete,
            buttonLabels,
            2,
            EditorStyles.toolbarButton
            );

        EditorGUILayout.EndVertical();

        drawTilePreviews();

        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.LabelField("Tile Preview Columns", EditorStyles.boldLabel);
        tilePreviewColumnWidth = EditorGUILayout.IntSlider(tilePreviewColumnWidth, 1, 10);

        EditorGUILayout.EndVertical();

        useAltTiles = GUILayout.Toggle(useAltTiles, "Use ALT Tiles", "Button", GUILayout.Height(20));

        bool freezeMap = false;
        freezeMap = GUILayout.Toggle(freezeMap, "Freeze Map", "Button", GUILayout.Height(20));

        if (freezeMap)
        {
            YuME_freezeMap.combineTiles();
        }

        updateGridColors();
        YuME_sceneGizmoFunctions.displayGizmoGrid();

        _currentTileSetIndex = currentTileSetIndex;
    }

    // ----------------------------------------------------------------------------------------------------
    // ----- Draw Scene Tools
    // ----------------------------------------------------------------------------------------------------

    static void OnSceneGUI(SceneView sceneView)
    {
        if (toolEnabled)
        {
            // ----------------------------------------------------------------------------------------------------
            // ----- Draw Scene Editor Tools
            // ----------------------------------------------------------------------------------------------------

            if (selectedTool > 0 && toolEnabled)
            {
                controlId = GUIUtility.GetControlID(FocusType.Passive);
                updateSceneMousePosition();
                checkTilePositionIsValid(sceneView.position);
                YuME_sceneGizmoFunctions.drawBrushGizmo();
            }

            // ----------------------------------------------------------------------------------------------------
            // ----- Draw Editor Tool Bar
            // ----------------------------------------------------------------------------------------------------

            YuME_editorSceneUI.drawToolUI(sceneView);

            // ----------------------------------------------------------------------------------------------------
            // ----- Check Keyboard and Mouse Shortcuts
            // ----------------------------------------------------------------------------------------------------

            YuME_keyboardShortcuts.checkKeyboardShortcuts(Event.current);
            YuME_mouseShorcuts.checkMouseShortcuts(Event.current);

            foreach (GameObject selected in selectedTiles)
            {
                if (selected != null)
                {
                    YuME_sceneGizmoFunctions.drawSceneGizmoCube(selected.transform.position, Vector3.one, Color.green);
                }
            }

            // ----------------------------------------------------------------------------------------------------
            // ----- Momenteray handling of the editor tool bar
            // ----------------------------------------------------------------------------------------------------

            switch (selectedTool)
            {
                case toolIcons.defaultTools:
                    YuME_brushFunctions.destroyBrushTile();
                    break;
                case toolIcons.brushTool:
                    YuME_brushFunctions.createBrushTile();
                    selectedTiles.Clear();
                    break;
                case toolIcons.pickTool:
                    YuME_brushFunctions.destroyBrushTile();
                    selectedTiles.Clear();
                    break;
                case toolIcons.eraseTool:
                    YuME_brushFunctions.destroyBrushTile();
                    selectedTiles.Clear();
                    break;
                case toolIcons.selectTool:
                    YuME_brushFunctions.destroyBrushTile();
                    break;
                case toolIcons.copyTool:
                    YuME_customBrushFunctions.createCopyBrush(false);
                    selectedTool = toolIcons.brushTool;
                    break;
                case toolIcons.moveTool:
                    YuME_customBrushFunctions.createCopyBrush(true);
                    selectedTool = toolIcons.brushTool;
                    break;
                case toolIcons.trashTool:
                    YuME_tileFunctions.trashTiles();
                    selectedTool = previousSelectedTool;
                    break;
                case toolIcons.customBrushTool:
                    selectedTool = previousSelectedTool;
                    YuME_customBrushFunctions.createCustomBrush();
                    break;
                case toolIcons.showGizmos:
                    showGizmos = !showGizmos;
                    YuTools_Utils.disableTileGizmo(showGizmos);
                    selectedTool = previousSelectedTool;
                    break;
                case toolIcons.gridUpTool:
					if(Event.current.alt)
					{
	                    gridHeight+=(globalScale * 0.25f);
					}
					else
					{
						gridHeight+=globalScale * editorPreferences.gridLayerHeightScaler;
					}
					selectedTool = previousSelectedTool;
                    break;
                case toolIcons.gridDownTool:
					if(Event.current.alt)
					{
						gridHeight-=(globalScale * 0.25f);
					}
					else
					{
						gridHeight-= globalScale * editorPreferences.gridLayerHeightScaler;
					}
                    selectedTool = previousSelectedTool;
                    break;
                case toolIcons.rotateTool:
                    tileRotation+=90f;
                    selectedTool = previousSelectedTool;
                    break;
                case toolIcons.rotateXTool:
                    tileRotationX += 90f;
                    selectedTool = previousSelectedTool;
                    break;
                case toolIcons.flipHorizontalTool:
                    YuME_tileFunctions.flipHorizontal();
                    selectedTool = previousSelectedTool;
                    break;
                case toolIcons.flipVerticalTool:
                    YuME_tileFunctions.flipVertical();
                    selectedTool = previousSelectedTool;
                    break;
                case toolIcons.isolateTool:
                    YuME_tileFunctions.isolateTilesToggle();
                    selectedTool = previousSelectedTool;
                    break;
                case toolIcons.isolateLayerTool:
                    YuME_tileFunctions.isolateLayerToggle();
                    selectedTool = previousSelectedTool;
                    break;
                case toolIcons.layerUp:
                    currentLayer++;
                    selectedTool = previousSelectedTool;
                    break;
                case toolIcons.layerDown:
                    currentLayer--;
                    selectedTool = previousSelectedTool;
                    break;
                case toolIcons.refreshMap:
                    YuME_mapManagerFunctions.refreshMap();
                    selectedTool = previousSelectedTool;
                    break;
            }

            // ----------------------------------------------------------------------------------------------------
            // ----- Check Scene View Inputs for Drawing, Picking etc.
            // ----------------------------------------------------------------------------------------------------

            if (selectedTool > toolIcons.defaultTools)
            {
                if ((Event.current.type == EventType.MouseDrag || Event.current.type == EventType.MouseDown) &&
                    Event.current.button == 0 &&
                    Event.current.alt == false &&
                    Event.current.shift == false &&
                    Event.current.control == false &&
                    allowTileRedraw)
                {
                    switch (selectedTool)
                    {
                        case toolIcons.brushTool:
                            switch (currentBrushType)
                            {
                                case brushTypes.standardBrush:
                                    addEraseTiles(false);
                                    break;
                                case brushTypes.customBrush:
                                    if (currentTile.GetComponent<YuME_tileGizmo>())
                                    {
                                        addEraseTiles(false); // Custom brushes now work the same as normal tiles
                                    }
                                    else
                                    {
                                        if(!oldCustomBrushWarning)
                                        {
                                            oldCustomBrushWarning = true;
                                            Debug.LogWarning("Please note: How custom brushes are created has been updated in YuME 1.1.2 and above. This is an old brush. We recommend recreating this using the new custom brush system for stability.");
                                            Debug.LogWarning("To increase stability, UNDO has been disabled on OLD custom brushes. New custom brushes use the same system as normal tiles and can be un-done.");
                                        }

                                        YuME_customBrushFunctions.pasteCustomBrush(tilePosition); // for legacy custom brushes
                                    }

                                    break;
                                case brushTypes.copyBrush:
                                    YuME_customBrushFunctions.pasteCopyBrush(tilePosition);
                                    break;
                            }
                            break;
                        case toolIcons.pickTool:
                            YuME_tileFunctions.pickTile(tilePosition);
                            break;
                        case toolIcons.eraseTool:
                            addEraseTiles(true);
                            break;
                        case toolIcons.selectTool:
                            YuME_tileFunctions.selectTile(tilePosition);
                            break;
                    }

                    allowTileRedraw = false;
                }
                else if ((Event.current.type == EventType.MouseDrag || Event.current.type == EventType.MouseDown) &&
                    Event.current.button == 0 &&
                    Event.current.alt == false &&
                    Event.current.shift == true &&
                    Event.current.control == false &&
                    allowTileRedraw)
                {
                    switch (selectedTool)
                    {
                        case toolIcons.brushTool:
                            addEraseTiles(true);
                            break;
                    }

                    allowTileRedraw = false;
                }
                else if ((Event.current.type == EventType.MouseDrag || Event.current.type == EventType.MouseDown) &&
                    Event.current.button == 0 &&
                    Event.current.alt == false &&
                    Event.current.shift == false &&
                    Event.current.control == true &&
                    allowTileRedraw)
                {
                    switch (selectedTool)
                    {
                        case toolIcons.brushTool:
                            YuME_tileFunctions.pickTile(tilePosition);
                            break;
                        case toolIcons.selectTool:
                            YuME_tileFunctions.deSelectTile(tilePosition);
                            break;
                    }

                    allowTileRedraw = false;
                }

                HandleUtility.AddDefaultControl(controlId);
            }

            if(showGizmos)
            {
                if(selectedTiles.Count > 0)
                {
                    foreach(GameObject tile in selectedTiles)
                    {
                        YuME_sceneGizmoFunctions.handleInfo data;
                        data.tileName = tile.name;
                        data.layer = tile.transform.parent.name;
                        data.grid = tile.transform.position.y;
                        YuME_sceneGizmoFunctions.drawTileInfo(tile.transform.position, data);
                    }
                }
            }

            // ----------------------------------------------------------------------------------------------------
            // ----- Scene Housekeeping
            // ----------------------------------------------------------------------------------------------------

            YuME_brushFunctions.updateBrushPosition();
            checkGlobalScale();
            repaintSceneView();
            previousSelectedTool = selectedTool;
        }
    }

    public static List<T> FindObjectsOfTypeAll<T>()
    {
        List<T> results = new List<T>();
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            var s = SceneManager.GetSceneAt(i);
            if (s.isLoaded)
            {
                var allGameObjects = s.GetRootGameObjects();
                for (int j = 0; j < allGameObjects.Length; j++)
                {
                    var go = allGameObjects[j];
                    results.AddRange(go.GetComponentsInChildren<T>(true));
                }
            }
        }
        return results;
    }

    static void reloadTileSets()
    {
        importTileSets(true);
        loadCustomBrushes();
        loadPreviewTiles();
    }

    static void findUI()
    {
        uiObjects = FindObjectsOfTypeAll<Canvas>();
        showUI(true);
    }

    static void showUI(bool showState)
    {
        if (editorPreferences.hideUIObjects)
        {
            uiState = showState;

            for (int i = 0; i < uiObjects.Count; i++)
            {
                if(uiObjects[i].gameObject == null)
                {
                    findUI();
                    break;
                }

                uiObjects[i].gameObject.SetActive(uiState);
            }
        }
    }

    static bool checkForForzenMap()
    {
        if (findTileMapParent())
        {
            foreach (Transform chid in tileMapParent.transform)
            {
                if (chid.gameObject.name == "frozenMap")
                {
                    toolEnabled = false;
                    YuTools_Utils.showUnityGrid(true);
                    return true;
                }
            }
        }

        return false;
    }

    void unFreezeMap()
    {
        toolEnabled = false;
        EditorGUILayout.Space();

        GUILayout.Label(editorData.mapEditorHeader);

        displayMapList();

        if (GUILayout.Button("Save frozen object", GUILayout.Height(30)))
        {
            string meshFolder = EditorUtility.OpenFolderPanel("Frozen Map Destination Folder", "", "");

            if (meshFolder == "")
            {
                meshFolder = "Assets/";
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            YuME_freezeMap.saveFrozenMesh(meshFolder);
        }

        EditorGUILayout.BeginVertical("box");

        if (GUILayout.Button("Unfreeze Map", GUILayout.Height(30)))
        {
            if (findTileMapParent())
            {
                foreach (Transform child in tileMapParent.transform)
                {
                    if (child.name.Contains("layer"))
                    {
                        child.gameObject.SetActive(true);
                    }
                    else if (child.gameObject.name == "frozenMap")
                    {
                        DestroyImmediate(child.gameObject);
                    }
                }
            }
            YuME_brushFunctions.destroyBrushTile();
            currentBrushType = YuME_mapEditor.brushTypes.standardBrush;
            setTileBrush(0);
            showUI(false);
            toolEnabled = true;
        }

        if(!uiState)
        {
            showUI(true);
        }


        YuME_sceneGizmoFunctions.displayGizmoGrid();

        EditorGUILayout.EndVertical();
    }

    public static void cloneMap(GameObject sourceMap)
    {
        GameObject mainMap = YuME_mapManagerFunctions.buildNewMap(sourceMap.name+" (clone)");
        Transform[] cloneLayers = mainMap.GetComponentsInChildren<Transform>();
        int cloneLayerIndex = 1;

        foreach (Transform layers in sourceMap.transform)
        {
            foreach(Transform tiles in layers)
            {
#if UNITY_2018_3_OR_NEWER
                GameObject clone = (GameObject)PrefabUtility.InstantiatePrefab(PrefabUtility.GetCorrespondingObjectFromSource(tiles.gameObject) as GameObject);
#else
                GameObject clone = (GameObject)PrefabUtility.InstantiatePrefab(PrefabUtility.GetPrefabParent(tiles.gameObject) as GameObject);
#endif
                if (clone != null)
                {
                    clone.transform.position = tiles.position;
                    clone.transform.localScale = tiles.localScale;
                    clone.transform.eulerAngles = tiles.eulerAngles;
                    clone.transform.parent = cloneLayers[cloneLayerIndex];
                }
            }
            cloneLayerIndex++;
        }
    }

    static void displayMapList()
    {
        if (ref_MapManager == null)
        {
            YuME_mapManagerFunctions.getGridSceneObjectReference();
        }

        mapNames = new string[ref_MapManager.mapList.Count];

        if (mapNames.Length > 0)
        {
            for (int i = 0; i < ref_MapManager.mapList.Count; i++)
            {
                if (ref_MapManager.mapList[i] != null)
                {
                    mapNames[i] = ref_MapManager.mapList[i].name;
                }
            }

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();

            currentMapIndex = EditorGUILayout.Popup(currentMapIndex, mapNames);

            if (oldCurrentMapIndex != currentMapIndex)
            {
                YuME_mapManagerFunctions.setActiveMap();
            }

            oldCurrentMapIndex = currentMapIndex;

            openConfig = GUILayout.Toggle(openConfig, "+", "Button", GUILayout.Width(30), GUILayout.Height(15));

            if (openConfig == true)
            {
                EditorWindow.GetWindow<YuME_MapManagerUI>(true, "Map Manager");
            }

            openConfig = false;

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }
        else
        {
            YuME_mapManagerFunctions.setDefaultMap();
        }
    }

    public static void checkGlobalScale()
    {
        globalScale = editorPreferences.gridScaleFactor;
        if (_globalScale != globalScale)
        {
            updateGridScale();
        }
    }

    public static void addEraseTiles(bool eraseOnly)
    {
        if (standardBrushSize)
        {
            YuME_tileFunctions.eraseTile(tilePosition);

            if (!eraseOnly)
                YuME_tileFunctions.addTile(tilePosition);
        }
        else
        {
            Vector3 newTilePos = tilePosition;

            for (int y = 0; y < (int)brushSize.y; y++)
            {
                newTilePos.y = tilePosition.y + (y * globalScale);
                newTilePos.z = tilePosition.z - (((brushSize.z - 1) * globalScale) / 2);

                for (int z = 0; z < (int)brushSize.z; z++)
                {
                    newTilePos.x = tilePosition.x - (((brushSize.x - 1) * globalScale) / 2);
                    for (int x = 0; x < (int)brushSize.x; x++)
                    {
                        YuME_tileFunctions.eraseTile(newTilePos);

                        if (!eraseOnly)
                            YuME_tileFunctions.addTile(newTilePos);

                        newTilePos.x += globalScale;
                    }
                        
                    newTilePos.z += globalScale;
                }
            }
        }
    }

    static void repaintSceneView()
    {
        if (tilePosition != oldTilePosition)
        {
            SceneView.RepaintAll();
            allowTileRedraw = true;
            oldTilePosition = tilePosition;
        }
    }

    // ----------------------------------------------------------------------------------------------------
    // ----- Tileset functions for preview in the editor window
    // ----------------------------------------------------------------------------------------------------

    public static void importTileSets(bool fullRescan)
    {
        // find all assest of type YuME_tileSetData
        string[] guids = AssetDatabase.FindAssets("t:YuME_tileSetData");

        if (guids.Length > 0)
        {
            availableTileSets = new List<YuME_tilesetData>();

            foreach (string guid in guids)
            {
                YuME_tilesetData tempData = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid), typeof(YuME_tilesetData)) as YuME_tilesetData;

                if (fullRescan)
                {
                    EditorUtility.DisplayProgressBar("Reloading Tile Set: " + tempData.name, "Note: Reimport can take some time to complete", 0f);
                    string path = YuTools_Utils.getAssetPath(tempData);
                    string[] containedPrefabs = YuTools_Utils.getDirectoryContents(YuTools_Utils.getAssetPath(tempData), "*.prefab");

                    if (containedPrefabs != null)
                    {
                        foreach (string prefab in containedPrefabs)
                        {
                            AssetDatabase.ImportAsset(path + prefab);
                        }
                    }
                    if (tempData != null)
                    {
                        path = YuTools_Utils.getAssetPath(tempData) + "CustomBrushes/";
                        containedPrefabs = YuTools_Utils.getDirectoryContents(YuTools_Utils.getAssetPath(tempData) + "CustomBrushes/", "*.prefab");

                        if (containedPrefabs != null)
                        {
                            foreach (string prefab in containedPrefabs)
                            {
                                AssetDatabase.ImportAsset(path + prefab);
                            }
                        }
                    }
                }

                availableTileSets.Add(tempData);
            }

            if (fullRescan)
            {
                EditorUtility.ClearProgressBar();
            }

            tileSetNames = new string[availableTileSets.Count];

            for (int i = 0; i < availableTileSets.Count; i++)
            {
                tileSetNames[i] = availableTileSets[i].tileSetName;
            }

			loadPreviewTiles();

        }
        else
        {
            Debug.Log("No tile sets have been created");
        }
    }

    static void loadPreviewTiles()
    {
        try
        {
            string path = YuTools_Utils.getAssetPath(availableTileSets[currentTileSetIndex]);

            currentTileSetObjects = YuTools_Utils.loadDirectoryContents(path, "*.prefab");

            altTiles = new List<s_AltTiles>();

            for (int i = 0; i < currentTileSetObjects.Length; i++)
            {
                if (AssetDatabase.IsValidFolder(path+ currentTileSetObjects[i].name))
                {
                    GameObject[] loadAltTiles = YuTools_Utils.loadDirectoryContents(path + currentTileSetObjects[i].name, "*.prefab");

                    s_AltTiles newAltTiles;
                    newAltTiles.masterTile = currentTileSetObjects[i].name;
                    newAltTiles.altTileObjects = loadAltTiles;

                    altTiles.Add(newAltTiles);
                }
            }

            currentTile = currentTileSetObjects[0];

        }
        catch
        {
            Debug.Log("Tile Sets seem to be missing. Please reload the tile sets");
        }
    }

    public static void loadCustomBrushes()
    {
        try
        {
            string path = YuTools_Utils.getAssetPath(availableTileSets[currentTileSetIndex]) + "CustomBrushes";

            if (path != null)
            {
                currentCustomBrushes = YuTools_Utils.loadDirectoryContents(path, "*_YuME.prefab");

                if (currentCustomBrushes == null)
                {
                    createCustomBrushFolder(path);
                }
            }
        }
        catch
        {
            Debug.Log("Custom Brush Folder missing");
        }
    }

    static void drawTilePreviews()
    {
        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

        int horizontalCounter = 0;

        EditorGUILayout.BeginHorizontal();

        if (brushPallete == brushOptions.tilesetBrush)
        {
            if (currentTileSetObjects != null)
            {
                for (int i = 0; i < currentTileSetObjects.Length; i++)
                {
                    if(currentTileSetObjects[i] != null)
                    {
                        //if (!currentTileSetObjects[i].name.Contains(userSettings.altIdentifier))
                        //{
                            EditorGUILayout.BeginVertical();

                            drawTileButtons(i);
                            EditorGUILayout.BeginHorizontal("Box");
                            EditorGUILayout.LabelField(currentTileSetObjects[i].name, GUILayout.MaxWidth(132));
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.EndVertical();

                            horizontalCounter++;

                            if (horizontalCounter == tilePreviewColumnWidth)
                            {
                                horizontalCounter = 0;
                                EditorGUILayout.EndHorizontal();
                                EditorGUILayout.BeginHorizontal();
                            }
                        //}
                    }
                }
            }
        }
        else if (brushPallete == brushOptions.customBrush)
        {
            if (currentCustomBrushes != null)
            {
                for (int i = 0; i < currentCustomBrushes.Length; i++)
                {
                    drawCustomBrushButtons(i);
                    horizontalCounter++;
                    if (horizontalCounter == tilePreviewColumnWidth)
                    {
                        horizontalCounter = 0;
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                    }
                }
            }
        }

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndScrollView();
    }

    static void drawTileButtons(int index)
    {
        if(currentTileSetObjects[index] != null)
        {
            //By passing a Prefab or GameObject into AssetPreview.GetAssetPreview you get a texture that shows this object
            Texture2D previewImage = AssetPreview.GetAssetPreview(currentTileSetObjects[index]);
            GUIContent buttonContent = new GUIContent(previewImage);

            bool isActive = false;

            if (currentTileSetObjects[index] != null && currentTile != null)
            {
                if (currentTile.name == currentTileSetObjects[index].name)
                {
                    isActive = true;
                }
            }

            bool isToggleDown = GUILayout.Toggle(isActive, buttonContent, GUI.skin.button);

            //If this button is clicked but it wasn't clicked before (ie. if the user has just pressed the button)
            if (isToggleDown == true && isActive == false)
            {
                setTileBrush(index);
            }
        }
    }

    public static void setTileBrush(int index)
    {
        if (currentTileSetObjects[index] != null)
        {
            currentBrushIndex = index;
            currentBrushType = brushTypes.standardBrush;
            currentTile = currentTileSetObjects[index];
            tileRotation = 0f;
            tileRotationX = 0f;
            YuME_brushFunctions.updateBrushTile();
            selectedTool = toolIcons.brushTool;
        }
    }

    static void drawCustomBrushButtons(int index)
    {
        if (currentTileSetObjects[index] != null)
        {
            Texture2D previewImage = AssetPreview.GetAssetPreview(currentCustomBrushes[index]);
            GUIContent buttonContent = new GUIContent(previewImage);

            bool isActive = false;

            if (currentTileSetObjects[index] != null && currentTile != null)
            {
                if (currentTile.name == currentCustomBrushes[index].name)
                {
                    isActive = true;
                }
            }

            EditorGUILayout.BeginVertical();

            bool isToggleDown = GUILayout.Toggle(isActive, buttonContent, GUI.skin.button);

            if (isToggleDown == true && isActive == false)
            {
                currentTile = currentCustomBrushes[index];
                currentBrushType = brushTypes.customBrush;
                tileRotation = 0f;
                tileRotationX = 0f;
                YuME_brushFunctions.updateBrushTile();
                selectedTool = toolIcons.brushTool;
            }

            if (GUILayout.Button("Delete Brush"))
            {
                if (EditorUtility.DisplayDialog("Delete Custom Brush?", "Are you sure you want to delete the custom brush prefab from the project", "Delete", "No"))
                {
                    string destinationPath = availableTileSets[currentTileSetIndex].customBrushDestinationFolder + "/";

                    if (currentCustomBrushes[index].GetComponent<YuME_tileGizmo>())
                    {
                        List<string> meshesToDelete = currentCustomBrushes[index].GetComponent<YuME_tileGizmo>().customBrushMeshName;

                        foreach (string deleteThis in meshesToDelete)
                        {
                            AssetDatabase.DeleteAsset(destinationPath + deleteThis + ".asset");
                        }
                    }

                    AssetDatabase.DeleteAsset(destinationPath + currentCustomBrushes[index].name + ".prefab");
                    loadCustomBrushes();
                }
            }

            EditorGUILayout.EndVertical();
        }
    }

    // ----------------------------------------------------------------------------------------------------
    // ----- Make sure the editor aways has the essential scene objects
    // ----------------------------------------------------------------------------------------------------
    
    public static bool findEditorGameObject()
    {
        editorGameObject = GameObject.Find("YuME_MapEditorObject");

        if (editorGameObject != null)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static bool findTileMapParent()
    {
        if (ref_MapManager != null)
        {
            if (ref_MapManager.mapList.Count > 0)
            {
                tileMapParent = ref_MapManager.mapList[currentMapIndex];
            }
            else
            {
                YuME_mapManagerFunctions.setDefaultMap();
            }
        }

        if (tileMapParent != null)
        {
            int i = 0;
            foreach (Transform child in tileMapParent.transform)
            {
                if (child.name.Contains("layer"))
                {
                    mapLayers[i] = child.gameObject;
                    i++;
                }
            }
            return true;
        }
        else
        {
            return false;
        }
    }

    void updateGridColors()
    {
        if (gridSceneObject == null)
        {
            gridSceneObject = GameObject.Find("YuME_MapEditorObject");
        }
        else
        {
            if (_gridColorBorder != editorPreferences.gridColorBorder || _gridColorFill != editorPreferences.gridColorFill || _gridColorNormal != editorPreferences.gridColorNormal)
            {
                gridSceneObject.GetComponent<YuME_GizmoGrid>().gridColorNormal = editorPreferences.gridColorNormal;
                gridSceneObject.GetComponent<YuME_GizmoGrid>().gridColorFill = editorPreferences.gridColorFill;
                gridSceneObject.GetComponent<YuME_GizmoGrid>().gridColorBorder = editorPreferences.gridColorBorder;

                _gridColorBorder = editorPreferences.gridColorBorder;
                _gridColorFill = editorPreferences.gridColorFill;
                _gridColorNormal = editorPreferences.gridColorNormal;

                SceneView.RepaintAll();
            }
        }
    }

    static void updateGridType()
    {
        if (gridSceneObject == null)
        {
            gridSceneObject = GameObject.Find("YuME_MapEditorObject");
        }
        else
        {
                gridSceneObject.GetComponent<YuME_GizmoGrid>().twoPointFiveDMode = editorPreferences.twoPointFiveDMode;
                SceneView.RepaintAll();
        }
    }

    static void updateGridScale()
    {
        if (gridSceneObject == null)
        {
            gridSceneObject = GameObject.Find("YuME_MapEditorObject");
        }
        else
        { 
            try
            {
                gridSceneObject.GetComponent<YuME_GizmoGrid>().tileSize = globalScale;
                gridSceneObject.GetComponent<YuME_GizmoGrid>().centreGrid = editorPreferences.centreGrid;
            }
            catch
            {
                gridSceneObject.GetComponent<YuME_GizmoGrid>().tileSize = 1f;
                gridSceneObject.GetComponent<YuME_GizmoGrid>().centreGrid = true;
            }
        }

        _globalScale = globalScale;
        //gridDimensions = gridDimensions;
    }

    // ----------------------------------------------------------------------------------------------------
    // ----- Check Mouse Positions
    // ----------------------------------------------------------------------------------------------------

    static void checkTilePositionIsValid(Rect sceneViewRect)
    {
        //Make sure the cube handle is only drawn when the mouse is within a position that we want
        //In this case we simply hide the cube cursor when the mouse is hovering over custom GUI elements in the lower
        //are of the sceneView which we will create in E07
        bool isInValidArea = Event.current.mousePosition.y < sceneViewRect.height - 35;

        if (isInValidArea != validTilePosition)
        {
            validTilePosition = isInValidArea;
            SceneView.RepaintAll();
        }
    }

    static void updateSceneMousePosition()
    {
        if (Event.current == null)
        {
            return;
        }

        Vector2 mousePosition = new Vector2(Event.current.mousePosition.x, Event.current.mousePosition.y);

        Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << LayerMask.NameToLayer("YuME_TileMap")) == true)
        {
            Vector3 shiftOffset = gridSceneObject.transform.position;
            shiftOffset.x = shiftOffset.x - (int)shiftOffset.x;
            shiftOffset.y = shiftOffset.y - (int)shiftOffset.y;
            shiftOffset.z = shiftOffset.z - (int)shiftOffset.z;

            if (!editorPreferences.twoPointFiveDMode)
            {
                tilePosition.x = Mathf.Round(((hit.point.x + shiftOffset.x) - hit.normal.x * 0.001f) / globalScale) * globalScale - shiftOffset.x;
                tilePosition.z = Mathf.Round(((hit.point.z + shiftOffset.z) - hit.normal.z * 0.001f) / globalScale) * globalScale - shiftOffset.z;
                tilePosition.y = gridHeight + gridSceneObject.transform.position.y;
            }
            else
            {
                tilePosition.x = Mathf.Round(((hit.point.x + shiftOffset.x) - hit.normal.x * 0.001f) / globalScale) * globalScale - shiftOffset.x;
                tilePosition.y = Mathf.Round(((hit.point.y + shiftOffset.y) - hit.normal.y * 0.001f) / globalScale) * globalScale - shiftOffset.y;
                tilePosition.z = gridHeight + gridSceneObject.transform.position.z;
            }
        }
    }

    // ----------------------------------------------------------------------------------------------------
    // ----- Custom Brush File System Functions
    // ----------------------------------------------------------------------------------------------------

    public static void createCustomBrushFolder(string path)
    {
        Debug.Log("Directory" + path + " is missing. Creating now."); // No folder can be found so lets create on
        string newPath = path.Replace("/CustomBrushes", "");
        AssetDatabase.CreateFolder(newPath, "CustomBrushes");
    }

    // ----------------------------------------------------------------------------------------------------
    // ----- Tool Settings
    // ----------------------------------------------------------------------------------------------------

    public static int toolSelect
    {
        get
        {
            return EditorPrefs.GetInt("selectedTool", 0);
        }
        set
        {
            EditorPrefs.SetInt("selectedTool", value);
        }
    }

    public static bool toolEnabled
    {
        get
        {
            return EditorPrefs.GetBool("toolEnabled", true);
        }
        set
        {
            EditorPrefs.SetBool("toolEnabled", value);
        }
    }

    public static int currentTileSetIndex
    {
        get
        {
            return EditorPrefs.GetInt("currentTileSetIndex", 0);
        }
        set
        {
            EditorPrefs.SetInt("currentTileSetIndex", value);
        }
    }

    public static int tilePreviewColumnWidth
    {
        get
        {
            return EditorPrefs.GetInt("tilePreviewColumnWidth", 2);
        }
        set
        {
            EditorPrefs.SetInt("tilePreviewColumnWidth", value);
        }
    }

    public static float gridHeight
    {
        get
        {
            GameObject tempGrid = GameObject.Find("YuME_MapEditorObject");

            if (tempGrid != null)
            {
                return GameObject.Find("YuME_MapEditorObject").GetComponent<YuME_GizmoGrid>().gridHeight;
            }
            else
            {
                return 0;
            }
        }
        set
        {
            GameObject tempGrid = GameObject.Find("YuME_MapEditorObject");

            if (tempGrid != null)
            {
                tempGrid.GetComponent<YuME_GizmoGrid>().gridHeight = value;
                tempGrid.GetComponent<YuME_GizmoGrid>().moveGrid();
            }
        }
    }

    public static float gridOffset
    {
        get
        {
            return editorPreferences.gridOffset;
        }
        set
        {
            GameObject gridTemp = GameObject.Find("YuME_MapEditorObject");
            editorPreferences.gridOffset = value;
            if(gridTemp != null)
            {
                gridTemp.GetComponent<YuME_GizmoGrid>().gridOffset = value;
            }
        }
    }

    public static Vector2 gridDimensions
    {
        get
        {
            return editorPreferences.gridDimensions;
        }
        set
        {
            editorPreferences.gridDimensions = value;

            GameObject gridTemp = GameObject.Find("YuME_MapEditorObject");

            if (gridTemp != null)
            {
                gridTemp.GetComponent<YuME_GizmoGrid>().gridWidth = (int)value.x;
                gridTemp.GetComponent<YuME_GizmoGrid>().gridDepth = (int)value.y;
                Vector3 tempGridSize;
                if (!editorPreferences.twoPointFiveDMode)
                {
                    tempGridSize.x = (int)value.x * globalScale;
                    tempGridSize.y = 0.1f;
                    tempGridSize.z = (int)value.y * globalScale;
                }
                else
                {
                    tempGridSize.x = (int)value.x * globalScale;
                    tempGridSize.y = (int)value.y * globalScale;
                    tempGridSize.z = 0.1f;
                }
                gridTemp.GetComponent<BoxCollider>().size = tempGridSize;
            }
            EditorUtility.SetDirty(editorData);
        }
    }

    public static bool standardBrushSize
    {
        get
        {
            if (brushSize.x == 1f && brushSize.y == 1f && brushSize.z == 1f)
                return true;
            else
                return false;
        }

    }

	public static Vector3 brushSize
	{
		get
		{
			return _brushSize;
		}
		set
		{
			_brushSize = value;

            if (_brushSize.x < 1f) _brushSize.x = 1f;
            if (_brushSize.y < 1f) _brushSize.y = 1f;
            if (_brushSize.z < 1f) _brushSize.z = 1f;
        }
    }

    public static int currentLayer
    {
        get
        {
            return _currentLayer;
        }
        set
        {
            _currentLayer = value;

            if (_currentLayer > 8)
            {
                _currentLayer = 8;
            }
            else if (_currentLayer < 1)
            {
                _currentLayer = 1;
            }
        }
    }

    public static float tileRotation
    {
        get
        {
            return _tileRotation;
        }
        set
        {
            _tileRotation = value;

            if (_tileRotation >= 360)
            {
                _tileRotation = 0f;
            }
            else if (_tileRotation < 0f)
            {
                _tileRotation = 270f;
            }

        }
    }

    public static float tileRotationX
    {
        get
        {
            return _tileRotationX;
        }
        set
        {
            _tileRotationX = value;

            if (_tileRotationX >= 360)
            {
                _tileRotationX = 0f;
            }
            else if (_tileRotationX < 0f)
            {
                _tileRotationX = 270f;
            }

        }
    }

    public static Vector3 pickTileScale = Vector3.zero;
    public static Vector3 tileScale = Vector3.one;
}
