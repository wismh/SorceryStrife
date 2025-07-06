using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Game
{
    public class MeteorProjectile : MonoBehaviour
    {
        private const float EcsHitRadius = 0.6f;

        public event Action OnCollisionFloor;
        private Rigidbody _rigidbody;
        private int _floorLayer;
        private MeteorCaster _meteorCaster;
        private bool _startFalling;

        public void Construct(MeteorCaster caster)
        {
            _meteorCaster = caster;
        }

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _floorLayer = LayerMask.NameToLayer("Terrain");
        }

        private void Start()
        {
            DelayAsync().Forget();
        }

        private void FixedUpdate()
        {
            if (!_startFalling)
                return;

            _rigidbody.linearVelocity = Vector3.down * 10f;

            if (EcsEnemyHits.DamageInRange(transform.position, EcsHitRadius, _meteorCaster.Damage / 3f))
                Destroy(gameObject);
        }

        private async UniTaskVoid DelayAsync()
        {
            await UniTask.WaitForSeconds(_meteorCaster.Delay, cancellationToken: this.GetCancellationTokenOnDestroy());
            _startFalling = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == _floorLayer)
                OnCollisionFloor?.Invoke();

            Destroy(gameObject);
        }
    }
}