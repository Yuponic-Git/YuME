using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "YuME_prefabSamplerSettings", menuName = "Yuponic/Utils", order = 1)]
public class YuME_prefabSamplerData : ScriptableObject
{
    // Importer Settings
    public string destinationFolder = "Assets/";
    public string appendName = "";
    public int yPivotType = 0;
    public string[] yPivotTypes = new string[3];
    public Texture2D configButton;
}
