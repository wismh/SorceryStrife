using System;
using UnityEngine;

namespace Game
{
    public abstract class Caster
    {
        protected Caster(Spell spell, PlayerInventory inventory)
        {
            Spell = spell;
            PlayerInventory = inventory;
        }
        
        public event Action OnCast;
        public Spell Spell { get; private set; }
        public PlayerInventory PlayerInventory { get; set; }
        public int Level { get; set; }
        
        public float Cooldown => PlayerInventory.ApplyModifiers(StatType.Cooldown, Spell.Cooldown.ValueAtLevel(Level));
        
        protected abstract void CastInternal(Transform caster);

        public void Cast(Transform caster)
        {
            CastInternal(caster);
            OnCast?.Invoke();
        }
    }
}