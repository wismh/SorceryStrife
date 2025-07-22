using Unity.Entities;

namespace Game
{
    /// <summary>Wraps the existing Team enum (Mechanics/Combat/Team.cs) as ECS component data.</summary>
    public struct UnitTeam : IComponentData
    {
        public Team Value;
    }
}
