using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering;

namespace Game
{
    using Entity = Unity.Entities.Entity;

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

        private EntityManager _entityManager;
        private Entity _prefabEntity;

        private void Start()
        {
            _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            _prefabEntity = CreatePrefabEntity();
        }

        public Entity SpawnProjectile(float3 position, float3 velocity, float damage, Team team, float lifetime)
        {
            Entity instance = _entityManager.Instantiate(_prefabEntity);

            _entityManager.SetComponentData(instance, new LocalTransform
            {
                Position = position,
                Rotation = quaternion.identity,
                Scale = 1f,
            });
            _entityManager.SetComponentData(instance, new ProjectileVelocity { Value = velocity });
            _entityManager.SetComponentData(instance, new ProjectileDamage { Value = damage });
            _entityManager.SetComponentData(instance, new ProjectileLifetime { Remaining = lifetime });
            _entityManager.SetComponentData(instance, new UnitTeam { Value = team });

            return instance;
        }

        private Entity CreatePrefabEntity()
        {
            Entity entity = _entityManager.CreateEntity(
                typeof(Prefab),
                typeof(LocalTransform),
                typeof(ProjectileVelocity),
                typeof(ProjectileDamage),
                typeof(ProjectileLifetime),
                typeof(UnitTeam));

            _entityManager.SetComponentData(entity, LocalTransform.Identity);

            var renderMeshArray = new RenderMeshArray(new[] { _material }, new[] { _mesh });
            var renderMeshDescription = new RenderMeshDescription(ShadowCastingMode.Off);
            RenderMeshUtility.AddComponents(
                entity,
                _entityManager,
                renderMeshDescription,
                renderMeshArray,
                MaterialMeshInfo.FromRenderMeshArrayIndices(0, 0));

            return entity;
        }
    }
}
