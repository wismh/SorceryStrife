using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

// No "using Game;" in this file on purpose - see ProjectileVelocity.cs for why. Game-namespace
// types (Enemy, EntityDamagable, Team) are always written fully qualified below so Game.Entity
// never enters scope, even transitively through the IJobEntity source generator's companion file.
namespace ProjectileEcs
{
    /// <summary>
    /// Bridges Zenject-managed MonoBehaviour enemies into ECS: builds a per-frame position/radius
    /// snapshot of alive enemies (main thread - reads managed Enemy/Collider/Entity.IsAlive), runs
    /// the actual overlap math in a Burst job, then applies damage through the same
    /// EntityDamagable.Damage(float) every existing MonoBehaviour projectile already calls.
    /// Enemy targets are pushed in once by EcsWorldBridge - see that file for the DI seam.
    /// </summary>
    [UpdateAfter(typeof(ProjectileMovementSystem))]
    public partial class EnemyHitDetectionSystem : SystemBase
    {
        private List<Game.Enemy> _enemyTargets = new();

        public void SetEnemyTargets(List<Game.Enemy> enemyTargets)
        {
            _enemyTargets = enemyTargets;
        }

        protected override void OnUpdate()
        {
            if (_enemyTargets.Count == 0)
                return;

            var positions = new NativeList<float3>(_enemyTargets.Count, Allocator.TempJob);
            var radii = new NativeList<float>(_enemyTargets.Count, Allocator.TempJob);
            var liveEnemies = new List<Game.Enemy>(_enemyTargets.Count);

            foreach (Game.Enemy enemy in _enemyTargets)
            {
                if (!enemy.TryGetComponent(out Game.Entity entityComponent) || !entityComponent.IsAlive)
                    continue;

                float radius = enemy.TryGetComponent(out Collider collider) ? collider.bounds.extents.x : 0.5f;
                positions.Add(enemy.transform.position);
                radii.Add(radius);
                liveEnemies.Add(enemy);
            }

            var hits = new NativeQueue<HitResult>(Allocator.TempJob);

            new DetectHitsJob
            {
                EnemyPositions = positions.AsArray(),
                EnemyRadii = radii.AsArray(),
                Hits = hits.AsParallelWriter(),
            }.ScheduleParallel(Dependency).Complete();

            while (hits.TryDequeue(out HitResult hit))
            {
                Game.Enemy target = liveEnemies[hit.EnemyIndex];
                if (target.TryGetComponent(out Game.EntityDamagable damagable))
                    damagable.Damage(hit.Damage);

                if (EntityManager.Exists(hit.Projectile))
                    EntityManager.DestroyEntity(hit.Projectile);
            }

            positions.Dispose();
            radii.Dispose();
            hits.Dispose();
        }
    }

    internal struct HitResult
    {
        public Entity Projectile;
        public int EnemyIndex;
        public float Damage;
    }

    [BurstCompile]
    internal partial struct DetectHitsJob : IJobEntity
    {
        [ReadOnly] public NativeArray<float3> EnemyPositions;
        [ReadOnly] public NativeArray<float> EnemyRadii;
        public NativeQueue<HitResult>.ParallelWriter Hits;

        private void Execute(Entity entity, in LocalTransform transform, in ProjectileDamage damage, in Game.UnitTeam team)
        {
            if (team.Value != Game.Team.Ally)
                return;

            for (int i = 0; i < EnemyPositions.Length; i++)
            {
                if (math.distance(transform.Position, EnemyPositions[i]) <= EnemyRadii[i])
                {
                    Hits.Enqueue(new HitResult { Projectile = entity, EnemyIndex = i, Damage = damage.Value });
                    return;
                }
            }
        }
    }
}
