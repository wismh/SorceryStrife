using Unity.Entities;
using Unity.Mathematics;

namespace Game
{
    public struct ProjectileVelocity : IComponentData
    {
        public float3 Value;
    }
}
