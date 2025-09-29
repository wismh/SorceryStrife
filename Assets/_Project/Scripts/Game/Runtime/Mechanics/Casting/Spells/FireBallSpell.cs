using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    [CreateAssetMenu(fileName = "FireBallSpell", menuName = "Game/Spells/Fire Ball")]
    public class FireBallSpell : Spell
    {
        [field: SerializeField] public List<float> Damage { get; private set; }
        [field: SerializeField] public List<float> Speed { get; private set; }

        public override IEnumerable<SpellStatDisplay> GetDisplayStats()
        {
            foreach (var stat in base.GetDisplayStats())
                yield return stat;

            yield return new SpellStatDisplay(nameof(Damage), Damage);
            yield return new SpellStatDisplay(nameof(Speed), Speed);
        }
    }
}