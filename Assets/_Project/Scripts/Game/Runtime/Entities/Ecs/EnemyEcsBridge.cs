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
        [SerializeField] private DevilProjectile _devilProjectilePrefab;
        [SerializeField] private Chest _chestPrefab;

        private Player _player;
        private PickupEcsSpawner _pickupSpawner;
        private DiContainer _container;

        [Inject]
        public void Construct(
            Player player,
            PickupEcsSpawner pickupSpawner,
            DiContainer container)
        {
            _player = player;
            _pickupSpawner = pickupSpawner;
            _container = container;
        }

        private void Start()
        {
            World world = World.DefaultGameObjectInjectionWorld;

            EntityDamagable playerDamagable = _player.GetComponent<EntityDamagable>();
            world.GetOrCreateSystemManaged<EnemyAttackSystem>().SetDependencies(
                playerDamagable,
                _devilProjectilePrefab,
                _container);
            EcsEnemyHits.SetDamageNumberPrefab(playerDamagable.DamageNumberPrefab);

            Unity.Entities.Entity pickupPrefab = _pickupSpawner.GetOrCreatePrefabEntity();
            world.GetOrCreateSystemManaged<EnemyDeathSystem>().SetDependencies(
                pickupPrefab,
                pos => _container.InstantiatePrefab(
                    _chestPrefab,
                    new Vector3(pos.x, _player.transform.position.y, pos.z),
                    Quaternion.identity,
                    null));

            world.GetOrCreateSystemManaged<PickupMagnetSystem>().SetDependencies(_player);
        }
    }
}
