using RandomMap.Enumerations;
using UnityEngine;

public class CNode
{
    public Vector3 WorldPosition { get; }
    public int GridX { get; set; }
    public int GridY { get; set; }
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

    public bool isDiagonal(CNode to)
    {
        int dx = to.GridX - GridX;
        int dy = to.GridY - GridY;

        return dx != 0 && dy != 0;
    }

    public Directions getDirection(CNode to)
    {
        int dx = to.GridX - GridX;
        int dy = to.GridY - GridY;

        if (dx == 0)
            return dy == -1 ? Directions.UP : Directions.DOWN;
        else
            return dx == 1 ? Directions.RIGHT : Directions.LEFT;
    }
}
