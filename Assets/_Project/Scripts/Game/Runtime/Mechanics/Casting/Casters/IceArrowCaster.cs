using UnityEngine;
using Zenject;

namespace Game
{
    [SpellCaster(SpellType = typeof(IceArrowSpell))]
    public class IceArrowCaster : Caster
    {
        private const float ProjectileLifetime = 8f;

        public float Damage => PlayerInventory.ApplyModifiers(StatType.Damage, _spell.Damage.ValueAtLevel(Level));
        public float Speed => _spell.Speed.ValueAtLevel(Level);

        private readonly IceArrowSpell _spell;
        private readonly ListOfObject<Enemy> _enemies;
        private readonly ProjectileEcs.ProjectileEcsSpawner _spawner;

        [Inject]
        public IceArrowCaster(PlayerInventory inventory, IceArrowSpell spell, ListOfObject<Enemy> enemies, ProjectileEcs.ProjectileEcsSpawner spawner):
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

            var number = PlayerInventory.ApplyModifiers(StatType.ProjectileCount, 1);

            for (var i = 0; i < number; ++i)
            {
                if (!EnemyTargeting.TryGetNearestPosition(caster.position, _enemies, out Vector3 targetPosition))
                    return;

                var directionToEnemy = (targetPosition - caster.position).normalized;

                var angle = angleOffset * (i - number / 2);
                var direction = Quaternion.AngleAxis(angle, Vector3.up) * directionToEnemy;

                _spawner.SpawnProjectile(caster.position, direction * Speed, Damage, Team.Ally, ProjectileLifetime, piercing: true);
            }
        }
    }
}
