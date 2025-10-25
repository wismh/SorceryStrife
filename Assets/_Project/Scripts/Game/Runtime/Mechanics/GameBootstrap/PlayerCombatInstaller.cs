using Game.InventorySystem;
using PickupEcs;
using UnityEngine;
using Zenject;

namespace Game
{
    public class PlayerCombatInstaller : MonoInstaller
    {
        [SerializeField] private Player _player;
        [SerializeField] private PickupEcsSpawner _pickupEcsSpawner;

        public override void InstallBindings()
        {
            Container.Bind<Player>().FromInstance(_player).AsSingle();
            Container.Bind<PickupEcsSpawner>().FromInstance(_pickupEcsSpawner).AsSingle();

            Container.Bind<Controls>().AsSingle();
            Container.Bind<CastersRegister>().AsSingle();
            Container.Bind<ItemsRegister>().AsSingle();
            Container.Bind<PlayerInventory>().AsSingle();
        }
    }
}
