using _Tower_Defense_Prototype.Game.UI;
using Dreamteck.Splines;
using Zenject;

namespace _Tower_Defense_Prototype.Game.Scripts.Managers
{
    public class ProjectMonoInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<GameManager>().FromComponentInHierarchy().AsSingle().NonLazy();
            Container.Bind<LevelManager>().FromComponentInHierarchy().AsSingle().NonLazy();
            Container.Bind<PoolManager>().FromComponentInHierarchy().AsSingle().NonLazy();
            Container.Bind<UIManager>().FromComponentInHierarchy().AsSingle().NonLazy();
            Container.Bind<SplineComputer>().FromComponentInHierarchy().AsSingle().NonLazy();
        }
    }
}