using RandomMap.Elements;
using RandomMap.Enumerations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RandomMap
{
    public class Room
    {
        public int ID { get; }
        public Vector3 Position { get; set; }
        public RoomType Type { get; }

        public GameObject RoomHierarchyRoot { get; private set; }
        public GameObject WallsHierarchyRoot { get; set; }
        public GameObject FloorsHierarchyRoot { get; set; }

        public List<Edge> Edges { get; set; }

        public GameObject positionInstance;

        public Room(Vector3 position)
        {
            Position = position;
            Type = RoomType.VectorOnly;
        }

        public Room(int id, Vector3 position, RoomType type, GameObject instance, Transform parent)
        {
            ID = id;
            Position = position;
            Type = type;
            positionInstance = instance;

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

        public bool Equals(Room r)
        {
            return Position.Equals(r.Position);
        }
    }
}
