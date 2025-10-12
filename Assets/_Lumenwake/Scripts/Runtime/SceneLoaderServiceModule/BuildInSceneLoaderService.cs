using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace Project.Core.SceneLoaderServiceModule
{
    /// <summary>Wraps SceneManager for Build Settings scenes - the only scene source MiniJam has today.</summary>
    public class BuildInSceneLoaderService : ISceneLoaderService
    {
        public async UniTask LoadSceneAsync(int sceneIndex, bool unloadRedundant)
        {
            LoadSceneMode mode = unloadRedundant ? LoadSceneMode.Single : LoadSceneMode.Additive;
            await SceneManager.LoadSceneAsync(sceneIndex, mode).ToUniTask();
        }
    }
}
