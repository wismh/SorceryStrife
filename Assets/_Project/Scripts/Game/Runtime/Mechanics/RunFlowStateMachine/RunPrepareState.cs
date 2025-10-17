using Cysharp.Threading.Tasks;
using Project.Core.StateMachineModule;
using UnityEngine;

namespace Game
{
    public class RunPrepareState : StateBase
    {
        public override UniTask Enter()
        {
            Time.timeScale = 1f;
            return UniTask.CompletedTask;
        }

        public override UniTask Exit()
        {
            return UniTask.CompletedTask;
        }
    }
}
