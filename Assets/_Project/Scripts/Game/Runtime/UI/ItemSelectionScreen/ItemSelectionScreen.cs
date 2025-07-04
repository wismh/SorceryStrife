using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace Game
{
    [DefaultExecutionOrder(-1)]
    public class ItemSelectionScreen : BaseScreen
    {
        [SerializeField] private List<ItemCard> _itemsCards;

        private List<Item> _items;

        private PlayerInventory _inventory;
        private IAssetLoaderService _assetLoaderService;
        private IScreenManager _screenManager;

        [Inject]
        public void Construct(PlayerInventory inventory, IAssetLoaderService assetLoaderService, IScreenManager screenManager)
        {
            _inventory = inventory;
            _assetLoaderService = assetLoaderService;
            _screenManager = screenManager;
        }

        private void Awake()
        {
            _items = _assetLoaderService.LoadAllAssets<Item>("Items").ToList();
        }

        private void Start()
        {
            foreach (var card in _itemsCards)
                card.OnSelect += HandleCardSelected;

            Time.timeScale = 1;
            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            foreach (var card in _itemsCards)
                card.OnSelect -= HandleCardSelected;
        }

        private void HandleCardSelected()
        {
            _screenManager.CloseScreen<ItemSelectionScreen>().Forget();
        }

        public override void OnOpen()
        {
            var possibleItems = _items.ToList();
            foreach (
                var item in from item in _items
                where (item != null && _inventory.GetLevelOfItem(item) >= item.MaxLevel) ||
                      (item != null && _inventory.IsFull() && !_inventory.HasItem(item.GetType()))
                select item
            )
            {
                possibleItems.Remove(item);
            }

            if (possibleItems.Count == 0)
            {
                _screenManager.CloseScreen<ItemSelectionScreen>().Forget();
                return;
            }

            Time.timeScale = 0.01f;

            var numberOfVariants = possibleItems.Count >= 3 ? 3 : possibleItems.Count;
            for (var i = 0; i < numberOfVariants; ++i)
            {
                var randomItem = possibleItems[Random.Range(0, possibleItems.Count)];
                possibleItems.Remove(randomItem);

                SetCard(i, randomItem);
            }
        }

        public override UniTask OnClose()
        {
            Time.timeScale = 1;

            foreach (var card in _itemsCards)
                card.gameObject.SetActive(false);

            return UniTask.CompletedTask;
        }

        private void SetCard(int indexOfSelection, Item spell)
        {
            _itemsCards[indexOfSelection].gameObject.SetActive(true);
            _itemsCards[indexOfSelection].SetItem(spell);
        }
    }
}
