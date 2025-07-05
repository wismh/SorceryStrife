using Unity.Entities;

namespace ProjectileEcs
{
    public struct ProjectileLifetime : IComponentData
    {
        public float Remaining;
    }
}
