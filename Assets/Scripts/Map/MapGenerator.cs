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
                // 랜덤 맵을 생성하기에 그리드 맵이 충분히 넓은지 확인
                grid = CGrid.Instance;

                if (grid == null)
                {
                    throw new System.Exception("맵을 생성하려면 그리드 시스템을 추가해야 합니다. Hierarchy에 빈 오브젝트를 생성하고 Grid/CGrid.cs를 추가해주세요.");
                }

                float mapArea = grid.MaxMapWidth * grid.MaxMapHeight;
                float roomArea = Mathf.PI * Mathf.Pow(MAXIMUM_ROOM_SIZE.GetRadius(), 2);

                if (mapArea < RoomsPerArea * roomArea * 4 + Mathf.Pow(MazeGenerator.MAZE_MAP_SIZE + MAZE_ROOM_PADDING, 2))
                {
                    throw new System.Exception("그리드 맵의 넓이가 랜덤 맵을 생성하기에 충분하지 않습니다. 그리드를 넓히거나 생성할 방의 수를 줄여주세요.");
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

            // 각 구역 별로 맵 생성
            for (int i = 0; i < 2; ++i)
            {
                for (int j = 0; j < 2; ++j)
                {
                    Vector2 minGridPosition = new Vector2(i * halfXSize, j * halfYSize);
                    Vector2 maxGridPosition = new Vector2((i + 1) * halfXSize, (j + 1) * halfYSize);

                    // 1. 랜덤으로 방 위치 생성
                    InitializeRandomRooms(minGridPosition, maxGridPosition);

                    // 2. 들로네 삼각분할 수행 (참고 : https://www.gorillasun.de/blog/bowyer-watson-algorithm-for-delaunay-triangulation/)
#if DEBUG_DRAW
                    StartCoroutine(Triangulate(rooms));
#else
                    List<Triangle> triangulatedList = Triangulate();

                    // 3. 만들어진 삼각분할을 그래프로 변환
                    int[,] triangulatedGraph = TriangulatedToGraph(triangulatedList);

                    // 4. 그래프를 Minimum Spanning Tree로 변환
                    Edge[,] mstTree = GraphToMST(triangulatedGraph, triangulatedList[0].R1);

                    // 5. 선택되지 않은 간선 중 랜덤으로 선택해 MST Tree에 추가 (순환 복도 생성)
                    AddCycledEdgeToMST(triangulatedGraph, mstTree);

                    // 6. 방 오브젝트 생성 (복도 경로가 방의 벽 위치를 연달아 지나가는 경우를 방지하기 위해 벽을 일부만 생성)
                    InstantiateRooms(false, true);

                    // 7. A* 알고리즘 수행
                    EstablishHallwayPath(hallwayPaths, mstTree);

                    ++currentArea;
                }
            }

            // 8. (6)에서 생성하지 않았던 벽 추가
            InstantiateRooms(true, false);

            // 9. 맵 가운데에 미로 생성
            mazeGenerator.Generate();

            // 10. 복도 생성
            InstantiateHallways(hallwayPaths);

            // 11. 그리드 업데이트 (미로 벽 제거에 시간이 걸려 코루틴으로 지연 추가)
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
                    throw new System.Exception("맵을 생성할 수 없습니다. 그리드를 확장하거나 방 개수를 줄이는 걸 권장합니다.");

                // 방 중앙이 그리드 끝으로 설정되면 방이 그리드 바깥으로 벗어나므로 여유 공간 설정
                int randomX = Random.Range((int)min.x + 10, (int)max.x - 10);
                int randomZ = Random.Range((int)min.y + 10, (int)max.y - 10);

                CNode node = grid.Grid[randomZ, randomX];
                Vector3 randomPos = node.WorldPosition;
                bool isDuplicated = false;

                // 해당 노드가 이동이 불가능한 경우 위치 재선정
                if (!node.Walkable)
                {
                    --i;
                    continue;
                }

                // 이미 방이 생성된 위치인 경우 위치 재선정
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

                // 중앙 미로 방과 겹칠 경우 위치 재선정
                if (randomX >= mazeLeftBottom.x - MAZE_ROOM_PADDING && randomX <= mazeRightTop.x + MAZE_ROOM_PADDING
                    && randomZ <= mazeLeftBottom.y + MAZE_ROOM_PADDING && randomZ >= mazeRightTop.y - MAZE_ROOM_PADDING)
                {
                    --i;
                    continue;
                }

                // TODO 컴포넌트 창에서 확률 조정 가능하도록 수정
                RoomType roomType;
                int random = Random.Range(0, 10);

                if (random == 0)
                    roomType = MAXIMUM_ROOM_SIZE;
                else if (random <= 3)
                    roomType = RoomType.R13x13;
                else
                    roomType = RoomType.R9x9;

                // 방이 서로 겹치지 않도록 범위 내에 다른 방이 존재하는지 확인
                if (Physics.CheckSphere(randomPos, MAXIMUM_ROOM_SIZE.GetDoubleRadius(), roomPositionLayer))
                {
                    --i;
                    continue;
                }

                rooms.Add(new Room(i, currentArea, randomPos, roomType, Instantiate(UnitRoomPrefab, randomPos, Quaternion.identity), mapHierarchyRoot));
            }
        }

        #region 들로네 삼각분할 메소드
        Triangle SuperTriangle()
        {
            // 네 개의 구역으로 나눠서 생성하므로 width와 height를 절반으로 나눔
            int maxWidth = grid.MaxMapWidth;
            int maxHeight = grid.MaxMapHeight;

            // vertex가 SuperTriangle에 너무 가까이 생성되면 제대로 수행되지 않아 offset으로 거리 조절
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

        #region MST 메소드
        Edge[,] GraphToMST(int[,] graph, Room first)
        {
            Edge[,] mstTree = new Edge[RoomsPerArea, RoomsPerArea];
            MinHeap<Edge> heap = new MinHeap<Edge>((RoomsPerArea + 1) * RoomsPerArea);

            // 최소 힙에 첫 정점과 인접한 간선 추가
            for (int i = 0; i < RoomsPerArea; ++i)
            {
                if (graph[first.GetLocalID(RoomsPerArea), i] == 1)
                {
                    Edge e = new Edge(first, rooms[i + (currentArea * RoomsPerArea)]);
                    heap.Insert(e.Weight, e);
                }
            }

            // 1) 최소 힙에서 데이터를 꺼내 MST Tree에 추가하고 2) 해당 정점과 인접한 간선을 최소 힙에 추가
            // 1)과 2)를 최소 힙에 데이터가 없을 때까지 수행
            Edge min;
            while ((min = heap.Pop()) != null)
            {
                // 이미 MST Tree에 추가된 정점들일 경우 패스
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

                // MST Tree에 가중치가 가장 낮은 간선 추가
                mstTree[min.p1.GetLocalID(RoomsPerArea), min.p2.GetLocalID(RoomsPerArea)] = min;

                // 추가된 정점과 인접한 간선 추가
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
                        // 일정 확률로 탈락된 간선 추가
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

        #region 길찾기 관련 메소드
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

                        // 맵의 중심과 가장 가까운 방 조사
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

            // 맵의 중심과 가장 가까운 방과 맵의 중심을 잇는 복도 생성 (한 구역의 방과 미로를 연결)
            hallwayPaths.Add(astar.FindPath(grid.GetNodeFromWorldPosition(minimumDistanceRoom.Position), grid.GetNodeFromWorldPosition(Vector3.zero)));
        }
        #endregion

        #region 맵 오브젝트 생성 메소드
        /*
         * checkedBorder : true면 첫 노드에 벽을 세우지 않고 시작, false면 벽을 세우고 시작. 이후 하나씩 건너뛰며 벽 생성
         * createFloor : 바닥 생성 여부
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

                // 방의 최소 간격을 유지할 때 사용했던 Instance 제거
                Destroy(room.PositionInstance);

                // 벽 생성
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

            // 좌상단부터 벽과 바닥 배치
            int eachSideNodeNum = (int)room.Type;
            float roomRadius = nodeRadius * eachSideNodeNum;

            CNode start = grid.GetNodeFromWorldPosition(room.Position + Vector3.left * roomRadius + Vector3.forward * roomRadius);
            CNode node = start;

            for (int j = 0; j < eachSideNodeNum; ++j)
            {
                for (int k = 0; k < eachSideNodeNum; ++k)
                {
                    // 방 테두리에 벽 생성
                    if (j == 0 || j + 1 == eachSideNodeNum || k == 0 || k + 1 == eachSideNodeNum)
                    {
                        if (node.Hallway)
                        {
                            /* 문 생성 필요 */
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

                    // 방 안의 노드일 경우 continue (바닥의 크기가 노드보다 살짝 커 0.1f만큼 조정함)
                    if (Physics.CheckSphere(node.WorldPosition, nodeRadius - 0.1f, floorLayer))
                        continue;

                    // TODO : 미로 방에 바닥을 생성하면 위 IF로 처리하면 될 듯
                    // 미로 방 안일 경우 continue
                    if (node.GridX >= mazeLeftBottom.x && node.GridX <= mazeRightTop.x
                        && node.GridY <= mazeLeftBottom.y && node.GridY >= mazeRightTop.y)
                        continue;

                    // 주변 8개의 노드를 조사해 벽이 필요한 장소에만 생성
                    for (int j = node.GridY - 1; j <= node.GridY + 1; ++j)
                    {
                        for (int k = node.GridX - 1; k <= node.GridX + 1; ++k)
                        {
                            CNode currentNode = grid.Grid[j, k];

                            // 본인 노드 제외
                            if (node.GridX == k && node.GridY == j)
                                continue;

                            // 복도 노드이거나 이미 벽이 설치된 경우 제외
                            if (currentNode.Hallway || !currentNode.Walkable)
                                continue;

                            // 복도 벽 생성
                            Instantiate(WallNodePrefab, currentNode.WorldPosition + offsetBack, Quaternion.identity, hallwayHierarchyRoot.transform);

                            // 복도 바닥 생성
                            Instantiate(HallwayFloorNodePrefab, currentNode.WorldPosition + offset, Quaternion.identity, hallwayHierarchyRoot.transform);

                            currentNode.Walkable = false;
                        }
                    }

                    // 본인 노드에 바닥 생성
                    Instantiate(FloorNodePrefab, node.WorldPosition + offset, Quaternion.identity, hallwayHierarchyRoot.transform);
                }
            }
        }
        #endregion

        #region 디버깅용 도형 출력 메소드
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
