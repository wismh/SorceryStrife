using Lumenwake;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Lumenwake.UIModule
{
    public class BaseScreen : MonoBehaviour
    {
        private UniTask<Result> _loadingTask;

        public UniTask<Result> LoadScreen()
        {
            if (_loadingTask.Status == UniTaskStatus.Pending)
                return _loadingTask;

            return _loadingTask = LoadInternal();
        }

        protected virtual UniTask<Result> LoadInternal() =>
            UniTask.FromResult(Result.Success);

        public virtual void OnOpen() {}

        public virtual UniTask OnOpenAsync()
        {
            OnOpen();
            return UniTask.CompletedTask;
        }

        public virtual UniTask OnClose() =>
            UniTask.CompletedTask;
    }
}
