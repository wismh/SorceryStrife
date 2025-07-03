using Zenject;

namespace Game
{
    /// <summary>
    /// Installed on the ProjectContext prefab (Resources/ProjectContext.prefab) - runs once,
    /// before the first scene's own SceneContext, and its bindings are available to every
    /// scene for the life of the process.
    /// </summary>
    public class ProjectContextInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<GlobalGameStateMachine>().AsSingle();
        }
    }
}
