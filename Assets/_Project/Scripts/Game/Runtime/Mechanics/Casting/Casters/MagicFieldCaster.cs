using UnityEngine;
using Zenject;

namespace Game
{
    [SpellCaster(SpellType = typeof(MagicFieldSpell))]
    public class MagicFieldCaster : Caster
    {
        public float Duration => _spell.Duration.ValueAtLevel(Level);
        public float Radius => PlayerInventory.ApplyModifiers(StatType.Radius, _spell.Radius.ValueAtLevel(Level));
        public float Damage => PlayerInventory.ApplyModifiers(StatType.Damage, _spell.Damage.ValueAtLevel(Level));

        private readonly MagicFieldSpell _spell;
        private readonly DiContainer _container;
        
        [Inject]
        public MagicFieldCaster(DiContainer container, PlayerInventory inventory, MagicFieldSpell spell):
            base(spell, inventory)
        {
            _container = container;
            _spell = spell;
        }
        
        protected override void CastInternal(Transform caster)
        {
            var clone = _container.InstantiatePrefabForComponent<MagicFieldProjectile>(_spell.ProjectilePrefab, caster);
            clone.Construct(this);
            clone.transform.position = caster.position;
        }
    }
}