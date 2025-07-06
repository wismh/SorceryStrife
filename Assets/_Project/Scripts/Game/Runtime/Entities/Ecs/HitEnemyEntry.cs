using Unity.Entities;

namespace ProjectileEcs
{
    /// <summary>Per-projectile dedup buffer: enemy entities already damaged, so a Piercing projectile lingering near one doesn't restack damage every frame.</summary>
    public struct HitEnemyEntry : IBufferElementData
    {
        public Entity Value;
    }
}
