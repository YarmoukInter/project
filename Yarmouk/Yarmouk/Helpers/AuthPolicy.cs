using System;
using System.Net.Mail;
using Yarmouk.Models;

namespace Yarmouk.Helpers
{
    public static class AuthPolicy
    {
        public static string NormalizeRole(string role)
        {
            switch (role)
            {
                case "c_hr": return "company";
                case "uni_sp": return "academic";
                case "admin": return "coordinator";
                default: return role;
            }
        }
        public static string Home(string role)
        {
            switch (NormalizeRole(role))
            {
                case "student": return "~/StudentPages/Dashboard.aspx";
                case "company": return "~/CompanyPages/Dashboard.aspx";
                case "academic": return "~/AcademicPages/Dashboard.aspx";
                case "coordinator": return "~/CoordinatorPages/Dashboard.aspx";
                default: return null;
            }
        }
        public static bool ValidUser(User user, string uid)
        {
            return user != null && user.IsActive && (string.IsNullOrEmpty(user.Uid) || user.Uid == uid) && Home(user.Role) != null;
        }
        public static bool ValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email) || email.Length > 254) return false;
            try { return new MailAddress(email).Address == email; } catch (FormatException) { return false; }
        }
        public static string PasswordError(string current, string next, string confirmation)
        {
            if (string.IsNullOrEmpty(current)) return "Enter your current temporary password.";
            if (string.IsNullOrEmpty(next) || next.Length < 8 || next.Length > 128) return "Use a new password between 8 and 128 characters.";
            if (next == current) return "Choose a password different from your temporary password.";
            if (next != confirmation) return "The new passwords do not match.";
            return null;
        }
        public static string RequiredRole(string path)
        {
            if (path.StartsWith("~/StudentPages/", StringComparison.OrdinalIgnoreCase)) return "student";
            if (path.StartsWith("~/CompanyPages/", StringComparison.OrdinalIgnoreCase)) return "company";
            if (path.StartsWith("~/AcademicPages/", StringComparison.OrdinalIgnoreCase)) return "academic";
            if (path.StartsWith("~/CoordinatorPages/", StringComparison.OrdinalIgnoreCase)) return "coordinator";
            return null;
        }
        public static string ProtectedPageRedirect(User user, string path)
        {
            string role = RequiredRole(path);
            if (role == null) return null;
            if (user == null || !user.IsActive || Home(user.Role) == null) return "~/PublicPages/Login";
            bool setup = path.Equals("~/StudentPages/ChangePassword.aspx", StringComparison.OrdinalIgnoreCase)
                || path.Equals("~/StudentPages/ChangePassword", StringComparison.OrdinalIgnoreCase);
            string userRole = NormalizeRole(user.Role);
            if (user.MustChangePassword)
                return setup ? null : "~/StudentPages/ChangePassword.aspx";
            if (userRole != role || setup) return Home(userRole);
            return null;
        }
    }
}
