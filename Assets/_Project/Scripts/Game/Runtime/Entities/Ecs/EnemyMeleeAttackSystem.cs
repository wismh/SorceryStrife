using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

// No "using Game;" in this file - see ProjectileVelocity.cs for why. Game-namespace types
// (EntityDamagable) written fully qualified.
namespace EnemyEcs
{
    /// <summary>
    /// Ports EnemyMeleeFight's Update()/AttackAsync() to ECS: no separate cooldown, just a
    /// range-gated wind-up (BaseAttackDuration / AttackSpeed) during which the entity can't move
    /// (see EnemyMovementSystem), animation trigger at wind-up *start*, damage at wind-up *end*
    /// with no re-check of range/player-alive (matches the original exactly).
    /// </summary>
    [UpdateAfter(typeof(EnemyMovementSystem))]
    public partial class EnemyMeleeAttackSystem : SystemBase
    {
        private Game.EntityDamagable _playerDamagable;
        private EnemyCompanionAssignmentSystem _companionSystem;

        public void SetDependencies(Game.EntityDamagable playerDamagable, EnemyCompanionAssignmentSystem companionSystem)
        {
            _playerDamagable = playerDamagable;
            _companionSystem = companionSystem;
        }

        protected override void OnUpdate()
        {
            if (_playerDamagable == null || !SystemAPI.TryGetSingleton(out PlayerPositionSingleton player))
                return;

            var attackStarts = new NativeQueue<AttackStartEvent>(Allocator.TempJob);
            var damageEvents = new NativeQueue<float>(Allocator.TempJob);

            new MeleeAttackJob
            {
                DeltaTime = SystemAPI.Time.DeltaTime,
                PlayerPosition = player.Position,
                BaseAttackDuration = Game.Entity.BaseAttackDuration,
                AttackStarts = attackStarts.AsParallelWriter(),
                DamageEvents = damageEvents.AsParallelWriter(),
            }.ScheduleParallel(Dependency).Complete();

            while (attackStarts.TryDequeue(out AttackStartEvent start))
            {
                if (_companionSystem != null && _companionSystem.TryGetCompanion(start.Entity, out Game.EnemyCompanion companion))
                    companion.PlayAttack(start.Duration);
            }

            while (damageEvents.TryDequeue(out float damage))
                _playerDamagable.Damage(damage);

            attackStarts.Dispose();
            damageEvents.Dispose();
        }
    }

    internal struct AttackStartEvent
    {
        public Entity Entity;
        public float Duration;
    }

    [BurstCompile]
    internal partial struct MeleeAttackJob : IJobEntity
    {
        public float DeltaTime;
        public float3 PlayerPosition;
        public float BaseAttackDuration;
        public NativeQueue<AttackStartEvent>.ParallelWriter AttackStarts;
        public NativeQueue<float>.ParallelWriter DamageEvents;

        private void Execute(Entity entity, in LocalTransform transform, in Game.AttackStats attackStats, in Game.Health health, ref AttackState state)
        {
            if (health.Value <= 0f)
                return;

            if (state.Phase == AttackPhase.Idle)
            {
                float distance = math.distance(transform.Position, PlayerPosition);
                if (distance >= attackStats.RangeOfAttack)
                    return;

                float duration = BaseAttackDuration / attackStats.AttackSpeed;
                state.Phase = AttackPhase.WindingUp;
                state.Timer = duration;
                AttackStarts.Enqueue(new AttackStartEvent { Entity = entity, Duration = duration });
                return;
            }

            state.Timer -= DeltaTime;
            if (state.Timer > 0f)
                return;

            state.Phase = AttackPhase.Idle;
            DamageEvents.Enqueue(attackStats.Attack);
        }
    }
}
