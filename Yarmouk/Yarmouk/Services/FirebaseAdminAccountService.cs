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
        { return UpdateAccountCredentialsAsync(uid, "student", email, password); }

        public Task<string> UpdateAccountCredentialsAsync(string uid, string role, string email, string password)
        { return RunAsync(new { action = "credentials", uid, role, email, password }); }

        public Task<string> DeleteStudentAsync(string uid)
        { return RunAsync(new { action = "deleteStudent", uid }); }

        private static async Task<string> RunAsync(object payload)
        {
            string root = FindProjectRoot();
            string script = Path.Combine(root, "onboarding-worker", "admin-cli.js");
            string credential = Path.Combine(root, "yarmouk-uims-firebase-adminsdk-fbsvc-a3b29f5bc6.json");
            var start = new ProcessStartInfo(System.Configuration.ConfigurationManager.AppSettings["FirebaseAdminNodePath"] ?? "node", "\"" + script + "\"") {
                UseShellExecute = false, RedirectStandardInput = true, RedirectStandardOutput = true,
                RedirectStandardError = true, CreateNoWindow = true
            };
            start.EnvironmentVariables["YARMOUK_FIREBASE_SERVICE_ACCOUNT"] = credential;
            Process started;
            try { started = Process.Start(start); }
            catch (System.ComponentModel.Win32Exception ex)
            { throw new InvalidOperationException("Firebase Admin could not start Node.js. Install Node.js and make node.exe available to the local web server.", ex); }
            using (var process = started)
            {
                if (process == null) throw new InvalidOperationException("Could not start Firebase Admin service.");
                await process.StandardInput.WriteAsync(JsonConvert.SerializeObject(payload)).ConfigureAwait(false);
                process.StandardInput.Close();
                Task<string> outputTask = process.StandardOutput.ReadToEndAsync();
                Task<string> errorTask = process.StandardError.ReadToEndAsync();
                Task<bool> exitTask = Task.Run(() => process.WaitForExit(30000));
                Task completed = await Task.WhenAny(exitTask, Task.Delay(35000)).ConfigureAwait(false);
                if (completed != exitTask)
                {
                    try { process.Kill(); } catch { }
                    throw new TimeoutException("Firebase Admin did not finish within 35 seconds. Check local Node.js and Firebase connectivity, then retry.");
                }
                if (!await exitTask.ConfigureAwait(false))
                {
                    try { process.Kill(); } catch { }
                    throw new TimeoutException("Firebase Admin did not finish within 30 seconds. Check local Node.js and Firebase connectivity, then retry.");
                }
                string output = await outputTask.ConfigureAwait(false);
                string error = await errorTask.ConfigureAwait(false);
                JObject result;
                try { result = JObject.Parse(output); }
                catch { throw new InvalidOperationException("Firebase Admin could not complete the request. Check the local Node.js installation, service-account permissions, and Firebase connectivity." + (string.IsNullOrWhiteSpace(error) ? "" : " Details: " + error.Trim())); }
                if ((bool?)result["ok"] != true)
                    throw new AuthException((string)result["error"] ?? "Firebase Admin operation failed.");
                if (process.ExitCode != 0)
                    throw new InvalidOperationException("Firebase Admin exited unexpectedly (code " + process.ExitCode + "). The account was not confirmed deleted; check Firebase connectivity and service-account permissions.");
                return (string)result["message"] ?? "Account updated.";
            }
        }

        // ASP.NET may set BaseDirectory to either the web root or its bin folder.
        // Walk upward instead of assuming a fixed number of parent directories.
        private static string FindProjectRoot()
        {
            var directory = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            while (directory != null)
            {
                string script = Path.Combine(directory.FullName, "onboarding-worker", "admin-cli.js");
                string credential = Path.Combine(directory.FullName, "yarmouk-uims-firebase-adminsdk-fbsvc-a3b29f5bc6.json");
                if (File.Exists(script) && File.Exists(credential)) return directory.FullName;
                directory = directory.Parent;
            }
            throw new InvalidOperationException("Firebase Admin files were not found. Keep onboarding-worker/admin-cli.js and the Firebase service-account JSON in the project root.");
        }
    }
}
