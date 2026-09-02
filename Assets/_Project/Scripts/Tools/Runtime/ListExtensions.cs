using System.Collections.Generic;

namespace Game
{
    public static class ListExtensions
    {
        /// <summary>
        /// Per-level value with the last entry clamped for any level beyond the list.
        /// </summary>
        public static float ValueAtLevel(this List<float> values, int level)
        {
            return level >= values.Count ? values[^1] : values[level];
        }
    }
}
