using Zenject;

namespace Game
{
    public class RunFlowInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<RunPrepareState>().AsSingle();
            Container.Bind<WaveState>().AsSingle();
            Container.Bind<LevelUpInterstitialState>().AsSingle();
            Container.Bind<RunOverState>().AsSingle();
            Container.BindInterfacesAndSelfTo<RunFlowStateMachine>().AsSingle();
        }
    }
}
