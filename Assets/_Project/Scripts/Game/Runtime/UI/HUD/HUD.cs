using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using Zenject;

namespace Game
{
    public class HUD : BaseScreen
    {
        [SerializeField] private ValueBar _healthBar;
        [SerializeField] private ValueBar _experienceBar;
        [SerializeField] private TextMeshProUGUI _levelLabel;

        private Player _player;
        private Entity _playerAsEntity;
        private IScreenManager _screenManager;
        private string _levelPattern;

        [Inject]
        public void Construct(Player player, IScreenManager screenManager)
        {
            _player = player;
            _playerAsEntity = _player.GetComponent<Entity>();
            _screenManager = screenManager;
        }

        private void Awake()
        {
            _levelPattern = _levelLabel.text;
            UpdateLevelLabel();
        }

        private void Start()
        {
            OpenAsync().Forget();
            _player.OnLevelUp += UpdateLevelLabel;
        }

        private async UniTaskVoid OpenAsync()
        {
            await _screenManager.OpenScreen<HUD>();
        }

        private void OnDestroy()
        {
            _player.OnLevelUp -= UpdateLevelLabel;
        }

        private void UpdateLevelLabel()
        {
            _levelLabel.text = _levelPattern.Replace("{}", (_player.Level + 1).ToString());
        }

        private void Update()
        {
            _healthBar.Value = _playerAsEntity.Health / _playerAsEntity.MaxHealth;
            _experienceBar.Value = _player.Experience / _player.RequiredExperienceForLevelUp;
        }
    }
}