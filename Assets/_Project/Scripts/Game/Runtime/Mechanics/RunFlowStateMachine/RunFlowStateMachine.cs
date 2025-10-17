using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Project.Core.StateMachineModule;
using Zenject;

namespace Game
{
    /// <summary>
    /// Governs the session gameplay loop: RunPrepareState -> WaveState <-> LevelUpInterstitialState -> RunOverState.
    /// </summary>
    public class RunFlowStateMachine : StateMachineBehaviour<StateBase>, IInitializable
    {
        public RunFlowStateMachine(
            RunPrepareState runPrepareState,
            WaveState waveState,
            LevelUpInterstitialState levelUpInterstitialState,
            RunOverState runOverState)
        {
            SetStates(new List<StateBase>
            {
                runPrepareState,
                waveState,
                levelUpInterstitialState,
                runOverState
            });
        }

        public void Initialize()
        {
            StartFlowAsync().Forget();
        }

        private async UniTaskVoid StartFlowAsync()
        {
            await Enter<RunPrepareState>();
            await Enter<WaveState>();
        }
    }
}
