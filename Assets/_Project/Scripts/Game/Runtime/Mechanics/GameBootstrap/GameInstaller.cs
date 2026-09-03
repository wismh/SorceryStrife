using System.Collections.Generic;
using Game.InventorySystem;
using UnityEngine;
using Zenject;

namespace Game {
    public class GameInstaller : MonoInstaller
    {
        [SerializeField] private Player _player;
        [SerializeField] private Experience _experiencePrefab;
        [SerializeField] private List<BaseScreen> _screens;

        public override void InstallBindings()
        {
            var screenManager = new BaseScreenManager(_screens);
            Container.Bind<BaseScreenManager>().FromInstance(screenManager);
            Container.Bind<IScreenManager>().FromInstance(screenManager);

            foreach (BaseScreen screen in _screens)
                Container.Bind(screen.GetType()).FromInstance(screen);

            Container.Bind<ListOfObject<Enemy>>().AsSingle();
            Container.Bind<ListOfObject<Projectile>>().AsSingle();
            Container.Bind<PoolOfObject<Experience>>().FromInstance(
                new PoolOfObject<Experience>(Container, _experiencePrefab)).AsSingle();

            Container.Bind<CastersRegister>().AsSingle();
            Container.Bind<ItemsRegister>().AsSingle();
            Container.Bind<PlayerInventory>().AsSingle();

            Container.Bind<Controls>().AsSingle();

            Container.Bind<Player>().FromInstance(_player).AsSingle();
        }
    }
}
