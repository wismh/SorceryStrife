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
            _singletonEntity = entityManager.CreateEntity(typeof(PlayerPositionSingleton));
        }

        private void Update()
        {
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            var playerAsEntity = _player.GetComponent<Game.Entity>();

            entityManager.SetComponentData(_singletonEntity, new PlayerPositionSingleton
            {
                Position = _player.transform.position,
                IsAlive = playerAsEntity.IsAlive,
                PickupRadius = playerAsEntity.RangeOfPickUp,
            });
        }
    }
}
