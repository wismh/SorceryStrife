using Unity.Entities;

namespace EnemyEcs
{
    /// <summary>Wraps Game.EnemyType so EnemyCompanionAssignmentSystem knows which companion pool an entity uses.</summary>
    public struct EnemyEcsType : IComponentData
    {
        public Game.EnemyType Value;
    }
}
