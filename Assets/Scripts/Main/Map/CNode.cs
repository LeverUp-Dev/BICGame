using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CNode
{
    public Vector3 Position { get; set; }
    public bool Walkable { get; set; }

    public CNode(Vector3 position, bool walkable)
    {
        Position = position;
        Walkable = walkable;
    }
}
