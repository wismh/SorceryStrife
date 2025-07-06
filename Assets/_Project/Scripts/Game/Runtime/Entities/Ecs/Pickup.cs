using Unity.Entities;

namespace PickupEcs
{
    /// <summary>Marker tag distinguishing Experience-pickup entities in ECS queries.</summary>
    public struct Pickup : IComponentData
    {
    }
}
