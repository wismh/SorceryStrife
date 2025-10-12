using System;
using Lumenwake.UIModule;
using TMPro;
using UnityEngine;
using Zenject;

namespace Game
{
    /// <summary>
    /// Always-on base layer, not a stack member - never call OpenScreen/CloseScreen on it. It stays
    /// active by its own default scene state; UpgradeScreen/ItemSelectionScreen show as overlays on
    /// top of it via BaseScreenManager without ever touching HUD's active state.
    /// </summary>
    public class HUD : BaseScreen
    {
        [SerializeField] private ValueBar _healthBar;
        [SerializeField] private ValueBar _experienceBar;
        [SerializeField] private TextMeshProUGUI _levelLabel;

        private Player _player;
        private Entity _playerAsEntity;
        private string _levelPattern;

        [Inject]
        public void Construct(Player player)
        {
            _player = player;
            _playerAsEntity = _player.GetComponent<Entity>();
        }

        private void Awake()
        {
            _levelPattern = _levelLabel.text;
            UpdateLevelLabel();
        }

        private void Start()
        {
            _player.OnLevelUp += UpdateLevelLabel;
        }

        private void OnDestroy()
        {
            _player.OnLevelUp -= UpdateLevelLabel;
        }

        private void UpdateLevelLabel()
        {
            var levelNumber = _player.Level + 1;
            _levelLabel.text = !string.IsNullOrEmpty(_levelPattern) && _levelPattern.Contains("{}")
                ? _levelPattern.Replace("{}", levelNumber.ToString())
                : $"Level {levelNumber}";
        }

        private void Update()
        {
            if (_playerAsEntity.MaxHealth > 0)
                _healthBar.Value = _playerAsEntity.Health / _playerAsEntity.MaxHealth;

            if (_player.RequiredExperienceForLevelUp > 0)
                _experienceBar.Value = _player.Experience / _player.RequiredExperienceForLevelUp;
        }
    }
}
