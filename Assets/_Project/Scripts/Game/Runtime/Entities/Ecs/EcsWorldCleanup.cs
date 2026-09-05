using PickupEcs;
using Unity.Entities;

namespace EnemyEcs
{
    /// <summary>
    /// Purges all runtime gameplay entities (enemies, pickups, player singleton) from the ECS world
    /// and resets system states so menus stay clean and subsequent gameplay runs start fresh.
    /// </summary>
    public static class EcsWorldCleanup
    {
        public static void CleanUpGameplayEntities()
        {
            World world = World.DefaultGameObjectInjectionWorld;
            if (world == null || !world.IsCreated)
                return;

            EntityManager entityManager = world.EntityManager;

            // Destroy all spawned enemies (excluding prefabs)
            var enemyQuery = entityManager.CreateEntityQuery(
                ComponentType.ReadOnly<EnemyEcsType>(),
                ComponentType.Exclude<Prefab>());
            if (!enemyQuery.IsEmpty)
            {
                entityManager.DestroyEntity(enemyQuery);
            }

            // Destroy all spawned pickups (excluding prefabs)
            var pickupQuery = entityManager.CreateEntityQuery(
                ComponentType.ReadOnly<Pickup>(),
                ComponentType.Exclude<Prefab>());
            if (!pickupQuery.IsEmpty)
            {
                entityManager.DestroyEntity(pickupQuery);
            }

            // Destroy any lingering player singletons
            var playerQuery = entityManager.CreateEntityQuery(
                ComponentType.ReadOnly<PlayerPositionSingleton>());
            if (!playerQuery.IsEmpty)
            {
                entityManager.DestroyEntity(playerQuery);
            }

            // Reset systems holding scene state or references
            var waveSystem = world.GetExistingSystemManaged<WaveSpawnSystem>();
            waveSystem?.Reset();

            var attackSystem = world.GetExistingSystemManaged<EnemyAttackSystem>();
            attackSystem?.Reset();

            var deathSystem = world.GetExistingSystemManaged<EnemyDeathSystem>();
            deathSystem?.Reset();

            var magnetSystem = world.GetExistingSystemManaged<PickupMagnetSystem>();
            magnetSystem?.Reset();
        }
    }
}
