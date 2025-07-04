using System;
using Cysharp.Threading.Tasks;

namespace Game
{
    public interface IScreenManager
    {
        UniTask<Result> OpenScreen(Type type);
        UniTask<Result> OpenScreen<T>() where T : BaseScreen;
        UniTask CloseScreen<T>() where T : BaseScreen;
        UniTask CloseAllScreens();
    }
}
