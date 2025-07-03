using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Game
{
    public class FriendDialogs : MonoBehaviour
    {
        [SerializeField] private List<string> _texts;
        [SerializeField] private TextMeshProUGUI _textLabel;
        [SerializeField] private Transform _canvas;
        [SerializeField] private float _interval;

        private Camera _camera;

        private void Start()
        {
            _camera = Camera.main;
            StartDialogsAsync().Forget();
        }

        private void Update()
        {
            _canvas.transform.forward = _camera.transform.forward;
        }

        private async UniTaskVoid StartDialogsAsync()
        {
            var cancellationToken = this.GetCancellationTokenOnDestroy();

            await UniTask.WaitForSeconds(3, cancellationToken: cancellationToken);

            while (true)
            {
                _textLabel.text = _texts[Random.Range(0, _texts.Count)];

                _textLabel.alpha = 0;
                DOTween.To(() => _textLabel.alpha, value => _textLabel.alpha = value, 1f, 1f).OnComplete(() =>
                {
                    DOTween.To(() => _textLabel.alpha, value => _textLabel.alpha = value, 0f, 1f).SetDelay(3f);
                });

                await UniTask.WaitForSeconds(_interval, cancellationToken: cancellationToken);
            }
        }
    }
}
