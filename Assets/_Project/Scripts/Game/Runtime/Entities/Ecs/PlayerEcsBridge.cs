using Game;
using Unity.Entities;
using UnityEngine;
using Zenject;

namespace EnemyEcs
{
    /// <summary>Keeps PlayerPositionSingleton in sync every frame - the player moves, unlike крок-7's one-shot EcsWorldBridge.</summary>
    public class PlayerEcsBridge : MonoBehaviour
    {
        private Player _player;
        private Unity.Entities.Entity _singletonEntity;

        [Inject]
        public void Construct(Player player)
        {
            _player = player;
        }

        private void Start()
        {
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            var query = entityManager.CreateEntityQuery(typeof(PlayerPositionSingleton));
            if (!query.IsEmpty)
            {
                entityManager.DestroyEntity(query);
            }

            _singletonEntity = entityManager.CreateEntity(typeof(PlayerPositionSingleton));
        }

        private void Update()
        {
            if (_singletonEntity == Unity.Entities.Entity.Null)
                return;

            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            if (!entityManager.Exists(_singletonEntity))
                return;

            var playerAsEntity = _player.GetComponent<Game.Entity>();

            entityManager.SetComponentData(_singletonEntity, new PlayerPositionSingleton
            {
                Position = _player.transform.position,
                IsAlive = playerAsEntity.IsAlive,
                PickupRadius = playerAsEntity.RangeOfPickUp,
            });
        }

        private void OnDestroy()
        {
            World world = World.DefaultGameObjectInjectionWorld;
            if (world == null || !world.IsCreated || _singletonEntity == Unity.Entities.Entity.Null)
                return;

            if (world.EntityManager.Exists(_singletonEntity))
            {
                world.EntityManager.DestroyEntity(_singletonEntity);
            }

            _singletonEntity = Unity.Entities.Entity.Null;
        }
    }
}
