using Game;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering;

namespace ProjectileEcs
{
    /// <summary>
    /// Builds one reusable ECS "prefab" entity (Entities Graphics rendering set up entirely from
    /// code via RenderMeshUtility.AddComponents - no SubScene/Baker needed for a runtime-only
    /// spawn path) and instantiates it on demand. Not called by any real Caster yet - see
    /// ProjectileEcsSmokeTest for the throwaway verification harness.
    /// </summary>
    public class ProjectileEcsSpawner : MonoBehaviour
    {
        [SerializeField] private Mesh _mesh;
        [SerializeField] private Material _material;

        // Fetched fresh rather than cached in Start() - a caller on a different GameObject can
        // invoke SpawnProjectile() from its own Start() before this component's Start() has run,
        // since Unity doesn't guarantee cross-GameObject Start() ordering.
        private static EntityManager EntityManager => World.DefaultGameObjectInjectionWorld.EntityManager;

        private Unity.Entities.Entity _prefabEntity;
        private bool _prefabCreated;

        public Unity.Entities.Entity SpawnProjectile(float3 position, float3 velocity, float damage, Team team, float lifetime)
        {
            EnsurePrefabEntity();

            EntityManager entityManager = EntityManager;
            Unity.Entities.Entity instance = entityManager.Instantiate(_prefabEntity);

            entityManager.SetComponentData(instance, new LocalTransform
            {
                Position = position,
                Rotation = quaternion.identity,
                Scale = 1f,
            });
            entityManager.SetComponentData(instance, new ProjectileVelocity { Value = velocity });
            entityManager.SetComponentData(instance, new ProjectileDamage { Value = damage });
            entityManager.SetComponentData(instance, new ProjectileLifetime { Remaining = lifetime });
            entityManager.SetComponentData(instance, new UnitTeam { Value = team });

            return instance;
        }

        private void EnsurePrefabEntity()
        {
            if (_prefabCreated)
                return;

            _prefabEntity = CreatePrefabEntity();
            _prefabCreated = true;
        }

        private Unity.Entities.Entity CreatePrefabEntity()
        {
            EntityManager entityManager = EntityManager;
            Unity.Entities.Entity entity = entityManager.CreateEntity(
                typeof(Prefab),
                typeof(LocalTransform),
                typeof(ProjectileVelocity),
                typeof(ProjectileDamage),
                typeof(ProjectileLifetime),
                typeof(UnitTeam));

            entityManager.SetComponentData(entity, LocalTransform.Identity);

            var renderMeshArray = new RenderMeshArray(new[] { _material }, new[] { _mesh });
            var renderMeshDescription = new RenderMeshDescription(ShadowCastingMode.Off);
            RenderMeshUtility.AddComponents(
                entity,
                entityManager,
                renderMeshDescription,
                renderMeshArray,
                MaterialMeshInfo.FromRenderMeshArrayIndices(0, 0));

            return entity;
        }
    }
}
