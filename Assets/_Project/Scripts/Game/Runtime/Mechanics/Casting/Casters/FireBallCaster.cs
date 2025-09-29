using UnityEngine;
using Zenject;

namespace Game
{
    [SpellCaster(SpellType = typeof(FireBallSpell))]
    public class FireBallCaster : Caster
    {
        private const float ProjectileLifetime = 8f;

        public float Damage => PlayerInventory.ApplyModifiers(StatType.Damage, _spell.Damage.ValueAtLevel(Level));
        public float Speed => _spell.Speed.ValueAtLevel(Level);

        private readonly FireBallSpell _spell;
        private readonly ListOfObject<Enemy> _enemies;
        private readonly ProjectileEcs.ProjectileEcsSpawner _spawner;

        [Inject]
        public FireBallCaster(PlayerInventory inventory, FireBallSpell spell, ListOfObject<Enemy> enemies, ProjectileEcs.ProjectileEcsSpawner spawner):
            base(spell, inventory)
        {
            _spell = spell;
            _enemies = enemies;
            _spawner = spawner;
        }

        // ReSharper disable Unity.PerformanceAnalysis
        protected override void CastInternal(Transform caster)
        {
            const float angleOffset = 25f;

            if (!EnemyTargeting.TryGetNearestPosition(caster.position, _enemies, out Vector3 targetPosition))
                return;

            var directionToEnemy = (targetPosition - caster.position).normalized;

            var number = PlayerInventory.ApplyModifiers(StatType.ProjectileCount, 3);

            for (var i = 0; i < number; ++i)
            {
                var angle = angleOffset * (i - number / 2);
                var direction = Quaternion.AngleAxis(angle, Vector3.up) * directionToEnemy;

                _spawner.SpawnProjectile(caster.position, direction * Speed, Damage, Team.Ally, ProjectileLifetime);
            }
        }
    }
}
