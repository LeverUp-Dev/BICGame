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

        public GameObject Instance { get; }

        public List<Edge> Edges { get; set; }

        public Room(Vector3 position)
        {
            Position = position;
            Type = RoomType.VectorOnly;
        }

        public Room(int id, Vector3 position, RoomType type, GameObject instance)
        {
            ID = id;
            Position = position;
            Type = type;
            Instance = instance;

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
