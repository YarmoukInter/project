using System;
using System.IO;
using System.Linq;

namespace Yarmouk.Helpers
{
    public static class StudentProfileImages
    {
        public static string GetUrl(string fileName)
        {
            var safeName = Path.GetFileName(fileName ?? string.Empty);
            if (string.IsNullOrWhiteSpace(safeName)
                || !string.Equals(safeName, fileName, StringComparison.Ordinal)
                || !new[] { ".png", ".jpg", ".jpeg" }.Contains(Path.GetExtension(safeName), StringComparer.OrdinalIgnoreCase))
                return string.Empty;

            return System.Web.VirtualPathUtility.ToAbsolute("~/Uploads/StudentProfileImages/")
                + Uri.EscapeDataString(safeName);
        }
    }
}
