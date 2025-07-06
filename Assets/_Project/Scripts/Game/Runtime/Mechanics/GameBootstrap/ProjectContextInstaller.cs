using Lumenwake.UIModule;
using Project.Core.AssetLoaderModule;
using Project.Core.AudioSystem;
using Project.Core.SceneLoaderServiceModule;
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
            Container.Bind<ISceneLoaderService>().To<BuildInSceneLoaderService>().AsSingle();
            Container.Bind<IAssetLoaderService>().To<ResourceAssetLoaderService>().AsSingle();
            Container.BindInterfacesAndSelfTo<AudioSystem>().FromComponentInHierarchy().AsSingle();
            Container.BindInterfacesAndSelfTo<InputRebindManager>().AsSingle();

            Container.Bind<MenuState>().AsSingle();
            Container.Bind<GameplayState>().AsSingle();
            Container.Bind<GlobalGameStateMachine>().AsSingle();
        }
    }
}
