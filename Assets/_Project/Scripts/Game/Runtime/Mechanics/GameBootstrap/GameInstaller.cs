using System.Collections.Generic;
using Game.InventorySystem;
using PickupEcs;
using UnityEngine;
using Zenject;

namespace Game {
    public class GameInstaller : MonoInstaller
    {
        [SerializeField] private Player _player;
        [SerializeField] private List<BaseScreen> _screens;
        [SerializeField] private PickupEcsSpawner _pickupEcsSpawner;
        [SerializeField] private EnemyCompanion _minionCompanionPrefab;
        [SerializeField] private EnemyCompanion _mutantCompanionPrefab;
        [SerializeField] private EnemyCompanion _ogrCompanionPrefab;
        [SerializeField] private EnemyCompanion _oldMutantCompanionPrefab;
        [SerializeField] private EnemyCompanion _devilCompanionPrefab;
        [SerializeField] private EnemyCompanion _hotDevilCompanionPrefab;
        [SerializeField] private EnemyCompanion _eyeCompanionPrefab;
        [SerializeField] private EnemyCompanion _bigEyeCompanionPrefab;

        public override void InstallBindings()
        {
            Container.Bind<PickupEcsSpawner>().FromInstance(_pickupEcsSpawner).AsSingle();

            var companionPools = new EnemyCompanionPools(new[]
            {
                new PoolOfObject<EnemyCompanion>(Container, _minionCompanionPrefab),
                new PoolOfObject<EnemyCompanion>(Container, _mutantCompanionPrefab),
                new PoolOfObject<EnemyCompanion>(Container, _ogrCompanionPrefab),
                new PoolOfObject<EnemyCompanion>(Container, _oldMutantCompanionPrefab),
                new PoolOfObject<EnemyCompanion>(Container, _devilCompanionPrefab),
                new PoolOfObject<EnemyCompanion>(Container, _hotDevilCompanionPrefab),
                new PoolOfObject<EnemyCompanion>(Container, _eyeCompanionPrefab),
                new PoolOfObject<EnemyCompanion>(Container, _bigEyeCompanionPrefab),
            });
            Container.Bind<EnemyCompanionPools>().FromInstance(companionPools).AsSingle();

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
