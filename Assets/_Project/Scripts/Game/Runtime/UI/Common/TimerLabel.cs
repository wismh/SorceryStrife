using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Game
{
    public class TimerLabel : MonoBehaviour
    {
        private TextMeshProUGUI _timerLabel;

        private void Awake()
        {
            _timerLabel = GetComponent<TextMeshProUGUI>();
        }

        private void Start()
        {
            UpdateTimerAsync().Forget();
        }

        private async UniTaskVoid UpdateTimerAsync()
        {
            var cancellationToken = this.GetCancellationTokenOnDestroy();

            while (true)
            {
                var time = TimeSpan.FromSeconds(Time.timeSinceLevelLoad);
                _timerLabel.text = time.ToString(@"mm\:ss");
                await UniTask.WaitForSeconds(1, cancellationToken: cancellationToken);
            }
        }
    }
}
