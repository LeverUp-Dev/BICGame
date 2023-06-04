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
                // 랜덤 맵을 생성하기에 그리드 맵이 충분히 넓은지 확인
                CGrid grid = CGrid.Instance;

                if (grid == null)
                {
                    throw new System.Exception("맵을 생성하려면 그리드 시스템을 추가해야 합니다. Hierarchy에 빈 오브젝트를 생성하고 Grid/CGrid.cs를 추가해주세요.");
                }

                float mapArea = grid.MaxMapWidth * grid.MaxMapHeight;
                float roomArea = Mathf.PI * Mathf.Pow(RoomMinimumDistance, 2);

                if (mapArea / roomArea < MaxRoomCount)
                {
                    throw new System.Exception("그리드 맵의 넓이가 랜덤 맵을 생성하기에 충분하지 않습니다. 그리드를 넓히거나 생성할 방의 수를 줄여주세요.");
                }

                Generate();
            }
        }

        void Generate()
        {
            mapHierarchyRoot = new GameObject("Map");
            mapHierarchyRoot.transform.SetParent(transform);

            rooms = new List<Room>();

            // 1. 랜덤으로 방 위치 생성
            InitializeRandomRooms();

            // 2. 들로네 삼각분할 수행 (참고 : https://www.gorillasun.de/blog/bowyer-watson-algorithm-for-delaunay-triangulation/)
#if DEBUG_DRAW
            StartCoroutine(Triangulate(rooms));
#else
            List<Triangle> triangulatedList = Triangulate(rooms);

            // 3. 만들어진 삼각분할을 그래프로 변환
            int[,] triangulatedGraph = TriangulatedToGraph(triangulatedList);

            // 4. 그래프를 Minimum Spanning Tree로 변환
            Edge[,] mstTree = GraphToMST(triangulatedGraph, triangulatedList[0].R1);

            // 5. 선택되지 않은 간선 중 랜덤으로 선택해 MST Tree에 추가 (순환 복도 생성)
            AddCycledEdgeToMST(triangulatedGraph, mstTree);

            // 6. 방 오브젝트 생성 (복도 경로가 방의 벽 위치를 연달아 지나가는 경우를 방지하기 위해 벽을 일부만 생성)
            InstantiateRooms(false, true);

            // 7. A* 알고리즘 수행
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

            // 8. (6)에서 생성하지 않았던 벽 추가
            InstantiateRooms(true, false);

            // 9. 복도 생성
            InstantiateHallways(hallwayPaths);
#endif
        }

        void InitializeRandomRooms()
        {
            LayerMask roomPositionLayer = LayerMask.GetMask("RoomPosition");

            for (int i = 0; i < MaxRoomCount; ++i)
            {
                // 방 중앙이 그리드 끝으로 설정되면 방이 그리드 바깥으로 벗어나므로 여유 공간 설정
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

                // TODO 컴포넌트 창에서 확률 조정 가능하도록 수정
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

        #region 들로네 삼각분할 메소드
        Triangle SuperTriangle()
        {
            int maxWidth = CGrid.Instance.MaxMapWidth;
            int maxHeight = CGrid.Instance.MaxMapHeight;

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

        #region MST 메소드
        Edge[,] GraphToMST(int[,] graph, Room first)
        {
            Edge[,] mstTree = new Edge[MaxRoomCount, MaxRoomCount];
            MinHeap<Edge> heap = new MinHeap<Edge>(MaxRoomCount * (MaxRoomCount + 1) / 2);

            // 최소 힙에 첫 정점과 인접한 간선 추가
            for (int i = 0; i < MaxRoomCount; ++i)
            {
                if (graph[first.ID, i] == 1)
                {
                    Edge e = new Edge(first, rooms[i]);
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

                // MST Tree에 가중치가 가장 낮은 간선 추가
                mstTree[min.p1.ID, min.p2.ID] = min;

                // 추가된 정점과 인접한 간선 추가
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
                        // 일정 확률로 탈락된 간선 추가
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

        #region 맵 오브젝트 생성 메소드
        /*
         * checkedBorder : true면 첫 노드에 벽을 세우지 않고 시작, false면 벽을 세우고 시작. 이후 하나씩 건너뛰며 벽 생성
         * createFloor : 바닥 생성 여부
         */
        void InstantiateRooms(bool checkedBorder, bool createFloor)
        {
            for (int i = 0; i < rooms.Count; ++i)
            {
                Room room = rooms[i];

                // 방의 최소 간격을 유지할 때 사용했던 Instance 제거
                Destroy(room.positionInstance);

                // 벽 생성
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

            // 좌상단부터 벽과 바닥 배치
            int eachSideNodeNum = (int)room.Type;
            float roomRadius = nodeRadius * eachSideNodeNum;

            CNode start = CGrid.Instance.GetNodeFromWorldPosition(room.Position + Vector3.left * roomRadius + Vector3.forward * roomRadius);
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

                    // 방 안의 노드일 경우 continue (바닥의 크기가 노드보다 살짝 커 0.1f만큼 조정함)
                    if (Physics.CheckSphere(node.WorldPosition, nodeRadius - 0.1f, floorLayer))
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
