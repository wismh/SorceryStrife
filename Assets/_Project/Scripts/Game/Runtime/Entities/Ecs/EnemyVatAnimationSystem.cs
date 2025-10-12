using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace EnemyEcs
{
    /// <summary>
    /// Updates VAT animation parameters every frame and pushes them to EnemyVatAnimParams
    /// for GPU instancing by Entities Graphics.
    /// </summary>
    [UpdateAfter(typeof(EnemyAttackSystem))]
    public partial class EnemyVatAnimationSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            float deltaTime = SystemAPI.Time.DeltaTime;

            new VatAnimationJob
            {
                DeltaTime = deltaTime,
            }.ScheduleParallel();
        }
    }

    [BurstCompile]
    internal partial struct VatAnimationJob : IJobEntity
    {
        public float DeltaTime;

        private void Execute(
            ref EnemyVatPlayback playback,
            ref EnemyVatAnimParams animParams,
            in AttackState attackState,
            in Game.Health health)
        {
            if (health.Value <= 0f)
                return;

            playback.Time += DeltaTime;

            float startFrame;
            float frameCount;

            if (attackState.Phase == AttackPhase.WindingUp)
            {
                startFrame = playback.AttackStart;
                frameCount = math.max(playback.AttackCount, 1);
            }
            else
            {
                startFrame = playback.WalkStart;
                frameCount = math.max(playback.WalkCount, 1);
            }

            animParams.Value = new float4(
                startFrame,
                frameCount,
                playback.Time,
                playback.Fps);
        }
    }
}
