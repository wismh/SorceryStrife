using Zenject;

namespace Project.Tools.ComponentRegistry
{
    public static class ComponentRegistryInstallerExtensions
    {
        public static void BindComponentRegistry<T>(this DiContainer container)
        {
            container.Bind<IComponentRegistry<T>>()
                .To<ComponentRegistry<T>>()
                .AsSingle()
                .IfNotBound();
        }
    }
}
