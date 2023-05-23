using UnityEngine;

public class CGrid : MonoBehaviour
{
    public static CGrid instance;

    public CNode[,] Grid { get; private set; }

    public LayerMask unwalkableMask;
    public bool visible;

    public int maxMapWidth;
    public int maxMapHeight;

    public int GridXSize { get; private set; }
    public int GridYSize { get; private set; }

    public int gridNodeDiameter;
    float gridNodeRadius;
    public float gridLineWidth;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        // ¾À ÀüÈ¯ ½Ã¿¡ ½Ì±ÛÅæ °´Ã¼°¡ ÆÄ±«µÇÁö ¾Êµµ·Ï À¯Áö
        DontDestroyOnLoad(gameObject);

        GenerateGrid();
    }

    void GenerateGrid()
    {
        gridNodeRadius = gridNodeDiameter / 2f;

        GridXSize = maxMapWidth / gridNodeDiameter + (maxMapWidth % gridNodeDiameter >= gridNodeRadius ? 1 : 0);
        GridYSize = maxMapHeight / gridNodeDiameter + (maxMapHeight % gridNodeDiameter >= gridNodeRadius ? 1 : 0);

        Grid = new CNode[GridXSize, GridYSize];

        Vector3 topLeftNodePosition = transform.position + (Vector3.left * maxMapWidth / 2f) + (Vector3.forward * maxMapHeight / 2f);
        /*topLeftNodePosition.x += gridNodeRadius;
        topLeftNodePosition.z -= gridNodeRadius;*/

        for (int i = 0; i < GridYSize; ++i)
        {
            for (int j = 0; j < GridXSize; ++j)
            {
                Vector3 position = new Vector3(topLeftNodePosition.x + j * gridNodeDiameter, 0, topLeftNodePosition.z - i * gridNodeDiameter);
                bool walkable = !Physics.CheckSphere(position, gridNodeRadius, unwalkableMask);

                Grid[i, j] = new CNode(position, j, i, walkable);
            }
        }
    }

    public CNode GetNodeFromWorldPosition(Vector3 position)
    {
        int x = Mathf.RoundToInt(GridXSize * (position.x / maxMapWidth + 0.5f));
        int y = Mathf.RoundToInt(GridYSize * (-(position.z / maxMapHeight) + 0.5f));

        if (x < 0 || x >= GridXSize)
            return null;

        if (y < 0 || y >= GridYSize)
            return null;

        return Grid[y, x];
    }

    void OnDrawGizmos()
    {
        if (Grid != null && visible)
        {
            foreach (CNode node in Grid)
            {
                Gizmos.color = node.Walkable ? Color.white : Color.red;
                Gizmos.DrawCube(node.WorldPosition, Vector3.one * (gridNodeDiameter - gridLineWidth));
            }
        }
    }
}
