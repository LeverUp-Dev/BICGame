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

namespace RandomMap
{
    using RandomMap.DS;
    using RandomMap.Elements;
    using RandomMap.AStar;
    using RandomMap.Enumerations;

    public class MapGenerator : MonoBehaviour
    {
        public bool run;

        public int maxRoomCount;

        public float cycleHallwayCreationChance;
        public int roomMinimumDistance;

        public GameObject unitRoomPrefab;
        public GameObject floorPrefab;
        public GameObject hallwayFloorPrefab;
        public GameObject wall2mPrefab;
        public GameObject wall4mPrefab;
        public GameObject doorWallPrefab;
        public GameObject vaultPrefab;
        public GameObject vaultColumnPrefab;

        public int floor;

        GameObject MapHierarchyRoot;
        List<Room> rooms;

        public GameObject debugLine;
#if DEBUG_DRAW
        List<GameObject> lines;
        List<GameObject> circles;
        List<GameObject> edges;
#endif
        List<GameObject> edges;
        void Awake()
        {
            if (cycleHallwayCreationChance < 0 || cycleHallwayCreationChance > 100)
            {
                cycleHallwayCreationChance = 12.5f;
            }

            if (run)
            {
                // 랜덤 맵을 생성하기에 그리드 맵이 충분히 넓은지 확인
                CGrid grid = CGrid.instance;
                float mapArea = grid.maxMapWidth * grid.maxMapHeight;
                float roomArea = Mathf.PI * Mathf.Pow(roomMinimumDistance, 2);

                if (mapArea / roomArea < maxRoomCount)
                {
                    Debug.LogError("그리드 맵의 넓이가 랜덤 맵을 생성하기에 충분하지 않습니다. 그리드를 넓히거나 생성할 방의 수를 줄여주세요.");
                    return;
                }

                Generate();
            }

            /*AStarPathfinder astar = new AStarPathfinder();

            Instantiate(unitRoomObject, new Vector3(-33, 0, -48), Quaternion.identity);
            Instantiate(unitRoomObject, new Vector3(18, 0, -63), Quaternion.identity);

            astar.FindPath(CGrid.instance.GetNodeFromWorldPosition(new Vector3(-33, 0, -48)), CGrid.instance.GetNodeFromWorldPosition(new Vector3(18, 0, -63)));*/

            /*Triangle tri = new Triangle(new Room(new Vector3(44, 0, -45)), new Room(new Vector3(-15, 0, -47)), new Room(new Vector3(44, 0, -9)));
            DrawTriangle(tri);
            DrawCircumCircle(tri);*/
        }

