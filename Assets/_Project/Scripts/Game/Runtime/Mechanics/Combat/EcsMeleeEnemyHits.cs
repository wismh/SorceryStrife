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
    /// Lets MonoBehaviour projectiles damage (and optionally push) melee-type ECS enemies directly.
    /// EnemyCompanion - the only GameObject a melee enemy has - is a visual-only proxy with no
    /// Collider (see EnemyCompanion.cs), so OnCollisionEnter/OnTriggerEnter never fires for them.
    /// This is a point-in-time EntityManager query mirroring what a real Collider hit would have
    /// done, called from each projectile's own hit logic rather than a per-frame system.
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
