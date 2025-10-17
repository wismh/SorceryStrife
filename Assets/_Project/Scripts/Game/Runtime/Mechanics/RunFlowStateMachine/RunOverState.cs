using Cysharp.Threading.Tasks;
using Project.Core.StateMachineModule;
using UnityEngine;

namespace Game
{
    public class RunOverState : StateBase
    {
        private readonly GlobalGameStateMachine _globalGameStateMachine;

        public RunOverState(GlobalGameStateMachine globalGameStateMachine)
        {
            _globalGameStateMachine = globalGameStateMachine;
        }

        public override async UniTask Enter()
        {
            Time.timeScale = 1f;
            await UniTask.WaitForSeconds(4f, ignoreTimeScale: true);
            await _globalGameStateMachine.Enter<MenuState>();
        }

        public override UniTask Exit()
        {
            return UniTask.CompletedTask;
        }
    }
}
