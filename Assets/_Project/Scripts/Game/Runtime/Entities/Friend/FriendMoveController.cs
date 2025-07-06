using UnityEngine;
using Zenject;

namespace Game
{
    public class FriendMoveController : MonoBehaviour
    {
        [SerializeField] private float _speed;
        [SerializeField] private float _maxSpeed;
        [SerializeField] private float _maxSqrDistanceFromPlayer;

        private Player _player;
        private Rigidbody _rigidbody;

        [Inject]
        public void Construct(Player player)
        {
            _player = player;
            _rigidbody = GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            Vector3 targetPosition = _player.transform.position;

            if (EnemyTargeting.TryGetNearestPosition(_player.transform.position, out Vector3 nearestEnemyPos))
            {
                var offset = nearestEnemyPos - transform.position;
                if (offset.sqrMagnitude <= _maxSqrDistanceFromPlayer)
                {
                    targetPosition = nearestEnemyPos;
                }
            }

            var toTarget = targetPosition - transform.position;
            if (toTarget.sqrMagnitude <= 0.01f)
                return;

            var direction = toTarget.normalized;

            _rigidbody.AddForce(direction * _speed);
            _rigidbody.linearVelocity = Vector3.ClampMagnitude(_rigidbody.linearVelocity, _maxSpeed);

            transform.forward = direction;
            transform.rotation = new Quaternion(0, transform.rotation.y, 0, transform.rotation.w);
        }
    }
}