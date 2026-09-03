using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace Game
{
    public class MenuState : StateBase
    {
        public override UniTask Enter()
        {
            SceneManager.LoadScene(SceneInBuild.MainMenu);
            return UniTask.CompletedTask;
        }

        public override UniTask Exit()
        {
            return UniTask.CompletedTask;
        }
    }
}
