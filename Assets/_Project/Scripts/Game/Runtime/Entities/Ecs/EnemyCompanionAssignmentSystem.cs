using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

// No "using Game;" in this file - see ProjectileVelocity.cs for why (this file has no IJobEntity
// struct itself, but stays consistent with its sibling EnemyEcs files that do).
namespace EnemyEcs
{
    /// <summary>
    /// Budgeted GameObject-companion pool assignment (migration plan §5.3): claims a companion
    /// from the entity's type pool for any alive melee enemy within VisibleRadius of the player
    /// that doesn't already have one, up to a fixed per-type budget; releases companions whose
    /// entity died or left range. First-come-first-served, no distance-sort eviction of
    /// already-assigned companions - an entity that can't get one this tick stays fully
    /// simulated, just briefly invisible.
    /// </summary>
    public partial class EnemyCompanionAssignmentSystem : SystemBase
    {
        private const float TickInterval = 0.2f;
        private const float VisibleRadius = 25f;
        private const int BudgetPerType = 10;

        private float _timer;
        private Game.Player _player;
        private Game.EnemyCompanionPools _pools;
        private readonly Dictionary<Entity, Game.EnemyCompanion> _assigned = new();
        private readonly int[] _assignedCountByType = new int[System.Enum.GetValues(typeof(Game.EnemyType)).Length];
        private readonly List<Entity> _releaseScratch = new();

        public void SetDependencies(Game.Player player, Game.EnemyCompanionPools pools)
        {
            _player = player;
            _pools = pools;
        }

        public bool TryGetCompanion(Entity entity, out Game.EnemyCompanion companion) =>
            _assigned.TryGetValue(entity, out companion);

        /// <summary>Called by EnemyDeathSystem while the entity still exists, before it's destroyed.</summary>
        public void HandleDeath(Entity entity, Game.EnemyType enemyType)
        {
            if (!_assigned.Remove(entity, out Game.EnemyCompanion companion))
                return;

            _assignedCountByType[(int)enemyType]--;
            companion.PlayDeathAndRelease(() => _pools[enemyType].Destroy(companion));
        }

        protected override void OnUpdate()
        {
            if (_pools == null)
                return;

            SyncAssignedTransforms();

            _timer += SystemAPI.Time.DeltaTime;
            if (_timer < TickInterval)
                return;
            _timer = 0f;

            float3 playerPosition = _player.transform.position;
            ReleaseOutOfRange(playerPosition);
            AssignInRange(playerPosition);
        }

        private void SyncAssignedTransforms()
        {
            foreach (var (transform, entity) in
                     SystemAPI.Query<RefRO<LocalTransform>>().WithAll<EnemyEcsType>().WithEntityAccess())
            {
                if (_assigned.TryGetValue(entity, out Game.EnemyCompanion companion))
                    companion.SetTransform(transform.ValueRO.Position, transform.ValueRO.Rotation);
            }
        }

        private void ReleaseOutOfRange(float3 playerPosition)
        {
            _releaseScratch.Clear();

            foreach (var (transform, entity) in
                     SystemAPI.Query<RefRO<LocalTransform>>().WithAll<EnemyEcsType>().WithEntityAccess())
            {
                if (!_assigned.ContainsKey(entity))
                    continue;
                if (math.distance(transform.ValueRO.Position, playerPosition) <= VisibleRadius)
                    continue;

                _releaseScratch.Add(entity);
            }

            foreach (Entity entity in _releaseScratch)
            {
                Game.EnemyType enemyType = EntityManager.GetComponentData<EnemyEcsType>(entity).Value;
                Game.EnemyCompanion companion = _assigned[entity];
                _assigned.Remove(entity);
                _assignedCountByType[(int)enemyType]--;

                companion.ResetForReuse();
                _pools[enemyType].Destroy(companion);
            }
        }

        private void AssignInRange(float3 playerPosition)
        {
            foreach (var (transform, health, enemyType, entity) in
                     SystemAPI.Query<RefRO<LocalTransform>, RefRO<Game.Health>, RefRO<EnemyEcsType>>().WithEntityAccess())
            {
                if (health.ValueRO.Value <= 0f || _assigned.ContainsKey(entity))
                    continue;

                Game.EnemyType type = enemyType.ValueRO.Value;
                if (_assignedCountByType[(int)type] >= BudgetPerType)
                    continue;
                if (math.distance(transform.ValueRO.Position, playerPosition) > VisibleRadius)
                    continue;

                Game.EnemyCompanion companion = _pools[type].Instantiate();
                companion.ResetForReuse();
                companion.SetTransform(transform.ValueRO.Position, transform.ValueRO.Rotation);

                _assigned[entity] = companion;
                _assignedCountByType[(int)type]++;
            }
        }
    }
}
