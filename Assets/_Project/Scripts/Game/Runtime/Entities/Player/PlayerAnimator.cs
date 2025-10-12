using UnityEngine;

namespace Game
{
    public class PlayerAnimator : MonoBehaviour
    {
        private static readonly int k_dieTrigger = Animator.StringToHash("Die");
        private static readonly int k_idle = Animator.StringToHash("Idle");

        private Animator _animator;
        private MoveController _moveController;
        private Entity _playerAsEntity;
        private bool _wasIdle;
        
        private void Awake()
        {
            _playerAsEntity = GetComponent<Entity>();
            _animator = GetComponentInChildren<Animator>();
            _moveController = GetComponent<MoveController>();
        }

        private void Start()
        {
            _playerAsEntity.OnDeath += HandleDeath;
            _wasIdle = _moveController.Direction == Vector2.zero;
            _animator.SetBool(k_idle, _wasIdle);
        }

        private void OnDestroy()
        {
            _playerAsEntity.OnDeath -= HandleDeath;
        }

        private void Update()
        {
            var isIdle = _moveController.Direction == Vector2.zero;
            if (isIdle != _wasIdle)
            {
                _wasIdle = isIdle;
                _animator.SetBool(k_idle, isIdle);
            }
        }

        private void HandleDeath()
        {
            _animator.SetTrigger(k_dieTrigger);
        }
    }
}