using Unity.Entities;

namespace Game
{
    public struct ProjectileLifetime : IComponentData
    {
        public float Remaining;
    }
}
