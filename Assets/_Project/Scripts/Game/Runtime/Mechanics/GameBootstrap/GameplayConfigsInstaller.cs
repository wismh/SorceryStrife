using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Game
{
    public class GameplayConfigsInstaller : MonoInstaller
    {
        [SerializeField] private List<Spell> _spells;
        [SerializeField] private List<Item> _items;

        public override void InstallBindings()
        {
            Container.Bind<List<Spell>>().FromInstance(_spells).AsSingle();
            Container.Bind<List<Item>>().FromInstance(_items).AsSingle();
        }
    }
}
