using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;

namespace EnemyEcs
{
    /// <summary>
    /// Overrides the per-instance _AnimParams property in the VAT shader.
    /// x: StartFrame, y: FrameCount, z: AnimTime, w: FPS.
    /// </summary>
    [MaterialProperty("_AnimParams")]
    public struct EnemyVatAnimParams : IComponentData
    {
        public float4 Value;
    }

    /// <summary>
    /// Runtime state for VAT animation playback.
    /// </summary>
    public struct EnemyVatPlayback : IComponentData
    {
        public float Time;
        public float Fps;
        public int WalkStart;
        public int WalkCount;
        public int AttackStart;
        public int AttackCount;
        public int DeathStart;
        public int DeathCount;
    }
}
