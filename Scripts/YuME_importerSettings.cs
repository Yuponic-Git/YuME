using UnityEngine;

[System.Serializable]
public class YuME_importerSettings : ScriptableObject
{
    // Importer Settings
    public GameObject sourceTiles = null;
    public Material sourceMaterial = null;
    public Texture sourceTexture = null;

    public bool tileStatic = true;

    public int shadowCastingMode = 1;
    public bool receiveShadows = true;

    public string altIdentifier = "_alt";

    public bool bypassCollisionSetup = false;
    public bool importCollision = true;
    public string collisionIdentifier = "_collision";

    public bool importCustomBounds = true;
    public string customBoundsIdentifier = "_bounds";

    public bool splitByType = false;

    public string destinationFolder = "Assets/";

    public string meshFolder = "Assets/";

    public string appendName = "";

    // Convertor Settings

    public string c_name = "Yuponic Tiles";

    public bool c_tileStatic = true;
    public float c_scale = 1f;
    public Vector3 c_offset = Vector3.zero;
    public int c_shadowCastingMode = 1;
    public bool c_receiveShadows = true;
    public bool c_bypassCollisionSetup = false;
    public string c_destinationFolder = "Assets/";
    public string c_sourceFolder = "Assets/";
    public string c_appendName = "";
}
