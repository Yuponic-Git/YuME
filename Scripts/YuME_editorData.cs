using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class YuME_editorData : ScriptableObject
{
	public Texture2D mapEditorHeader;
	public Texture2D tileImporterHeader;
	public Texture2D tileconverterHeader;
    public Texture2D configButton;
    public List<Texture> primaryIconData = new List<Texture>();
    public List<string> primaryIconTooltip = new List<string>();
    public List<Texture> secondaryIconData = new List<Texture>();
    public List<string> secondaryIconTooltip = new List<string>();
    public List<Texture> selectIconData = new List<Texture>();
    public List<string> selectionIconTooltip = new List<string>();
    public List<Texture> layerIconData = new List<Texture>();
    public List<string> layerIconTooltip = new List<string>();
}
