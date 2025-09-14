using Unity.Entities;

namespace EnemyEcs
{
    public enum AttackPhase
    {
        Idle,
        WindingUp
    }

    /// <summary>Mirrors EnemyMeleeFight's _attacking bool + wind-up wait, minus the cooldown timer it never had either.</summary>
    public struct AttackState : IComponentData
    {
        public AttackPhase Phase;
        public float Timer;
    }
}
