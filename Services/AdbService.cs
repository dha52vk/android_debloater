using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace AndroidDebloaterStudio.Services
{
    public class AdbService
    {
        private string _adbPath = "adb";

        public async Task<bool> EnsureAdbExistsAsync(Action<string>? logAction)
        {
            string localAdbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "platform-tools", "adb.exe");
            if (File.Exists(localAdbPath))
            {
                _adbPath = localAdbPath;
                return true;
            }

            // Check if adb is in PATH
            try
            {
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = "adb",
                    Arguments = "version",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using var process = new Process { StartInfo = processStartInfo };
                process.Start();
                await process.WaitForExitAsync();
                if (process.ExitCode == 0)
                {
                    _adbPath = "adb";
                    return true;
                }
            }
            catch
            {
                // adb not found in PATH
            }

            logAction?.Invoke("Error: ADB not found. Please download Android SDK Platform-Tools, extract it, and place it in the Assets/platform-tools directory, or add ADB to your system PATH.");
            return false;
        }

        public async Task<string> RunAdbCommandAsync(string arguments)
        {
            try
            {
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = _adbPath,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = new Process { StartInfo = processStartInfo };
                process.Start();

                string output = await process.StandardOutput.ReadToEndAsync();
                string error = await process.StandardError.ReadToEndAsync();

                await process.WaitForExitAsync();

                if (process.ExitCode != 0 && !string.IsNullOrWhiteSpace(error))
                {
                    // Sometimes ADB outputs warnings to stderr but succeeds.
                    // For simplicity, we just return the error if exit code is non-zero.
                    return $"Error: {error}";
                }

                return output;
            }
            catch (Exception ex)
            {
                return $"Exception: {ex.Message}";
            }
        }

        public async Task<bool> IsDeviceConnectedAsync()
        {
            string output = await RunAdbCommandAsync("devices");
            // Output looks like:
            // List of devices attached
            // 1234567890abcdef    device
            
            var lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                if (line.EndsWith("\tdevice") || line.EndsWith(" device"))
                {
                    return true;
                }
            }
            return false;
        }

        public async Task<string> GetDeviceNameAsync()
        {
            string model = await RunAdbCommandAsync("shell getprop ro.product.model");
            string device = await RunAdbCommandAsync("shell getprop ro.product.device");
            
            model = model?.Trim() ?? "";
            device = device?.Trim() ?? "";
            
            // Sometimes getprop might return an error message starting with "Error" if adb fails
            if (model.StartsWith("Error", StringComparison.OrdinalIgnoreCase)) model = "";
            if (device.StartsWith("Error", StringComparison.OrdinalIgnoreCase)) device = "";
            
            if (!string.IsNullOrEmpty(model) || !string.IsNullOrEmpty(device))
            {
                return $"{model} ({device})";
            }
            return "Unknown Device";
        }

        public async Task<Dictionary<string, string>> GetPackageStatesAsync()
        {
            var states = new Dictionary<string, string>();
            
            string allOutput = await RunAdbCommandAsync("shell pm list packages -u");
            string installedOutput = await RunAdbCommandAsync("shell pm list packages");
            string disabledOutput = await RunAdbCommandAsync("shell pm list packages -d");
            
            var allLines = allOutput.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var installedLines = installedOutput.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var disabledLines = disabledOutput.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            
            var allSet = new HashSet<string>();
            foreach (var line in allLines) { if (line.StartsWith("package:")) allSet.Add(line.Substring(8).Trim()); }
            
            var installedSet = new HashSet<string>();
            foreach (var line in installedLines) { if (line.StartsWith("package:")) installedSet.Add(line.Substring(8).Trim()); }
            
            var disabledSet = new HashSet<string>();
            foreach (var line in disabledLines) { if (line.StartsWith("package:")) disabledSet.Add(line.Substring(8).Trim()); }

            foreach (var pkg in allSet)
            {
                if (!installedSet.Contains(pkg))
                {
                    states[pkg] = "Uninstalled";
                }
                else if (disabledSet.Contains(pkg))
                {
                    states[pkg] = "Disabled";
                }
                else
                {
                    states[pkg] = "Installed";
                }
            }

            return states;
        }

        public async Task<string> UninstallPackageAsync(string packageName)
        {
            return await RunAdbCommandAsync($"shell pm uninstall --user 0 {packageName}");
        }

        public async Task<string> DisablePackageAsync(string packageName)
        {
            return await RunAdbCommandAsync($"shell pm disable-user --no-restart --user 0 {packageName}");
        }

        public async Task<string> RestorePackageAsync(string packageName)
        {
            return await RunAdbCommandAsync($"shell cmd package install-existing --user 0 {packageName}");
        }
    }
}
