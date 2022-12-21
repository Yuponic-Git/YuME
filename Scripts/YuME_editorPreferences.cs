using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class YuME_editorPreferences : ScriptableObject
{
    public List<string> layerNames = new List<string>();
    public List<bool> layerFreeze = new List<bool>();
    public List<bool> layerStatic = new List<bool>();
    public bool convexCollision = false;
    public float iconWidth = 32f;
    public Color brushCursorColor = new Color(1f, 0.78f,0.52f,1f);
    public Color pickCursorColor = new Color(0.35f, 0.93f, 0.45f, 1f);
    public Color eraseCursorColor = new Color(1f, 0f, 0f, 1f);
    public float gridHeight = 0;
    public Color gridColorNormal = new Color(0.32f, 0.28f, 0.28f, 1f);
    public Color gridColorBorder = new Color(0.19f, 0.34f, 0.54f, 1f);
    public Color gridColorFill = new Color(0.36f, 0.36f, 0.36f, 0.78F);
    public float gridOffset = 0.01f;
    public float gridLayerHeightScaler = 1f;
    public Vector2 gridDimensions = new Vector2(40f, 40f);
    public float gridScaleFactor = 1f;
    public bool invertMouseWheel = false;
    public Vector3 initialOffset = Vector3.zero;
    public bool twoPointFiveDMode = false;
    public bool centreGrid = true;
    public bool useAlternativeKeyShortcuts = false;
    public bool hideUIObjects = false;
}
