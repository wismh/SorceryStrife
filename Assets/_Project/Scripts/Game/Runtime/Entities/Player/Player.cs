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

        private PoolOfObject<Experience> _experiencePool;
        private GlobalGameStateMachine _stateMachine;
        private Entity _playerAsEntity;

        [Inject]
        public void Construct(PoolOfObject<Experience> experiencePool, GlobalGameStateMachine stateMachine)
        {
            _experiencePool = experiencePool;
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

        private void OnCollisionEnter(Collision other)
        {
            if (!_playerAsEntity.IsAlive)
                return;
            
            if (!other.transform.TryGetComponent(out Experience experience))
                return;
            
            _experiencePool.Destroy(experience);
            
            Experience += 1;
            if (Experience < RequiredExperienceForLevelUp) 
                return;
            
            Level += 1;
            Experience = 0;
            RequiredExperienceForLevelUp *= _multiplierFactorOfRequiredExperienceByLevel;

            _playerAsEntity.Health += (_playerAsEntity.MaxHealth * 0.1f);
            _playerAsEntity.Health = Mathf.Clamp(_playerAsEntity.Health, 0, _playerAsEntity.MaxHealth);
            
            OnLevelUp?.Invoke();
        }

        private void HandleDeath()
        {
            LoadMenuAsync().Forget();
        }

        private async UniTaskVoid LoadMenuAsync()
        {
            await UniTask.WaitForSeconds(4, cancellationToken: this.GetCancellationTokenOnDestroy());
            _stateMachine.Enter(new MenuState());
        }
    }
}