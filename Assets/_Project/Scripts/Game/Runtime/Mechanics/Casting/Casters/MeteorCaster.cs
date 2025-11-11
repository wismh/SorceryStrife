using System;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Game
{
    [SpellCaster(SpellType = typeof(MeteorSpell))]
    public class MeteorCaster : Caster
    {
        public float Damage => PlayerInventory.ApplyModifiers(StatType.Damage, _spell.Damage.ValueAtLevel(Level));
        public float Projectiles => PlayerInventory.ApplyModifiers(StatType.ProjectileCount, _spell.Projectiles.ValueAtLevel(Level));
        public float Radius => PlayerInventory.ApplyModifiers(
            StatType.Radius,
            _spell.Radius != null && _spell.Radius.Count > 0 ? _spell.Radius.ValueAtLevel(Level) : 3.5f);
        public float Delay => _spell.Delay;
        
        private readonly MeteorSpell _spell;
        private readonly DiContainer _container;
        
        [Inject]
        public MeteorCaster(DiContainer container, PlayerInventory inventory, MeteorSpell spell):
            base(spell, inventory)
        {
            _container = container;
            _spell = spell;
        }

        protected override void CastInternal(Transform caster)
        {
            var projectileCount = Mathf.Max(1, Mathf.RoundToInt(Projectiles));

            for (var i = 0; i < projectileCount; i++)
            {
                var randomOffset = Random.insideUnitCircle.normalized * Random.Range(_spell.Range.x, _spell.Range.y);
                var position = caster.position + new Vector3(randomOffset.x, -0.4f, randomOffset.y);

                var sight = Object.Instantiate(_spell.SightPrefab);
                sight.transform.position = position;

                var clone = _container.InstantiatePrefabForComponent<MeteorProjectile>(_spell.ProjectilePrefab);
                clone.Construct(this);
                clone.transform.position = position + Vector3.up * 15;

                Action onCollision = null;
                onCollision = () =>
                {
                    clone.OnCollisionFloor -= onCollision;
                    SpawnExplosion(position);
                };

                clone.OnCollisionFloor += onCollision;
            }
        }

        private void SpawnExplosion(Vector3 position)
        {
            var explosion = Object.Instantiate(_spell.ExplosionPrefab);
            explosion.Construct(this);
            explosion.transform.position = position;   
        }
    }
}