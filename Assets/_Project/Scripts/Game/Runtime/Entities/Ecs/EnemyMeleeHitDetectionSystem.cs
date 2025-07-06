using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

// No "using Game;" in this file - see ProjectileVelocity.cs for why. Game-namespace types
// (Health, UnitTeam, Team) written fully qualified.
namespace EnemyEcs
{
    /// <summary>
    /// Pure ECS-vs-ECS sibling to крок-7's EnemyHitDetectionSystem, for the 4 melee types only -
    /// no MonoBehaviour snapshot needed on the enemy side anymore, damage is a direct
    /// Health.Value decrement. Devil/HotDevil/Eye/BigEye still go through крок-7's original
    /// MonoBehaviour-bridging system, untouched. Every hit is deduped through the projectile's own
    /// HitEnemyEntry buffer (so a lingering Piercing projectile can't restack damage on the same
    /// enemy every frame it stays in range) and only destroys the projectile when it isn't Piercing.
    /// </summary>
    [UpdateAfter(typeof(EnemyMovementSystem))]
    [UpdateAfter(typeof(ProjectileEcs.ProjectileMovementSystem))]
    public partial struct EnemyMeleeHitDetectionSystem : ISystem
    {
        private const float EnemyHitRadius = 0.6f;

        public void OnUpdate(ref SystemState state)
        {
            var enemyQuery = SystemAPI.QueryBuilder()
                .WithAll<Game.Health, Game.UnitTeam, LocalTransform, EnemyEcsType>()
                .Build();

            int enemyCount = enemyQuery.CalculateEntityCount();
            if (enemyCount == 0)
                return;

            var enemyEntities = enemyQuery.ToEntityArray(Allocator.TempJob);
            var enemyTransforms = enemyQuery.ToComponentDataArray<LocalTransform>(Allocator.TempJob);
            var enemyPositions = new NativeArray<float3>(enemyCount, Allocator.TempJob);
            for (int i = 0; i < enemyCount; i++)
                enemyPositions[i] = enemyTransforms[i].Position;

            var hits = new NativeQueue<MeleeHitResult>(Allocator.TempJob);

            new DetectMeleeHitsJob
            {
                EnemyPositions = enemyPositions,
                HitRadius = EnemyHitRadius,
                Hits = hits.AsParallelWriter(),
            }.ScheduleParallel(state.Dependency).Complete();

            var ecb = new EntityCommandBuffer(Allocator.TempJob);

            while (hits.TryDequeue(out MeleeHitResult hit))
            {
                if (!state.EntityManager.Exists(hit.Projectile))
                    continue;

                Entity enemyEntity = enemyEntities[hit.EnemyIndex];
                if (state.EntityManager.Exists(enemyEntity))
                {
                    DynamicBuffer<ProjectileEcs.HitEnemyEntry> hitHistory = state.EntityManager.GetBuffer<ProjectileEcs.HitEnemyEntry>(hit.Projectile);
                    var alreadyHit = false;
                    for (var i = 0; i < hitHistory.Length; i++)
                    {
                        if (hitHistory[i].Value != enemyEntity)
                            continue;
                        alreadyHit = true;
                        break;
                    }

                    if (!alreadyHit)
                    {
                        hitHistory.Add(new ProjectileEcs.HitEnemyEntry { Value = enemyEntity });

                        Game.Health health = state.EntityManager.GetComponentData<Game.Health>(enemyEntity);
                        health.Value -= hit.Damage;
                        state.EntityManager.SetComponentData(enemyEntity, health);
                    }
                }

                if (!state.EntityManager.HasComponent<ProjectileEcs.Piercing>(hit.Projectile))
                    ecb.DestroyEntity(hit.Projectile);
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();

            enemyEntities.Dispose();
            enemyTransforms.Dispose();
            enemyPositions.Dispose();
            hits.Dispose();
        }
    }

    internal struct MeleeHitResult
    {
        public Entity Projectile;
        public int EnemyIndex;
        public float Damage;
    }

    [BurstCompile]
    internal partial struct DetectMeleeHitsJob : IJobEntity
    {
        [ReadOnly] public NativeArray<float3> EnemyPositions;
        public float HitRadius;
        public NativeQueue<MeleeHitResult>.ParallelWriter Hits;

        private void Execute(Entity entity, in LocalTransform transform, in ProjectileEcs.ProjectileDamage damage, in Game.UnitTeam team)
        {
            if (team.Value != Game.Team.Ally)
                return;

            for (int i = 0; i < EnemyPositions.Length; i++)
            {
                if (math.distance(transform.Position, EnemyPositions[i]) <= HitRadius)
                {
                    Hits.Enqueue(new MeleeHitResult { Projectile = entity, EnemyIndex = i, Damage = damage.Value });
                    return;
                }
            }
        }
    }
}
