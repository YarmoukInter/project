using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.SessionState;
using Yarmouk.Services;

namespace Yarmouk.Helpers
{
    public static class StudentProfileImages
    {
        public static string GetStorageDirectory(HttpServerUtility server)
        {
            string configured = Environment.GetEnvironmentVariable("YARMOUK_PROFILE_IMAGE_DIRECTORY");
            if (string.IsNullOrWhiteSpace(configured)) return server.MapPath("~/Uploads/StudentProfileImages");
            if (!Path.IsPathRooted(configured)) throw new InvalidOperationException("The profile image directory must be an absolute server path.");
            return Path.GetFullPath(configured);
        }

        public static string GetPath(string fileName, HttpServerUtility server)
        {
            if (!ValidName(fileName)) return null;
            string shared = Path.Combine(GetStorageDirectory(server), fileName);
            if (File.Exists(shared)) return shared;
            // Old uploads remain readable while migrating to the persistent folder.
            string legacy = server.MapPath("~/Uploads/StudentProfileImages/" + fileName);
            return File.Exists(legacy) ? legacy : null;
        }

        private static bool ValidName(string fileName)
        {
            return !string.IsNullOrWhiteSpace(fileName) && Path.GetFileName(fileName) == fileName
                && fileName.IndexOfAny(new[] { '/', '\\', ':' }) < 0
                && new[] { ".png", ".jpg", ".jpeg" }.Contains(Path.GetExtension(fileName), StringComparer.OrdinalIgnoreCase);
        }

        public static string GetUrl(string fileName)
        {
            if (!ValidName(fileName)) return string.Empty;
            if (HttpContext.Current != null && GetPath(fileName, HttpContext.Current.Server) == null) return string.Empty;
            return VirtualPathUtility.ToAbsolute("~/StudentProfileImage.ashx?file=") + Uri.EscapeDataString(fileName);
        }
    }

    public sealed class StudentProfileImageHandler : IHttpHandler, IReadOnlySessionState
    {
        public bool IsReusable { get { return false; } }
        public void ProcessRequest(HttpContext context)
        {
            var auth = context.Session[AuthGuard.SessionKey] as AuthSession;
            if (auth == null || auth.ExpiresUtc <= DateTime.UtcNow || auth.User == null || !auth.User.IsActive)
            { context.Response.StatusCode = 403; return; }
            string path = StudentProfileImages.GetPath(context.Request.QueryString["file"], context.Server);
            if (path == null) { context.Response.StatusCode = 404; return; }
            context.Response.Cache.SetCacheability(HttpCacheability.Private);
            context.Response.AddHeader("X-Content-Type-Options", "nosniff");
            context.Response.ContentType = Path.GetExtension(path).Equals(".png", StringComparison.OrdinalIgnoreCase) ? "image/png" : "image/jpeg";
            context.Response.TransmitFile(path);
        }
    }
}
