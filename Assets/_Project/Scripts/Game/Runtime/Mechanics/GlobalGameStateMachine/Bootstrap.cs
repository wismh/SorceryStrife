using UnityEngine;

namespace Game
{
    /// <summary>
    /// Runs once before the first scene loads and owns the GlobalGameStateMachine for
    /// the rest of the process. Stand-in for a real Zenject ProjectContext (migration
    /// plan step 4) - a ProjectContext prefab and a MainMenu-scene SceneContext both need
    /// authoring in the Editor, so this uses Unity's RuntimeInitializeOnLoadMethod
    /// instead: no scene or prefab to hand-edit, same "runs before anything else, survives
    /// every scene load" guarantee. Swap this for a real ProjectContextInstaller once
    /// that Editor setup is done; every call site already goes through
    /// Bootstrap.StateMachine rather than a raw SceneManager.LoadScene, so the swap is
    /// localized to this file.
    /// </summary>
    public static class Bootstrap
    {
        public static GlobalGameStateMachine StateMachine { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            StateMachine = new GlobalGameStateMachine();
            StateMachine.SetInitial(new MenuState());
        }
    }
}
