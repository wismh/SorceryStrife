namespace Project.Core.DamagePopupModule
{
    public readonly struct DamagePopupInfo
    {
        public DamagePopupInfo(float amount, bool isCrit = false)
        {
            Amount = amount;
            IsCrit = isCrit;
        }

        public float Amount { get; }
        public bool IsCrit { get; }
    }
}
