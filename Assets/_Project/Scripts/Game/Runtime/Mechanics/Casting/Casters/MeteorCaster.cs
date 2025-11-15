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
        private readonly MeteorProjectile.Pool _meteorPool;
        private readonly ExplosionProjectile.Pool _explosionPool;
        
        [Inject]
        public MeteorCaster(
            PlayerInventory inventory,
            MeteorSpell spell,
            MeteorProjectile.Pool meteorPool,
            ExplosionProjectile.Pool explosionPool) :
            base(spell, inventory)
        {
            _spell = spell;
            _meteorPool = meteorPool;
            _explosionPool = explosionPool;
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

                var clone = _meteorPool.Spawn();
                clone.transform.position = position + Vector3.up * 15;
                clone.Construct(this, _meteorPool);

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
            var explosion = _explosionPool.Spawn();
            explosion.transform.position = position;
            explosion.Construct(this, _explosionPool);
        }
    }
}