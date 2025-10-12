using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Lumenwake.UIModule;
using UnityEngine;
using Zenject;

namespace Game
{
    public class UpgradeScreen : BaseScreen
    {
        [SerializeField] private List<UpgradeCard> _upgradeCards;

        private List<Spell> _spells;

        private CastersRegister _castersRegister;
        private PlayerCaster _playerCaster;
        private Player _player;
        private IScreenManager _screenManager;

        [Inject]
        public void Construct(
            Player player,
            CastersRegister castersRegister,
            List<Spell> spells,
            IScreenManager screenManager)
        {
            _player = player;
            _castersRegister = castersRegister;
            _playerCaster = player.GetComponent<PlayerCaster>();
            _spells = spells;
            _screenManager = screenManager;
        }

        private void Start()
        {
            _player.OnLevelUp += HandleLevelUp;
            foreach (var card in _upgradeCards)
                card.OnSelect += HandleCardSelected;

            HandleLevelUp();
        }

        private void OnDestroy()
        {
            _player.OnLevelUp -= HandleLevelUp;
            foreach (var card in _upgradeCards)
                card.OnSelect -= HandleCardSelected;
        }

        private void HandleLevelUp()
        {
            _screenManager.OpenScreen<UpgradeScreen>().Forget();
        }

        private void HandleCardSelected()
        {
            _screenManager.CloseScreen<UpgradeScreen>().Forget();
        }

        public override void OnOpen()
        {
            var possibleSpells = _spells.ToList();

            foreach (
                var spell in from spell in _spells
                //where _player.Level != 0 || (_player.Level == 0 && !spell.CanOnFirstLevel)
                let caster = _playerCaster.GetCasterOfSpell(spell.GetType())
                where (caster != null && caster.Level >= spell.MaxLevel) ||
                      (_playerCaster.IsFull() && !_playerCaster.HasSpell(spell.GetType()))
                select spell
            )
            {
                possibleSpells.Remove(spell);
            }

            if (possibleSpells.Count == 0)
            {
                _screenManager.CloseScreen<UpgradeScreen>().Forget();
                return;
            }

            Time.timeScale = 0.01f;

            var numberOfVariants = possibleSpells.Count >= 3 ? 3 : possibleSpells.Count;
            for (var i = 0; i < numberOfVariants; ++i)
            {
                var randomSpell = possibleSpells[Random.Range(0, possibleSpells.Count)];
                possibleSpells.Remove(randomSpell);

                SetUpgradeSpell(i, randomSpell);
            }
        }

        public override UniTask OnClose()
        {
            Time.timeScale = 1;

            foreach (var card in _upgradeCards)
                card.gameObject.SetActive(false);

            return UniTask.CompletedTask;
        }

        private void SetUpgradeSpell(int indexOfSelection, Spell spell)
        {
            _upgradeCards[indexOfSelection].gameObject.SetActive(true);
            _upgradeCards[indexOfSelection].SetSpell(spell);
        }
    }
}
