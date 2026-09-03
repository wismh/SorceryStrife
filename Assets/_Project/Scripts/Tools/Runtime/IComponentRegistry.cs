using System.Collections.Generic;

namespace Game
{
    public interface IComponentRegistry<T>
    {
        IReadOnlyList<T> Items { get; }

        void Add(T item);
        void Remove(T item);
        void Clear();
    }
}
