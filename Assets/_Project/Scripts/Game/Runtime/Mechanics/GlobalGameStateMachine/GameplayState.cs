using Cysharp.Threading.Tasks;
using EnemyEcs;
using Project.Core.SceneLoaderServiceModule;
using Project.Core.StateMachineModule;
using UnityEngine;

namespace Game
{
    public class GameplayState : StateBase
    {
        private readonly ISceneLoaderService _sceneLoaderService;

        public GameplayState(ISceneLoaderService sceneLoaderService)
        {
            _sceneLoaderService = sceneLoaderService;
        }

        public override async UniTask Enter()
        {
            Time.timeScale = 1f;
            EcsWorldCleanup.CleanUpGameplayEntities();
            await _sceneLoaderService.LoadSceneAsync(SceneInBuild.Gameplay, unloadRedundant: true);
        }

        public override UniTask Exit()
        {
            EcsWorldCleanup.CleanUpGameplayEntities();
            return UniTask.CompletedTask;
        }
    }
}
