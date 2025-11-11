using UnityEngine;
using Zenject;

namespace Game
{
    [SpellCaster(SpellType = typeof(IceArrowSpell))]
    public class IceArrowCaster : Caster
    {
        public float Damage => PlayerInventory.ApplyModifiers(StatType.Damage, _spell.Damage.ValueAtLevel(Level));
        public float Speed => _spell.Speed.ValueAtLevel(Level);

        private readonly IceArrowSpell _spell;
        private readonly DiContainer _container;

        [Inject]
        public IceArrowCaster(DiContainer container, PlayerInventory inventory, IceArrowSpell spell) :
            base(spell, inventory)
        {
            _container = container;
            _spell = spell;
        }

        // ReSharper disable Unity.PerformanceAnalysis
        protected override void CastInternal(Transform caster)
        {
            const float angleOffset = 25f;

            if (!EnemyTargeting.TryGetNearestPosition(caster.position, out Vector3 targetPosition))
                return;

            var directionToEnemy = (targetPosition - caster.position).normalized;

            var count = Mathf.Max(1, Mathf.RoundToInt(PlayerInventory.ApplyModifiers(StatType.ProjectileCount, 1)));

            for (var i = 0; i < count; ++i)
            {
                var angle = count > 1 ? angleOffset * (i - (count - 1) * 0.5f) : 0f;
                var direction = Quaternion.AngleAxis(angle, Vector3.up) * directionToEnemy;

                var clone = _container.InstantiatePrefabForComponent<IceArrowProjectile>(_spell.ProjectilePrefab);
                clone.Construct(this, direction);
                clone.transform.position = caster.position;
            }
        }
    }
}
