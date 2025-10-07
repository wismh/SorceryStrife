using Unity.Entities;

namespace EnemyEcs
{
    public enum AttackType : byte
    {
        Melee,
        Ranged
    }

    public struct EnemyAttackType : IComponentData
    {
        public AttackType Value;
    }
}
