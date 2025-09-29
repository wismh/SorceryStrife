using UnityEngine;

namespace Game
{
    /// <summary>
    /// Nearest-enemy targeting spanning both enemy representations: MonoBehaviour
    /// (ListOfObject&lt;Enemy&gt;, Devil/HotDevil/Eye/BigEye) and melee-type ECS entities
    /// (EcsMeleeEnemyHits, Minion/Mutant/Ogr/OldMutant). A Caster that only asked
    /// ListOfObject&lt;Enemy&gt;.GetNearestTo would never see a melee-type enemy at all and could
    /// go silent for the whole fight if none of the MonoBehaviour types happened to be alive.
    /// </summary>
    public static class EnemyTargeting
    {
        public static bool TryGetNearestPosition(Vector3 from, ListOfObject<Enemy> monoBehaviourEnemies, out Vector3 position)
        {
            Enemy nearestMonoBehaviour = monoBehaviourEnemies.GetNearestTo(from);
            var hasEcsTarget = EcsMeleeEnemyHits.TryGetNearestPosition(from, out Vector3 ecsPosition);

            if (nearestMonoBehaviour && hasEcsTarget)
            {
                Vector3 monoBehaviourPosition = nearestMonoBehaviour.transform.position;
                position = (from - monoBehaviourPosition).sqrMagnitude <= (from - ecsPosition).sqrMagnitude
                    ? monoBehaviourPosition
                    : ecsPosition;
                return true;
            }

            if (nearestMonoBehaviour)
            {
                position = nearestMonoBehaviour.transform.position;
                return true;
            }

            if (hasEcsTarget)
            {
                position = ecsPosition;
                return true;
            }

            position = default;
            return false;
        }
    }
}
