using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Game
{
    public class TempObject : MonoBehaviour
    {
        [SerializeField] private float _timeOfLife;

        private CancellationTokenSource _timerCancellation;

        public float TimeOfLife
        {
            get => _timeOfLife;
            set
            {
                _timeOfLife = value;
                RestartTimer();
            }
        }

        private void Start()
        {
            if (_timeOfLife != 0)
                RestartTimer();
        }

        private void RestartTimer()
        {
            _timerCancellation?.Cancel();
            _timerCancellation = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());

            StartTimerAsync(_timeOfLife, _timerCancellation.Token).Forget();
        }

        private async UniTaskVoid StartTimerAsync(float duration, CancellationToken cancellationToken)
        {
            await UniTask.WaitForSeconds(duration, cancellationToken: cancellationToken);
            Destroy(gameObject);
        }
    }
}
