using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace Game
{
    public class ExplosionProjectile : MonoBehaviour
    {
        private const float BaseRadius = 3f;
        private const float BaseVisualScale = 1.5f;
        private const float ExplosionDuration = 1.2f;

        private MeteorCaster _caster;
        private IMemoryPool _pool;

        public void Construct(MeteorCaster caster, IMemoryPool pool)
        {
            _caster = caster;
            _pool = pool;

            var radius = _caster.Radius;
            var scaleMultiplier = radius / BaseRadius;
            transform.localScale = Vector3.one * (BaseVisualScale * scaleMultiplier);

            EcsEnemyHits.DamageInRange(transform.position, radius, _caster.Damage);

            DespawnAfterDelayAsync().Forget();
        }

        private void Awake()
        {
            if (TryGetComponent(out TempObject tempObject))
            {
                Destroy(tempObject);
            }
        }

        private async UniTaskVoid DespawnAfterDelayAsync()
        {
            await UniTask.WaitForSeconds(ExplosionDuration, cancellationToken: this.GetCancellationTokenOnDestroy());
            DespawnSelf();
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

        public class Pool : MonoMemoryPool<ExplosionProjectile>
        {
        }
    }
}
