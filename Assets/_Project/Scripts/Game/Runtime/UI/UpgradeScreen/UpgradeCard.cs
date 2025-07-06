using System;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

namespace Game
{
    public class UpgradeCard : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private Image _iconImage;
        [SerializeField] private TextMeshProUGUI _titleLabel;
        [SerializeField] private TextMeshProUGUI _levelLabel;
        [SerializeField] private TextMeshProUGUI _descriptionLabel;
        
        [SerializeField] private TextMeshProUGUI _namesLabel;
        [SerializeField] private TextMeshProUGUI _valuesLabel;

        public event Action OnSelect;
        
        private string k_levelPattern;
        private PlayerCaster _playerCaster;
        private Spell _spell;

        [Inject]
        public void Construct(Player player)
        {
            _playerCaster = player.GetComponent<PlayerCaster>();
        }
        
        public void SetSpell(Spell spell)
        {
            _spell = spell;
            
            var spellCaster = _playerCaster.GetCasterOfSpell(spell.GetType());
            var currentTier = spellCaster?.Level ?? -1;
            var spellLevel = currentTier + 2;
            
            _iconImage.sprite = spell.Icon;
            _titleLabel.text = spell.Title;
            _levelLabel.text = k_levelPattern.Replace("{}", spellLevel.ToString());
            _descriptionLabel.text = spell.Description;

            var namesBuilder = new StringBuilder();
            var valuesBuilder = new StringBuilder("<mspace=0.54em>");

            foreach (var stat in spell.GetDisplayStats())
            {
                StatDisplayFormatter.AppendSpellStatRow(namesBuilder, valuesBuilder, stat.Name, stat.ValuePerLevel, currentTier);
            }

            _namesLabel.text = namesBuilder.ToString();
            _valuesLabel.text = valuesBuilder.ToString();
        }
        
        private void Awake()
        {
            k_levelPattern = _levelLabel.text;
        }
        
        public void OnPointerClick(PointerEventData eventData)
        {
            var spellType = _spell.GetType();
            var spellCaster = _playerCaster.GetCasterOfSpell(spellType);
            
            if (spellCaster != null)
                spellCaster.Level += 1;
            else 
                _playerCaster.AddSpell(spellType);
            
            OnSelect?.Invoke();
        }
    }
}