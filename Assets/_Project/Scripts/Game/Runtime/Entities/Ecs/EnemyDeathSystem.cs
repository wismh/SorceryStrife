using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

// No "using Game;" in this file - see ProjectileVelocity.cs for why.
namespace EnemyEcs
{
    /// <summary>On Health &lt;= 0: releases the assigned companion (death animation + delayed pool return, matching EnemyAnimator's 2s fade), spawns an Experience pickup entity (крок-10: PickupEcs.PickupEcsSpawner's prefab, instantiated via this method's own ECB alongside the entity destroy - not a direct EntityManager call, since that would be unsafe mid-iteration of the SystemAPI.Query below), destroys the entity.</summary>
    [UpdateAfter(typeof(EnemyAttackSystem))]
    public partial class EnemyDeathSystem : SystemBase
    {
        private Entity _pickupPrefab;
        private System.Action<float3> _minibossDeathCallback;

        public void SetDependencies(
            Entity pickupPrefab,
            System.Action<float3> minibossDeathCallback = null)
        {
            _pickupPrefab = pickupPrefab;
            _minibossDeathCallback = minibossDeathCallback;
        }

        protected override void OnUpdate()
        {
            if (_pickupPrefab == Entity.Null)
                return;

            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (health, transform, enemyType, entity) in
                     SystemAPI.Query<RefRO<Game.Health>, RefRO<LocalTransform>, RefRO<EnemyEcsType>>().WithEntityAccess())
            {
                if (health.ValueRO.Value > 0f)
                    continue;

                if (enemyType.ValueRO.Value == Game.EnemyType.Eye || enemyType.ValueRO.Value == Game.EnemyType.BigEye)
                    _minibossDeathCallback?.Invoke(transform.ValueRO.Position);

                Entity pickup = ecb.Instantiate(_pickupPrefab);
                ecb.SetComponent(pickup, new LocalTransform
                {
                    Position = transform.ValueRO.Position,
                    Rotation = quaternion.identity,
                    Scale = 0.2f,
                });

                ecb.DestroyEntity(entity);
            }

            ecb.Playback(EntityManager);
            ecb.Dispose();
        }
    }
}
