using UnityEngine;
using Zenject;

namespace Game
{
    [SpellCaster(SpellType = typeof(IceArrowSpell))]
    public class IceArrowCaster : Caster
    {
        public float Damage => _spell.Damage.ValueAtLevel(Level) * PlayerInventory.GetSumOfBuff(nameof(Damage));
        public float Speed => _spell.Speed.ValueAtLevel(Level);
        
        private readonly IceArrowSpell _spell;
        private readonly ListOfObject<Enemy> _enemies;
        private readonly DiContainer _container;
        
        [Inject]
        public IceArrowCaster(DiContainer container, PlayerInventory inventory, IceArrowSpell spell, ListOfObject<Enemy> enemies):
            base(spell, inventory)
        {
            _container = container;
            _spell = spell;
            _enemies = enemies;
        }

        // ReSharper disable Unity.PerformanceAnalysis
        protected override void CastInternal(Transform caster)
        {
            const float angleOffset = 25f;

            var number = 1 * PlayerInventory.GetSumOfBuff("Projectiles");
            
            for (var i = 0; i < number; ++i)
            {
                var nearestEnemy = _enemies.GetNearestTo(caster.position);
                if (!nearestEnemy)
                    return;

                var directionToEnemy = (nearestEnemy.transform.position - caster.position).normalized;
                
                var angle = angleOffset * (i - number / 2);
                var direction = Quaternion.AngleAxis(angle, Vector3.up) * directionToEnemy;

                var clone = _container.InstantiatePrefabForComponent<IceArrowProjectile>(_spell.ProjectilePrefab);
                clone.Construct(this, direction);
                clone.transform.position = caster.position;
            }
        }
    }
}