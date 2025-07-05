using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

// No "using Game;" in this file - see ProjectileVelocity.cs for why.
namespace EnemyEcs
{
    /// <summary>On Health &lt;= 0: releases the assigned companion (death animation + delayed pool return, matching EnemyAnimator's 2s fade), spawns Experience via the same PoolOfObject Enemy.SpawnExperience already uses, destroys the entity.</summary>
    [UpdateAfter(typeof(EnemyMeleeAttackSystem))]
    [UpdateAfter(typeof(EnemyMeleeHitDetectionSystem))]
    public partial class EnemyDeathSystem : SystemBase
    {
        private Game.PoolOfObject<Game.Experience> _experiencePool;
        private EnemyCompanionAssignmentSystem _companionSystem;

        public void SetDependencies(Game.PoolOfObject<Game.Experience> experiencePool, EnemyCompanionAssignmentSystem companionSystem)
        {
            _experiencePool = experiencePool;
            _companionSystem = companionSystem;
        }

        protected override void OnUpdate()
        {
            if (_experiencePool == null)
                return;

            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (health, transform, enemyType, entity) in
                     SystemAPI.Query<RefRO<Game.Health>, RefRO<LocalTransform>, RefRO<EnemyEcsType>>().WithEntityAccess())
            {
                if (health.ValueRO.Value > 0f)
                    continue;

                _companionSystem?.HandleDeath(entity, enemyType.ValueRO.Value);

                Game.Experience experience = _experiencePool.Instantiate();
                experience.transform.position = transform.ValueRO.Position;

                ecb.DestroyEntity(entity);
            }

            ecb.Playback(EntityManager);
            ecb.Dispose();
        }
    }
}
