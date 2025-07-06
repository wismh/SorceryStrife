using System.Collections.Generic;
using UnityEngine;
using Entity = Unity.Entities.Entity;

namespace Game
{
    public class IceArrowProjectile : MonoBehaviour
    {
        private const float EcsHitRadius = 0.6f;

        private Vector3 _direction;
        private Rigidbody _rigidbody;
        private IceArrowCaster _caster;
        private readonly HashSet<Entity> _hitEcsEnemies = new();

        public void Construct(IceArrowCaster caster, Vector3 direction)
        {
            _caster = caster;
            _direction = direction;
        }

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            transform.right = _direction;
            _rigidbody.linearVelocity = _direction * _caster.Speed;

            EcsMeleeEnemyHits.DamageInRange(transform.position, EcsHitRadius, _caster.Damage, _hitEcsEnemies);
        }

        private void OnTriggerEnter(Collider collision)
        {
            if (!collision.transform.TryGetComponent(out EntityDamagable damagable))
                return;

            damagable.Damage(_caster.Damage);
        }
    }
}