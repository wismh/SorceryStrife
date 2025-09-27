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
                EcsMeleeEnemyHits.DamageInRange(transform.position, sphereCollider.radius, _caster.Damage);
        }

        private void OnTriggerEnter(Collider collision)
        {
            if (collision.TryGetComponent(out EntityDamagable damagable))
                damagable.Damage(_caster.Damage);
        }
    }
}
