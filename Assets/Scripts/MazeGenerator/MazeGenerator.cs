using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Hypocrites.Defines;

namespace Maze
{
    public class MazeGenerator : MonoBehaviour
    {
        const int MAZE_MAP_SIZE = 49;

        public GameObject wallPrefab;
        private int mapSize;

        // 해당 방에서 어떤 방향의 벽을 허물 지 저장
        Directions[,] mazeWallMap;
        GameObject[,] mazeWalls;

        MeshRenderer wallMeshRenderer;

        void Clear(bool[] isBlock) { for (int j = 0; j < isBlock.Length; ++j) isBlock[j] = false; }
        private bool isAllTrue(bool[] isFalse) { for (int i = 0; i < isFalse.Length; i++) if (!isFalse[i]) return false; return true; }
        private void Awake()
        {
            // 미로 정보 초기화
            mapSize = MAZE_MAP_SIZE * 2 + 1;

            mazeWallMap = new Directions[MAZE_MAP_SIZE, MAZE_MAP_SIZE];
            mazeWalls = new GameObject[mapSize, mapSize];
        }

        private void Start()
        {
            //CreateMaze();
            //HuntAndKill();
        }


        //대가리 깨질 뻔
        private void HuntAndKill()
        {
            int x = Random.Range(0, MAZE_MAP_SIZE), y = Random.Range(0, MAZE_MAP_SIZE);
            bool isEnd = false;

            mazeWallMap[x, y] = Directions.RIGHT; // 처음 자리 잡은 방에 방문 표시

            
            while(!isEnd)
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
                            if (MAZE_MAP_SIZE == y + 1 || mazeWallMap[x, y + 1] != Directions.NONE) isBlock[randIndex] = true;
                            else y++;
                            break;
                        case Directions.RIGHT: // 1
                            if (MAZE_MAP_SIZE == x + 1 || mazeWallMap[x + 1, y] != Directions.NONE) isBlock[randIndex] = true;
                            else x++;
                            break;
                        case Directions.UP: // 0
                            if (0 > y - 1 || mazeWallMap[x, y - 1] != Directions.NONE) isBlock[randIndex] = true;
                            else y--;
                            break;
                        case Directions.LEFT: // 3
                            if (0 > x - 1 || mazeWallMap[x - 1, y] != Directions.NONE) isBlock[randIndex] = true;
                            else x--;
                            break;
                    }
                    if (isBlock[randIndex]) continue;

                    Clear(isBlock);
                    mazeWallMap[prevX, prevY] |= r; // 원래 방문 벽 허물기
                    mazeWallMap[x, y] |= r.GetOppositeDirection(); //이동할 방향
                }

                for (int yPos = 0;  yPos < MAZE_MAP_SIZE; yPos++)
                {
                    for(int xPos= 0; xPos < MAZE_MAP_SIZE; xPos++)
                    {
                        if (mazeWallMap[xPos, yPos] == Directions.NONE)
                        {
                            List<Directions> list = new List<Directions>();
                            if(xPos + 1 != MAZE_MAP_SIZE && mazeWallMap[xPos + 1, yPos]!= Directions.NONE) list.Add(Directions.RIGHT);
                            if(xPos - 1 > -1 && mazeWallMap[xPos - 1, yPos] != Directions.NONE) list.Add(Directions.LEFT);
                            if(yPos + 1 != MAZE_MAP_SIZE && mazeWallMap[xPos, yPos + 1] != Directions.NONE) list.Add(Directions.DOWN);
                            if(yPos - 1 > -1 && mazeWallMap[xPos, yPos - 1] != Directions.NONE) list.Add(Directions.UP);

                            if(list.Count > 0)
                            {
                                Directions randDir = list[Random.Range(0, list.Count)];
                                switch(randDir)
                                {
                                    case Directions.RIGHT:
                                        mazeWallMap[xPos + 1, yPos] |= Directions.LEFT;
                                        mazeWallMap[xPos, yPos] |= Directions.RIGHT;
                                        break;
                                    case Directions.LEFT:
                                        mazeWallMap[xPos, yPos] |= Directions.LEFT;
                                        break;
                                    case Directions.DOWN:
                                        mazeWallMap[xPos, yPos] |= Directions.DOWN; 
                                        break;
                                    case Directions.UP:
                                        mazeWallMap[xPos, yPos - 1] |= Directions.DOWN;
                                        mazeWallMap[xPos, yPos] |= Directions.UP; 
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
    }
}