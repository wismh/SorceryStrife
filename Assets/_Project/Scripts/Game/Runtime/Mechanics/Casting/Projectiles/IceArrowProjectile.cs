using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace Game
{
    public class IceArrowProjectile : MonoBehaviour
    {
        private const float EcsHitRadius = 0.6f;
        private const float MaxLifetime = 5f;

        private Vector3 _direction;
        private Rigidbody _rigidbody;
        private IceArrowCaster _caster;
        private readonly HashSet<Unity.Entities.Entity> _hitEcsEnemies = new();
        private IMemoryPool _pool;
        private CancellationTokenSource _lifetimeCts;
        private bool _isDespawned;

        public void Construct(IceArrowCaster caster, Vector3 direction, IMemoryPool pool)
        {
            _caster = caster;
            _direction = direction;
            _pool = pool;
            _isDespawned = false;
            _hitEcsEnemies.Clear();

            enabled = true;

            _lifetimeCts?.Cancel();
            _lifetimeCts?.Dispose();
            _lifetimeCts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
            LifetimeTimerAsync(_lifetimeCts.Token).Forget();
        }

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();

            if (TryGetComponent(out TempObject tempObject))
            {
                Destroy(tempObject);
            }
        }

        private void FixedUpdate()
        {
            if (_isDespawned)
            {
                return;
            }

            transform.right = _direction;
            _rigidbody.linearVelocity = _direction * _caster.Speed;

            EcsEnemyHits.DamageInRange(transform.position, EcsHitRadius, _caster.Damage, _hitEcsEnemies);
        }

        private async UniTaskVoid LifetimeTimerAsync(CancellationToken cancellationToken)
        {
            await UniTask.WaitForSeconds(MaxLifetime, cancellationToken: cancellationToken);
            DespawnSelf();
        }

        private void DespawnSelf()
        {
            if (_isDespawned)
            {
                return;
            }

            _isDespawned = true;
            enabled = false;
            _lifetimeCts?.Cancel();

            if (_pool != null)
            {
                _pool.Despawn(this);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public class Pool : MonoMemoryPool<IceArrowProjectile>
        {
        }
    }
}
