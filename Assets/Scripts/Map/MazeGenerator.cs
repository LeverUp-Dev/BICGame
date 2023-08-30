using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hypocrites.Maze
{
    using Defines;
    using Hypocrites.Grid;
    using UnityEditor.Experimental.GraphView;

    public class MazeGenerator
    {
        public const int MAZE_MAP_SIZE = 15;
        public const int MAZE_ROOM_PADDING = 8;
        [field: SerializeField] public GameObject[] AlterWallNodePrefab { get; private set; }

        Transform mapTransform;
        Transform hierarchyRoot;
        GameObject wallPrefab;
        GameObject floorPrefab;
        GameObject[] alterwallPrefab;

        private int mapSize;
        private int mazeCenter = MAZE_MAP_SIZE / 2, StartSize = 2;

        // 해당 방에서 어떤 방향의 벽을 허물 지 저장
        Directions[,] mazeWallMap;
        GameObject[,] mazeWalls;

        public MazeGenerator(Transform mapTransform, Transform hierarchyRoot, GameObject wallPrefab, GameObject floorPrefab, GameObject[] alt)
        {
            this.mapTransform = mapTransform;
            this.hierarchyRoot = hierarchyRoot;
            this.wallPrefab = wallPrefab;
            this.floorPrefab = floorPrefab;
            this.alterwallPrefab = alt;
            // 미로 정보 초기화
            mapSize = MAZE_MAP_SIZE * 2 + 1;

            mazeWallMap = new Directions[MAZE_MAP_SIZE, MAZE_MAP_SIZE];
            mazeWalls = new GameObject[mapSize, mapSize];
        }

        public void Generate()
        {
            HuntAndKill();
            CreateMaze();
        }

        public void GetCorner(out CNode leftBottom, out CNode rightTop)
        {
            Vector3 mazeHalfSize = new Vector3(mapSize, 0, mapSize) / 2;
            Vector3 offset = Vector3.forward * CGrid.Instance.GridNodeDiameter + Vector3.left * 0.5f;

            leftBottom = CGrid.Instance.GetNodeFromWorldPosition(mapTransform.position + -mazeHalfSize + offset);
            rightTop = CGrid.Instance.GetNodeFromWorldPosition(mapTransform.position + mazeHalfSize + offset);
        }

        private bool StartRange(int x, int y, int n = 0)
        {
            if ((x > mazeCenter - StartSize && x <= mazeCenter + StartSize - n)
                && (y > mazeCenter - StartSize + n && y <= mazeCenter + StartSize)) return true;
            else return false;
        }

        private void HuntAndKill()
        {
            int x, y;
            do
            {
                x = Random.Range(0, MAZE_MAP_SIZE);
                y = Random.Range(0, MAZE_MAP_SIZE);
            } while (StartRange(x, y));
            bool isEnd = false;

            mazeWallMap[x, y] = Directions.RIGHT; // 처음 자리 잡은 방에 방문 표시
            while (!isEnd)
            {
                while (true)
                {
                    List<Directions> dir = new List<Directions>();
                    if (MAZE_MAP_SIZE != y + 1 && mazeWallMap[x, y + 1] == Directions.NONE && !StartRange(x, y + 2)) dir.Add(Directions.DOWN);
                    if (MAZE_MAP_SIZE != x + 1 && mazeWallMap[x + 1, y] == Directions.NONE && !StartRange(x + 2, y)) dir.Add(Directions.RIGHT);
                    if (0 <= y - 1 && mazeWallMap[x, y - 1] == Directions.NONE && !StartRange(x, y - 2)) dir.Add(Directions.UP);
                    if (0 <= x - 1 && mazeWallMap[x - 1, y] == Directions.NONE && !StartRange(x - 2, y)) dir.Add(Directions.LEFT);

                    if (dir.Count != 0)
                    {
                        int prevX = x, prevY = y;
                        Directions r = dir[Random.Range(0, dir.Count)];
                        switch (r)
                        {
                            case Directions.DOWN: y++; break;
                            case Directions.RIGHT: x++; break;
                            case Directions.UP: y--; break;
                            case Directions.LEFT: x--; break;
                        }
                        mazeWallMap[prevX, prevY] |= r; // 원래 방문 벽 허물기
                        mazeWallMap[x, y] |= r.GetOppositeDirection(); //이동할 방향
                    }
                    else break;
                }

                bool isRemain = false;
                for (int yPos = 0; yPos < MAZE_MAP_SIZE; yPos++)
                {
                    for (int xPos = 0; xPos < MAZE_MAP_SIZE; xPos++)
                    {
                        if (StartRange(xPos, yPos)) continue;
                        else if (mazeWallMap[xPos, yPos] == Directions.NONE)
                        {
                            List<Directions> list = new List<Directions>();
                            if (xPos + 1 != MAZE_MAP_SIZE && mazeWallMap[xPos + 1, yPos] != Directions.NONE) list.Add(Directions.RIGHT);
                            if (xPos - 1 > -1 && mazeWallMap[xPos - 1, yPos] != Directions.NONE) list.Add(Directions.LEFT);
                            if (yPos + 1 != MAZE_MAP_SIZE && mazeWallMap[xPos, yPos + 1] != Directions.NONE) list.Add(Directions.DOWN);
                            if (yPos - 1 > -1 && mazeWallMap[xPos, yPos - 1] != Directions.NONE) list.Add(Directions.UP);

                            if (list.Count > 0)
                            {
                                Directions randDir = list[Random.Range(0, list.Count)];
                                switch (randDir)
                                {
                                    case Directions.RIGHT:
                                        mazeWallMap[xPos + 1, yPos] |= randDir.GetOppositeDirection();
                                        mazeWallMap[xPos, yPos] |= randDir;
                                        break;
                                    case Directions.LEFT:
                                        mazeWallMap[xPos, yPos] |= randDir;
                                        break;
                                    case Directions.DOWN:
                                        mazeWallMap[xPos, yPos] |= randDir;
                                        break;
                                    case Directions.UP:
                                        mazeWallMap[xPos, yPos - 1] |= randDir.GetOppositeDirection();
                                        mazeWallMap[xPos, yPos] |= randDir;
                                        break;
                                }
                                x = xPos; y = yPos;
                                isRemain = true;
                                yPos = MAZE_MAP_SIZE + 1;
                                break;
                            }
                        }
                    }
                }
                if (!isRemain) isEnd = true;
            }
        }

        private void CreateMaze()
        {
            /*
             * 1. mazeWallMap은 mazeWalls보다 크기가 1/2 - 1 만큼 작으므로 주의
             * 2. 2차원 배열의 왼쪽 맨 아래부터 위-오른쪽으로 순회하므로 순서 주의
             */

            int countX = 3, countY;
            // 방의 네 면 중 하나를 무작위로 출입구 결정
            CGrid grid = new CGrid();
            int xStart = mazeCenter, yStart = mazeCenter;
            Directions randDirStart = (Directions)(1 << Random.Range(0, 4));
            switch (randDirStart)
            {
                case Directions.RIGHT:
                case Directions.LEFT:
                    xStart += StartSize * (randDirStart == Directions.RIGHT ? 1 : -1) + 1;
                    yStart += Random.Range(-StartSize + 1, StartSize - 1);
                    break;
                case Directions.DOWN:
                case Directions.UP:
                    xStart += Random.Range(-StartSize + 1, StartSize - 1);
                    yStart += StartSize * (randDirStart == Directions.DOWN ? 1 : -1);
                    break;
            }
            for (int i = 0; i < mapSize; i++)
            {
                countY = countX;
                for (int j = 0; j < mapSize; j++)
                {
                    countY++; if (countY > 3) countY = 0;
                    int x = (i - 1) / 2;
                    int y = (mapSize - j - 1) / 2;
                    bool odd = i % 2 == 1 && j % 2 == 1;

                    var mazeHalfSize = new Vector3(mapSize, 0, mapSize) / 2;
                    var wallPosition = new Vector3(i, 0, j) - mazeHalfSize + mapTransform.position + Vector3.left; //구 프리팹은 '* 0.5f' 추가
                    var floorPosition = new Vector3(i, 0, j) - mazeHalfSize + mapTransform.position + (Vector3.back + Vector3.left) * grid.GridNodeRadius;
                    Object.Instantiate(floorPrefab, floorPosition, Quaternion.identity, hierarchyRoot); // 바닥 형성

                    // 방의 출입구 무작위 배치
                    if (x == xStart && y == yStart && odd)
                    {
                        if (randDirStart == Directions.RIGHT || randDirStart == Directions.LEFT) 
                            Object.Destroy(mazeWalls[i - 1, mapSize - j - 1]); //가로
                        else Object.Destroy(mazeWalls[i, mapSize - j]); // 세로
                    }
                    // 외벽에 네 구역과 연결하기 위해 벽 허물기
                    if (i == 0 || i == mapSize - 1 || j == 0 || j == mapSize - 1)
                    {
                        if (CGrid.Instance.GetNodeFromWorldPosition(wallPosition + Vector3.forward * CGrid.Instance.GridNodeDiameter).Hallway)
                            continue;
                    }

                    if (StartRange(x,y,1)) continue;
                    else if (odd)
                    {
                        /* 벽 허물기 (왼쪽 맨 아래부터 위-오른쪽으로 순회) */

                        Directions crashDirections = mazeWallMap[x, y];

                        if (crashDirections.Contains(Directions.LEFT) || (mazeWallMap[x, y] == Directions.NONE && x != mazeCenter - StartSize + 1))
                            Object.Destroy(mazeWalls[i - 1, mapSize - j - 1]);

                        if (crashDirections.Contains(Directions.DOWN) || (mazeWallMap[x, y] == Directions.NONE && y != mazeCenter + StartSize))
                            Object.Destroy(mazeWalls[i, mapSize - j]);

                        continue;
                    }
                    //코드 아래에 주석 처리된 구문을 'wallPrefab' 대신 대입할 경우 새로운 오브젝트 반영. alterwallPrefab[countY]
                    mazeWalls[i, mapSize - j - 1] = Object.Instantiate(alterwallPrefab[countY], wallPosition, Quaternion.identity, hierarchyRoot);
                }
                countX--; if (countX < 0) countX = 3;
            }
        }
    }
}