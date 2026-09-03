using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Game
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] private Button _startGameButton;
        [SerializeField] private Button _exitGameButton;

        private GlobalGameStateMachine _stateMachine;

        [Inject]
        public void Construct(GlobalGameStateMachine stateMachine)
        {
            _stateMachine = stateMachine;
        }

        private void Start()
        {
            _startGameButton.onClick.AddListener(HandleStartGame);
            _exitGameButton.onClick.AddListener(HandleExitGame);
        }

        private void OnDestroy()
        {
            _startGameButton.onClick.RemoveAllListeners();
            _exitGameButton.onClick.RemoveAllListeners();
        }

        private void HandleStartGame()
        {
            EnterGameplayAsync().Forget();
        }

        private async UniTaskVoid EnterGameplayAsync()
        {
            await _stateMachine.Enter<GameplayState>();
        }

        private static void HandleExitGame()
        {
            Application.Quit();
        }
    }
}
