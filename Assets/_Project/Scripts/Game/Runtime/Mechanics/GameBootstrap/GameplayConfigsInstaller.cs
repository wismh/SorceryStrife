using System.Collections.Generic;
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
        }

        private void InstallDamagePopupPool()
        {
            var prefab = _damagePopupPrefab != null
                ? _damagePopupPrefab
                : Resources.Load<DamagePopupView>("DamagePopup");

            DamagePopupPoolsInstaller.Install(Container, prefab, _damagePopupInitialPoolSize);
        }
    }
}