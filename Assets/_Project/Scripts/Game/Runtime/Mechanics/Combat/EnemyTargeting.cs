using UnityEngine;

namespace Game
{
    /// <summary>
    /// Nearest-enemy targeting querying ECS entities via EcsEnemyHits.
    /// All enemy types (melee and ranged) are simulated in ECS.
    /// </summary>
    public static class EnemyTargeting
    {
        public static bool TryGetNearestPosition(Vector3 from, out Vector3 position)
        {
            var hasTarget = EcsEnemyHits.TryGetNearestPosition(from, out position);
            return hasTarget;
        }
    }
}
