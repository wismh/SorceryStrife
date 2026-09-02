using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace Game
{
    public class EnemyMeleeFight : MonoBehaviour
    {
        public event Action<float> OnAttack;

        private Entity _entity;
        private Player _player;
        private EntityDamagable _damagable;

        private bool _attacking = false;

        [Inject]
        public void Construct(Player player)
        {
            _player = player;
            _entity = GetComponent<Entity>();
            _damagable = _player.GetComponent<EntityDamagable>();
        }

        private void Update()
        {
            if (_attacking || !_player || !_entity.IsAlive)
                return;

            var offset = _player.transform.position - transform.position;
            if (!(offset.magnitude < _entity.RangeOfAttack))
                return;

            _attacking = true;
            _entity.CanMove = false;

            var duration = Entity.BaseAttackDuration / _entity.AttackSpeed;
            OnAttack?.Invoke(duration);

            AttackAsync(duration).Forget();
        }

        private async UniTaskVoid AttackAsync(float duration)
        {
            await UniTask.WaitForSeconds(duration, cancellationToken: this.GetCancellationTokenOnDestroy());

            _attacking = false;
            _entity.CanMove = true;

            if (_entity.IsAlive)
                _damagable.Damage(_entity.Attack);
        }
    }
}
