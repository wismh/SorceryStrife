using Unity.Entities;

namespace EnemyEcs
{
    /// <summary>Wraps Game.EnemyType so systems know which VAT archetype and stats an entity uses.</summary>
    public struct EnemyEcsType : IComponentData
    {
        public Game.EnemyType Value;
    }
}
