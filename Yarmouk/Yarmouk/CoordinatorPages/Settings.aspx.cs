using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json.Linq;
using Yarmouk.Helpers;
using Yarmouk.Models;
using Yarmouk.Services;

namespace Yarmouk.CoordinatorPages
{
    public partial class Settings : CoordinatorPage
    {
        protected override int MaxAdminPayloadLength { get { return 2300000; } }
        private JObject current;
        protected override Task<object> LoadDataAsync(AuthSession session)
        {
            EnsureCoordinator(session);
            current = SiteSettingsStore.Read(Server);
            return Task.FromResult((object)current);
        }

        protected override Task<string> SaveDataAsync(string action, JObject details, AuthSession session)
        {
            EnsureCoordinator(session);
            if (action != "save-site-settings") throw new ArgumentException("Unknown settings action.");
            current = SiteSettingsStore.Read(Server);
            if (Has(details, "defaultTheme")) SetChoice(details, current, "defaultTheme", new[] { "light", "dark" });
            if (Has(details, "fontFamily")) SetChoice(details, current, "fontFamily", new[] { "DM Sans", "Inter", "system-ui", "Arial", "Georgia" });
            if (Has(details, "homeTitle")) SetText(details, current, "homeTitle", 120);
            if (Has(details, "homeDescription")) SetText(details, current, "homeDescription", 320);
            if (Has(details, "metaKeywords")) SetText(details, current, "metaKeywords", 240);
            if (Has(details, "headPlugins")) SetText(details, current, "headPlugins", 8000);
            if (Has(details, "bodyPlugins")) SetText(details, current, "bodyPlugins", 8000);
            if (Has(details, "firebaseDatabaseUrl")) SetDatabaseUrl(details, current);
            var subscription = current["subscription"] as JObject ?? new JObject();
            var requestedSubscription = details["subscription"] as JObject;
            if (requestedSubscription != null)
            {
                if (Has(requestedSubscription, "currentPlan")) SetChoice(requestedSubscription, subscription, "currentPlan", new[] { "basic", "advanced", "custom" });
                if (Has(requestedSubscription, "trialStart")) SetDate(requestedSubscription, subscription, "trialStart");
                if (Has(requestedSubscription, "trialEnd")) SetDate(requestedSubscription, subscription, "trialEnd");
            }
            current["subscription"] = subscription;
            var logo = Has(details, "logoDataUrl") ? (string)details["logoDataUrl"] : null;
            if (!string.IsNullOrEmpty(logo))
            {
                int comma = logo.IndexOf(',');
                if (comma < 0 || logo.Length > 1400000 || !(logo.StartsWith("data:image/png;base64,", StringComparison.OrdinalIgnoreCase) || logo.StartsWith("data:image/jpeg;base64,", StringComparison.OrdinalIgnoreCase) || logo.StartsWith("data:image/webp;base64,", StringComparison.OrdinalIgnoreCase)))
                    throw new ArgumentException("Upload a PNG, JPEG or WebP logo smaller than 1 MB.");
                try { if (Convert.FromBase64String(logo.Substring(comma + 1)).Length > 1024 * 1024) throw new ArgumentException("Logo files must be 1 MB or smaller."); }
                catch (FormatException) { throw new ArgumentException("The selected logo file is invalid."); }
                current["logoDataUrl"] = logo;
            }
            var favicon = Has(details, "faviconDataUrl") ? (string)details["faviconDataUrl"] : null;
            if (!string.IsNullOrEmpty(favicon))
            {
                int comma = favicon.IndexOf(',');
                if (comma < 0 || favicon.Length > 720000 || !(favicon.StartsWith("data:image/png;base64,", StringComparison.OrdinalIgnoreCase)
                    || favicon.StartsWith("data:image/x-icon;base64,", StringComparison.OrdinalIgnoreCase)
                    || favicon.StartsWith("data:image/vnd.microsoft.icon;base64,", StringComparison.OrdinalIgnoreCase)))
                    throw new ArgumentException("Upload a PNG or ICO favicon smaller than 512 KB.");
                try { if (Convert.FromBase64String(favicon.Substring(comma + 1)).Length > 512 * 1024) throw new ArgumentException("Favicon files must be 512 KB or smaller."); }
                catch (FormatException) { throw new ArgumentException("The selected favicon file is invalid."); }
                current["faviconDataUrl"] = favicon;
            }
            SiteSettingsStore.Write(Server, current);
            return Task.FromResult("Site settings saved.");
        }

        private static void EnsureCoordinator(AuthSession session)
        {
            if (session == null || session.ExpiresUtc <= DateTime.UtcNow || session.User == null || !session.User.IsActive || AuthPolicy.NormalizeRole(session.User.Role) != "coordinator")
                throw new UnauthorizedAccessException("Sign in with an active coordinator account.");
        }
        private static bool Has(JObject source, string key)
        {
            return source.Property(key) != null && source[key].Type != JTokenType.Null;
        }
        private static void SetText(JObject source, JObject target, string key, int max)
        {
            string value = ((string)source[key] ?? "").Trim();
            if (value.Length > max) throw new ArgumentException("The " + key + " field is too long.");
            target[key] = value;
        }
        private static void SetChoice(JObject source, JObject target, string key, string[] choices)
        {
            string value = ((string)source[key] ?? "").Trim();
            if (!choices.Contains(value, StringComparer.Ordinal)) throw new ArgumentException("Choose a valid value for " + key + ".");
            target[key] = value;
        }
        private static void SetDate(JObject source, JObject target, string key)
        {
            string value = ((string)source[key] ?? "").Trim();
            DateTime parsed;
            if (value.Length > 0 && (!DateTime.TryParseExact(value, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out parsed))) throw new ArgumentException("Enter a valid " + key + " date.");
            target[key] = value;
        }
        private static void SetDatabaseUrl(JObject source, JObject target)
        {
            string value = ((string)source["firebaseDatabaseUrl"] ?? "").Trim().TrimEnd('/');
            if (value.Length == 0)
            {
                target["firebaseDatabaseUrl"] = System.Configuration.ConfigurationManager.AppSettings["FirebaseDatabaseUrl"] ?? "";
                return;
            }
            Uri uri;
            if (!Uri.TryCreate(value, UriKind.Absolute, out uri) || uri.Scheme != Uri.UriSchemeHttps || !string.IsNullOrEmpty(uri.UserInfo) || !string.IsNullOrEmpty(uri.Query) || !string.IsNullOrEmpty(uri.Fragment)
                || !(uri.Host.EndsWith(".firebaseio.com", StringComparison.OrdinalIgnoreCase) || uri.Host.EndsWith(".firebasedatabase.app", StringComparison.OrdinalIgnoreCase)))
                throw new ArgumentException("Enter a valid HTTPS Firebase Realtime Database URL.");
            target["firebaseDatabaseUrl"] = value;
        }
    }
}
