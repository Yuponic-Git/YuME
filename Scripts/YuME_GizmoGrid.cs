using UnityEngine;

// V1.01: Updated to allow for grid scale values > 1

public class YuME_GizmoGrid : MonoBehaviour
{
    [HideInInspector]
    public float tileSize = 1;
    //[HideInInspector]
    public int gridWidth = 40;
    //[HideInInspector]
    public int gridDepth = 40;
    [HideInInspector]
    public bool twoPointFiveDMode = false;
    [HideInInspector]
    public bool centreGrid = true;

    [HideInInspector]
    public float gridHeight
    {
        get { return _gridHeight; }
        set { _gridHeight = value; }
    }

    float _gridHeight = 0;
    bool _toolEnabled = true;

    [HideInInspector]
    public bool toolEnabled {
        get { return _toolEnabled; }
        set { _toolEnabled = value; moveGrid(); }
    }

	public float gridOffset = 0.01f;
    float tileOffset = 0.5f;

    [HideInInspector]
    public Color gridColorNormal = Color.white;
    [HideInInspector]
    public Color gridColorBorder = Color.green;
    [HideInInspector]
    public Color gridColorFill = new Color(1, 0, 0, 0.5F);

    float gridWidthOffset;
    float gridDepthOffset;

    //bool testDepthGrid = true;

    Vector3 gridColliderPosition;

    void OnEnable()
    {
        gameObject.SetActive(false);
    }

    Vector3 gridMin;
    Vector3 gridMax;

    void OnDrawGizmos()
    {
        if (toolEnabled)
        {
            if (twoPointFiveDMode)
            {
                tileOffset = tileSize / 2;
                if (centreGrid)
                {
                    gridWidthOffset = gridWidth * tileSize / 2;
                    gridDepthOffset = gridDepth * tileSize / 2;
                }
                else
                {
                    gridWidthOffset = 0;
                    gridDepthOffset = 0;
                }
                gridMin.x = gameObject.transform.position.x - gridWidthOffset - tileOffset;
                gridMin.z = gameObject.transform.position.z + gridHeight - gridOffset + tileOffset;
                gridMin.y = gameObject.transform.position.y - gridDepthOffset - tileOffset;
                gridMax.x = gridMin.x + (tileSize * gridWidth);
                gridMax.y = gridMin.y + (tileSize * gridDepth);
                gridMax.z = gridMin.z;
            }
            else
            {
                tileOffset = tileSize / 2;
                if (centreGrid)
                {
                    gridWidthOffset = gridWidth * tileSize / 2;
                    gridDepthOffset = gridDepth * tileSize / 2;
                }
                else
                {
                    gridWidthOffset = 0;
                    gridDepthOffset = 0;
                }
                gridMin.x = gameObject.transform.position.x - gridWidthOffset - tileOffset;
                gridMin.y = gameObject.transform.position.y + gridHeight - tileOffset - gridOffset;
                gridMin.z = gameObject.transform.position.z - gridDepthOffset - tileOffset;
                gridMax.x = gridMin.x + (tileSize * gridWidth);
                gridMax.z = gridMin.z + (tileSize * gridDepth);
                gridMax.y = gridMin.y;
            }

            drawGridBase();
            drawMainGrid();
            drawGridBorder();

            moveGrid();
        }
    }

    public void moveGrid()
    {
        BoxCollider box = gameObject.GetComponent<BoxCollider>();
        box.enabled = toolEnabled;
        gridColliderPosition = box.center;

        if (twoPointFiveDMode)
        {
            if (centreGrid)
            {
                gridColliderPosition.x = 0 - tileOffset;
                gridColliderPosition.y = 0 - tileOffset;
                gridColliderPosition.z = 0 + gridHeight + tileOffset;
            }
            else
            {
                gridColliderPosition.x = gridWidth / 2 * tileSize - tileOffset;
                gridColliderPosition.y = gridDepth / 2 * tileSize - tileOffset;
                gridColliderPosition.z = gridHeight + tileOffset;
            }
        }
        else
        {
            if (centreGrid)
            {
                gridColliderPosition.x = 0 - tileOffset;
                gridColliderPosition.y = 0 + gridHeight - tileOffset;
                gridColliderPosition.z = 0 - tileOffset;
            }
            else
            {
                gridColliderPosition.x = 0 + gridWidth / 2 * tileSize - tileOffset;
                gridColliderPosition.y = 0 + gridHeight - tileOffset;
                gridColliderPosition.z = 0 + gridDepth / 2 * tileSize - tileOffset;
            }
        }

        box.center = gridColliderPosition;
    }

