using System.Collections.Generic;
using UnityEngine;

namespace Hypocrites.Map.Elements
{
    using Hypocrites.Map.Enumerations;
    using System.Runtime.CompilerServices;

    public class Room
    {
        public int ID { get; }
        public Vector3 Position { get; set; }
        public RoomType Type { get; }
        public int Area { get; private set; }

        public GameObject RoomHierarchyRoot { get; private set; }
        public GameObject WallsHierarchyRoot { get; set; }
        public GameObject FloorsHierarchyRoot { get; set; }

        public List<Edge> Edges { get; set; }

        public GameObject PositionInstance { get; set; }

        public Room(Vector3 position)
        {
            Position = position;
            Type = RoomType.VectorOnly;
        }

        public Room(int id, int area, Vector3 position, RoomType type, GameObject instance, Transform parent)
        {
            ID = id;
            Area = area;
            Position = position;
            Type = type;
            PositionInstance = instance;

            RoomHierarchyRoot = new GameObject("Room" + id);
            WallsHierarchyRoot = new GameObject("walls");
            FloorsHierarchyRoot = new GameObject("floors");

            RoomHierarchyRoot.transform.SetParent(parent);
            WallsHierarchyRoot.transform.SetParent(RoomHierarchyRoot.transform);
            FloorsHierarchyRoot.transform.SetParent(RoomHierarchyRoot.transform);

            Edges = new List<Edge>();
        }

        public void AddEdge(Edge edge)
        {
            foreach (Edge e in Edges)
            {
                if (e.Equals(edge))
                    return;
            }

            Edges.Add(edge);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetLocalID(int roomsPerArea)
        {
            return ID - Area * roomsPerArea;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Room r)
        {
            return Position.Equals(r.Position);
        }
    }
}
