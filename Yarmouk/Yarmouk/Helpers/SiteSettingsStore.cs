using System;
using System.IO;
using System.Configuration;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Yarmouk.Helpers
{
    public static class SiteSettingsStore
    {
        public static JObject Defaults()
        {
            var defaults = JObject.Parse("{\"defaultTheme\":\"light\",\"fontFamily\":\"DM Sans\",\"logoDataUrl\":\"\",\"faviconDataUrl\":\"\",\"homeTitle\":\"Yarmouk University — Your practical training journey\",\"homeDescription\":\"Explore practical training at Yarmouk University. Discover an opportunity, confirm your placement, develop your skills and complete your internship.\",\"metaKeywords\":\"Yarmouk University, practical training, internship\",\"headPlugins\":\"\",\"bodyPlugins\":\"\",\"planCaps\":{\"basicStudents\":100,\"basicCompanies\":10,\"advancedStudents\":500,\"advancedCompanies\":50},\"subscription\":{\"currentPlan\":\"basic\",\"trialStart\":\"\",\"trialEnd\":\"\"}} ");
            defaults["firebaseDatabaseUrl"] = ConfigurationManager.AppSettings["FirebaseDatabaseUrl"] ?? "";
            return defaults;
        }

        public static string GetFirebaseDatabaseUrl()
        {
            string fallback = ConfigurationManager.AppSettings["FirebaseDatabaseUrl"] ?? "";
            try
            {
                var context = HttpContext.Current;
                string path = context == null
                    ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "site-settings.json")
                    : context.Server.MapPath("~/App_Data/site-settings.json");
                if (!File.Exists(path)) return fallback.TrimEnd('/');
                string configured = (string)JObject.Parse(File.ReadAllText(path))["firebaseDatabaseUrl"];
                return (string.IsNullOrWhiteSpace(configured) ? fallback : configured).TrimEnd('/');
            }
            catch { return fallback.TrimEnd('/'); }
        }

        public static string GetHeadPlugins(HttpServerUtility server) { return (string)Read(server)["headPlugins"] ?? ""; }
        public static string GetBodyPlugins(HttpServerUtility server) { return (string)Read(server)["bodyPlugins"] ?? ""; }

        public static JObject Read(HttpServerUtility server)
        {
            string path = server.MapPath("~/App_Data/site-settings.json");
            if (!File.Exists(path)) return Defaults();
            try
            {
                var saved = JObject.Parse(File.ReadAllText(path));
                var result = Defaults();
                result.Merge(saved, new JsonMergeSettings { MergeArrayHandling = MergeArrayHandling.Replace });
                return result;
            }
            catch (Exception error)
            {
                System.Diagnostics.Trace.TraceWarning("Site settings could not be read: " + error.GetType().Name);
                return Defaults();
            }
        }

        public static void Write(HttpServerUtility server, JObject settings)
        {
            string path = server.MapPath("~/App_Data/site-settings.json");
            string directory = Path.GetDirectoryName(path);
            Directory.CreateDirectory(directory);
            string temp = path + ".tmp";
            File.WriteAllText(temp, settings.ToString(Formatting.Indented));
            if (File.Exists(path)) File.Replace(temp, path, null);
            else File.Move(temp, path);
        }
    }
}