    private void drawGridBase() // fixed for scale
    {
        Gizmos.color = gridColorFill;

        if (twoPointFiveDMode)
        {
            if (centreGrid)
            {
                Gizmos.DrawCube(new Vector3(gameObject.transform.position.x - tileOffset,
                    gameObject.transform.position.y - tileOffset - gridOffset,
                    gameObject.transform.position.z + gridHeight - gridOffset + tileOffset),
                    new Vector3((gridWidth * tileSize), (gridDepth * tileSize), 0.01f));

            }
            else
            {
                Gizmos.DrawCube(new Vector3(gameObject.transform.position.x + (gridWidth / 2 * tileSize) - tileOffset,
                    gameObject.transform.position.y + (gridDepth / 2 * tileSize) - tileOffset - gridOffset,
                    gameObject.transform.position.z + gridHeight - gridOffset + tileOffset),
                    new Vector3((gridWidth * tileSize), (gridDepth * tileSize), 0.01f));
            }
        }
        else
        {
            if (centreGrid)
            {
                Gizmos.DrawCube(new Vector3(gameObject.transform.position.x - tileOffset, gameObject.transform.position.y + gridHeight - tileOffset - gridOffset, gameObject.transform.position.z - tileOffset),
                    new Vector3((gridWidth * tileSize), 0.01f, (gridDepth * tileSize)));
            }
            else
            {
                Gizmos.DrawCube(new Vector3(gameObject.transform.position.x + (gridWidth / 2 * tileSize) - tileOffset,
                    gameObject.transform.position.y + gridHeight - tileOffset - gridOffset,
                    gameObject.transform.position.z + (gridDepth / 2 * tileSize) - tileOffset),
                    new Vector3((gridWidth * tileSize), 0.01f, (gridDepth * tileSize)));
            }
        }
    }

    private void drawGridBorder() // fixed for scale
    {
        Gizmos.color = gridColorBorder;

        if (!twoPointFiveDMode)
        {
            // left side
            Gizmos.DrawLine(new Vector3(gridMin.x, gridMin.y, gridMin.z), new Vector3(gridMin.x, gridMin.y, gridMax.z));

            //bottom
            Gizmos.DrawLine(new Vector3(gridMin.x, gridMin.y, gridMin.z), new Vector3(gridMax.x, gridMin.y, gridMin.z));

            // Right side
            Gizmos.DrawLine(new Vector3(gridMax.x, gridMin.y, gridMin.z), new Vector3(gridMax.x, gridMin.y, gridMax.z));

            //top
            Gizmos.DrawLine(new Vector3(gridMin.x, gridMin.y, gridMax.z), new Vector3(gridMax.x, gridMin.y, gridMax.z));
        }
        else
        {
            // left side
            Gizmos.DrawLine(new Vector3(gridMin.x, gridMin.y, gridMin.z), new Vector3(gridMin.x, gridMax.y, gridMin.z));

            //bottom
            Gizmos.DrawLine(new Vector3(gridMin.x, gridMin.y, gridMin.z), new Vector3(gridMax.x, gridMin.y, gridMin.z));

            // Right side
            Gizmos.DrawLine(new Vector3(gridMax.x, gridMin.y, gridMin.z), new Vector3(gridMax.x, gridMax.y, gridMin.z));

            //top
            Gizmos.DrawLine(new Vector3(gridMin.x, gridMax.y, gridMin.z), new Vector3(gridMax.x, gridMax.y, gridMin.z));
        }
    }


    private void drawMainGrid() // fixed for scale
    {
        Gizmos.color = gridColorNormal;

        if (tileSize != 0)
        {
            if (!twoPointFiveDMode)
            {
                for (float i = tileSize; i < (gridWidth * tileSize); i += tileSize)
                {
                    Gizmos.DrawLine(
                        new Vector3((float)i + gridMin.x, gridMin.y, gridMin.z),
                        new Vector3((float)i + gridMin.x, gridMin.y, gridMax.z)
                        );
                }
            }
            else
            {
                for (float i = tileSize; i < (gridWidth * tileSize); i += tileSize)
                {
                    Gizmos.DrawLine(
                        new Vector3((float)i + gridMin.x, gridMin.y, gridMin.z),
                        new Vector3((float)i + gridMin.x, gridMax.y, gridMin.z)
                        );
                }
            }
        }

        if (tileSize != 0)
        {
            if (!twoPointFiveDMode)
            {
                for (float j = tileSize; j < (gridDepth * tileSize); j += tileSize)
                {
                    Gizmos.DrawLine(
                        new Vector3(gridMin.x, gridMin.y, j + gridMin.z),
                        new Vector3(gridMax.x, gridMin.y, j + gridMin.z)
                        );
                }
            }
            else
            {
                for (float j = tileSize; j < (gridDepth * tileSize); j += tileSize)
                {
                    Gizmos.DrawLine(
                        new Vector3(gridMin.x, j+ gridMin.y, gridMin.z),
                        new Vector3(gridMax.x, j+ gridMin.y, gridMin.z)
                        );
                }
            }
        }
    }
}
