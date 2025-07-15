namespace Game
{
    /// <summary>Minimal state machine: one state active at a time, no transition table.</summary>
    public class StateMachine
    {
        public IState Current { get; private set; }

        /// <summary>Records the starting state without calling Enter() - use when already in that state (e.g. the scene it would load is the one already running).</summary>
        public void SetInitial(IState state)
        {
            Current = state;
        }

        public void Enter(IState state)
        {
            Current?.Exit();
            Current = state;
            state.Enter();
        }
    }
}
