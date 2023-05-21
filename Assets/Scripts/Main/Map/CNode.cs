using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CNode
{
    public Vector3 Position { get; }
    public bool Walkable { get; }

    public CNode(Vector3 position, bool walkable)
    {
        Position = position;
        Walkable = walkable;
    }
}
