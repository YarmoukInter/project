using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Yarmouk.Services
{
    public sealed class FirebaseAdminAccountService
    {
        public Task<string> UpdateStudentCredentialsAsync(string uid, string email, string password)
        { return RunAsync(new { action = "credentials", uid, email, password }); }

        public Task<string> DeleteStudentAsync(string uid)
        { return RunAsync(new { action = "deleteStudent", uid }); }

        private static async Task<string> RunAsync(object payload)
        {
            string root = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", ".."));
            string script = Path.Combine(root, "onboarding-worker", "admin-cli.js");
            string credential = Path.Combine(root, "yarmouk-uims-firebase-adminsdk-fbsvc-a3b29f5bc6.json");
            if (string.IsNullOrEmpty(script) || !File.Exists(script) || string.IsNullOrEmpty(credential) || !File.Exists(credential))
                throw new InvalidOperationException("Firebase Admin files are missing from the local server.");
            var start = new ProcessStartInfo("node", "\"" + script + "\"") {
                UseShellExecute = false, RedirectStandardInput = true, RedirectStandardOutput = true,
                RedirectStandardError = true, CreateNoWindow = true
            };
            start.EnvironmentVariables["YARMOUK_FIREBASE_SERVICE_ACCOUNT"] = credential;
            using (var process = Process.Start(start))
            {
                if (process == null) throw new InvalidOperationException("Could not start Firebase Admin service.");
                await process.StandardInput.WriteAsync(JsonConvert.SerializeObject(payload)).ConfigureAwait(false);
                process.StandardInput.Close();
                string output = await process.StandardOutput.ReadToEndAsync().ConfigureAwait(false);
                string error = await process.StandardError.ReadToEndAsync().ConfigureAwait(false);
                await Task.Run(() => process.WaitForExit(30000)).ConfigureAwait(false);
                JObject result;
                try { result = JObject.Parse(output); }
                catch { throw new InvalidOperationException("Firebase Admin returned an invalid response. " + error); }
                if ((bool?)result["ok"] != true)
                    throw new AuthException((string)result["error"] ?? "Firebase Admin operation failed.");
                return (string)result["message"] ?? "Account updated.";
            }
        }
    }
}
