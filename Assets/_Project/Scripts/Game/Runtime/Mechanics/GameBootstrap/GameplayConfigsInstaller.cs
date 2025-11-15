using System.Collections.Generic;
using System.Linq;
using Project.Core.DamagePopupModule;
using UnityEngine;
using Zenject;

namespace Game
{
    public class GameplayConfigsInstaller : MonoInstaller
    {
        [SerializeField] private List<Spell> _spells;
        [SerializeField] private List<Item> _items;
        [SerializeField] private DamagePopupView _damagePopupPrefab;
        [SerializeField] private int _damagePopupInitialPoolSize = 32;

        public override void InstallBindings()
        {
            Container.Bind<List<Spell>>().FromInstance(_spells).AsSingle();
            Container.Bind<List<Item>>().FromInstance(_items).AsSingle();

            InstallDamagePopupPool();
            InstallProjectilePools();
        }

        private void InstallDamagePopupPool()
        {
            var prefab = _damagePopupPrefab != null
                ? _damagePopupPrefab
                : Resources.Load<DamagePopupView>("DamagePopup");

            DamagePopupPoolsInstaller.Install(Container, prefab, _damagePopupInitialPoolSize);
        }

        private void InstallProjectilePools()
        {
            var poolRoot = new GameObject("ProjectilesPool").transform;

            var fireBallSpell = _spells.OfType<FireBallSpell>().FirstOrDefault();
            if (fireBallSpell && fireBallSpell.ProjectilePrefab)
            {
                Container.BindMemoryPool<FireBallProjectile, FireBallProjectile.Pool>()
                    .WithInitialSize(16)
                    .FromComponentInNewPrefab(fireBallSpell.ProjectilePrefab)
                    .UnderTransform(poolRoot);
            }

            var iceArrowSpell = _spells.OfType<IceArrowSpell>().FirstOrDefault();
            if (iceArrowSpell && iceArrowSpell.ProjectilePrefab)
            {
                Container.BindMemoryPool<IceArrowProjectile, IceArrowProjectile.Pool>()
                    .WithInitialSize(16)
                    .FromComponentInNewPrefab(iceArrowSpell.ProjectilePrefab)
                    .UnderTransform(poolRoot);
            }

            var meteorSpell = _spells.OfType<MeteorSpell>().FirstOrDefault();
            if (meteorSpell)
            {
                if (meteorSpell.ProjectilePrefab)
                {
                    Container.BindMemoryPool<MeteorProjectile, MeteorProjectile.Pool>()
                        .WithInitialSize(8)
                        .FromComponentInNewPrefab(meteorSpell.ProjectilePrefab)
                        .UnderTransform(poolRoot);
                }

                if (meteorSpell.ExplosionPrefab)
                {
                    Container.BindMemoryPool<ExplosionProjectile, ExplosionProjectile.Pool>()
                        .WithInitialSize(8)
                        .FromComponentInNewPrefab(meteorSpell.ExplosionPrefab)
                        .UnderTransform(poolRoot);
                }
            }
        }
    }
}
