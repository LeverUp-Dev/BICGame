using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RandomMap.Elements
{
    public class Edge
    {
        public Room p1 { get; }
        public Room p2 { get; }
        private float weight;
        public float Weight { 
            get
            {
                if (weight == -1)
                    weight = Mathf.Pow(p1.Position.x - p2.Position.x, 2) + Mathf.Pow(p1.Position.z - p2.Position.z, 2);

                return weight;
            }
            set
            {
                weight = value;
            }
        }

        public Edge(Room _p1, Room _p2)
        {
            p1 = _p1;
            p2 = _p2;
            Weight = -1;
        }

        public bool Equals(Edge target)
        {
            return (p1.Equals(target.p1) && p2.Equals(target.p2)) || (p1.Equals(target.p2) && p2.Equals(target.p1));
        }
    }
}
