using Cysharp.Threading.Tasks;

namespace Game
{
    public interface ISceneLoaderService
    {
        UniTask LoadSceneAsync(int sceneIndex, bool unloadRedundant);
    }
}
