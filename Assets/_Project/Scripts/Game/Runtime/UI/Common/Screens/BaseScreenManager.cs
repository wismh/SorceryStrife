using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;

namespace Game
{
    public class BaseScreenManager : IScreenManager
    {
        private Dictionary<Type, BaseScreen> _allScreens = new();
        private HashSet<BaseScreen> _openingScreens = new();

        private List<BaseScreen> _stackScreens = new();

        public BaseScreenManager(List<BaseScreen> allScreens)
        {
            foreach (var screen in allScreens)
                _allScreens.Add(screen.GetType(), screen);
        }

        public async UniTask<Result> OpenScreen(Type type)
        {
            if (!_allScreens.TryGetValue(type, out BaseScreen screen))
                return Result.Failure;

            if (!_openingScreens.Add(screen))
                return Result.Success;

            try
            {
                var result = await screen.LoadScreen();
                if (result == Result.Failure)
                    return result;

                if (_stackScreens.Count > 0)
                    _stackScreens[^1].gameObject.SetActive(false);

                int index = _stackScreens.IndexOf(screen);
                if (index != -1)
                    _stackScreens.RemoveAt(index);

                _stackScreens.Add(screen);

                _stackScreens[^1].gameObject.SetActive(true);
                await screen.OnOpenAsync();

                return result;
            }
            finally
            {
                _openingScreens.Remove(screen);
            }
        }

        public UniTask<Result> OpenScreen<T>() where T : BaseScreen
        {
            return OpenScreen(typeof(T));
        }

        public async UniTask CloseScreen<T>() where T : BaseScreen
        {
            var screen = _allScreens[typeof(T)];

            if (!_stackScreens.Contains(screen))
                return;

            _stackScreens.Remove(screen);
            await screen.OnClose();

            screen.gameObject.SetActive(false);
            if (_stackScreens.Count > 0)
                _stackScreens[^1].gameObject.SetActive(true);
        }

        public async UniTask CloseAllScreens()
        {
            var screens = _stackScreens.ToList();
            _stackScreens.Clear();

            var tasks = screens
                .Select(async screen =>
                {
                    await screen.OnClose();
                    screen.gameObject.SetActive(false);
                })
                .ToArray();

            await UniTask.WhenAll(tasks);
        }

        public bool HasScreenInStack<T>() where T : BaseScreen
        {
            foreach (BaseScreen screen in _stackScreens)
            {
                if (screen is T)
                    return true;
            }

            return false;
        }
    }
}
