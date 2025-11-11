using UnityEngine;

namespace Game
{
    public class ExplosionProjectile : MonoBehaviour
    {
        private const float BaseRadius = 3f;
        private const float BaseVisualScale = 1.5f;

        private MeteorCaster _caster;

        public void Construct(MeteorCaster caster)
        {
            _caster = caster;
        }

        private void Start()
        {
            var radius = _caster.Radius;
            var scaleMultiplier = radius / BaseRadius;
            transform.localScale = Vector3.one * (BaseVisualScale * scaleMultiplier);

            EcsEnemyHits.DamageInRange(transform.position, radius, _caster.Damage);
        }
    }
}
