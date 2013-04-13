using Ninject;
using Ninject.Modules;
using Ninject.Web.Common;
using Raven.Client;
using Raven.Client.Embedded;
using Raven.Database.Server;

namespace MvcWebRole1.KernelModules
{
    public class RavenDBNinjectModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IDocumentStore>()
          .ToMethod(context =>
          {
              NonAdminHttp.EnsureCanListenToWhenInNonAdminContext(8080);
              var documentStore = new EmbeddableDocumentStore
              {
                  DataDirectory = "App_Data",
                  UseEmbeddedHttpServer = true,
                  Configuration =
                  {
                      Port = 8081,
                      AnonymousUserAccessMode = AnonymousUserAccessMode.All
                  }
              };
              return documentStore.Initialize();
          })
          .InSingletonScope();

            Bind<IDocumentSession>().ToMethod(context => context.Kernel.Get<IDocumentStore>().OpenSession()).InRequestScope();
        }
    }
}