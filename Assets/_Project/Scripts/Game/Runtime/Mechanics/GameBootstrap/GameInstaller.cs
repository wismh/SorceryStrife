using System.Collections.Generic;
using Game.InventorySystem;
using Lumenwake.UIModule;
using PickupEcs;
using UnityEngine;
using Zenject;

namespace Game {
    public class GameInstaller : MonoInstaller
    {
        [SerializeField] private Player _player;
        [SerializeField] private List<BaseScreen> _screens;
        [SerializeField] private PickupEcsSpawner _pickupEcsSpawner;

        [SerializeField] private List<Spell> _spells;
        [SerializeField] private List<Item> _items;

        public override void InstallBindings()
        {
            Container.Bind<PickupEcsSpawner>().FromInstance(_pickupEcsSpawner).AsSingle();

            Container.Bind<List<Spell>>().FromInstance(_spells).AsSingle();
            Container.Bind<List<Item>>().FromInstance(_items).AsSingle();

            var screenManager = new BaseScreenManager(_screens);
            Container.Bind<BaseScreenManager>().FromInstance(screenManager);
            Container.Bind<IScreenManager>().FromInstance(screenManager);

            foreach (BaseScreen screen in _screens)
                Container.Bind(screen.GetType()).FromInstance(screen);

            Container.Bind<CastersRegister>().AsSingle();
            Container.Bind<ItemsRegister>().AsSingle();
            Container.Bind<PlayerInventory>().AsSingle();

            Container.Bind<Controls>().AsSingle();

            Container.Bind<Player>().FromInstance(_player).AsSingle();
        }
    }
}
