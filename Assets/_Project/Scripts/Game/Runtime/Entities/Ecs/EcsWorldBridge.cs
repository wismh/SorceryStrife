using Game;
using Unity.Entities;
using UnityEngine;
using Zenject;

namespace ProjectileEcs
{
    /// <summary>
    /// The entire Zenject-DI to ECS-World seam for крок-7: pushes the Zenject-managed enemy list
    /// into EnemyHitDetectionSystem once at startup. ECS Systems aren't part of the Zenject
    /// container, so this is a plain scene MonoBehaviour reaching into the default World directly.
    /// </summary>
    public class EcsWorldBridge : MonoBehaviour
    {
        private ListOfObject<Enemy> _enemies;

        [Inject]
        public void Construct(ListOfObject<Enemy> enemies)
        {
            _enemies = enemies;
        }

        private void Start()
        {
            World.DefaultGameObjectInjectionWorld
                .GetOrCreateSystemManaged<EnemyHitDetectionSystem>()
                .SetEnemyTargets(_enemies.Objects);
        }
    }
}
