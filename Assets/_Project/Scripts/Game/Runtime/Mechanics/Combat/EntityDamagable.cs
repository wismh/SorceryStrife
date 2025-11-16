using Project.Core.DamagePopupModule;
using UnityEngine;
using Zenject;

namespace Game
{
    public class EntityDamagable : MonoBehaviour
    {
        [SerializeField] private DamageNumber _damageNumberPrefab;

        public DamageNumber DamageNumberPrefab => _damageNumberPrefab;

        private Entity _entity;
        private IDamagePopupSpawner _damagePopupSpawner;

        [Inject]
        public void Construct([InjectOptional] IDamagePopupSpawner damagePopupSpawner = null)
        {
            _damagePopupSpawner = damagePopupSpawner;
            _entity = GetComponent<Entity>();
        }

        private void Start()
        {
            _entity.OnHit += ShowDamageNumber;
        }

        private void OnDestroy()
        {
            _entity.OnHit -= ShowDamageNumber;
        }

        public void Damage(float amount)
        {
            if (!_entity.IsAlive)
            {
                return;
            }

            _entity.Health -= amount;

            _entity.OnHit?.Invoke(amount);
            if (_entity.Health > 0)
            {
                return;
            }
            
            if (TryGetComponent(out BoxCollider boxCollider))
            {
                boxCollider.enabled = false;
            }

            if (TryGetComponent(out Rigidbody body))
            {
                body.isKinematic = true;
            }
            
            _entity.IsAlive = false;
            _entity.OnDeath?.Invoke();
        }

        private void ShowDamageNumber(float amount)
        {
            var isPlayer = GetComponent<Player>() != null;
            var damageColor = isPlayer
                ? new Color(0.96f, 0.26f, 0.88f, 1f)
                : new Color(1f, 0.28f, 0.24f, 1f);

            if (_damagePopupSpawner != null)
            {
                _damagePopupSpawner.Spawn(transform, transform.position, new DamagePopupInfo(amount, isCrit: false, damageColor));
                return;
            }

            if (!_damageNumberPrefab)
            {
                return;
            }

            var clone = Instantiate(_damageNumberPrefab);
            clone.transform.position = transform.position;
            clone.Text = amount.ToString("0.#");
            clone.SetColor(damageColor);
        }
    }
}