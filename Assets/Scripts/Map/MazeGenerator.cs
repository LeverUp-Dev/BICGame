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

        Transform mapTransform;
        Transform hierarchyRoot;
        GameObject wallPrefab;
        GameObject floorPrefab;

        private int mapSize;
        private int mazeCenter = MAZE_MAP_SIZE / 2, StartSize = 2;
        // 해당 방에서 어떤 방향의 벽을 허물 지 저장
        Directions[,] mazeWallMap;
        GameObject[,] mazeWalls;

        void Clear(bool[] isBlock) { for (int j = 0; j < isBlock.Length; ++j) isBlock[j] = false; }
        private bool isAllTrue(bool[] isFalse) { for (int i = 0; i < isFalse.Length; i++) if (!isFalse[i]) return false; return true; }

        public MazeGenerator(Transform mapTransform, Transform hierarchyRoot, GameObject wallPrefab, GameObject floorPrefab)
        {
            this.mapTransform = mapTransform;
            this.hierarchyRoot = hierarchyRoot;
            this.wallPrefab = wallPrefab;
            this.floorPrefab = floorPrefab;

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

        private bool StartRange(int x, int y)
        {
            if ((x > mazeCenter - StartSize && x <= mazeCenter + StartSize)
                && (y > mazeCenter - StartSize && y <= mazeCenter + StartSize)) return true;
            else return false;
        }

        //대가리 깨질 뻔
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
                bool[] isBlock = new bool[4];
                bool isRemain = false;
                while (!isAllTrue(isBlock))
                {
                    int prevX = x, prevY = y;
                    int randIndex = Random.Range(0, 4);
                    Directions r = (Directions)(1 << randIndex);
                    switch (r)
                    {
                        case Directions.DOWN: // 2
                            if (MAZE_MAP_SIZE == y + 1 || mazeWallMap[x, y + 1] != Directions.NONE || StartRange(x, y + 2)) isBlock[randIndex] = true;
                            else y++;
                            break;
                        case Directions.RIGHT: // 1
                            if (MAZE_MAP_SIZE == x + 1 || mazeWallMap[x + 1, y] != Directions.NONE || StartRange(x + 2, y)) isBlock[randIndex] = true;
                            else x++;
                            break;
                        case Directions.UP: // 0
                            if (0 > y - 1 || mazeWallMap[x, y - 1] != Directions.NONE || StartRange(x, y - 2)) isBlock[randIndex] = true;
                            else y--;
                            break;
                        case Directions.LEFT: // 3
                            if (0 > x - 1 || mazeWallMap[x - 1, y] != Directions.NONE || StartRange(x - 2, y)) isBlock[randIndex] = true;
                            else x--;
                            break;
                    }
                    if (isBlock[randIndex]) continue;

                    Clear(isBlock);
                    mazeWallMap[prevX, prevY] |= r; // 원래 방문 벽 허물기
                    mazeWallMap[x, y] |= r.GetOppositeDirection(); //이동할 방향
                }

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
            for (int i = 0; i < mapSize; i++)
            {
                for (int j = 0; j < mapSize; j++)
                {
                    int x = (i - 1) / 2;
                    int y = (mapSize - j - 1) / 2;
                    bool startRangeUnblock = x > mazeCenter - StartSize && x < mazeCenter + StartSize &&
                        y > mazeCenter - StartSize + 1 && y <= mazeCenter + StartSize;

                    var mazeHalfSize = new Vector3(mapSize, 0, mapSize) / 2;
                    var wallPosition = new Vector3(i, 0, j) - mazeHalfSize + mapTransform.position + Vector3.left * 0.5f;
                    var floorPosition = new Vector3(i, 0, j) - mazeHalfSize + mapTransform.position + Vector3.down * 0.1f;

                    Object.Instantiate(floorPrefab, floorPosition, Quaternion.identity, hierarchyRoot); // 바닥 형성
                    // 외벽에 네 구역과 연결하기 위해 벽 허물기
                    if (i == 0 || i == mapSize - 1 || j == 0 || j == mapSize - 1)
                    {
                        if (CGrid.Instance.GetNodeFromWorldPosition(wallPosition + Vector3.forward * CGrid.Instance.GridNodeDiameter).Hallway)
                            continue;
                    }

                    if (i > 0 && i < mapSize && j > 0 && j < mapSize)
                    {
                        if (startRangeUnblock) continue;
                        else if (i % 2 == 1 && j % 2 == 1)
                        {
                            /* 벽 허물기 (왼쪽 맨 아래부터 위-오른쪽으로 순회) */

                            Directions crashDirections = mazeWallMap[x, y];
                            
                            if (crashDirections.Contains(Directions.LEFT) || mazeWallMap[x, y] == Directions.NONE)
                                Object.Destroy(mazeWalls[i - 1, mapSize - j - 1]);

                            if (crashDirections.Contains(Directions.DOWN) || mazeWallMap[x, y] == Directions.NONE)
                                Object.Destroy(mazeWalls[i, mapSize - j]);

                            continue;
                        }
                    }

                    mazeWalls[i, mapSize - j - 1] = Object.Instantiate(wallPrefab, wallPosition, Quaternion.identity, hierarchyRoot);
                }
            }
        }
    }
}