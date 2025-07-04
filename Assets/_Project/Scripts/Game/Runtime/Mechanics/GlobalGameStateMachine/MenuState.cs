using Cysharp.Threading.Tasks;

namespace Game
{
    public class MenuState : StateBase
    {
        private readonly ISceneLoaderService _sceneLoaderService;

        public MenuState(ISceneLoaderService sceneLoaderService)
        {
            _sceneLoaderService = sceneLoaderService;
        }

        public override async UniTask Enter()
        {
            await _sceneLoaderService.LoadSceneAsync(SceneInBuild.MainMenu, unloadRedundant: true);
        }

        public override UniTask Exit()
        {
            return UniTask.CompletedTask;
        }
    }
}
