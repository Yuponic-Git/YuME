using UnityEngine;
using UnityEditor;

public class YuME_keyboardShortcuts : EditorWindow 
{
	public static void checkKeyboardShortcuts(Event keyEvent)
	{
        YuME_mapEditor.eraseToolOverride = false;
        YuME_mapEditor.pickToolOverride = false;

        if (keyEvent.shift && !keyEvent.control && !keyEvent.alt)
        {
            if (YuME_mapEditor.selectedTool == YuME_mapEditor.toolIcons.brushTool)
            {
                YuME_mapEditor.eraseToolOverride = true;
            }
        }
        else if (keyEvent.control && !keyEvent.alt && !keyEvent.shift)
        {
            if (YuME_mapEditor.selectedTool == YuME_mapEditor.toolIcons.brushTool)
            {
                YuME_mapEditor.pickToolOverride = true;
            }

        }

        if (keyEvent.type == EventType.KeyDown)
        {
            switch (keyEvent.keyCode)
            {
                case KeyCode.Escape:
                    Event.current.Use();
                    YuME_mapEditor.selectedTool = YuME_mapEditor.toolIcons.defaultTools;
                    YuME_mapEditor.currentBrushType = YuME_mapEditor.brushTypes.standardBrush;
                    break;
                case KeyCode.Equals:
                    Event.current.Use();
                    YuME_mapEditor.gridHeight += YuME_mapEditor.globalScale;
                    break;
                case KeyCode.Minus:
                    Event.current.Use();
                    YuME_mapEditor.gridHeight -= YuME_mapEditor.globalScale;
                    break;
                case KeyCode.LeftBracket:
                    if (YuME_mapEditor.selectedTool == YuME_mapEditor.toolIcons.brushTool || YuME_mapEditor.selectedTool == YuME_mapEditor.toolIcons.eraseTool)
                    {
                        Event.current.Use();
                        Vector3 newBrushSize = YuME_mapEditor.brushSize;
                        newBrushSize.x -= 2;
                        newBrushSize.z -= 2;
                        YuME_mapEditor.brushSize = newBrushSize;
                        SceneView.RepaintAll();
                    }
                    break;
                case KeyCode.RightBracket:
                    if (YuME_mapEditor.selectedTool == YuME_mapEditor.toolIcons.brushTool || YuME_mapEditor.selectedTool == YuME_mapEditor.toolIcons.eraseTool)
                    {
                        Event.current.Use();
                        Vector3 newBrushSize = YuME_mapEditor.brushSize;
                        newBrushSize.x += 2;
                        newBrushSize.z += 2;
                        YuME_mapEditor.brushSize = newBrushSize;
                        SceneView.RepaintAll();
                    }
                    break;
                case KeyCode.LeftArrow:
                    if (YuME_mapEditor.selectedTool == YuME_mapEditor.toolIcons.brushTool || YuME_mapEditor.selectedTool == YuME_mapEditor.toolIcons.eraseTool)
                    {
                        Event.current.Use();
                        Vector3 newBrushSize = YuME_mapEditor.brushSize;
                        newBrushSize.x -= 2;
                        YuME_mapEditor.brushSize = newBrushSize;
                        SceneView.RepaintAll();
                    }
                    break;
                case KeyCode.RightArrow:
                    if (YuME_mapEditor.selectedTool == YuME_mapEditor.toolIcons.brushTool || YuME_mapEditor.selectedTool == YuME_mapEditor.toolIcons.eraseTool)
                    {
                        Event.current.Use();
                        Vector3 newBrushSize = YuME_mapEditor.brushSize;
                        newBrushSize.x += 2;
                        YuME_mapEditor.brushSize = newBrushSize;
                        SceneView.RepaintAll();
                    }
                    break;
                case KeyCode.DownArrow:
                    if (YuME_mapEditor.selectedTool == YuME_mapEditor.toolIcons.brushTool || YuME_mapEditor.selectedTool == YuME_mapEditor.toolIcons.eraseTool)
                    {
                        if (Event.current.shift)
                        {
                            Event.current.Use();
                            Vector3 newBrushSize = YuME_mapEditor.brushSize;
                            newBrushSize.y -= 1;
                            YuME_mapEditor.brushSize = newBrushSize;
                            SceneView.RepaintAll();
                        }
                        else
                        {
                            Event.current.Use();
                            Vector3 newBrushSize = YuME_mapEditor.brushSize;
                            newBrushSize.z -= 2;
                            YuME_mapEditor.brushSize = newBrushSize;
                            SceneView.RepaintAll();
                        }
                    }
                    break;
                case KeyCode.UpArrow:
                    if (YuME_mapEditor.selectedTool == YuME_mapEditor.toolIcons.brushTool || YuME_mapEditor.selectedTool == YuME_mapEditor.toolIcons.eraseTool)
                    {
                        if (Event.current.shift)
                        {
                            Event.current.Use();
                            Vector3 newBrushSize = YuME_mapEditor.brushSize;
                            newBrushSize.y += 1;
                            YuME_mapEditor.brushSize = newBrushSize;
                            SceneView.RepaintAll();
                        }
                        else
                        {
                            Event.current.Use();
                            Vector3 newBrushSize = YuME_mapEditor.brushSize;
                            newBrushSize.z += 2;
                            YuME_mapEditor.brushSize = newBrushSize;
                            SceneView.RepaintAll();
                        }
                    }
                    break;
                case KeyCode.Return:
                    if (YuME_mapEditor.selectedTool == YuME_mapEditor.toolIcons.brushTool || YuME_mapEditor.selectedTool == YuME_mapEditor.toolIcons.eraseTool)
                    {
                        Event.current.Use();
                        YuME_mapEditor.setTileBrush(0);
                        YuME_mapEditor.brushSize = Vector3.one;
                        YuME_mapEditor.selectedTiles.Clear();
                        SceneView.RepaintAll();
                    }
                    else
                    {
                        Event.current.Use();
                        YuME_mapEditor.selectedTiles.Clear();
                        SceneView.RepaintAll();
                    }
                    break;
                case KeyCode.Z:
                    Event.current.Use();
                    YuME_mapEditor.tileRotation -= 90f;
                    break;
                case KeyCode.X:
                    Event.current.Use();
                    YuME_mapEditor.tileRotation += 90f;
                    break;
                case KeyCode.I:
                    Event.current.Use();
                    YuME_tileFunctions.isolateTilesToggle();
                    break;
                case KeyCode.C:
                    Event.current.Use();
                    YuME_tileFunctions.flipVertical();
                    break;
                case KeyCode.V:
                    Event.current.Use();
                    YuME_tileFunctions.flipHorizontal();
                    break;
                case KeyCode.B:
                    Event.current.Use();
                    YuME_mapEditor.tileRotationX -= 90f;
                    break;
                case KeyCode.N:
                    Event.current.Use();
                    YuME_mapEditor.tileRotationX += 90f;
                    break;
                case KeyCode.Space:
                    if (YuME_mapEditor.selectedTool == YuME_mapEditor.toolIcons.selectTool || YuME_mapEditor.selectedTool == YuME_mapEditor.toolIcons.defaultTools)
                    {
                        Event.current.Use();
                        YuME_tileFunctions.selectAllTiles();
                    }
                    break;
                case KeyCode.R:
                    if (YuME_mapEditor.selectedTool == YuME_mapEditor.toolIcons.brushTool)
                    {
                        Event.current.Use();
                        YuME_mapEditor.randomRotationMode = !YuME_mapEditor.randomRotationMode;
                    }
                    break;
            }

            if (!YuME_mapEditor.editorPreferences.useAlternativeKeyShortcuts)
            {
                switch (keyEvent.keyCode)
                {
                    case KeyCode.A:
                        Event.current.Use();
                        YuME_mapEditor.selectedTool = YuME_mapEditor.toolIcons.brushTool;
                        break;
                    case KeyCode.S:
                        Event.current.Use();
                        YuME_mapEditor.selectedTool = YuME_mapEditor.toolIcons.pickTool;
                        break;
                    case KeyCode.D:
                        Event.current.Use();
                        YuME_mapEditor.selectedTool = YuME_mapEditor.toolIcons.eraseTool;
                        break;
                }
            }
            else
            {
                switch (keyEvent.keyCode)
                {
                    case KeyCode.G:
                        Event.current.Use();
                        YuME_mapEditor.selectedTool = YuME_mapEditor.toolIcons.brushTool;
                        break;
                    case KeyCode.H:
                        Event.current.Use();
                        YuME_mapEditor.selectedTool = YuME_mapEditor.toolIcons.pickTool;
                        break;
                    case KeyCode.J:
                        Event.current.Use();
                        YuME_mapEditor.selectedTool = YuME_mapEditor.toolIcons.eraseTool;
                        break;

                }
            }
        }
	}
}
