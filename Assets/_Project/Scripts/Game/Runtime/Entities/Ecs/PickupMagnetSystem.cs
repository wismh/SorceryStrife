using EnemyEcs;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

// No "using Game;" in this file - see ProjectileVelocity.cs for why. Game.Player referenced fully
// qualified.
namespace PickupEcs
{
    /// <summary>
    /// крок-10: magnet movement + collection for Experience pickups, replacing Experience.cs's
    /// Rigidbody-based Update() and Player.OnCollisionEnter. Reads player position/pickup-radius off
    /// крок-8's PlayerPositionSingleton (extended with PickupRadius). Preserves Experience.cs's
    /// original "sqrDistance > RangeOfPickUp" comparison exactly - RangeOfPickUp is NOT squared
    /// before that comparison in the original code (an existing quirk, not fixed here). CollectDistance
    /// is a new constant approximating the old SphereCollider (0.1 world radius) vs. player
    /// BoxCollider (0.5 half-extent) contact, since ECS pickups carry no physics colliders.
    /// </summary>
    public partial class PickupMagnetSystem : SystemBase
    {
        private const float Speed = 10f;
        private const float CollectDistance = 0.6f;

        private Game.Player _player;

        public void SetDependencies(Game.Player player)
        {
            _player = player;
        }

        protected override void OnUpdate()
        {
            if (_player == null || !SystemAPI.TryGetSingleton(out PlayerPositionSingleton player))
                return;

            var ecb = new EntityCommandBuffer(Allocator.TempJob);
            var collectEvents = new NativeQueue<float>(Allocator.TempJob);

            new PickupMagnetJob
            {
                DeltaTime = SystemAPI.Time.DeltaTime,
                PlayerPosition = player.Position,
                PlayerPickupRadius = player.PickupRadius,
                CollectDistance = CollectDistance,
                Speed = Speed,
                Ecb = ecb.AsParallelWriter(),
                CollectEvents = collectEvents.AsParallelWriter(),
            }.ScheduleParallel(Dependency).Complete();

            while (collectEvents.TryDequeue(out float amount))
                _player.AwardExperience(amount);

            ecb.Playback(EntityManager);
            ecb.Dispose();
            collectEvents.Dispose();
        }
    }

    [BurstCompile]
    internal partial struct PickupMagnetJob : IJobEntity
    {
        public float DeltaTime;
        public float3 PlayerPosition;
        public float PlayerPickupRadius;
        public float CollectDistance;
        public float Speed;
        public EntityCommandBuffer.ParallelWriter Ecb;
        public NativeQueue<float>.ParallelWriter CollectEvents;

        private void Execute([EntityIndexInQuery] int indexInQuery, Entity entity, ref LocalTransform transform, in Pickup pickup)
        {
            float3 offset = PlayerPosition - transform.Position;
            float sqrDistance = math.lengthsq(offset);

            if (sqrDistance > PlayerPickupRadius)
                return;

            if (sqrDistance <= CollectDistance * CollectDistance)
            {
                CollectEvents.Enqueue(1f);
                Ecb.DestroyEntity(indexInQuery, entity);
                return;
            }

            transform.Position += (offset / math.sqrt(sqrDistance)) * Speed * DeltaTime;
        }
    }
}
