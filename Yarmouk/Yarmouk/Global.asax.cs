using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;

namespace Yarmouk
{
    public class Global : HttpApplication
    {
        public override void Init()
        {
            base.Init();
            var guard = new EventHandlerTaskAsyncHelper(Yarmouk.Helpers.AuthGuard.CheckAsync);
            AddOnPostAcquireRequestStateAsync(guard.BeginEventHandler, guard.EndEventHandler);
        }
        void Application_Start(object sender, EventArgs e)
        {
            // Code that runs on application startup
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }
}
