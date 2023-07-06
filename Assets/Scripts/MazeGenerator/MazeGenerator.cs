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
                    int r = Random.Range(0, 4);
                    switch (r)
                    {
                        case 0: if (MAZE_MAP_SIZE == y + 1 || mazeWallMap[x, y + 1] != Directions.NONE) isBlock[r] = true; break;
                        case 1: if (MAZE_MAP_SIZE == x + 1 || mazeWallMap[x + 1, y] != Directions.NONE) isBlock[r] = true; break;
                        case 2: if (0 > y - 1 || mazeWallMap[x, y - 1] != Directions.NONE) isBlock[r] = true; break;
                        case 3: if (0 > x - 1 || mazeWallMap[x - 1, y] != Directions.NONE) isBlock[r] = true; break;
                    }
                    if (isBlock[r]) continue;

                    if (r == 0)
                    {
                        Clear(isBlock);
                        mazeWallMap[x, y] |= Directions.DOWN; //이동할 방향
                        mazeWallMap[x, ++y] |= Directions.UP; // 다음 방 방문 표시
                    }
                    else if (r == 1)
                    {

                        Clear(isBlock);
                        mazeWallMap[x, y] |= Directions.RIGHT; //이전 방 방문 표시
                        mazeWallMap[++x, y] |= Directions.LEFT; //이동할 방향

                    }
                    else if (r == 2)
                    {
                        Clear(isBlock);
                        mazeWallMap[x, --y] |= Directions.DOWN; //이동할 방향
                        mazeWallMap[x, y] |= Directions.UP; //이전 방 방문표시
                    }
                    else if (r == 3)
                    {
                        Clear(isBlock);
                        mazeWallMap[x, y] |= Directions.LEFT; // 이동할 방향
                        mazeWallMap[--x, y] |= Directions.RIGHT; //다음 방 방문 표시
                    }
                }

                for (int yPos = 0;  yPos < MAZE_MAP_SIZE; yPos++)
                {
                    for(int xPos= 0; xPos < MAZE_MAP_SIZE; xPos++)
                    {
                        if (mazeWallMap[xPos, yPos] == Directions.NONE)
                        {
                            List<Directions> list = new List<Directions>();
                            if(xPos + 1 != MAZE_MAP_SIZE) if (mazeWallMap[xPos + 1, yPos]!= Directions.NONE) list.Add(Directions.RIGHT);
                            if(xPos - 1 > -1) if (mazeWallMap[xPos - 1, yPos] != Directions.NONE) list.Add(Directions.LEFT);
                            if(yPos + 1 != MAZE_MAP_SIZE) if (mazeWallMap[xPos, yPos + 1] != Directions.NONE) list.Add(Directions.DOWN);
                            if(yPos - 1 > -1) if (mazeWallMap[xPos, yPos - 1] != Directions.NONE) list.Add(Directions.UP);

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