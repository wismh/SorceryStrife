using Cysharp.Threading.Tasks;

namespace Game
{
    public abstract class PayLoadedStateBase<TPayload> : StateBase
    {
        public virtual UniTask Enter(TPayload payload)
        {
            return Enter();
        }
    }
}
