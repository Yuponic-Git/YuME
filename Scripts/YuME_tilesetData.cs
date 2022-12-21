using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class YuME_tilesetData : ScriptableObject
{
	public string tileSetName;
    public string customBrushDestinationFolder;
    public List<string> tileData = new List<string>();
    public List<string> customBrushData = new List<string>();
}
