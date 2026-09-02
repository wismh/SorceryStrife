using System.Collections.Generic;

namespace Game
{
    /// <summary>One labeled per-level stat row for the upgrade card UI.</summary>
    public readonly struct SpellStatDisplay
    {
        public readonly string Name;
        public readonly List<float> ValuePerLevel;

        public SpellStatDisplay(string name, List<float> valuePerLevel)
        {
            Name = name;
            ValuePerLevel = valuePerLevel;
        }
    }
}
