using Cysharp.Threading.Tasks;
using Project.Core.StateMachineModule;
using UnityEngine;

namespace Game
{
    public class LevelUpInterstitialState : StateBase
    {
        public override UniTask Enter()
        {
            Time.timeScale = 0.01f;
            return UniTask.CompletedTask;
        }

        public override UniTask Exit()
        {
            Time.timeScale = 1f;
            return UniTask.CompletedTask;
        }
    }
}