        void Generate()
        {
            MapHierarchyRoot = new GameObject("Map");
            // 전체 방 수 * (바닥 수 + 벽 수) * 2
            MapHierarchyRoot.transform.hierarchyCapacity = maxRoomCount * ((int)RoomType.R4x4 * (int)RoomType.R4x4 + (int)RoomType.R4x4 * 4) * 2;
            MapHierarchyRoot.transform.SetParent(transform);

            rooms = new List<Room>();

            // 1. 랜덤으로 방 위치 생성
            LayerMask roomPositionLayer = LayerMask.GetMask("RoomPosition");
            for (int i = 0; i < maxRoomCount; ++i)
            {
                // 방 중앙이 그리드 끝으로 설정되면 방이 그리드 바깥으로 벗어나므로 여유 공간 설정
                int randomX = Random.Range(10, CGrid.instance.GridXSize - 10);
                int randomZ = Random.Range(10, CGrid.instance.GridYSize - 10);

                CNode node = i == 0 ? CGrid.instance.GetNodeFromWorldPosition(Vector3.zero) : CGrid.instance.Grid[randomX, randomZ];
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
                
                if (isDuplicated || Physics.CheckSphere(randomPos, roomMinimumDistance, roomPositionLayer))
                {
                    --i;
                    continue;
                }

                // TODO 컴포넌트 창에서 확률 조정 가능하도록 수정
                RoomType roomType;
                int random = Random.Range(0, 10);

                if (random == 0)
                    roomType = RoomType.R4x4;
                else if (random <= 3)
                    roomType = RoomType.R3x3;
                else
                    roomType = RoomType.R2x2;

                rooms.Add(new Room(i, randomPos, roomType, Instantiate(unitRoomPrefab, randomPos, Quaternion.identity)));
            }

            // 2. 들로네 삼각분할 수행 (참고 : https://www.gorillasun.de/blog/bowyer-watson-algorithm-for-delaunay-triangulation/)
#if DEBUG_DRAW
            StartCoroutine(Triangulate(rooms));
#else
            List<Triangle> triangulatedList = Triangulate(rooms);

            // 3. 만들어진 삼각분할을 그래프로 변환
            int[,] triangulatedGraph = new int[maxRoomCount, maxRoomCount];
            
            foreach (Triangle triangle in triangulatedList)
            {
                triangulatedGraph[triangle.R1.ID, triangle.R2.ID] = 1;
                triangulatedGraph[triangle.R1.ID, triangle.R3.ID] = 1;
                triangulatedGraph[triangle.R2.ID, triangle.R1.ID] = 1;
                triangulatedGraph[triangle.R2.ID, triangle.R3.ID] = 1;
                triangulatedGraph[triangle.R3.ID, triangle.R1.ID] = 1;
                triangulatedGraph[triangle.R3.ID, triangle.R2.ID] = 1;
            }

            // 4. 그래프를 Minimum Spanning Tree로 변환
            Edge[,] mstTree = GraphToMST(triangulatedGraph, triangulatedList[0].R1);

            // 5. 선택되지 않은 간선 중 랜덤으로 선택해 MST Tree에 추가 (순환 복도 생성)
            for (int i = 0; i < maxRoomCount; ++i)
            {
                for (int j = 0; j < maxRoomCount; ++j)
                {
                    if (mstTree[i, j] == null && triangulatedGraph[i, j] == 1)
                    {
                        // 일정 확률로 탈락된 간선 추가
                        if (cycleHallwayCreationChance != 0)
                        {
                            if (Random.Range(0, (int)(100 / cycleHallwayCreationChance)) == 0)
                            {
                                if (mstTree[j, i] == null)
                                    mstTree[i, j] = new Edge(rooms[i], rooms[j]);
                            }
                        }
                    }
                }
            }

            // 6. 맵 오브젝트 생성
            InstantiateRooms();

            // 7. A* 알고리즘 수행
            AStarPathfinder astar = new AStarPathfinder();
            CGrid grid = CGrid.instance;
            List<List<AStarNode>> hallwayPaths = new List<List<AStarNode>>();

            for (int i = 0; i < maxRoomCount; ++i)
            {
                for (int j = 0; j < maxRoomCount; ++j)
                {
                    if (mstTree[i, j] != null)
                    {
                        Edge edge = mstTree[i, j];
                        hallwayPaths.Add(astar.FindPath(grid.GetNodeFromWorldPosition(edge.p1.Position), grid.GetNodeFromWorldPosition(edge.p2.Position)));

                        mstTree[j, i] = null;
                    }
                }
            }

            // 8. 불필요한 문 제거
            RemoveUselessDoors();

            // 9. 복도 생성
            InstantiateHallways(hallwayPaths);
#endif
        }

