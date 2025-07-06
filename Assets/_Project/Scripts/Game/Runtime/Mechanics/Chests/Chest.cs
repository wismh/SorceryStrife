using Cysharp.Threading.Tasks;
using Lumenwake.UIModule;
using UnityEngine;
using Zenject;

namespace Game
{
    public class Chest : MonoBehaviour
    {
        [SerializeField] private float _animationDuration;

        private IScreenManager _screenManager;
        private bool _isOpened;

        [Inject]
        public void Construct(IScreenManager screenManager)
        {
            _screenManager = screenManager;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_isOpened)
                return;

            if (!other.transform.TryGetComponent(out Player player))
                return;

            _isOpened = true;
            StartDelayAsync().Forget();
        }

        private async UniTaskVoid StartDelayAsync()
        {
            await UniTask.WaitForSeconds(_animationDuration, cancellationToken: this.GetCancellationTokenOnDestroy());
            await _screenManager.OpenScreen<ItemSelectionScreen>();
            Destroy(gameObject);
        }
    }
}
