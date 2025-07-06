using Cysharp.Threading.Tasks;

namespace Project.Core.SceneLoaderServiceModule
{
    public interface ISceneLoaderService
    {
        UniTask LoadSceneAsync(int sceneIndex, bool unloadRedundant);
    }
}