        #region 들로네 삼각분할 메소드
        Triangle SuperTriangle()
        {
            int maxWidth = CGrid.instance.maxMapWidth;
            int maxHeight = CGrid.instance.maxMapHeight;

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
        #endregion

        #region MST 메소드
        Edge[,] GraphToMST(int[,] graph, Room first)
        {
            Edge[,] mstTree = new Edge[maxRoomCount, maxRoomCount];
            MinHeap<Edge> heap = new MinHeap<Edge>(maxRoomCount * (maxRoomCount + 1) / 2);

            // 최소 힙에 첫 정점과 인접한 간선 추가
            for (int i = 0; i < maxRoomCount; ++i)
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
                for (int i = 0; i < maxRoomCount; ++i)
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
                for (int i = 0; i < maxRoomCount; ++i)
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
        #endregion

        #region 맵 오브젝트 생성 메소드
        void InstantiateRoomsOrigin()
        {
            MeshRenderer floorMesh = floorPrefab.GetComponent<MeshRenderer>();
            MeshRenderer wallMesh = wall2mPrefab.GetComponent<MeshRenderer>();
            MeshRenderer doorWallMesh = doorWallPrefab.GetComponent<MeshRenderer>();

            // TODO 벽의 회전과 스케일 값 통일 필요
            float floorHalfWidth = Mathf.RoundToInt(floorMesh.bounds.size.x) / 2;
            float wallHalfWidth = Mathf.RoundToInt(wallMesh.bounds.size.z) / 2;
            float doorWallHalfWidth = Mathf.RoundToInt(doorWallMesh.bounds.size.x) / 2;

            Vector3[] wallPositionsA = new Vector3[]
            {
                Vector3.left,
                Vector3.back,
                Vector3.left,
                Vector3.back
            };
            Vector3[] wallPositionsB = new Vector3[]
            {
                Vector3.forward * doorWallHalfWidth,
                Vector3.right * doorWallHalfWidth,
                Vector3.back * doorWallHalfWidth,
                Vector3.left * doorWallHalfWidth
            };
            Quaternion[] doorWallRotations = new Quaternion[]
            {
                Quaternion.Euler(-90, 0, 0),
                Quaternion.Euler(-90, 90, 0),
                Quaternion.Euler(-90, 0, 0),
                Quaternion.Euler(-90, 90, 0)
            };

            foreach (Room room in rooms)
            {
                if (room.Type == RoomType.Unit || room.Type == RoomType.VectorOnly)
                    continue;

                GameObject roomHierarchyRoot = new GameObject($"Room{room.ID}");
                roomHierarchyRoot.transform.SetParent(MapHierarchyRoot.transform);

                // 바닥 배치
                int eachSideWallNum = (int)room.Type;
                float floorOffset = floorHalfWidth * (eachSideWallNum - 1);

                Vector3 start = room.Position + Vector3.left * floorOffset + Vector3.forward * floorOffset;
                for (int i = 0; i < eachSideWallNum; ++i)
                {
                    for (int j = 0; j < eachSideWallNum; ++j)
                    {
                        Vector3 position = start + Vector3.right * floorHalfWidth * 2 * j;
                        Instantiate(floorPrefab, position, Quaternion.Euler(-90, 0, 0), roomHierarchyRoot.transform);
                    }

                    start += 2 * floorHalfWidth * Vector3.back;
                }

                // 벽 배치
                float wallOffset = wallHalfWidth * (eachSideWallNum - 1);
                float doorWallOffset = doorWallHalfWidth * (eachSideWallNum - 1);

                for (int i = 0; i < 4; ++i)
                {
                    for (int j = 0; j < eachSideWallNum; ++j)
                    {
                        Vector3 position = room.Position +
                            wallPositionsA[i] * doorWallOffset + // 첫 번째 벽 배치 위치
                            2 * doorWallHalfWidth * j * -wallPositionsA[i] + // 현재 벽 배치 위치
                            wallPositionsB[i] * eachSideWallNum; // 방 중앙에서부터 벽까지의 x축(앞뒤 벽) 또는 z축(좌우 벽) 거리

                        Instantiate(doorWallPrefab, position, doorWallRotations[i], roomHierarchyRoot.transform);
                    }
                }
            }

            CGrid.instance.UpdateGrid();
        }

        void InstantiateRooms()
        {
            MeshRenderer floorMesh = floorPrefab.GetComponent<MeshRenderer>();

            float floorHalfWidth = Mathf.RoundToInt(floorMesh.bounds.size.z) / 2;
            Vector3[] wallDirections = new Vector3[]
            {
                Vector3.forward,
                Vector3.right,
                Vector3.back,
                Vector3.left
            };

            foreach (Room room in rooms)
            {
                Destroy(room.Instance);

                if (room.Type == RoomType.Unit || room.Type == RoomType.VectorOnly)
                    continue;

                GameObject roomHierarchyRoot = new GameObject($"Room{room.ID}");
                GameObject floorsHierarchyRoot = new GameObject("floors");
                GameObject wallsHierarchyRoot = new GameObject("walls");

                roomHierarchyRoot.transform.SetParent(MapHierarchyRoot.transform);
                floorsHierarchyRoot.transform.SetParent(roomHierarchyRoot.transform);
                wallsHierarchyRoot.transform.SetParent(roomHierarchyRoot.transform);

                int eachSideWallNum = (int)room.Type;
                float offset = floorHalfWidth * eachSideWallNum;

                // 바닥 배치
                Vector3 first = room.Position + Vector3.left * offset + Vector3.back * offset;
                Vector3 start = first;
                for (int i = 0; i < eachSideWallNum; ++i)
                {
                    for (int j = 0; j < eachSideWallNum; ++j)
                    {
                        Vector3 position = start + 2 * floorHalfWidth * j * Vector3.right;
                        Instantiate(floorPrefab, position, Quaternion.identity, floorsHierarchyRoot.transform);
                    }

                    start += 2 * floorHalfWidth * Vector3.forward;
                }

                // 벽 배치
                int rotation = 0;
                start = first;

                for (int i = 0; i < 4; ++i)
                {
                    for (int j = 0; j < eachSideWallNum; ++j)
                    {
                        Vector3 position = start + 2 * floorHalfWidth * j * wallDirections[i];
                        Instantiate(doorWallPrefab, position, Quaternion.Euler(0, rotation, 0), wallsHierarchyRoot.transform);
                    }

                    start += 2 * floorHalfWidth * eachSideWallNum * wallDirections[i];
                    rotation += 90;
                }
            }

            CGrid.instance.UpdateGrid();
        }

        void RemoveUselessDoors()
        {
            CGrid grid = CGrid.instance;

            MeshRenderer floorMesh = floorPrefab.GetComponent<MeshRenderer>();
            float floorHalfWidth = Mathf.RoundToInt(floorMesh.bounds.size.z) / 2;
            
            for (int i = 0; i < MapHierarchyRoot.transform.childCount; ++i)
            {
                Transform walls = MapHierarchyRoot.transform.GetChild(i).GetChild(1);

                for (int j = walls.childCount - 1; j >= 0; --j)
                {
                    Transform wall = walls.GetChild(j);
                    Vector3 center = wall.position;

                    switch (wall.rotation.eulerAngles.y)
                    {
                        case 0:
                            center.z += floorHalfWidth;
                            break;

                        case 90:
                            center.x += floorHalfWidth;
                            break;

                        case 180:
                            center.z -= floorHalfWidth;
                            break;

                        case 270:
                            center.x -= floorHalfWidth;
                            break;
                    }

                    // 복도가 연결된 문이 아니면 벽으로 변경
                    CNode node = grid.GetNodeFromWorldPosition(center);
                    if (!node.Hallway)
                    {
                        GameObject obj = Instantiate(wall2mPrefab, wall.position, wall.rotation, walls);
                        Destroy(wall.gameObject);
                    }
                }
            }
        }

        void InstantiateHallways(List<List<AStarNode>> hallwayPaths)
        {
            GameObject hallwayHierarchyRoot = new GameObject("Hallways");
            hallwayHierarchyRoot.transform.SetParent(MapHierarchyRoot.transform);

            int gridNodeDiameter = CGrid.instance.gridNodeDiameter;
            LayerMask floorLayer = LayerMask.GetMask("Floor");

            float[] scales = new float[2];
            Vector3[] positions = new Vector3[2];

            foreach (List<AStarNode> path in hallwayPaths)
            {
                CNode startNode = null;
                Directions startDirection = Directions.NONE;
                int straight = 0;
                
                for (int i = 0; i < path.Count; ++i)
                {
                    CNode node = path[i].Node;

                    if (i + 1 == path.Count)
                        break;

                    bool isReached = false;
                    if (Physics.CheckSphere(node.WorldPosition, gridNodeDiameter / 2f, floorLayer))
                    {
                        if (startNode == null)
                            continue;

                        isReached = true;
                    }

                    Directions dir = node.getDirection(path[i + 1].Node);
                    if (startNode == null || startDirection == Directions.NONE)
                    {
                        startNode = node;
                        startDirection = dir;
                    }
                    
                    ++straight;
                    
                    if (startDirection != dir || straight == 4 || isReached)
                    {
                        int rotation = 0;

                        scales[0] = 1;
                        scales[1] = 1;

                        Directions dirFromStartToBefore = startNode.getDirection(path[i - straight].Node);

                        if (isReached)
                        {
                            dir = dirFromStartToBefore;
                        }

                        switch (startDirection)
                        {
                            case Directions.UP:
                                positions[0] = startNode.WorldPosition + Vector3.left * gridNodeDiameter + Vector3.back * gridNodeDiameter;
                                positions[1] = startNode.WorldPosition + Vector3.right * gridNodeDiameter + Vector3.back * gridNodeDiameter;

                                if (dirFromStartToBefore == Directions.LEFT)
                                {
                                    positions[0] += gridNodeDiameter * 2 * Vector3.forward;
                                }
                                else if (dirFromStartToBefore == Directions.RIGHT)
                                {
                                    positions[1] += gridNodeDiameter * 2 * Vector3.forward;
                                }

                                if (startDirection != dir)
                                {
                                    if (dir == Directions.LEFT)
                                    {
                                        scales[0] = (straight - 1) / 4f;
                                        scales[1] = straight / 4f;
                                    }
                                    else
                                    {
                                        scales[0] = straight / 4f;
                                        scales[1] = (straight - 1) / 4f;
                                    }
                                }
                                break;

                            case Directions.RIGHT:
                                positions[0] = startNode.WorldPosition + Vector3.left * gridNodeDiameter + Vector3.forward * gridNodeDiameter;
                                positions[1] = startNode.WorldPosition + Vector3.left * gridNodeDiameter + Vector3.back * gridNodeDiameter;
                                rotation = 90;

                                if (dirFromStartToBefore == Directions.UP)
                                {
                                    positions[0] += gridNodeDiameter * 2 * Vector3.right;
                                }
                                else if (dirFromStartToBefore == Directions.DOWN)
                                {
                                    positions[1] += gridNodeDiameter * 2 * Vector3.right;
                                }

                                if (startDirection != dir)
                                {
                                    if (dir == Directions.UP)
                                    {
                                        scales[0] = (straight - 1) / 4f;
                                        scales[1] = straight / 4f;
                                    }
                                    else
                                    {
                                        scales[0] = straight / 4f;
                                        scales[1] = (straight - 1) / 4f;
                                    }
                                }
                                break;

                            case Directions.DOWN:
                                positions[0] = startNode.WorldPosition + Vector3.left * gridNodeDiameter + Vector3.forward * gridNodeDiameter;
                                positions[1] = startNode.WorldPosition + Vector3.right * gridNodeDiameter + Vector3.forward * gridNodeDiameter;
                                rotation = 180;

                                if (dirFromStartToBefore == Directions.LEFT)
                                {
                                    positions[0] += gridNodeDiameter * 2 * Vector3.back;
                                }
                                else if (dirFromStartToBefore == Directions.RIGHT)
                                {
                                    positions[1] += gridNodeDiameter * 2 * Vector3.back;
                                }

                                if (startDirection != dir)
                                {
                                    if (dir == Directions.LEFT)
                                    {
                                        scales[0] = (straight - 1) / 4f;
                                        scales[1] = straight / 4f;
                                    }
                                    else
                                    {
                                        scales[0] = straight / 4f;
                                        scales[1] = (straight - 1) / 4f;
                                    }
                                }
                                break;

                            case Directions.LEFT:
                                positions[0] = startNode.WorldPosition + Vector3.right * gridNodeDiameter + Vector3.forward * gridNodeDiameter;
                                positions[1] = startNode.WorldPosition + Vector3.right * gridNodeDiameter + Vector3.back * gridNodeDiameter;
                                rotation = 270;

                                if (dirFromStartToBefore == Directions.UP)
                                {
                                    positions[0] += gridNodeDiameter * 2 * Vector3.left;
                                }
                                else if (dirFromStartToBefore == Directions.DOWN)
                                {
                                    positions[1] += gridNodeDiameter * 2 * Vector3.left;
                                }

                                if (startDirection != dir)
                                {
                                    if (dir == Directions.UP)
                                    {
                                        scales[0] = (straight - 1) / 4f;
                                        scales[1] = straight / 4f;
                                    }
                                    else
                                    {
                                        scales[0] = straight / 4f;
                                        scales[1] = (straight - 1) / 4f;
                                    }
                                }
                                break;
                        }

                        for (int j = 0; j < 2; ++j)
                        {
                            GameObject hallwayWall = Instantiate(wall2mPrefab, positions[j], Quaternion.Euler(0, rotation, 0), hallwayHierarchyRoot.transform);
                            hallwayWall.transform.localScale = new Vector3(1, 1, scales[j]);
                        }

                        if (isReached)
                        {
                            startNode = null;
                            straight = 0;
                            startDirection = Directions.NONE;
                        }
                        else
                        {
                            startNode = node;
                            straight = 1;
                            startDirection = node.getDirection(path[i + 1].Node);
                        }
                    }

                    Vector3 hallwayFloorPosition = node.WorldPosition + gridNodeDiameter / 2f * Vector3.left + gridNodeDiameter / 2f * Vector3.back;
                    GameObject hallwayFloor = Instantiate(hallwayFloorPrefab, hallwayFloorPosition, Quaternion.identity, hallwayHierarchyRoot.transform);
                    hallwayFloor.transform.localScale = new Vector3(0.3f, 1, 0.3f);
                }
            }

            CGrid.instance.UpdateGrid();
        }
        #endregion

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
