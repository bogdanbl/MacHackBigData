using Ninject.Modules;
namespace MvcWebRole1.KernelModules
{
    public class ServicesModule : NinjectModule
    {
        public override void Load()
        {
            //Bind(typeof(IRepository<>)).To(typeof(Repository<>));
            //Bind<ITestService>().To<TestService>();
        }
    }
}