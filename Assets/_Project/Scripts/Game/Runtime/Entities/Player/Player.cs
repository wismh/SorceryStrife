using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace Game
{
    public class Player : MonoBehaviour
    {
        [SerializeField] private float _baseRequiredExperienceForLevelUp;
        [SerializeField] private float _multiplierFactorOfRequiredExperienceByLevel;
        
        public event Action OnLevelUp;
        
        public int Level { get; private set; }
        public float Experience { get; private set; }
        public float RequiredExperienceForLevelUp { get; private set; }

        private GlobalGameStateMachine _stateMachine;
        private Entity _playerAsEntity;

        [Inject]
        public void Construct(GlobalGameStateMachine stateMachine)
        {
            _stateMachine = stateMachine;
            _playerAsEntity = GetComponent<Entity>();
        }
        
        private void Awake()
        {
            RequiredExperienceForLevelUp = _baseRequiredExperienceForLevelUp;
        }

        private void Start()
        {
            _playerAsEntity.OnDeath += HandleDeath;
        }

        private void OnDestroy()
        {
            _playerAsEntity.OnDeath -= HandleDeath;
        }

        /// <summary>Called by PickupEcs.PickupMagnetSystem when an Experience pickup entity reaches the player - крок-10's ECS replacement for the old Experience-vs-Player OnCollisionEnter.</summary>
        public void AwardExperience(float amount)
        {
            if (!_playerAsEntity.IsAlive)
                return;

            Experience += amount;
            while (Experience >= RequiredExperienceForLevelUp)
            {
                Experience -= RequiredExperienceForLevelUp;
                Level += 1;
                RequiredExperienceForLevelUp *= _multiplierFactorOfRequiredExperienceByLevel;

                _playerAsEntity.Health += (_playerAsEntity.MaxHealth * 0.1f);
                _playerAsEntity.Health = Mathf.Clamp(_playerAsEntity.Health, 0, _playerAsEntity.MaxHealth);

                OnLevelUp?.Invoke();
            }
        }

        private void HandleDeath()
        {
            LoadMenuAsync().Forget();
        }

        private async UniTaskVoid LoadMenuAsync()
        {
            await UniTask.WaitForSeconds(4, cancellationToken: this.GetCancellationTokenOnDestroy());
            await _stateMachine.Enter<MenuState>();
        }
    }
}