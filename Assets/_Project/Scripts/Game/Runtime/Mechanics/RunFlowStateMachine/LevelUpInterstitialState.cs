using System;
using Cysharp.Threading.Tasks;
using Project.Core.StateMachineModule;
using UnityEngine;

namespace Game
{
    public class LevelUpInterstitialState : StateBase
    {
        public event Action OnEntered;
        public event Action OnExited;

        public override UniTask Enter()
        {
            Time.timeScale = 0.01f;
            OnEntered?.Invoke();
            return UniTask.CompletedTask;
        }

        public override UniTask Exit()
        {
            Time.timeScale = 1f;
            OnExited?.Invoke();
            return UniTask.CompletedTask;
        }
    }
}
