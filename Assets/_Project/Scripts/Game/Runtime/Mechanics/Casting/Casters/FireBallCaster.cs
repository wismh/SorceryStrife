using UnityEngine;
using Zenject;

namespace Game
{
    [SpellCaster(SpellType = typeof(FireBallSpell))]
    public class FireBallCaster : Caster
    {
        public float Damage => PlayerInventory.ApplyModifiers(StatType.Damage, _spell.Damage.ValueAtLevel(Level));
        public float Speed => _spell.Speed.ValueAtLevel(Level);

        private readonly FireBallSpell _spell;
        private readonly FireBallProjectile.Pool _pool;

        [Inject]
        public FireBallCaster(PlayerInventory inventory, FireBallSpell spell, FireBallProjectile.Pool pool) :
            base(spell, inventory)
        {
            _spell = spell;
            _pool = pool;
        }

        protected override void CastInternal(Transform caster)
        {
            const float angleOffset = 25f;

            if (!EnemyTargeting.TryGetNearestPosition(caster.position, out Vector3 targetPosition))
            {
                return;
            }

            var directionToEnemy = (targetPosition - caster.position).normalized;
            var count = Mathf.Max(1, Mathf.RoundToInt(PlayerInventory.ApplyModifiers(StatType.ProjectileCount, 3)));

            for (var i = 0; i < count; ++i)
            {
                var angle = count > 1 ? angleOffset * (i - (count - 1) * 0.5f) : 0f;
                var direction = Quaternion.AngleAxis(angle, Vector3.up) * directionToEnemy;

                var clone = _pool.Spawn();
                clone.transform.position = caster.position;
                clone.Construct(this, direction, _pool);
            }
        }
    }
}
