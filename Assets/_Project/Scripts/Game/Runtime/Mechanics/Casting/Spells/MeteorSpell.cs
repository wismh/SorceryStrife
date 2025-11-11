using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    [CreateAssetMenu(fileName = "MeteorSpell", menuName = "Game/Spells/Meteor Spell")]
    public class MeteorSpell : Spell
    {
        [field: SerializeField] public MeteorProjectile ProjectilePrefab { get; private set; }
        [field: SerializeField] public Transform SightPrefab { get; private set; }
        [field: SerializeField] public ExplosionProjectile ExplosionPrefab { get; private set; }
        
        [field: SerializeField] public float Delay { get; private set; }
        [field: SerializeField] public float Duration { get; private set; }
        [field: SerializeField] public Vector2 Range { get; private set; }
        [field: SerializeField] public List<float> Damage { get; private set; }
        [field: SerializeField] public List<float> Projectiles { get; private set; }
        [field: SerializeField] public List<float> Radius { get; private set; }

        public override IEnumerable<SpellStatDisplay> GetDisplayStats()
        {
            foreach (var stat in base.GetDisplayStats())
            {
                yield return stat;
            }

            yield return new SpellStatDisplay(nameof(Damage), Damage);
            yield return new SpellStatDisplay(nameof(Projectiles), Projectiles);
            yield return new SpellStatDisplay(nameof(Radius), Radius);
        }
    }
}