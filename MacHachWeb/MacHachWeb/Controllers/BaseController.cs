using System.Web.Mvc;
using Raven.Client;
using Raven.Client.Document;

namespace MacHachWeb.Controllers
{
    public class BaseController : Controller
    {
        private readonly IDocumentStore _documentStore;
        protected IDocumentSession RaveSession;

        public BaseController()
        {
            _documentStore = new DocumentStore
            {
                ConnectionStringName = "RavenHQ"
            };
            _documentStore.Initialize();
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            RaveSession = _documentStore.OpenSession();
        }

        protected override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            RaveSession.Dispose();
        }

    }
}
