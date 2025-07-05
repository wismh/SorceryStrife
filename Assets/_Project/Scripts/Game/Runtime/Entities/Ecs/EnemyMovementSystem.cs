using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

// No "using Game;" in this file - see ProjectileVelocity.cs for why (IJobEntity source generator
// picks up Game.Entity by mistake if it's in scope). Game-namespace types written fully qualified.
namespace EnemyEcs
{
    /// <summary>Straight-line steering toward the player, matching EnemyMoveController.FixedUpdate() exactly (flee if player dead).</summary>
    public partial struct EnemyMovementSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            if (!SystemAPI.TryGetSingleton(out PlayerPositionSingleton player))
                return;

            new SteerTowardPlayerJob
            {
                DeltaTime = SystemAPI.Time.DeltaTime,
                PlayerPosition = player.Position,
                PlayerAlive = player.IsAlive,
            }.ScheduleParallel(state.Dependency).Complete();
        }
    }

    [BurstCompile]
    internal partial struct SteerTowardPlayerJob : IJobEntity
    {
        public float DeltaTime;
        public float3 PlayerPosition;
        public bool PlayerAlive;

        private void Execute(ref LocalTransform transform, in Game.MoveSpeed moveSpeed, in Game.Health health, in AttackState attackState)
        {
            if (health.Value <= 0f || attackState.Phase == AttackPhase.WindingUp)
                return;

            float3 offset = PlayerPosition - transform.Position;
            float distanceSq = math.lengthsq(offset);
            if (distanceSq < 0.0001f)
                return;

            float3 direction = offset * math.rsqrt(distanceSq);
            if (!PlayerAlive)
                direction = -direction;

            transform.Position += direction * moveSpeed.Value * DeltaTime;
            transform.Rotation = quaternion.LookRotationSafe(direction, math.up());
        }
    }
}
