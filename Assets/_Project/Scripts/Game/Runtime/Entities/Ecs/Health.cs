using Unity.Entities;

namespace Game
{
    public struct Health : IComponentData
    {
        public float Value;
        public float Max;
    }
}
