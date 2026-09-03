using System.Collections.Generic;

namespace Game
{
    public class ComponentRegistry<T> : IComponentRegistry<T>
    {
        private readonly List<T> _items = new();

        public IReadOnlyList<T> Items => _items;

        public void Add(T item)
        {
            _items.Add(item);
        }

        public void Remove(T item)
        {
            _items.Remove(item);
        }

        public void Clear()
        {
            _items.Clear();
        }
    }
}
