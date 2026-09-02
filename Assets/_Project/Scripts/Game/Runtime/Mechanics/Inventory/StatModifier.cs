using System;
using System.Collections.Generic;

namespace Game
{
    [Serializable]
    public struct StatModifier
    {
        public StatType Stat;
        public ModifierOp Op;
        public List<float> ValuePerLevel;
    }
}
