using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// Bridges MonoBehaviour projectiles and targeting into the ECS enemy world (all enemies).
    /// </summary>
    public static class EcsEnemyHits
    {
        private static World _lastWorld;
        private static EntityQuery _query;
        private static DamageNumber _damageNumberPrefab;

        public static void SetDamageNumberPrefab(DamageNumber prefab)
        {
            _damageNumberPrefab = prefab;
        }

        private static void ShowDamageNumber(Vector3 position, float amount)
        {
            if (!_damageNumberPrefab)
                return;

            var clone = Object.Instantiate(_damageNumberPrefab);
            clone.transform.position = position;
            clone.Text = amount.ToString("0.#");
            clone.SetColor(Color.red);
        }

        /// <summary>Damages every alive melee enemy within range not already in <paramref name="alreadyHit"/> (if given). Returns true if at least one was hit.</summary>
        public static bool DamageInRange(Vector3 position, float range, float damage, HashSet<Unity.Entities.Entity> alreadyHit = null)
        {
            return DamageAndPushInRange(position, range, damage, pushDistance: 0f, alreadyHit);
        }

        /// <summary>Nearest alive melee enemy to <paramref name="from"/>, for spells that pick a target direction via ListOfObject&lt;Enemy&gt;.GetNearestTo - that list never contains melee-type enemies at all.</summary>
        public static bool TryGetNearestPosition(Vector3 from, out Vector3 position)
        {
            position = default;

            if (!TryGetEntityManager(out EntityManager entityManager))
                return false;

            using NativeArray<Unity.Entities.Entity> entities = _query.ToEntityArray(Allocator.Temp);
            float3 origin = from;
            var found = false;
            var bestDistanceSq = float.MaxValue;
            float3 bestPosition = default;

            foreach (Unity.Entities.Entity entity in entities)
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

        /// <summary>
        /// Smoothly repels alive melee ECS enemies as the field's collider radius expands,
        /// matching the continuous physical push of a SphereCollider. Applies damage once per spell instance.
        /// </summary>
        public static bool PushAndDamageExpandingField(
            Vector3 position,
            float currentRadius,
            float damage,
            HashSet<Unity.Entities.Entity> alreadyDamaged,
            float enemyRadius = 0.4f)
        {
            if (!TryGetEntityManager(out EntityManager entityManager))
                return false;

            using NativeArray<Unity.Entities.Entity> entities = _query.ToEntityArray(Allocator.Temp);
            float3 center = position;
            var hitAny = false;
            float effectiveRadius = currentRadius + enemyRadius;
            float effectiveRadiusSq = effectiveRadius * effectiveRadius;

            foreach (Unity.Entities.Entity entity in entities)
            {
                var health = entityManager.GetComponentData<Health>(entity);
                if (health.Value <= 0f)
                    continue;

                var transform = entityManager.GetComponentData<LocalTransform>(entity);
                float3 offset = transform.Position - center;
                float horizontalDistanceSq = offset.x * offset.x + offset.z * offset.z;

                if (horizontalDistanceSq > effectiveRadiusSq)
                    continue;

                if (alreadyDamaged != null && alreadyDamaged.Add(entity))
                {
                    health.Value -= damage;
                    entityManager.SetComponentData(entity, health);
                    hitAny = true;
                    ShowDamageNumber(transform.Position, damage);
                }

                if (health.Value <= 0f)
                    continue;

                float horizontalDistance = math.sqrt(horizontalDistanceSq);
                float2 pushDir;
                if (horizontalDistance > 0.0001f)
                {
                    pushDir = new float2(offset.x, offset.z) / horizontalDistance;
                }
                else
                {
                    pushDir = new float2(0f, 1f);
                }

                transform.Position = new float3(
                    center.x + pushDir.x * effectiveRadius,
                    transform.Position.y,
                    center.z + pushDir.y * effectiveRadius);

                entityManager.SetComponentData(entity, transform);
            }

            return hitAny;
        }

        public static bool DamageAndPushInRange(Vector3 position, float range, float damage, float pushDistance, HashSet<Unity.Entities.Entity> alreadyHit = null)
        {
            if (!TryGetEntityManager(out EntityManager entityManager))
                return false;

            using NativeArray<Unity.Entities.Entity> entities = _query.ToEntityArray(Allocator.Temp);
            float3 center = position;
            var hitAny = false;

            foreach (Unity.Entities.Entity entity in entities)
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
                ShowDamageNumber(transform.Position, damage);

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

            if (_lastWorld != world)
            {
                _query = entityManager.CreateEntityQuery(typeof(Health), typeof(LocalTransform), typeof(EnemyEcs.EnemyEcsType));
                _lastWorld = world;
            }

            return true;
        }
    }
}
