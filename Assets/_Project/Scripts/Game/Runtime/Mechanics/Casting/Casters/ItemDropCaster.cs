using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace Game
{
    [SpellCaster(SpellType = typeof(ItemDropSpell))]
    public class ItemDropCaster : Caster
    {
        private readonly IScreenManager _screenManager;

        [Inject]
        public ItemDropCaster(ItemDropSpell spell, PlayerInventory inventory, IScreenManager screenManager) :
            base(spell, inventory)
        {
            _screenManager = screenManager;
        }


        // ReSharper disable Unity.PerformanceAnalysis
        protected override void CastInternal(Transform caster)
        {
            OpenAsync().Forget();
        }

        private async UniTaskVoid OpenAsync()
        {
            await _screenManager.OpenScreen<ItemSelectionScreen>();
        }
    }
}
