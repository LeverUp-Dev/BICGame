using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CGrid : MonoBehaviour
{
    CNode[,] grid;

    public LayerMask unwalkableMask;
    public bool visible;

    public int maxMapWidth;
    public int maxMapHeight;

    public int GridXSize { get; set; }
    public int GridYSize { get; set; }

    public int gridNodeDiameter;
    float gridNodeRadius;
    public float gridLineWidth;

    void Awake()
    {
        GenerateGrid();
    }

    void GenerateGrid()
    {
        gridNodeRadius = gridNodeDiameter / 2f;

        GridXSize = maxMapWidth / gridNodeDiameter + (maxMapWidth % gridNodeDiameter >= gridNodeRadius ? 1 : 0);
        GridYSize = maxMapHeight / gridNodeDiameter + (maxMapHeight % gridNodeDiameter >= gridNodeRadius ? 1 : 0);

        grid = new CNode[GridXSize, GridYSize];

        Vector3 topLeftNodePosition = transform.position + (Vector3.left * maxMapWidth / 2f) + (Vector3.forward * maxMapHeight / 2f);
        topLeftNodePosition.x += gridNodeRadius;
        topLeftNodePosition.z -= gridNodeRadius;

        for (int i = 0; i < GridYSize; ++i)
        {
            for (int j = 0; j < GridXSize; ++j)
            {
                Vector3 position = new Vector3(topLeftNodePosition.x + j * gridNodeDiameter, 0, topLeftNodePosition.z - i * gridNodeDiameter);
                bool walkable = !Physics.CheckSphere(position, gridNodeRadius, unwalkableMask);

                grid[i, j] = new CNode(position, walkable);
            }
        }
    }

    void OnDrawGizmos()
    {
        if (grid != null && visible)
        {
            foreach (CNode node in grid)
            {
                Gizmos.color = node.Walkable ? Color.white : Color.red;
                Gizmos.DrawCube(node.Position, Vector3.one * (gridNodeDiameter - gridLineWidth));
            }
        }
    }
}
