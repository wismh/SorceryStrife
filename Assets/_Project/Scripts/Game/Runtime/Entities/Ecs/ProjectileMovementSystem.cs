using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace ProjectileEcs
{
    /// <summary>Straight-line movement + lifetime countdown for every ECS projectile, regardless of owner.</summary>
    public partial struct ProjectileMovementSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            float deltaTime = SystemAPI.Time.DeltaTime;
            var ecb = new EntityCommandBuffer(Allocator.TempJob);

            new MoveAndAgeJob
            {
                DeltaTime = deltaTime,
                Ecb = ecb.AsParallelWriter(),
            }.ScheduleParallel(state.Dependency).Complete();

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }

    [BurstCompile]
    internal partial struct MoveAndAgeJob : IJobEntity
    {
        public float DeltaTime;
        public EntityCommandBuffer.ParallelWriter Ecb;

        private void Execute(
            [EntityIndexInQuery] int indexInQuery,
            Entity entity,
            ref LocalTransform transform,
            in ProjectileVelocity velocity,
            ref ProjectileLifetime lifetime)
        {
            transform.Position += velocity.Value * DeltaTime;
            lifetime.Remaining -= DeltaTime;

            if (lifetime.Remaining <= 0f)
                Ecb.DestroyEntity(indexInQuery, entity);
        }
    }
}
