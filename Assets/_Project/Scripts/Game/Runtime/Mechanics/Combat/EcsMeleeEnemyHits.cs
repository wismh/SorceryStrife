using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Entity = Unity.Entities.Entity;

namespace Game
{
    /// <summary>
    /// Bridges MonoBehaviour code into the melee-type ECS enemy world (Minion/Mutant/Ogr/OldMutant) -
    /// they're the only enemies with no Collider/EntityDamagable at all (EnemyCompanion is a
    /// visual-only proxy, see EnemyCompanion.cs), so anything that used to reach them through
    /// physics callbacks or ListOfObject&lt;Enemy&gt; (which only ever holds the still-MonoBehaviour
    /// Devil/HotDevil/Eye/BigEye) needs a direct EntityManager query instead: damaging/pushing them
    /// from a projectile's own hit logic, or finding the nearest one to aim at.
    /// </summary>
    public static class EcsMeleeEnemyHits
    {
        private static EntityQuery _query;
        private static bool _queryReady;

        /// <summary>Damages every alive melee enemy within range not already in <paramref name="alreadyHit"/> (if given). Returns true if at least one was hit.</summary>
        public static bool DamageInRange(Vector3 position, float range, float damage, HashSet<Entity> alreadyHit = null)
        {
            return DamageAndPushInRange(position, range, damage, pushDistance: 0f, alreadyHit);
        }

        /// <summary>Nearest alive melee enemy to <paramref name="from"/>, for spells that pick a target direction via ListOfObject&lt;Enemy&gt;.GetNearestTo - that list never contains melee-type enemies at all.</summary>
        public static bool TryGetNearestPosition(Vector3 from, out Vector3 position)
        {
            position = default;

            if (!TryGetEntityManager(out EntityManager entityManager))
                return false;

            using NativeArray<Entity> entities = _query.ToEntityArray(Allocator.Temp);
            float3 origin = from;
            var found = false;
            var bestDistanceSq = float.MaxValue;
            float3 bestPosition = default;

            foreach (Entity entity in entities)
            {
                var health = entityManager.GetComponentData<Health>(entity);
                if (health.Value <= 0f)
                    continue;

                var transform = entityManager.GetComponentData<LocalTransform>(entity);
                float distanceSq = math.distancesq(transform.Position, origin);
                if (distanceSq >= bestDistanceSq)
                    continue;

                bestDistanceSq = distanceSq;
                bestPosition = transform.Position;
                found = true;
            }

            position = bestPosition;
            return found;
        }

        public static bool DamageAndPushInRange(Vector3 position, float range, float damage, float pushDistance, HashSet<Entity> alreadyHit = null)
        {
            if (!TryGetEntityManager(out EntityManager entityManager))
                return false;

            using NativeArray<Entity> entities = _query.ToEntityArray(Allocator.Temp);
            float3 center = position;
            var hitAny = false;

            foreach (Entity entity in entities)
            {
                if (alreadyHit != null && alreadyHit.Contains(entity))
                    continue;

                var health = entityManager.GetComponentData<Health>(entity);
                if (health.Value <= 0f)
                    continue;

                var transform = entityManager.GetComponentData<LocalTransform>(entity);
                float3 offset = transform.Position - center;
                float distance = math.length(offset);
                if (distance > range)
                    continue;

                health.Value -= damage;
                entityManager.SetComponentData(entity, health);

                if (pushDistance > 0f && distance > 0.0001f)
                {
                    transform.Position += (offset / distance) * pushDistance;
                    entityManager.SetComponentData(entity, transform);
                }

                alreadyHit?.Add(entity);
                hitAny = true;
            }

            return hitAny;
        }

        private static bool TryGetEntityManager(out EntityManager entityManager)
        {
            World world = World.DefaultGameObjectInjectionWorld;
            if (world == null || !world.IsCreated)
            {
                entityManager = default;
                return false;
            }

            entityManager = world.EntityManager;

            if (!_queryReady)
            {
                _query = entityManager.CreateEntityQuery(typeof(Health), typeof(LocalTransform), typeof(EnemyEcs.EnemyEcsType));
                _queryReady = true;
            }

            return true;
        }
    }
}
