/*
#define DEBUG_DRAW
#define DEBUG_DRAW_TRIANGLE
//#define DEBUG_DRAW_CIRCUMCIRCLE
*/

#if DEBUG_DRAW
using System.Collections;
#endif
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hypocrites.Map
{
    using Hypocrites.Map.DS;
    using Hypocrites.Map.Elements;
    using Hypocrites.Map.AStar;
    using Hypocrites.Map.Enumerations;
    using Hypocrites.Grid;
    using Hypocrites.Enumerations;

    public class MapGenerator : MonoBehaviour
    {
        GameObject mapHierarchyRoot;
        List<Room> rooms;

        /* Inspector Values */
        [field: SerializeField] public bool Run { get; private set; }

        [field: SerializeField] public int MaxRoomCount { get; private set; }
        [field: SerializeField] public float CycleHallwayCreationChance { get; private set; }
        [field: SerializeField] public int RoomMinimumDistance { get; private set; }

        [field: SerializeField] public GameObject UnitRoomPrefab { get; private set; }
        [field: SerializeField] public GameObject WallNodePrefab { get; private set; }
        [field: SerializeField] public GameObject FloorNodePrefab { get; private set; }
        [field: SerializeField] public GameObject HallwayFloorNodePrefab { get; private set; }
        [field: SerializeField] public GameObject DebugLinePrefab { get; private set; }

        [field: SerializeField] public int Floor { get; private set; }

#if DEBUG_DRAW
        List<GameObject> lines;
        List<GameObject> circles;
        List<GameObject> edges;
#endif

        void Awake()
        {
            if (CycleHallwayCreationChance < 0 || CycleHallwayCreationChance > 100)
            {
                CycleHallwayCreationChance = 12.5f;
            }

            if (Run)
            {
                // ���� ���� �����ϱ⿡ �׸��� ���� ����� ������ Ȯ��
                CGrid grid = CGrid.Instance;

                if (grid == null)
                {
                    throw new System.Exception("���� �����Ϸ��� �׸��� �ý����� �߰��ؾ� �մϴ�. Hierarchy�� �� ������Ʈ�� �����ϰ� Grid/CGrid.cs�� �߰����ּ���.");
                }

                float mapArea = grid.MaxMapWidth * grid.MaxMapHeight;
                float roomArea = Mathf.PI * Mathf.Pow(RoomMinimumDistance, 2);

                if (mapArea / roomArea < MaxRoomCount)
                {
                    throw new System.Exception("�׸��� ���� ���̰� ���� ���� �����ϱ⿡ ������� �ʽ��ϴ�. �׸��带 �����ų� ������ ���� ���� �ٿ��ּ���.");
                }

                Generate();
            }
        }

        void Generate()
        {
            mapHierarchyRoot = new GameObject("Map");
            mapHierarchyRoot.transform.SetParent(transform);

            rooms = new List<Room>();

            // 1. �������� �� ��ġ ����
            InitializeRandomRooms();

            // 2. ��γ� �ﰢ���� ���� (���� : https://www.gorillasun.de/blog/bowyer-watson-algorithm-for-delaunay-triangulation/)
#if DEBUG_DRAW
            StartCoroutine(Triangulate(rooms));
#else
            List<Triangle> triangulatedList = Triangulate(rooms);

            // 3. ������� �ﰢ������ �׷����� ��ȯ
            int[,] triangulatedGraph = TriangulatedToGraph(triangulatedList);

            // 4. �׷����� Minimum Spanning Tree�� ��ȯ
            Edge[,] mstTree = GraphToMST(triangulatedGraph, triangulatedList[0].R1);

            // 5. ���õ��� ���� ���� �� �������� ������ MST Tree�� �߰� (��ȯ ���� ����)
            AddCycledEdgeToMST(triangulatedGraph, mstTree);

            // 6. �� ������Ʈ ���� (���� ��ΰ� ���� �� ��ġ�� ���޾� �������� ��츦 �����ϱ� ���� ���� �Ϻθ� ����)
            InstantiateRooms(false, true);

            // 7. A* �˰��� ����
            AStarPathfinder astar = new AStarPathfinder();
            CGrid grid = CGrid.Instance;
            List<List<AStarNode>> hallwayPaths = new List<List<AStarNode>>();

            for (int i = 0; i < MaxRoomCount; ++i)
            {
                for (int j = 0; j < MaxRoomCount; ++j)
                {
                    if (mstTree[i, j] != null)
                    {
                        Edge edge = mstTree[i, j];
                        hallwayPaths.Add(astar.FindPath(grid.GetNodeFromWorldPosition(edge.p1.Position), grid.GetNodeFromWorldPosition(edge.p2.Position)));

                        mstTree[j, i] = null;
                    }
                }
            }

            // 8. (6)���� �������� �ʾҴ� �� �߰�
            InstantiateRooms(true, false);

            // 9. ���� ����
            InstantiateHallways(hallwayPaths);
#endif
        }

        void InitializeRandomRooms()
        {
            LayerMask roomPositionLayer = LayerMask.GetMask("RoomPosition");

            for (int i = 0; i < MaxRoomCount; ++i)
            {
                // �� �߾��� �׸��� ������ �����Ǹ� ���� �׸��� �ٱ����� ����Ƿ� ���� ���� ����
                int randomX = Random.Range(10, CGrid.Instance.GridXSize - 10);
                int randomZ = Random.Range(10, CGrid.Instance.GridYSize - 10);

                CNode node = i == 0 ? CGrid.Instance.GetNodeFromWorldPosition(Vector3.zero) : CGrid.Instance.Grid[randomX, randomZ];
                Vector3 randomPos = node.WorldPosition;
                bool isDuplicated = false;

                if (!node.Walkable)
                {
                    --i;
                    continue;
                }

                for (int j = 0; j < i; ++j)
                {
                    if (randomPos.Equals(rooms[j].Position))
                    {
                        isDuplicated = true;
                        break;
                    }
                }

                if (isDuplicated || Physics.CheckSphere(randomPos, RoomMinimumDistance, roomPositionLayer))
                {
                    --i;
                    continue;
                }

                // TODO ������Ʈ â���� Ȯ�� ���� �����ϵ��� ����
                RoomType roomType;
                int random = Random.Range(0, 10);

                if (random == 0)
                    roomType = RoomType.R17x17;
                else if (random <= 3)
                    roomType = RoomType.R13x13;
                else
                    roomType = RoomType.R9x9;

                rooms.Add(new Room(i, randomPos, roomType, Instantiate(UnitRoomPrefab, randomPos, Quaternion.identity), mapHierarchyRoot.transform));
            }
        }

        #region ��γ� �ﰢ���� �޼ҵ�
        Triangle SuperTriangle()
        {
            int maxWidth = CGrid.Instance.MaxMapWidth;
            int maxHeight = CGrid.Instance.MaxMapHeight;

            // vertex�� SuperTriangle�� �ʹ� ������ �����Ǹ� ����� ������� �ʾ� offset���� �Ÿ� ����
            float offset = 1.1f;

            Vector3 p1 = new Vector3(-maxWidth * 3, 0, -maxHeight * offset);
            Vector3 p2 = new Vector3(maxWidth * 3f, 0, -maxHeight * offset);
            Vector3 p3 = new Vector3(0, 0, maxHeight * 3f * offset);

            return new Triangle(new Room(p1), new Room(p2), new Room(p3));
        }

#if DEBUG_DRAW
        IEnumerator Triangulate(List<Room> vertices)
#else
        List<Triangle> Triangulate(List<Room> vertices)
#endif
        {
            Triangle superTriangle = SuperTriangle();

            List<Triangle> triangles = new List<Triangle>();
            triangles.Add(superTriangle);

            foreach (Room room in vertices)
            {
#if (DEBUG_DRAW && DEBUG_DRAW_TRIANGLE)
                DrawTriangles(triangles);
#endif
#if (DEBUG_DRAW && DEBUG_DRAW_CIRCUMCIRCLE)
                DrawCircumCircles(triangles);
#endif

                triangles = AddVertex(triangles, room);

#if DEBUG_DRAW
                //yield return new WaitForSeconds(1f);
#endif

#if (DEBUG_DRAW && DEBUG_DRAW_TRIANGLE)
                ClearTriangles();
#endif
#if (DEBUG_DRAW && DEBUG_DRAW_CIRCUMCIRCLE)
                ClearCircumCircles();
#endif
            }

            triangles = triangles.Where(triangle =>
            {
                return !(triangle.R1.Equals(superTriangle.R1) || triangle.R1.Equals(superTriangle.R2) || triangle.R1.Equals(superTriangle.R3) ||
                    triangle.R2.Equals(superTriangle.R1) || triangle.R2.Equals(superTriangle.R2) || triangle.R2.Equals(superTriangle.R3) ||
                    triangle.R3.Equals(superTriangle.R1) || triangle.R3.Equals(superTriangle.R2) || triangle.R3.Equals(superTriangle.R3));
            }).ToList();

#if (DEBUG_DRAW && DEBUG_DRAW_TRIANGLE)
            DrawTriangles(triangles);
#endif
#if (DEBUG_DRAW && DEBUG_DRAW_CIRCUMCIRCLE)
            DrawCircumCircles(triangles);
#endif

#if DEBUG_DRAW
            yield return null;
#else
            return triangles;
#endif

        }

        List<Triangle> AddVertex(List<Triangle> triangles, Room vertex)
        {
            List<Edge> edges = new List<Edge>();

            triangles = triangles.Where(triangle =>
            {
                if (triangle.CircumCircleContains(vertex.Position))
                {
                    edges.Add(new Edge(triangle.R1, triangle.R2));
                    edges.Add(new Edge(triangle.R2, triangle.R3));
                    edges.Add(new Edge(triangle.R3, triangle.R1));
                    return false;
                }

                return true;
            }).ToList();

            edges = uniqueEdges(edges);
            
            foreach(Edge edge in edges)
            {
                triangles.Add(new Triangle(edge.p1, edge.p2, vertex));
            }

            return triangles;
        }

        List<Edge> uniqueEdges(List<Edge> edges)
        {
            List<Edge> uniqueEdges = new List<Edge>();

            for (int i = 0; i < edges.Count; ++i)
            {
                bool isUnique = true;

                for (int j = 0; j < edges.Count; ++j)
                {
                    if (i != j && edges[i].Equals(edges[j]))
                    {
                        isUnique = false;
                        break;
                    }
                }

                if (isUnique)
                    uniqueEdges.Add(edges[i]);
            }

            return uniqueEdges;
        }

        int[,] TriangulatedToGraph(List<Triangle> triangulatedList)
        {
            int[,] triangulatedGraph = new int[MaxRoomCount, MaxRoomCount];

            for (int i = 0; i < triangulatedList.Count; ++i)
            {
                Triangle triangle = triangulatedList[i];

                triangulatedGraph[triangle.R1.ID, triangle.R2.ID] = 1;
                triangulatedGraph[triangle.R1.ID, triangle.R3.ID] = 1;
                triangulatedGraph[triangle.R2.ID, triangle.R1.ID] = 1;
                triangulatedGraph[triangle.R2.ID, triangle.R3.ID] = 1;
                triangulatedGraph[triangle.R3.ID, triangle.R1.ID] = 1;
                triangulatedGraph[triangle.R3.ID, triangle.R2.ID] = 1;
            }

            return triangulatedGraph;
        }
        #endregion

        #region MST �޼ҵ�
        Edge[,] GraphToMST(int[,] graph, Room first)
        {
            Edge[,] mstTree = new Edge[MaxRoomCount, MaxRoomCount];
            MinHeap<Edge> heap = new MinHeap<Edge>(MaxRoomCount * (MaxRoomCount + 1) / 2);

            // �ּ� ���� ù ������ ������ ���� �߰�
            for (int i = 0; i < MaxRoomCount; ++i)
            {
                if (graph[first.ID, i] == 1)
                {
                    Edge e = new Edge(first, rooms[i]);
                    heap.Insert(e.Weight, e);
                }
            }

            // 1) �ּ� ������ �����͸� ���� MST Tree�� �߰��ϰ� 2) �ش� ������ ������ ������ �ּ� ���� �߰�
            // 1)�� 2)�� �ּ� ���� �����Ͱ� ���� ������ ����
            Edge min;
            while ((min = heap.Pop()) != null)
            {
                // �̹� MST Tree�� �߰��� �������� ��� �н�
                bool isP1Set = false;
                bool isP2Set = false;
                for (int i = 0; i < MaxRoomCount; ++i)
                {
                    if (mstTree[min.p1.ID, i] != null)
                        isP1Set = true;

                    if (mstTree[min.p2.ID, i] != null)
                        isP2Set = true;

                    if (isP1Set && isP2Set)
                        break;
                }

                if (isP1Set && isP2Set)
                    continue;

                // MST Tree�� ����ġ�� ���� ���� ���� �߰�
                mstTree[min.p1.ID, min.p2.ID] = min;

                // �߰��� ������ ������ ���� �߰�
                for (int i = 0; i < MaxRoomCount; ++i)
                {
                    if (graph[min.p2.ID, i] == 1)
                    {
                        Edge e = new Edge(min.p2, rooms[i]);
                        heap.Insert(e.Weight, e);
                    }
                }
            }

            return mstTree;
        }

        void AddCycledEdgeToMST(int[,] triangulatedGraph, Edge[,] mstTree)
        {
            for (int i = 0; i < MaxRoomCount; ++i)
            {
                for (int j = 0; j < MaxRoomCount; ++j)
                {
                    if (mstTree[i, j] == null && triangulatedGraph[i, j] == 1)
                    {
                        // ���� Ȯ���� Ż���� ���� �߰�
                        if (CycleHallwayCreationChance != 0)
                        {
                            if (Random.Range(0, (int)(100 / CycleHallwayCreationChance)) == 0)
                            {
                                if (mstTree[j, i] == null)
                                    mstTree[i, j] = new Edge(rooms[i], rooms[j]);
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region �� ������Ʈ ���� �޼ҵ�
        /*
         * checkedBorder : true�� ù ��忡 ���� ������ �ʰ� ����, false�� ���� ����� ����. ���� �ϳ��� �ǳʶٸ� �� ����
         * createFloor : �ٴ� ���� ����
         */
        void InstantiateRooms(bool checkedBorder, bool createFloor)
        {
            for (int i = 0; i < rooms.Count; ++i)
            {
                Room room = rooms[i];

                // ���� �ּ� ������ ������ �� ����ߴ� Instance ����
                Destroy(room.positionInstance);

                // �� ����
                InstantiateCheckedBorderAndFloors(room, checkedBorder, createFloor);
            }
        }

        void InstantiateCheckedBorderAndFloors(Room room, bool checkedBorder, bool createFloor)
        {
            if (room.Type == RoomType.VectorOnly)
                return;

            float nodeRadius = CGrid.Instance.GridNodeRadius;

            Vector3 offsetBack = Vector3.back * nodeRadius;
            Vector3 offsetLeft = Vector3.left * nodeRadius;
            Vector3 offset = offsetBack + offsetLeft;

            // �»�ܺ��� ���� �ٴ� ��ġ
            int eachSideNodeNum = (int)room.Type;
            float roomRadius = nodeRadius * eachSideNodeNum;

            CNode start = CGrid.Instance.GetNodeFromWorldPosition(room.Position + Vector3.left * roomRadius + Vector3.forward * roomRadius);
            CNode node = start;

            for (int j = 0; j < eachSideNodeNum; ++j)
            {
                for (int k = 0; k < eachSideNodeNum; ++k)
                {
                    // �� �׵θ��� �� ����
                    if (j == 0 || j + 1 == eachSideNodeNum || k == 0 || k + 1 == eachSideNodeNum)
                    {
                        if (node.Hallway)
                        {
                            /* �� ���� �ʿ� */
                        }
                        else
                        {
                            if (!checkedBorder)
                            {
                                Instantiate(WallNodePrefab, node.WorldPosition + offsetBack, Quaternion.identity, room.FloorsHierarchyRoot.transform);
                                node.Walkable = false;
                            }
                        }
                    }

                    if (createFloor)
                        Instantiate(FloorNodePrefab, node.WorldPosition + offset, Quaternion.identity, room.FloorsHierarchyRoot.transform);

                    node = node.GetNext(Directions.RIGHT);

                    checkedBorder = !checkedBorder;
                }

                node = start = start.GetNext(Directions.DOWN);
            }
        }

        void InstantiateHallways(List<List<AStarNode>> hallwayPaths)
        {
            CGrid grid = CGrid.Instance;

            float nodeRadius = grid.GridNodeRadius;

            Vector3 offsetBack = Vector3.back * nodeRadius;
            Vector3 offsetLeft = Vector3.left * nodeRadius;
            Vector3 offset = offsetBack + offsetLeft;

            LayerMask floorLayer = LayerMask.GetMask("Floor");

            GameObject hallwayHierarchyRoot = new GameObject("Hallways");
            hallwayHierarchyRoot.transform.SetParent(mapHierarchyRoot.transform);

            foreach (List<AStarNode> path in hallwayPaths)
            {
                for (int i = 0; i < path.Count; ++i)
                {
                    CNode node = path[i].Node;

                    // �� ���� ����� ��� continue (�ٴ��� ũ�Ⱑ ��庸�� ��¦ Ŀ 0.1f��ŭ ������)
                    if (Physics.CheckSphere(node.WorldPosition, nodeRadius - 0.1f, floorLayer))
                        continue;

                    // �ֺ� 8���� ��带 ������ ���� �ʿ��� ��ҿ��� ����
                    for (int j = node.GridY - 1; j <= node.GridY + 1; ++j)
                    {
                        for (int k = node.GridX - 1; k <= node.GridX + 1; ++k)
                        {
                            CNode currentNode = grid.Grid[j, k];

                            // ���� ��� ����
                            if (node.GridX == k && node.GridY == j)
                                continue;

                            // ���� ����̰ų� �̹� ���� ��ġ�� ��� ����
                            if (currentNode.Hallway || !currentNode.Walkable)
                                continue;

                            // ���� �� ����
                            Instantiate(WallNodePrefab, currentNode.WorldPosition + offsetBack, Quaternion.identity, hallwayHierarchyRoot.transform);

                            // ���� �ٴ� ����
                            Instantiate(HallwayFloorNodePrefab, currentNode.WorldPosition + offset, Quaternion.identity, hallwayHierarchyRoot.transform);

                            currentNode.Walkable = false;
                        }
                    }

                    // ���� ��忡 �ٴ� ����
                    Instantiate(FloorNodePrefab, node.WorldPosition + offset, Quaternion.identity, hallwayHierarchyRoot.transform);
                }
            }
        }
        #endregion

        #region ������ ���� ��� �޼ҵ�
#if DEBUG_DRAW
        void DrawEdge(Edge edge)
        {
            DrawEdge(edge.p1.Position, edge.p2.Position);
        }

        void DrawEdge(Vector3 p1, Vector3 p2)
        {
            if (edges == null)
                edges = new List<GameObject>();

            GameObject obj = Instantiate(debugLine);
            LineRenderer lr = obj.GetComponent<LineRenderer>();
            lr.positionCount = 2;
            lr.SetPosition(0, p1);
            lr.SetPosition(1, p2);
            edges.Add(obj);
        }

        void ClearEdge()
        {
            foreach (GameObject obj in edges)
            {
                Destroy(obj);
            }
        }

        void DrawTriangles(List<Triangle> triangles)
        {
            foreach (Triangle tri in triangles)
            {
                DrawTriangle(tri);
            }
        }

        void ClearTriangles()
        {
            foreach (GameObject obj in lines)
            {
                Destroy(obj);
            }
        }

        void DrawTriangle(Triangle triangle)
        {
            if (lines == null)
                lines = new List<GameObject>();

            GameObject obj = Instantiate(debugLine);
            LineRenderer lr = obj.GetComponent<LineRenderer>();
            lr.positionCount = 4;
            lr.SetPosition(0, triangle.R1.Position);
            lr.SetPosition(1, triangle.R2.Position);
            lr.SetPosition(2, triangle.R3.Position);
            lr.SetPosition(3, triangle.R1.Position);
            lines.Add(obj);
        }

        void DrawCircumCircles(List<Triangle> triangles)
        {
            foreach (Triangle triangle in triangles)
            {
                DrawCircumCircle(triangle);
            }
        }

        void DrawCircumCircle(Triangle triangle)
        {
            if (circles == null)
                circles = new List<GameObject>();

            GameObject obj = Instantiate(debugLine);
            LineRenderer lr = obj.GetComponent<LineRenderer>();
            lr.positionCount = 101;

            Vector3 circumCenter = triangle.circumCenter;
            float angle = 0;
            float R = Mathf.Sqrt(triangle.circumRadius);

            for (int i = 0; i < lr.positionCount - 1; ++i)
            {
                float x = Mathf.Cos(angle * Mathf.Deg2Rad) * R;
                float z = Mathf.Sin(angle * Mathf.Deg2Rad) * R;

                Vector3 pos = new Vector3(circumCenter.x + x, 0, circumCenter.z + z);
                angle += 360f / (lr.positionCount - 1);

                lr.SetPosition(i, pos);
            }

            lr.SetPosition(lr.positionCount - 1, lr.GetPosition(0));
            circles.Add(obj);
        }

        void ClearCircumCircles()
        {
            foreach (GameObject obj in circles)
            {
                Destroy(obj);
            }
        }

        void DrawAllOfTriangle(Triangle triangle)
        {
            DrawTriangle(triangle);

            DrawEdge(triangle.circumCenter, triangle.R1.Position);
            DrawEdge(triangle.circumCenter, triangle.R2.Position);
            DrawEdge(triangle.circumCenter, triangle.R3.Position);

            DrawCircumCircle(triangle);

            Debug.Log(Vector3.Distance(triangle.circumCenter, triangle.R1.Position));
            Debug.Log(Vector3.Distance(triangle.circumCenter, triangle.R2.Position));
            Debug.Log(Vector3.Distance(triangle.circumCenter, triangle.R3.Position));
        }
#endif
        #endregion
    }
}
