using Game;
using PickupEcs;
using Unity.Entities;
using UnityEngine;
using Zenject;

namespace EnemyEcs
{
    /// <summary>
    /// The Zenject-DI to ECS-World seam for крок-8's melee enemy systems and крок-10's pickup
    /// systems - crок-7's EcsWorldBridge equivalent, pushing Player/companion-pools/pickup-spawner
    /// into the systems that need them once at startup (ECS Systems aren't part of the Zenject
    /// container).
    /// </summary>
    public class EnemyEcsBridge : MonoBehaviour
    {
        private Player _player;
        private EnemyCompanionPools _companionPools;
        private PickupEcsSpawner _pickupSpawner;

        [Inject]
        public void Construct(Player player, EnemyCompanionPools companionPools, PickupEcsSpawner pickupSpawner)
        {
            _player = player;
            _companionPools = companionPools;
            _pickupSpawner = pickupSpawner;
        }

        private void Start()
        {
            World world = World.DefaultGameObjectInjectionWorld;

            var companionSystem = world.GetOrCreateSystemManaged<EnemyCompanionAssignmentSystem>();
            companionSystem.SetDependencies(_player, _companionPools);

            EntityDamagable playerDamagable = _player.GetComponent<EntityDamagable>();
            world.GetOrCreateSystemManaged<EnemyMeleeAttackSystem>().SetDependencies(playerDamagable, companionSystem);

            Entity pickupPrefab = _pickupSpawner.GetOrCreatePrefabEntity();
            world.GetOrCreateSystemManaged<EnemyDeathSystem>().SetDependencies(pickupPrefab, companionSystem);

            world.GetOrCreateSystemManaged<PickupMagnetSystem>().SetDependencies(_player);
        }
    }
}
