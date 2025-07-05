using Game;
using Unity.Entities;
using UnityEngine;
using Zenject;

namespace EnemyEcs
{
    /// <summary>
    /// The Zenject-DI to ECS-World seam for крок-8's melee enemy systems - crок-7's EcsWorldBridge
    /// equivalent, pushing Player/companion-pools/experience-pool into the systems that need them
    /// once at startup (ECS Systems aren't part of the Zenject container).
    /// </summary>
    public class EnemyEcsBridge : MonoBehaviour
    {
        private Player _player;
        private EnemyCompanionPools _companionPools;
        private PoolOfObject<Experience> _experiencePool;

        [Inject]
        public void Construct(Player player, EnemyCompanionPools companionPools, PoolOfObject<Experience> experiencePool)
        {
            _player = player;
            _companionPools = companionPools;
            _experiencePool = experiencePool;
        }

        private void Start()
        {
            World world = World.DefaultGameObjectInjectionWorld;

            var companionSystem = world.GetOrCreateSystemManaged<EnemyCompanionAssignmentSystem>();
            companionSystem.SetDependencies(_player, _companionPools);

            EntityDamagable playerDamagable = _player.GetComponent<EntityDamagable>();
            world.GetOrCreateSystemManaged<EnemyMeleeAttackSystem>().SetDependencies(playerDamagable, companionSystem);

            world.GetOrCreateSystemManaged<EnemyDeathSystem>().SetDependencies(_experiencePool, companionSystem);
        }
    }
}
