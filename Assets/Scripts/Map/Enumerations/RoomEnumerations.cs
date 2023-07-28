using System.Collections.Generic;
using UnityEngine;

namespace Hypocrites.Map.Enumerations
{
    public enum RoomType
    {
        VectorOnly,
        R5x5 = 5,
        R9x9 = 9,
        R13x13 = 13,
        R17x17 = 17,
        R21x21 = 21
    }

    public static class RoomTypeMethods
    {
        static Dictionary<RoomType, float> roomRadiuses = new Dictionary<RoomType, float>();

        public static float GetRadius(this RoomType roomType)
        {
            if (roomRadiuses.TryGetValue(roomType, out float radius))
                return radius;

            radius = Mathf.Sqrt(Mathf.Pow(((int)roomType) / 2, 2));
            roomRadiuses.Add(roomType, radius);

            return radius;
        }

        public static float GetDoubleRadius(this RoomType roomType)
        {
            return GetRadius(roomType) * 2;
        }
    }
}