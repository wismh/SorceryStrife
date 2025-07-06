using System;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

namespace Game
{
    public class ItemCard : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private Image _iconImage;
        [SerializeField] private TextMeshProUGUI _titleLabel;
        [SerializeField] private TextMeshProUGUI _levelLabel;
        [SerializeField] private TextMeshProUGUI _descriptionLabel;
        
        [SerializeField] private TextMeshProUGUI _namesLabel;
        [SerializeField] private TextMeshProUGUI _valuesLabel;
        
        public event Action OnSelect;
        
        private string k_levelPattern;
        
        private Item _item;
        private PlayerInventory _inventory;
        
        [Inject]
        public void Construct(PlayerInventory inventory)
        {
            _inventory = inventory;
        }

        public void SetItem(Item item)
        {
            _item = item;
            
            var currentTier = _inventory.GetLevelOfItem(item);
            var itemLevel = currentTier + 2;

            _iconImage.sprite = item.Icon;
            _titleLabel.text = item.Title;
            _levelLabel.text = k_levelPattern.Replace("{}", itemLevel.ToString());
            _descriptionLabel.text = item.Description;
            
            var namesBuilder = new StringBuilder();
            var valuesBuilder = new StringBuilder("<mspace=0.54em>");

            foreach (var modifier in item.Modifiers)
            {
                StatDisplayFormatter.AppendItemModifierRow(
                    namesBuilder,
                    valuesBuilder,
                    modifier.Stat,
                    modifier.Op,
                    modifier.ValuePerLevel,
                    currentTier);
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
            _inventory.AddOrLevelUpItem(_item.GetType());
            OnSelect?.Invoke();
        }
    }
}