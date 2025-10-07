using UnityEngine;

namespace Game
{
    public class ExplosionProjectile : MonoBehaviour
    {
        private MeteorCaster _caster;

        public void Construct(MeteorCaster caster)
        {
            _caster = caster;
        }

        private void Start()
        {
            if (TryGetComponent(out SphereCollider sphereCollider))
                EcsEnemyHits.DamageInRange(transform.position, sphereCollider.radius, _caster.Damage);
        }
    }
}
