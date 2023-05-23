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

    public class MapGenerator : MonoBehaviour
    {
        public bool run;

        public LayerMask roomLayerMask;

        public int maxWidth;
        public int maxHeight;
        public int maxRoomCount;

        public float cycleHallwayCreationChance;
        public int roomMinimumDistance;

        public GameObject unitRoomObject;

        public int floor;

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
                Generate();

            /*Triangle tri = new Triangle(new Room(new Vector3(44, 0, -45)), new Room(new Vector3(-15, 0, -47)), new Room(new Vector3(44, 0, -9)));
            DrawTriangle(tri);
            DrawCircumCircle(tri);*/
        }

        void Generate()
        {
            rooms = new List<Room>();

            // 1. �������� �� ��ġ ����
            for (int i = 0; i < maxRoomCount; ++i)
            {
                int randomX = Random.Range(0, CGrid.instance.GridXSize);
                int randomZ = Random.Range(0, CGrid.instance.GridYSize);

                CNode node = CGrid.instance.Grid[randomX, randomZ];
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

                if (isDuplicated || Physics.CheckSphere(randomPos, roomMinimumDistance, roomLayerMask))
                    --i;
                else
                    rooms.Add(new Room(i, randomPos, 0, Instantiate(unitRoomObject, randomPos, Quaternion.identity)));
            }

            // 2. ��γ� �ﰢ���� ���� (���� : https://www.gorillasun.de/blog/bowyer-watson-algorithm-for-delaunay-triangulation/)
#if DEBUG_DRAW
            StartCoroutine(Triangulate(rooms));
#else
            List<Triangle> triangulatedList = Triangulate(rooms);

            // 3. ������� �ﰢ������ �׷����� ��ȯ
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

            // 4. �׷����� Minimum Spanning Tree�� ��ȯ
            Edge[,] mstTree = GraphToMST(triangulatedGraph, triangulatedList[0].R1);

            // 5. ���õ��� ���� ���� �� �������� ������ MST Tree�� �߰� (��ȯ ���� ����)
            for (int i = 0; i < maxRoomCount; ++i)
            {
                for (int j = 0; j < maxRoomCount; ++j)
                {
                    if (mstTree[i, j] == null && triangulatedGraph[i, j] == 1)
                    {
                        // ���� Ȯ���� Ż���� ���� �߰�
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

            // 6. A* �˰��� ����
            AStarPathfinder astar = new AStarPathfinder();
            CGrid grid = CGrid.instance;

            for (int i = 0; i < maxRoomCount; ++i)
            {
                for (int j = 0; j < maxRoomCount; ++j)
                {
                    if (mstTree[i, j] != null)
                    {
                        Edge edge = mstTree[i, j];
                        astar.FindPath(grid.GetNodeFromWorldPosition(edge.p1.Position), grid.GetNodeFromWorldPosition(edge.p2.Position));

                        mstTree[j, i] = null;
                    }
                }
            }
#endif
        }

        #region ��γ� �ﰢ���� �޼ҵ�
        Triangle SuperTriangle()
        {
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
        #endregion

        #region MST �޼ҵ�
        Edge[,] GraphToMST(int[,] graph, Room first)
        {
            Edge[,] mstTree = new Edge[maxRoomCount, maxRoomCount];
            MinHeap<Edge> heap = new MinHeap<Edge>(maxRoomCount * (maxRoomCount + 1) / 2);

            // �ּ� ���� ù ������ ������ ���� �߰�
            for (int i = 0; i < maxRoomCount; ++i)
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

                // MST Tree�� ����ġ�� ���� ���� ���� �߰�
                mstTree[min.p1.ID, min.p2.ID] = min;

                // �߰��� ������ ������ ���� �߰�
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
