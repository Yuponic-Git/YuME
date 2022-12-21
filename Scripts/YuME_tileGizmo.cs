using UnityEngine;
using System.Collections.Generic;

public class YuME_tileGizmo : MonoBehaviour
{
    [HideInInspector]
    public List<string> customBrushMeshName;

    void OnDrawGizmos()
    {
        Gizmos.DrawIcon(transform.position, "YuME_tileGizmo", true);
    }
}
