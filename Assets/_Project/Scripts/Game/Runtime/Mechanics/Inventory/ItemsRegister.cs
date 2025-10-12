using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.InventorySystem
{
    public class ItemsRegister
    {
        private readonly Dictionary<Type, Item> _items;

        public ItemsRegister(List<Item> items)
        {
            _items = items.ToDictionary(item => item.GetType(), item => item);
        }

        public Item GetItemByType(Type type)
        {
            return _items.GetValueOrDefault(type, null);
        }
    }
}