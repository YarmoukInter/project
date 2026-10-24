using System.Web;
using System.Web.SessionState;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Yarmouk.Helpers
{
    public sealed class SiteSettingsHandler : IHttpHandler, IReadOnlySessionState
    {
        public bool IsReusable { get { return true; } }
        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "application/json; charset=utf-8";
            context.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            context.Response.Cache.SetNoStore();
            JObject settings = SiteSettingsStore.Read(context.Server);
            var output = new JObject {
                ["defaultTheme"] = settings["defaultTheme"], ["fontFamily"] = settings["fontFamily"],
                ["firebaseDatabaseUrl"] = settings["firebaseDatabaseUrl"],
                ["logoDataUrl"] = settings["logoDataUrl"], ["faviconDataUrl"] = settings["faviconDataUrl"],
                ["homeTitle"] = settings["homeTitle"],
                ["homeDescription"] = settings["homeDescription"], ["metaKeywords"] = settings["metaKeywords"]
            };
            context.Response.Write(output.ToString(Formatting.None));
        }
    }
}
