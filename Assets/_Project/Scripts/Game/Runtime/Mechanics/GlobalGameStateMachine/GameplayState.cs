using Cysharp.Threading.Tasks;

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
            await _sceneLoaderService.LoadSceneAsync(SceneInBuild.Gameplay, unloadRedundant: true);
        }

        public override UniTask Exit()
        {
            return UniTask.CompletedTask;
        }
    }
}
