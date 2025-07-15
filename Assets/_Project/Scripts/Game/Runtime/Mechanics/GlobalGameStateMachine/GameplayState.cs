using UnityEngine.SceneManagement;

namespace Game
{
    public class GameplayState : IState
    {
        public void Enter()
        {
            SceneManager.LoadScene(SceneInBuild.Gameplay);
        }

        public void Exit()
        {
        }
    }
}
