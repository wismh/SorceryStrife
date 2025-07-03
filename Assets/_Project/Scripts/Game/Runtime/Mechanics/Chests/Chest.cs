using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace Game
{
    public class Chest : MonoBehaviour
    {
        [SerializeField] private float _animationDuration;

        private ItemSelectionScreen _itemSelectionScreen;
        private Animator _animator;

        [Inject]
        public void Construct(ItemSelectionScreen itemSelectionScreen)
        {
            _itemSelectionScreen = itemSelectionScreen;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.transform.TryGetComponent(out Player player))
                return;

            StartDelayAsync().Forget();
        }

        private async UniTaskVoid StartDelayAsync()
        {
            await UniTask.WaitForSeconds(_animationDuration, cancellationToken: this.GetCancellationTokenOnDestroy());
            _itemSelectionScreen.Show();
            Destroy(gameObject);
        }
    }
}
