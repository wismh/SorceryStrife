using Unity.Entities;
using Unity.Mathematics;

namespace EnemyEcs
{
    /// <summary>Pushed every frame by PlayerEcsBridge - the one piece of MonoBehaviour world state ECS enemy systems need to read.</summary>
    public struct PlayerPositionSingleton : IComponentData
    {
        public float3 Position;
        public bool IsAlive;
    }
}
