using UnityEngine.SceneManagement;

namespace Game
{
    public class MenuState : IState
    {
        public void Enter()
        {
            SceneManager.LoadScene(SceneInBuild.MainMenu);
        }

        public void Exit()
        {
        }
    }
}
