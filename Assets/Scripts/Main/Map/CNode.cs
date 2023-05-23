using UnityEngine;

public class CNode
{
    public Vector3 WorldPosition { get; }
    public int GridX { get; set; }
    public int GridY { get; set; }
    public bool Walkable { get; set; }
    
    public CNode(Vector3 position, int x, int y, bool walkable)
    {
        WorldPosition = position;
        GridX = x;
        GridY = y;
        Walkable = walkable;
    }

    public bool isDiagonal(CNode to)
    {
        int dx = to.GridX - GridX;
        int dy = to.GridY - GridY;

        return dx != 0 && dy != 0;
    }
}
