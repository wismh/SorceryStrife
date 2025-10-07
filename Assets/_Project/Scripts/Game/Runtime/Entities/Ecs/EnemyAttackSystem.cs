using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Zenject;

namespace EnemyEcs
{
    public struct RangedAttackEvent
    {
        public float3 Position;
        public float3 Direction;
        public float Damage;
    }

    [UpdateAfter(typeof(EnemyMovementSystem))]
    public partial class EnemyAttackSystem : SystemBase
    {
        private Game.EntityDamagable _playerDamagable;
        private EnemyCompanionAssignmentSystem _companionSystem;
        private Game.DevilProjectile _devilProjectilePrefab;
        private DiContainer _container;

        public void SetDependencies(
            Game.EntityDamagable playerDamagable,
            EnemyCompanionAssignmentSystem companionSystem,
            Game.DevilProjectile devilProjectilePrefab,
            DiContainer container)
        {
            _playerDamagable = playerDamagable;
            _companionSystem = companionSystem;
            _devilProjectilePrefab = devilProjectilePrefab;
            _container = container;
        }

        protected override void OnUpdate()
        {
            if (_playerDamagable == null || !SystemAPI.TryGetSingleton(out PlayerPositionSingleton player))
                return;

            var attackStarts = new NativeQueue<AttackStartEvent>(Allocator.TempJob);
            var meleeDamageEvents = new NativeQueue<float>(Allocator.TempJob);
            var rangedAttackEvents = new NativeQueue<RangedAttackEvent>(Allocator.TempJob);

            new AttackJob
            {
                DeltaTime = SystemAPI.Time.DeltaTime,
                PlayerPosition = player.Position,
                BaseAttackDuration = Game.Entity.BaseAttackDuration,
                AttackStarts = attackStarts.AsParallelWriter(),
                MeleeDamageEvents = meleeDamageEvents.AsParallelWriter(),
                RangedAttackEvents = rangedAttackEvents.AsParallelWriter(),
            }.ScheduleParallel(Dependency).Complete();

            while (attackStarts.TryDequeue(out AttackStartEvent start))
            {
                if (_companionSystem != null && _companionSystem.TryGetCompanion(start.Entity, out Game.EnemyCompanion companion))
                    companion.PlayAttack(start.Duration);
            }

            while (meleeDamageEvents.TryDequeue(out float damage))
                _playerDamagable.Damage(damage);

            while (rangedAttackEvents.TryDequeue(out RangedAttackEvent ranged))
            {
                if (_devilProjectilePrefab != null && _container != null)
                {
                    var clone = _container.InstantiatePrefabForComponent<Game.DevilProjectile>(_devilProjectilePrefab);
                    clone.transform.position = ranged.Position;
                    clone.Construct(ranged.Damage, ranged.Direction);
                }
            }

            attackStarts.Dispose();
            meleeDamageEvents.Dispose();
            rangedAttackEvents.Dispose();
        }
    }

    internal struct AttackStartEvent
    {
        public Entity Entity;
        public float Duration;
    }

    [BurstCompile]
    internal partial struct AttackJob : IJobEntity
    {
        public float DeltaTime;
        public float3 PlayerPosition;
        public float BaseAttackDuration;
        public NativeQueue<AttackStartEvent>.ParallelWriter AttackStarts;
        public NativeQueue<float>.ParallelWriter MeleeDamageEvents;
        public NativeQueue<RangedAttackEvent>.ParallelWriter RangedAttackEvents;

        private void Execute(
            Entity entity,
            in LocalTransform transform,
            in Game.AttackStats attackStats,
            in Game.Health health,
            in EnemyAttackType attackType,
            ref AttackState state)
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

            if (attackType.Value == AttackType.Melee)
            {
                MeleeDamageEvents.Enqueue(attackStats.Attack);
            }
            else
            {
                float3 offset = PlayerPosition - transform.Position;
                float distSq = math.lengthsq(offset);
                float3 dir = distSq > 0.0001f ? math.normalize(offset) : new float3(0f, 0f, 1f);
                RangedAttackEvents.Enqueue(new RangedAttackEvent
                {
                    Position = transform.Position,
                    Direction = dir,
                    Damage = attackStats.Attack,
                });
            }
        }
    }
}
