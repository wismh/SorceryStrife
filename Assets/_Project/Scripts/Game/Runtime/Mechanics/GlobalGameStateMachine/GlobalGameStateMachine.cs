using System.Collections.Generic;
using Project.Core.StateMachineModule;

namespace Game
{
    /// <summary>Which top-level scene is loaded: main menu or a run. See MenuState / GameplayState.</summary>
    public class GlobalGameStateMachine : StateMachineBehaviour<StateBase>
    {
        public GlobalGameStateMachine(MenuState menuState, GameplayState gameplayState)
        {
            SetStates(new List<StateBase> { menuState, gameplayState });
        }
    }
}
