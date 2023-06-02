using UnityEngine;

namespace RandomMap.Enumerations
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

    public enum Directions
    {
        NONE,
        UP,
        RIGHT,
        DOWN,
        LEFT
    }

    static class DirectionsMethods
    {
        public static Vector3 GetVector(this Directions dir)
        {
            switch (dir)
            {
                case Directions.UP:
                    return Vector3.up;

                case Directions.RIGHT:
                    return Vector3.right;

                case Directions.DOWN:
                    return Vector3.down;

                case Directions.LEFT:
                    return Vector3.left;

                default:
                    return Vector3.zero;
            }
        }
    }
}