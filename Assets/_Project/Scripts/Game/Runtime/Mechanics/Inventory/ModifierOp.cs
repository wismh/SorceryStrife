namespace Game
{
    public enum ModifierOp
    {
        /// <summary>Base value multiplied by (1 + sum of per-level values).</summary>
        AdditivePercent,

        /// <summary>Sum of per-level values added to the base value.</summary>
        Flat,
    }
}
