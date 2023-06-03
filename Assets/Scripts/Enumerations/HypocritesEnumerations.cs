using UnityEngine;

namespace Hypocrites.Enumerations
{
    public enum Directions
    {
        NONE    = 0,
        UP      = 1 << 0,
        RIGHT   = 1 << 1,
        DOWN    = 1 << 2,
        LEFT    = 1 << 3,
    }

    static class DirectionsMethods
    {
        public static bool Contains(this Directions dir, Directions target)
        {
            return (dir & target) == target;
        }

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
