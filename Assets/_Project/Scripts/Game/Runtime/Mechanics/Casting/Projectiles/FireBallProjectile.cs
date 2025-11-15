using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.VFX;
using Zenject;

namespace Game
{
    public class FireBallProjectile : MonoBehaviour
    {
        private const float EcsHitRadius = 0.6f;
        private const float MaxLifetime = 5f;

        private Vector3 _direction;
        private Rigidbody _rigidbody;
        private SphereCollider _collider;
        private VisualEffect _visualEffect;
        private FireBallCaster _caster;
        private IMemoryPool _pool;
        private CancellationTokenSource _lifetimeCts;
        private bool _isDespawned;

        public void Construct(FireBallCaster caster, Vector3 direction, IMemoryPool pool)
        {
            _caster = caster;
            _direction = direction;
            _pool = pool;
            _isDespawned = false;

            enabled = true;
            if (_collider)
            {
                _collider.enabled = true;
            }

            if (_visualEffect)
            {
                _visualEffect.Play();
            }

            _lifetimeCts?.Cancel();
            _lifetimeCts?.Dispose();
            _lifetimeCts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
            LifetimeTimerAsync(_lifetimeCts.Token).Forget();
        }

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _collider = GetComponentInChildren<SphereCollider>();
            _visualEffect = GetComponentInChildren<VisualEffect>();

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

            _rigidbody.linearVelocity = _direction * _caster.Speed;

            if (EcsEnemyHits.DamageInRange(transform.position, EcsHitRadius, _caster.Damage))
            {
                StopAndDespawn();
            }
        }

        private void StopAndDespawn()
        {
            if (_isDespawned)
            {
                return;
            }

            _isDespawned = true;
            enabled = false;
            if (_collider)
            {
                _collider.enabled = false;
            }

            if (_visualEffect)
            {
                _visualEffect.Stop();
            }

            _rigidbody.linearVelocity = Vector3.zero;
            _lifetimeCts?.Cancel();

            DespawnAfterDelayAsync().Forget();
        }

        private async UniTaskVoid DespawnAfterDelayAsync()
        {
            await UniTask.WaitForSeconds(0.4f, cancellationToken: this.GetCancellationTokenOnDestroy());
            DespawnSelf();
        }

        private async UniTaskVoid LifetimeTimerAsync(CancellationToken cancellationToken)
        {
            await UniTask.WaitForSeconds(MaxLifetime, cancellationToken: cancellationToken);
            StopAndDespawn();
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

        public class Pool : MonoMemoryPool<FireBallProjectile>
        {
        }
    }
}
