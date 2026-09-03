using Cysharp.Threading.Tasks;

namespace Game
{
    public abstract class StateBase
    {
        public abstract UniTask Enter();

        public abstract UniTask Exit();
    }
}
