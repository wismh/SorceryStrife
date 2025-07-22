using Unity.Entities;

namespace Game
{
    public struct AttackStats : IComponentData
    {
        public float Attack;
        public float RangeOfAttack;
        public float AttackSpeed;
    }
}
