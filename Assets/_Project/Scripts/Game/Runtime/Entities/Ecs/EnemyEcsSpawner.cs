using Game;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace EnemyEcs
{
    /// <summary>
    /// Spawns a melee enemy as a pure ECS entity - no GameObject, no baking/SubScene, stats read
    /// directly off the EntityStatsAuthoring component on the (never-instantiated) source prefab.
    /// Called from EnemySpawner's fork for the 4 melee types; Devil/HotDevil/Eye/BigEye keep
    /// going through the existing MonoBehaviour Instantiate path untouched.
    /// </summary>
    public class EnemyEcsSpawner : MonoBehaviour
    {
        private EntityManager _entityManager;

        private void Start()
        {
            _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        }

        public void Spawn(EntityStatsAuthoring stats, Vector3 position)
        {
            EntityCharacteristics characteristics = stats.Characteristics;

            var entity = _entityManager.CreateEntity(
                typeof(LocalTransform),
                typeof(MoveSpeed),
                typeof(AttackStats),
                typeof(Health),
                typeof(UnitTeam),
                typeof(AttackState),
                typeof(EnemyEcsType));

            _entityManager.SetComponentData(entity, new LocalTransform
            {
                Position = position,
                Rotation = quaternion.identity,
                Scale = 1f,
            });
            _entityManager.SetComponentData(entity, new MoveSpeed { Value = characteristics.MoveSpeed });
            _entityManager.SetComponentData(entity, new AttackStats
            {
                Attack = characteristics.Attack,
                RangeOfAttack = characteristics.RangeOfAttack,
                AttackSpeed = characteristics.AttackSpeed,
            });
            _entityManager.SetComponentData(entity, new Health { Value = characteristics.MaxHealth, Max = characteristics.MaxHealth });
            _entityManager.SetComponentData(entity, new UnitTeam { Value = stats.Team });
            _entityManager.SetComponentData(entity, new AttackState { Phase = AttackPhase.Idle, Timer = 0f });
            _entityManager.SetComponentData(entity, new EnemyEcsType { Value = stats.EnemyType });
        }
    }
}
