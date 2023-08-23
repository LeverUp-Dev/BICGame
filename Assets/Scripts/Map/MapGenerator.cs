/*
#define DEBUG_DRAW
#define DEBUG_DRAW_TRIANGLE
//#define DEBUG_DRAW_CIRCUMCIRCLE
*/

#if DEBUG_DRAW
using System.Collections;
#endif
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hypocrites.Map
{
    using Map.DS;
    using Map.Elements;
    using Map.AStar;
    using Map.Enumerations;
    using Grid;
    using Defines;
    using Maze;

    public class MapGenerator : SingletonMono<MapGenerator>
    {
        const RoomType MAXIMUM_ROOM_SIZE = RoomType.R17x17;
        const int MAZE_ROOM_PADDING = (int)MAXIMUM_ROOM_SIZE / 2;
        const int MAXIMUM_GENERATE_TRIAL = 500;

        public Transform mapHierarchyRoot;
        public Transform mazeHierarchyRoot;

        List<Room> rooms;

        MazeGenerator mazeGenerator;
        Vector2 mazeLeftBottom;
        Vector2 mazeRightTop;

        CGrid grid;
        AStarPathfinder astar;

        int currentArea;

        /* Inspector Values */
        [field: SerializeField] public bool Run { get; private set; }

        [field: SerializeField] public int RoomsPerArea { get; private set; }
        [field: SerializeField] public float CycleHallwayCreationChance { get; private set; }

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

        protected override void Awake()
        {
            base.Awake();

            if (CycleHallwayCreationChance < 0 || CycleHallwayCreationChance > 100)
            {
                CycleHallwayCreationChance = 12.5f;
            }

            if (Run)
            {
                // ���� ���� �����ϱ⿡ �׸��� ���� ����� ������ Ȯ��
                grid = CGrid.Instance;

                if (grid == null)
                {
                    throw new System.Exception("���� �����Ϸ��� �׸��� �ý����� �߰��ؾ� �մϴ�. Hierarchy�� �� ������Ʈ�� �����ϰ� Grid/CGrid.cs�� �߰����ּ���.");
                }

                float mapArea = grid.MaxMapWidth * grid.MaxMapHeight;
                float roomArea = Mathf.PI * Mathf.Pow(MAXIMUM_ROOM_SIZE.GetRadius(), 2);

                if (mapArea < RoomsPerArea * roomArea * 4 + Mathf.Pow(MazeGenerator.MAZE_MAP_SIZE + MAZE_ROOM_PADDING, 2))
                {
                    throw new System.Exception("�׸��� ���� ���̰� ���� ���� �����ϱ⿡ ������� �ʽ��ϴ�. �׸��带 �����ų� ������ ���� ���� �ٿ��ּ���.");
                }

                mazeGenerator = new MazeGenerator(transform, mazeHierarchyRoot, WallNodePrefab, FloorNodePrefab);

                mazeGenerator.GetCorner(out CNode mazeLeftBottomNode, out CNode mazeRightTopNode);
                mazeLeftBottom = new Vector2(mazeLeftBottomNode.GridX, mazeLeftBottomNode.GridY);
                mazeRightTop = new Vector2(mazeRightTopNode.GridX, mazeRightTopNode.GridY);

                Generate();
            }
        }

        void Generate()
        {
            rooms = new List<Room>();

            int halfXSize = grid.GridXSize / 2;
            int halfYSize = grid.GridYSize / 2;

            List<List<AStarNode>> hallwayPaths = new List<List<AStarNode>>();
            currentArea = 0;

            // �� ���� ���� �� ����
            for (int i = 0; i < 2; ++i)
            {
                for (int j = 0; j < 2; ++j)
                {
                    Vector2 minGridPosition = new Vector2(i * halfXSize, j * halfYSize);
                    Vector2 maxGridPosition = new Vector2((i + 1) * halfXSize, (j + 1) * halfYSize);

                    // 1. �������� �� ��ġ ����
                    InitializeRandomRooms(minGridPosition, maxGridPosition);

                    // 2. ��γ� �ﰢ���� ���� (���� : https://www.gorillasun.de/blog/bowyer-watson-algorithm-for-delaunay-triangulation/)
#if DEBUG_DRAW
                    StartCoroutine(Triangulate(rooms));
#else
                    List<Triangle> triangulatedList = Triangulate();

                    // 3. ������� �ﰢ������ �׷����� ��ȯ
                    int[,] triangulatedGraph = TriangulatedToGraph(triangulatedList);

                    // 4. �׷����� Minimum Spanning Tree�� ��ȯ
                    Edge[,] mstTree = GraphToMST(triangulatedGraph, triangulatedList[0].R1);

                    // 5. ���õ��� ���� ���� �� �������� ������ MST Tree�� �߰� (��ȯ ���� ����)
                    AddCycledEdgeToMST(triangulatedGraph, mstTree);

                    // 6. �� ������Ʈ ���� (���� ��ΰ� ���� �� ��ġ�� ���޾� �������� ��츦 �����ϱ� ���� ���� �Ϻθ� ����)
                    InstantiateRooms(false, true);

                    // 7. A* �˰��� ����
                    EstablishHallwayPath(hallwayPaths, mstTree);

                    ++currentArea;
                }
            }

            // 8. (6)���� �������� �ʾҴ� �� �߰�
            InstantiateRooms(true, false);

            // 9. �� ����� �̷� ����
            mazeGenerator.Generate();

            // 10. ���� ����
            InstantiateHallways(hallwayPaths);

            // 11. �׸��� ������Ʈ (�̷� �� ���ſ� �ð��� �ɷ� �ڷ�ƾ���� ���� �߰�)
            StartCoroutine(UpdateGrid());
#endif
        }

        IEnumerator UpdateGrid()
        {
            yield return new WaitForSeconds(0.1f);

            CGrid.Instance.UpdateGrid();
        }

        void InitializeRandomRooms(Vector2 min, Vector2 max)
        {
            LayerMask roomPositionLayer = LayerMask.GetMask("RoomPosition");

            int trials = 0;
            for (int i = currentArea * RoomsPerArea; i < (currentArea + 1) * RoomsPerArea; ++i)
            {
                if (trials++ == MAXIMUM_GENERATE_TRIAL)
                    throw new System.Exception("���� ������ �� �����ϴ�. �׸��带 Ȯ���ϰų� �� ������ ���̴� �� �����մϴ�.");

                // �� �߾��� �׸��� ������ �����Ǹ� ���� �׸��� �ٱ����� ����Ƿ� ���� ���� ����
                int randomX = Random.Range((int)min.x + 10, (int)max.x - 10);
                int randomZ = Random.Range((int)min.y + 10, (int)max.y - 10);

                CNode node = grid.Grid[randomZ, randomX];
                Vector3 randomPos = node.WorldPosition;
                bool isDuplicated = false;

                // �ش� ��尡 �̵��� �Ұ����� ��� ��ġ �缱��
                if (!node.Walkable)
                {
                    --i;
                    continue;
                }

                // �̹� ���� ������ ��ġ�� ��� ��ġ �缱��
                for (int j = currentArea * RoomsPerArea; j < i; ++j)
                {
                    if (randomPos.Equals(rooms[j].Position))
                    {
                        isDuplicated = true;
                        break;
                    }
                }

                if (isDuplicated)
                {
                    --i;
                    continue;
                }

                // �߾� �̷� ��� ��ĥ ��� ��ġ �缱��
                if (randomX >= mazeLeftBottom.x - MAZE_ROOM_PADDING && randomX <= mazeRightTop.x + MAZE_ROOM_PADDING
                    && randomZ <= mazeLeftBottom.y + MAZE_ROOM_PADDING && randomZ >= mazeRightTop.y - MAZE_ROOM_PADDING)
                {
                    --i;
                    continue;
                }

                // TODO ������Ʈ â���� Ȯ�� ���� �����ϵ��� ����
                RoomType roomType;
                int random = Random.Range(0, 10);

                if (random == 0)
                    roomType = MAXIMUM_ROOM_SIZE;
                else if (random <= 3)
                    roomType = RoomType.R13x13;
                else
                    roomType = RoomType.R9x9;

                // ���� ���� ��ġ�� �ʵ��� ���� ���� �ٸ� ���� �����ϴ��� Ȯ��
                if (Physics.CheckSphere(randomPos, MAXIMUM_ROOM_SIZE.GetDoubleRadius(), roomPositionLayer))
                {
                    --i;
                    continue;
                }

                rooms.Add(new Room(i, currentArea, randomPos, roomType, Instantiate(UnitRoomPrefab, randomPos, Quaternion.identity), mapHierarchyRoot));
            }
        }

        #region ��γ� �ﰢ���� �޼ҵ�
        Triangle SuperTriangle()
        {
            // �� ���� �������� ������ �����ϹǷ� width�� height�� �������� ����
            int maxWidth = grid.MaxMapWidth;
            int maxHeight = grid.MaxMapHeight;

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
        List<Triangle> Triangulate()
#endif
        {
            Triangle superTriangle = SuperTriangle();

            List<Triangle> triangles = new List<Triangle>();
            triangles.Add(superTriangle);

            for (int i = currentArea * RoomsPerArea; i < (currentArea + 1) * RoomsPerArea; ++i)
            {
#if (DEBUG_DRAW && DEBUG_DRAW_TRIANGLE)
                DrawTriangles(triangles);
#endif
#if (DEBUG_DRAW && DEBUG_DRAW_CIRCUMCIRCLE)
                DrawCircumCircles(triangles);
#endif

                triangles = AddVertex(triangles, rooms[i]);

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
            int[,] triangulatedGraph = new int[RoomsPerArea, RoomsPerArea];

            for (int i = 0; i < triangulatedList.Count; ++i)
            {
                Triangle triangle = triangulatedList[i];

                triangulatedGraph[triangle.R1.GetLocalID(RoomsPerArea), triangle.R2.GetLocalID(RoomsPerArea)] = 1;
                triangulatedGraph[triangle.R1.GetLocalID(RoomsPerArea), triangle.R3.GetLocalID(RoomsPerArea)] = 1;
                triangulatedGraph[triangle.R2.GetLocalID(RoomsPerArea), triangle.R1.GetLocalID(RoomsPerArea)] = 1;
                triangulatedGraph[triangle.R2.GetLocalID(RoomsPerArea), triangle.R3.GetLocalID(RoomsPerArea)] = 1;
                triangulatedGraph[triangle.R3.GetLocalID(RoomsPerArea), triangle.R1.GetLocalID(RoomsPerArea)] = 1;
                triangulatedGraph[triangle.R3.GetLocalID(RoomsPerArea), triangle.R2.GetLocalID(RoomsPerArea)] = 1;
            }

            return triangulatedGraph;
        }
        #endregion

        #region MST �޼ҵ�
        Edge[,] GraphToMST(int[,] graph, Room first)
        {
            Edge[,] mstTree = new Edge[RoomsPerArea, RoomsPerArea];
            MinHeap<Edge> heap = new MinHeap<Edge>((RoomsPerArea + 1) * RoomsPerArea);

            // �ּ� ���� ù ������ ������ ���� �߰�
            for (int i = 0; i < RoomsPerArea; ++i)
            {
                if (graph[first.GetLocalID(RoomsPerArea), i] == 1)
                {
                    Edge e = new Edge(first, rooms[i + (currentArea * RoomsPerArea)]);
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
                for (int i = 0; i < RoomsPerArea; ++i)
                {
                    if (mstTree[min.p1.GetLocalID(RoomsPerArea), i] != null)
                        isP1Set = true;

                    if (mstTree[min.p2.GetLocalID(RoomsPerArea), i] != null)
                        isP2Set = true;

                    if (isP1Set && isP2Set)
                        break;
                }

                if (isP1Set && isP2Set)
                    continue;

                // MST Tree�� ����ġ�� ���� ���� ���� �߰�
                mstTree[min.p1.GetLocalID(RoomsPerArea), min.p2.GetLocalID(RoomsPerArea)] = min;

                // �߰��� ������ ������ ���� �߰�
                for (int i = 0; i < RoomsPerArea; ++i)
                {
                    if (graph[min.p2.GetLocalID(RoomsPerArea), i] == 1)
                    {
                        Edge e = new Edge(min.p2, rooms[i + (currentArea * RoomsPerArea)]);
                        heap.Insert(e.Weight, e);
                    }
                }
            }

            return mstTree;
        }

        void AddCycledEdgeToMST(int[,] triangulatedGraph, Edge[,] mstTree)
        {
            for (int i = 0; i < RoomsPerArea; ++i)
            {
                for (int j = 0; j < RoomsPerArea; ++j)
                {
                    if (mstTree[i, j] == null && triangulatedGraph[i, j] == 1)
                    {
                        // ���� Ȯ���� Ż���� ���� �߰�
                        if (CycleHallwayCreationChance != 0)
                        {
                            if (Random.Range(0, (int)(100 / CycleHallwayCreationChance)) == 0)
                            {
                                if (mstTree[j, i] == null)
                                    mstTree[i, j] = new Edge(rooms[i + (currentArea * RoomsPerArea)], rooms[j + (currentArea * RoomsPerArea)]);
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region ��ã�� ���� �޼ҵ�
        void EstablishHallwayPath(List<List<AStarNode>> hallwayPaths, Edge[,] roomMst)
        {
            if (astar == null)
                astar = new AStarPathfinder();

            Room minimumDistanceRoom = null;
            float minimumDistance = 0;

            for (int i = 0; i < RoomsPerArea; ++i)
            {
                for (int j = 0; j < RoomsPerArea; ++j)
                {
                    if (roomMst[i, j] != null)
                    {
                        Edge edge = roomMst[i, j];
                        List<AStarNode> path = astar.FindPath(grid.GetNodeFromWorldPosition(edge.p1.Position), grid.GetNodeFromWorldPosition(edge.p2.Position));
                        hallwayPaths.Add(path);

                        roomMst[j, i] = null;

                        // ���� �߽ɰ� ���� ����� �� ����
                        if (minimumDistanceRoom == null)
                        {
                            minimumDistanceRoom = edge.p1;
                            minimumDistance = Vector3.Distance(minimumDistanceRoom.Position, Vector3.zero);
                        }
                        else
                        {
                            float distanceToCenter = Vector3.Distance(edge.p1.Position, minimumDistanceRoom.Position);
                            if (minimumDistance > distanceToCenter)
                            {
                                minimumDistanceRoom = edge.p1;
                                minimumDistance = distanceToCenter;
                            }
                        }
                    }
                }
            }

            // ���� �߽ɰ� ���� ����� ��� ���� �߽��� �մ� ���� ���� (�� ������ ��� �̷θ� ����)
            hallwayPaths.Add(astar.FindPath(grid.GetNodeFromWorldPosition(minimumDistanceRoom.Position), grid.GetNodeFromWorldPosition(Vector3.zero)));
        }
        #endregion

        #region �� ������Ʈ ���� �޼ҵ�
        /*
         * checkedBorder : true�� ù ��忡 ���� ������ �ʰ� ����, false�� ���� ����� ����. ���� �ϳ��� �ǳʶٸ� �� ����
         * createFloor : �ٴ� ���� ����
         */
        void InstantiateRooms(bool checkedBorder, bool createFloor)
        {
            int start;
            int end;

            if (checkedBorder)
            {
                start = 0;
                end = rooms.Count;
            }
            else
            {
                start = currentArea * RoomsPerArea;
                end = (currentArea + 1) * RoomsPerArea;
            }

            for (int i = start; i < end; ++i)
            {
                Room room = rooms[i];

                // ���� �ּ� ������ ������ �� ����ߴ� Instance ����
                Destroy(room.PositionInstance);

                // �� ����
                InstantiateCheckedBorderAndFloors(room, checkedBorder, createFloor);
            }
        }

        void InstantiateCheckedBorderAndFloors(Room room, bool checkedBorder, bool createFloor)
        {
            if (room.Type == RoomType.VectorOnly)
                return;

            float nodeRadius = grid.GridNodeRadius;

            Vector3 offsetBack = Vector3.back * nodeRadius;
            Vector3 offsetLeft = Vector3.left * nodeRadius;
            Vector3 offset = offsetBack + offsetLeft;

            // �»�ܺ��� ���� �ٴ� ��ġ
            int eachSideNodeNum = (int)room.Type;
            float roomRadius = nodeRadius * eachSideNodeNum;

            CNode start = grid.GetNodeFromWorldPosition(room.Position + Vector3.left * roomRadius + Vector3.forward * roomRadius);
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
            float nodeRadius = grid.GridNodeRadius;

            Vector3 offsetBack = Vector3.back * nodeRadius;
            Vector3 offsetLeft = Vector3.left * nodeRadius;
            Vector3 offset = offsetBack + offsetLeft;

            LayerMask floorLayer = LayerMask.GetMask("Floor");

            GameObject hallwayHierarchyRoot = new GameObject("Hallways");
            hallwayHierarchyRoot.transform.SetParent(mapHierarchyRoot);

            foreach (List<AStarNode> path in hallwayPaths)
            {
                for (int i = 0; i < path.Count; ++i)
                {
                    CNode node = path[i].Node;

                    // �� ���� ����� ��� continue (�ٴ��� ũ�Ⱑ ��庸�� ��¦ Ŀ 0.1f��ŭ ������)
                    if (Physics.CheckSphere(node.WorldPosition, nodeRadius - 0.1f, floorLayer))
                        continue;

                    // TODO : �̷� �濡 �ٴ��� �����ϸ� �� IF�� ó���ϸ� �� ��
                    // �̷� �� ���� ��� continue
                    if (node.GridX >= mazeLeftBottom.x && node.GridX <= mazeRightTop.x
                        && node.GridY <= mazeLeftBottom.y && node.GridY >= mazeRightTop.y)
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
