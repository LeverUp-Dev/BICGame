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

        GridXSize = Mathf.CeilToInt(maxMapWidth / gridNodeDiameter);
        GridYSize = Mathf.CeilToInt(maxMapHeight / gridNodeDiameter);

        GridXSize += GridXSize % 2 == 0 ? 1 : 0;
        GridYSize += GridYSize % 2 == 0 ? 1 : 0;

        maxMapWidth = GridXSize * gridNodeDiameter;
        maxMapHeight = GridYSize * gridNodeDiameter;

        Grid = new CNode[GridYSize, GridXSize];

        Vector3 topLeftNodePosition = transform.position + (Vector3.left * maxMapWidth / 2f) + (Vector3.forward * maxMapHeight / 2f);
        topLeftNodePosition.x += gridNodeRadius;
        topLeftNodePosition.z -= gridNodeRadius;

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

    public void UpdateGrid()
    {
        for (int i = 0; i < GridYSize; ++i)
        {
            for (int j = 0; j < GridXSize; ++j)
            {
                Grid[i, j].Walkable = !Physics.CheckSphere(Grid[i, j].WorldPosition, gridNodeRadius, unwalkableMask);
            }
        }
    }

    public CNode GetNodeFromWorldPosition(Vector3 position)
    {
        int x = (int)(GridXSize * (position.x / maxMapWidth + 0.5f));
        int y = (int)(GridYSize * (-(position.z / maxMapHeight) + 0.5f));

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
                //Gizmos.color = node.Walkable ? Color.white : Color.red;
                Gizmos.color = node.Hallway ? Color.blue : (node.Walkable ? Color.white : Color.red);
                Gizmos.DrawCube(node.WorldPosition, Vector3.one * (gridNodeDiameter - gridLineWidth));
            }
        }
    }
}
