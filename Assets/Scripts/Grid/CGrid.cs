using Hypocrites.Defines;
using System.Collections.Generic;
using UnityEngine;

namespace Hypocrites.Grid
{
    public class CGrid : MonoBehaviour
    {
        public static CGrid Instance { get; private set; }

        public CNode[,] Grid { get; private set; }

        public int GridXSize { get; private set; }
        public int GridYSize { get; private set; }

        public float GridNodeRadius { get; private set; }

        /* Inspector Values */
        [field: SerializeField] public bool Visible { get; private set; }
        [field: SerializeField] public LayerMask UnwalkableMask { get; private set; }
        [field: SerializeField] public int MaxMapWidth { get; private set; }
        [field: SerializeField] public int MaxMapHeight { get; private set; }
        [field: SerializeField] public int GridNodeDiameter { get; private set; }
        [field: SerializeField] public float GridLineWidth { get; private set; }

        public HashSet<Directions> unBlock = new HashSet<Directions>();
        public readonly bool[] closeWay = new bool[4];

        void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);

            // ¾À ÀüÈ¯ ½Ã¿¡ ½Ì±ÛÅæ °´Ã¼°¡ ÆÄ±«µÇÁö ¾Êµµ·Ï À¯Áö
            DontDestroyOnLoad(gameObject);

            GenerateGrid();
        }

        void GenerateGrid()
        {
            GridNodeRadius = GridNodeDiameter / 2f;

            GridXSize = Mathf.CeilToInt(MaxMapWidth / GridNodeDiameter);
            GridYSize = Mathf.CeilToInt(MaxMapHeight / GridNodeDiameter);

            GridXSize += GridXSize % 2 == 0 ? 1 : 0;
            GridYSize += GridYSize % 2 == 0 ? 1 : 0;

            MaxMapWidth = GridXSize * GridNodeDiameter;
            MaxMapHeight = GridYSize * GridNodeDiameter;

            Grid = new CNode[GridYSize, GridXSize];

            Vector3 topLeftNodePosition = transform.position + (Vector3.left * MaxMapWidth / 2f) + (Vector3.forward * MaxMapHeight / 2f);
            topLeftNodePosition.x += GridNodeRadius;
            topLeftNodePosition.z -= GridNodeRadius;

            for (int i = 0; i < GridYSize; ++i)
            {
                for (int j = 0; j < GridXSize; ++j)
                {
                    Vector3 position = new Vector3(topLeftNodePosition.x + j * GridNodeDiameter, 0, topLeftNodePosition.z - i * GridNodeDiameter);
                    bool walkable = !Physics.CheckSphere(position, GridNodeRadius, UnwalkableMask);

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
                    Grid[i, j].Walkable = !Physics.CheckSphere(Grid[i, j].WorldPosition, GridNodeRadius, UnwalkableMask);
                }
            }
        }

        public CNode GetNodeFromWorldPosition(Vector3 position)
        {
            int x = (int)(GridXSize * (position.x / MaxMapWidth + 0.5f));
            int y = (int)(GridYSize * (-(position.z / MaxMapHeight) + 0.5f));

            if (x < 0 || x >= GridXSize)
                return null;

            if (y < 0 || y >= GridYSize)
                return null;

            return Grid[y, x];
        }

        void OnDrawGizmos()
        {
            if (Grid != null && Visible)
            {
                foreach (CNode node in Grid)
                {
                    //Gizmos.color = node.Walkable ? Color.white : Color.red;
                    Gizmos.color = node.Hallway ? Color.blue : (node.Walkable ? Color.white : Color.red);
                    Gizmos.DrawCube(node.WorldPosition, Vector3.one * (GridNodeDiameter - GridLineWidth));
                }
            }
        }
    }
}
