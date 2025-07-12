using System;
using System.Collections.Generic;
using Game.InventorySystem;
using ModestTree;
using Zenject;

namespace Game
{
    public class PlayerInventory
    {
        public event Action<Item> OnAddItem;
        public event Action<Item> OnLevelUpItem;

        private readonly Dictionary<StatType, (float Percent, float Flat)> _cache = new();
        private readonly Dictionary<Item, int> _items = new();
        private readonly ItemsRegister _itemsRegister;

        [Inject]
        public PlayerInventory(ItemsRegister itemsRegister)
        {
            _itemsRegister = itemsRegister;
        }

        public bool IsFull()
        {
            return _items.Count == 3;
        }

        public bool HasItem(Type typeOfItem)
        {
            if (!typeOfItem.DerivesFrom(typeof(Item)))
                return false;

            var item = _itemsRegister.GetItemByType(typeOfItem);
            return _items.ContainsKey(item);
        }

        public void AddOrLevelUpItem(Type typeOfItem)
        {
            if (!typeOfItem.DerivesFrom(typeof(Item)))
                return;

            var item = _itemsRegister.GetItemByType(typeOfItem);

            if (!_items.TryAdd(item, 0))
            {
                _items[item] += 1;
                OnLevelUpItem?.Invoke(item);
            }
            else
                OnAddItem?.Invoke(item);

            _cache.Clear();
        }

        /// <summary>0-based modifier tier, matching Caster.Level. -1 when the item isn't owned.</summary>
        public int GetLevelOfItem(Item item)
        {
            return _items.TryGetValue(item, out var level) ? level : -1;
        }

        /// <summary>Applies every owned item's modifiers for <paramref name="stat"/> to a base value.</summary>
        public float ApplyModifiers(StatType stat, float baseValue)
        {
            if (!_cache.TryGetValue(stat, out var aggregate))
            {
                aggregate = ComputeAggregate(stat);
                _cache.Add(stat, aggregate);
            }

            return baseValue * aggregate.Percent + aggregate.Flat;
        }

        private (float Percent, float Flat) ComputeAggregate(StatType stat)
        {
            var percent = 1f;
            var flat = 0f;

            foreach (var ownedItem in _items)
            {
                foreach (var modifier in ownedItem.Key.Modifiers)
                {
                    if (modifier.Stat != stat)
                        continue;

                    var value = modifier.ValuePerLevel.ValueAtLevel(ownedItem.Value);

                    if (modifier.Op == ModifierOp.Flat)
                        flat += value;
                    else
                        percent += value;
                }
            }

            return (percent, flat);
        }
    }
}
