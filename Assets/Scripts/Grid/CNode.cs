using Hypocrites.Enumerations;
using UnityEngine;

namespace Hypocrites.Grid
{
    public class CNode
    {
        public Vector3 WorldPosition { get; }
        public int GridX { get; private set; }
        public int GridY { get; private set; }
        public bool Walkable { get; set; }

        public bool Hallway { get; set; }

        public CNode(Vector3 position, int x, int y, bool walkable)
        {
            WorldPosition = position;
            GridX = x;
            GridY = y;
            Walkable = walkable;

            Hallway = false;
        }

        public bool IsDiagonal(CNode to)
        {
            int dx = to.GridX - GridX;
            int dy = to.GridY - GridY;

            return dx != 0 && dy != 0;
        }

        // to 노드가 상하좌우 중 어떤 방향에 존재하는지 반환
        public Directions GetDirection(CNode to)
        {
            int dx = to.GridX - GridX;
            int dy = to.GridY - GridY;

            if (dx == 0)
                return dy == -1 ? Directions.UP : Directions.DOWN;
            else
                return dx == 1 ? Directions.RIGHT : Directions.LEFT;
        }

        public CNode GetNext(Directions dir)
        {
            CGrid grid = CGrid.Instance;

            int x = GridX;
            int y = GridY;

            switch (dir)
            {
                case Directions.UP:
                    --y;
                    break;

                case Directions.RIGHT:
                    ++x;
                    break;

                case Directions.DOWN:
                    ++y;
                    break;

                case Directions.LEFT:
                    --x;
                    break;

                default:
                    return this;
            }

            if (x < 0 || x >= grid.GridXSize || y < 0 || y >= grid.GridYSize)
                return null;

            return grid.Grid[y, x];
        }
    }
}