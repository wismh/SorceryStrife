using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace Game
{
    public class GameplayState : StateBase
    {
        public override UniTask Enter()
        {
            SceneManager.LoadScene(SceneInBuild.Gameplay);
            return UniTask.CompletedTask;
        }

        public override UniTask Exit()
        {
            return UniTask.CompletedTask;
        }
    }
}
