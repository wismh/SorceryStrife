using Unity.Entities;

namespace ProjectileEcs
{
    /// <summary>Tag: this projectile isn't destroyed on its first hit - see EnemyMeleeHitDetectionSystem/EnemyHitDetectionSystem.</summary>
    public struct Piercing : IComponentData
    {
    }
}
