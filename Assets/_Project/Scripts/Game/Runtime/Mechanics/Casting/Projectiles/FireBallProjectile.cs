using UnityEngine;
using UnityEngine.VFX;

namespace Game
{
    public class FireBallProjectile : MonoBehaviour
    {
        private const float EcsHitRadius = 0.6f;

        private Vector3 _direction;
        private Rigidbody _rigidbody;
        private SphereCollider _collider;
        private TempObject _tempObject;
        private VisualEffect _visualEffect;
        private FireBallCaster _caster;

        public void Construct(FireBallCaster caster, Vector3 direction)
        {
            _caster = caster;
            _direction = direction;
        }

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _collider = GetComponentInChildren<SphereCollider>();
            _tempObject = GetComponent<TempObject>();
            _visualEffect = GetComponentInChildren<VisualEffect>();
        }

        private void FixedUpdate()
        {
            _rigidbody.linearVelocity = _direction * _caster.Speed;

            if (EcsEnemyHits.DamageInRange(transform.position, EcsHitRadius, _caster.Damage))
            {
                StopAndDespawn();
            }
        }

        private void StopAndDespawn()
        {
            enabled = false;
            _collider.enabled = false;
            _tempObject.TimeOfLife = 2f;
            _visualEffect.Stop();
            _rigidbody.linearVelocity = Vector3.zero;
        }
    }
}
