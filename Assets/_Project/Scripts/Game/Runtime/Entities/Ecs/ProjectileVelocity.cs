using Unity.Entities;
using Unity.Mathematics;

// Deliberately NOT namespace Game (or a dot-nested Game.* sub-namespace) - Game.Entity (the
// MonoBehaviour) and Unity.Entities.Entity would both be in scope for any IJobEntity struct here,
// and Entities' source generator emits its companion code into the same namespace, picking up
// Game.Entity by mistake (see ProjectileMovementSystem.cs/EnemyHitDetectionSystem.cs for the
// IJobEntity structs this actually bit). Cross-reference Game-namespace types explicitly (Game.X).
namespace ProjectileEcs
{
    public struct ProjectileVelocity : IComponentData
    {
        public float3 Value;
    }
}
