using Unity.Entities;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// Bakes the same stats as Entity/EntityCharacteristics (Attack, RangeOfAttack,
    /// AttackSpeed, MoveSpeed, MaxHealth, RangeOfPickUp) plus Team into ECS components. The Baker
    /// itself stays unused (крок-7/9 skip SubScene/baking entirely for runtime-only spawns) - what
    /// крок-9 actually uses is the Characteristics/Team/EnemyType getters below, read directly off
    /// the prefab asset by WaveSpawnSystem.
    /// </summary>
    public class EntityStatsAuthoring : MonoBehaviour
    {
        [SerializeField] private EntityCharacteristics _characteristics;
        [SerializeField] private Team _team;
        [SerializeField] private EnemyType _enemyType;

        /// <summary>Read directly off the prefab asset at spawn time (крок-7/9 skip baking/SubScene entirely) - see WaveSpawnSystem.</summary>
        public EntityCharacteristics Characteristics => _characteristics;
        public Team Team => _team;
        public EnemyType EnemyType => _enemyType;

        private class Baker : Baker<EntityStatsAuthoring>
        {
            public override void Bake(EntityStatsAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                var characteristics = authoring._characteristics;

                AddComponent(entity, new MoveSpeed { Value = characteristics.MoveSpeed });
                AddComponent(entity, new AttackStats
                {
                    Attack = characteristics.Attack,
                    RangeOfAttack = characteristics.RangeOfAttack,
                    AttackSpeed = characteristics.AttackSpeed,
                });
                AddComponent(entity, new Health { Value = characteristics.MaxHealth, Max = characteristics.MaxHealth });
                AddComponent(entity, new PickupRadius { Value = characteristics.RangeOfPickUp });
                AddComponent(entity, new UnitTeam { Value = authoring._team });
            }
        }
    }
}
