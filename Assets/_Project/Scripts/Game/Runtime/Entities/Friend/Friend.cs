using UnityEngine;
using Zenject;

namespace Game
{
    public class Friend : MonoBehaviour
    {
        [SerializeField] private float _damage;
        [SerializeField] private float _gainDamageByLevel;
        [SerializeField] private float _attackRadius = 0.8f;
        [SerializeField] private float _attackInterval = 0.5f;

        private Player _player;
        private float _lastAttackTime;

        [Inject]
        public void Construct(Player player)
        {
            _player = player;
        }

        private void FixedUpdate()
        {
            if (Time.time - _lastAttackTime < _attackInterval)
            {
                return;
            }

            var damage = _damage + (_gainDamageByLevel * _player.Level);
            var hit = EcsEnemyHits.DamageAndPushInRange(transform.position, _attackRadius, damage, pushDistance: 0.5f);
            if (hit)
            {
                _lastAttackTime = Time.time;
            }
        }
        
        private void OnTriggerEnter(Collider collision)
        {
            if (collision.TryGetComponent(out EntityDamagable damagable))
            {
                damagable.Damage(_damage + (_gainDamageByLevel * _player.Level));
            }
        }
    }
}