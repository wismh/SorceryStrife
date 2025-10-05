using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering;

namespace PickupEcs
{
    /// <summary>
    /// крок-10: builds one reusable ECS "prefab" entity for Experience pickups - Entities Graphics
    /// rendering set up entirely from code via RenderMeshUtility.AddComponents, mirroring крок-7's
    /// ProjectileEcsSpawner (no SubScene/Baker, pickups are always spawned at runtime). Reuses the
    /// old Experience.prefab's mesh/material so pickups look identical to before. The prefab entity
    /// is built lazily on first access rather than cached in Start() - see крок-8's EntityManager-
    /// Start()-ordering fix (EnemyEcsSpawner/ProjectileEcsSpawner) for why: EnemyEcsBridge needs the
    /// prefab Entity handle up front to push into EnemyDeathSystem, and Unity doesn't guarantee its
    /// Start() runs after this component's.
    /// </summary>
    public class PickupEcsSpawner : MonoBehaviour
    {
        [SerializeField] private Mesh _mesh;
        [SerializeField] private Material _material;

        private static EntityManager EntityManager => World.DefaultGameObjectInjectionWorld.EntityManager;

        private Entity _prefabEntity;
        private bool _prefabCreated;

        public void SpawnPickup(Vector3 position)
        {
            EntityManager entityManager = EntityManager;
            Entity instance = entityManager.Instantiate(GetOrCreatePrefabEntity());

            entityManager.SetComponentData(instance, new LocalTransform
            {
                Position = position,
                Rotation = quaternion.identity,
                Scale = 0.2f,
            });
        }

        public Entity GetOrCreatePrefabEntity()
        {
            if (_prefabCreated)
                return _prefabEntity;

            _prefabEntity = CreatePrefabEntity();
            _prefabCreated = true;
            return _prefabEntity;
        }

        private Entity CreatePrefabEntity()
        {
            EntityManager entityManager = EntityManager;
            Entity entity = entityManager.CreateEntity(
                typeof(Prefab),
                typeof(LocalTransform),
                typeof(Pickup));

            entityManager.SetComponentData(entity, LocalTransform.FromScale(0.2f));

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
