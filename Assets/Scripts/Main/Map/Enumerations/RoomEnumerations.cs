using UnityEngine;

namespace RandomMap.Enumerations
{
    public enum RoomType
    {
        Unit,
        R1x1,
        R2x2,
        R3x3,
        R4x4,
        R5x5,
        VectorOnly
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