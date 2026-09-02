using Unity.Entities;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// Bakes the same stats as Entity/EntityCharacteristics (Attack, RangeOfAttack,
    /// AttackSpeed, MoveSpeed, MaxHealth, RangeOfPickUp) plus Team into ECS components.
    /// Not yet wired to any spawn path or SubScene - groundwork for the enemy/projectile
    /// ECS conversion (migration plan steps 7-8), which will decide whether entities come
    /// from a baked SubScene prefab or EntityManager.CreateEntity at runtime.
    /// </summary>
    public class EntityStatsAuthoring : MonoBehaviour
    {
        [SerializeField] private EntityCharacteristics _characteristics;
        [SerializeField] private Team _team;

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
