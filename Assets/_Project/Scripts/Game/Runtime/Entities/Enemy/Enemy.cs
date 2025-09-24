using PickupEcs;
using UnityEngine;
using Zenject;

namespace Game
{
    public class Enemy : MonoBehaviour
    {
        private ListOfObject<Enemy> _enemies;
        private PickupEcsSpawner _pickupSpawner;
        private Entity _entity;

        [Inject]
        public void Construct(ListOfObject<Enemy> enemies, PickupEcsSpawner pickupSpawner)
        {
            _pickupSpawner = pickupSpawner;
            _enemies = enemies;
            _entity = GetComponent<Entity>();

            enemies.Objects.Add(this);
        }

        private void Start()
        {
            _entity.OnDeath += SpawnExperience;
        }

        private void OnDestroy()
        {
            _entity.OnDeath -= SpawnExperience;
            _enemies.Objects.Remove(this);
        }

        private void SpawnExperience()
        {
            _pickupSpawner.SpawnPickup(transform.position);
        }
    }
}