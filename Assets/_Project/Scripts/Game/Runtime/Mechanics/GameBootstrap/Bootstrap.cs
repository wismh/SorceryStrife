using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace Game
{
    /// <summary>
    /// Lives only in BootstrapScene (Build Settings index 0) - immediately hands off to
    /// MainMenu via GlobalGameStateMachine. Kept scene-content-free so index 0 never needs
    /// game logic of its own, matching the Lumenwake template's Bootstrap -> MainScene handoff.
    /// </summary>
    public class Bootstrap : MonoBehaviour
    {
        [SerializeField] private int _shortDelayBeforeLoadMenu = 2500;

        private GlobalGameStateMachine _stateMachine;

        [Inject]
        public void Construct(GlobalGameStateMachine stateMachine)
        {
            _stateMachine = stateMachine;
        }

        private void Awake()
        {
            Application.targetFrameRate = 60;
        }

        private void Start()
        {
            EnterMenuAsync().Forget();
        }

        private async UniTaskVoid EnterMenuAsync()
        {
            if (_shortDelayBeforeLoadMenu > 0)
            {
                await UniTask.Delay(_shortDelayBeforeLoadMenu);
            }

            await _stateMachine.Enter<MenuState>();
        }
    }
}
