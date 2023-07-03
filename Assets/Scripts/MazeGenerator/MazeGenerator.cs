using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Hypocrites
{
    using Hypocrites.Defines;
    using Hypocrites.Grid;

    public class MazeGenerator : MonoBehaviour
    {

        public Vector2Int mazeSize = new Vector2Int(25, 25);
        private Vector2Int BlockSize => mazeSize / 2;
        private bool[,] unCrushWall;

        CGrid[,] Rooms;


        private void Awake()
        {
            Rooms = new CGrid[BlockSize.x, BlockSize.y];
            unCrushWall = new bool[mazeSize.x, mazeSize.y];
        }

        void Start()
        {
            InitRooms();
            HuntAndKill();
            MakePathRoot();
        }

        private void MakePathRoot()
        {
            //미로가 형성되는 경우, 모든 점의 홀수가 되는 좌표값을 방으로 잡는 구조
            for (int x = 0; x < BlockSize.x; x++)
            {
                for (int y = 0; y < BlockSize.y; y++)
                {
                    var adjustPosition = new Vector2Int(x * 2 + 1, y * 2 + 1);
                    unCrushWall[adjustPosition.x, adjustPosition.y] = true;
                    foreach (var dir in Rooms[x, y].unBlock)
                    {
                        if (dir == Directions.DOWN) unCrushWall[adjustPosition.x, adjustPosition.y - 1] = true;
                        else if (dir == Directions.RIGHT) unCrushWall[adjustPosition.x + 1, adjustPosition.y] = true;
                        else if (dir == Directions.UP) unCrushWall[adjustPosition.x, adjustPosition.y + 1] = true;
                        else if (dir == Directions.LEFT) unCrushWall[adjustPosition.x - 1, adjustPosition.y] = true;
                    }
                }
            }
        }

        public void HuntAndKill()
        {
            int x = Random.Range(0, BlockSize.x), y = Random.Range(0, BlockSize.y);
            bool[] isBlock = new bool[4];
            bool isEnd = false;
            Vector2Int targetPos = new Vector2Int(x, y);
            List<Vector2Int> list = new List<Vector2Int>();
            HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

            list.Add(targetPos);
            while (!isEnd)
            {
                bool isRemain = false;
                while (!isAllTrue(Rooms[x, y].closeWay))
                {
                    int r = Random.Range(1, 5);
                    if (Rooms[x, y].closeWay[r - 1]) continue;
                    if (r == 1)
                    {
                        targetPos = new Vector2Int(x, y - 1);
                        if (0 > targetPos.y || visited.Contains(targetPos) || list.Contains(targetPos))
                            Rooms[x, y].closeWay[r - 1] = true;
                        else
                        {
                            Rooms[x, y].unBlock.Add(Directions.DOWN);
                            list.Add(targetPos);
                            y--;
                        }
                    }
                    else if (r == 2)
                    {
                        targetPos = new Vector2Int(x + 1, y);
                        if (BlockSize.x == targetPos.x || visited.Contains(targetPos) || list.Contains(targetPos))
                            Rooms[x, y].closeWay[r - 1] = true;
                        else
                        {
                            Rooms[x, y].unBlock.Add(Directions.RIGHT);
                            list.Add(targetPos);
                            x++;
                        }
                    }
                    else if (r == 3)
                    {
                        targetPos = new Vector2Int(x, y + 1);
                        if (BlockSize.y == targetPos.y || visited.Contains(targetPos) || list.Contains(targetPos))
                            Rooms[x, y].closeWay[r - 1] = true;
                        else
                        {
                            Rooms[x, y].unBlock.Add(Directions.UP);
                            list.Add(targetPos);
                            y++;
                        }
                    }
                    else if (r == 4)
                    {
                        targetPos = new Vector2Int(x - 1, y);
                        if (0 > targetPos.x || visited.Contains(targetPos) || list.Contains(targetPos))
                            Rooms[x, y].closeWay[r - 1] = true;
                        else
                        {
                            Rooms[x, y].unBlock.Add(Directions.LEFT);
                            list.Add(targetPos);
                            x--;
                        }
                    }
                }

                visited.UnionWith(list);
                list.Clear();
                for (int yPos = 0; yPos < BlockSize.y; yPos++) // 포함되지 않은 방을 색출하여 주변 벽 중의 하나를 무작위 축출하기
                {
                    for (int xPos = 0; xPos < BlockSize.x; xPos++)
                    {
                        targetPos = new Vector2Int(xPos, yPos);
                        if (!visited.Contains(targetPos))
                        {
                            List<Directions> openWay = new List<Directions>();
                            if (visited.Any(v => (v.x == targetPos.x) && (v.y == targetPos.y + 1))) openWay.Add(Directions.UP);
                            if (visited.Any(v => (v.x == targetPos.x) && (v.y == targetPos.y - 1))) openWay.Add(Directions.DOWN);
                            if (visited.Any(v => (v.x == targetPos.x + 1) && (v.y == targetPos.y))) openWay.Add(Directions.RIGHT);
                            if (visited.Any(v => (v.x == targetPos.x - 1) && (v.y == targetPos.y))) openWay.Add(Directions.LEFT);

                            if (openWay.Count > 0)
                            {
                                list.Add(targetPos);
                                x = targetPos.x; y = targetPos.y;
                                Rooms[x, y].unBlock.Add(openWay[Random.Range(0, openWay.Count)]);
                                isRemain = true;
                                yPos = BlockSize.y + 1;
                                break;
                            }

                        }
                    }
                }
                if (!isRemain) isEnd = true;
            }
            visited.Clear();
        }



        // 보조 도구들
        private bool isAllTrue(bool[] isFalse) { for (int i = 0; i < isFalse.Length; i++) if (!isFalse[i]) return false; return true; }
        private void InitRooms()
        {
            for (int y = 0; y < BlockSize.y; y++)
            {
                for (int x = 0; x < BlockSize.x; x++)
                {
                    Rooms[x, y] = new CGrid();
                }
            }
        }

    }
}
