using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace Game
{
    public class MeteorProjectile : MonoBehaviour
    {
        private const float EcsHitRadius = 0.6f;

        public event Action OnCollisionFloor;
        private Rigidbody _rigidbody;
        private MeteorCaster _meteorCaster;
        private IMemoryPool _pool;
        private bool _startFalling;
        private bool _exploded;

        public void Construct(MeteorCaster caster, IMemoryPool pool)
        {
            _meteorCaster = caster;
            _pool = pool;
            _exploded = false;
            _startFalling = false;
            enabled = true;

            if (_rigidbody)
            {
                _rigidbody.linearVelocity = Vector3.zero;
            }

            DelayAsync().Forget();
        }

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();

            if (TryGetComponent(out TempObject tempObject))
            {
                Destroy(tempObject);
            }
        }

        private void Explode()
        {
            if (_exploded)
            {
                return;
            }

            _exploded = true;
            enabled = false;
            OnCollisionFloor?.Invoke();
            DespawnSelf();
        }

        private void FixedUpdate()
        {
            if (!_startFalling || _exploded)
            {
                return;
            }

            _rigidbody.linearVelocity = Vector3.down * 10f;

            if (EcsEnemyHits.DamageInRange(transform.position, EcsHitRadius, _meteorCaster.Damage / 3f))
            {
                Explode();
            }
        }

        private async UniTaskVoid DelayAsync()
        {
            await UniTask.WaitForSeconds(_meteorCaster.Delay, cancellationToken: this.GetCancellationTokenOnDestroy());
            if (!_exploded)
            {
                _startFalling = true;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            Explode();
        }

        private void DespawnSelf()
        {
            if (_pool != null)
            {
                _pool.Despawn(this);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public class Pool : MonoMemoryPool<MeteorProjectile>
        {
        }
    }
}