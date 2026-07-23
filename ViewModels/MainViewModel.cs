using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AndroidDebloaterStudio.Models;
using AndroidDebloaterStudio.Services;

namespace AndroidDebloaterStudio.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _deviceStatus = "No Device Connected";

        [ObservableProperty]
        private ObservableCollection<AndroidPackage> _packages = new();

        [ObservableProperty]
        private AndroidPackage _selectedPackage;

        [ObservableProperty]
        private string _searchQuery = "";

        partial void OnSearchQueryChanged(string value) => ApplyFilters();

        public ObservableCollection<string> OemFilters { get; } = new(new[] { "All", "Oem", "Carrier", "Google", "AOSP", "Samsung", "Misc" });
        public ObservableCollection<string> RiskFilters { get; } = new(new[] { "All", "Recommended", "Advanced", "Expert", "Unsafe" });

        [ObservableProperty]
        private string _selectedOemFilter = "All";
        partial void OnSelectedOemFilterChanged(string value) => ApplyFilters();

        [ObservableProperty]
        private string _selectedRiskFilter = "All";
        partial void OnSelectedRiskFilterChanged(string value) => ApplyFilters();

        private List<AndroidPackage> _allPackagesList = new();

        [ObservableProperty]
        private string _consoleOutput = "Welcome to Android Debloater Studio!\nReady.\n";

        private readonly PackageDatabaseService _dbService;
        private readonly AdbService _adbService;

        public MainViewModel()
        {
            _dbService = new PackageDatabaseService();
            _adbService = new AdbService();
            _ = LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            ConsoleOutput += "Loading database...\n";
            string? dbLoadError = await _dbService.LoadDatabaseAsync();
            if (!string.IsNullOrEmpty(dbLoadError))
            {
                ConsoleOutput += dbLoadError + "\n";
            }
            
            bool hasAdb = await _adbService.EnsureAdbExistsAsync(msg => 
            {
                App.Current.Dispatcher.Invoke(() => ConsoleOutput += msg + "\n");
            });
            
            if (!hasAdb)
            {
                DeviceStatus = "ADB Not Found";
                return;
            }

            ConsoleOutput += "Checking for connected ADB devices...\n";
            bool isConnected = await _adbService.IsDeviceConnectedAsync();
            
            if (isConnected)
            {
                string deviceName = await _adbService.GetDeviceNameAsync();
                DeviceStatus = $"Device Connected: {deviceName}";
                ConsoleOutput += $"Device found: {deviceName}. Fetching installed packages...\n";
                
                var packageStates = await _adbService.GetPackageStatesAsync();
                ConsoleOutput += $"Found {packageStates.Count} packages on device.\n";

                App.Current.Dispatcher.Invoke(() =>
                {
                    _allPackagesList.Clear();
                    foreach (var kvp in packageStates)
                    {
                        var info = _dbService.GetPackageInfo(kvp.Key);
                        info.State = kvp.Value;
                        _allPackagesList.Add(info);
                    }
                    ApplyFilters();
                });
            }
            else
            {
                DeviceStatus = "No Device Connected";
                ConsoleOutput += "No device found. Please connect a device with USB Debugging enabled.\n";
                
                // Show mock data or all DB known data for demo purposes when no device is connected
                var knownPackages = _dbService.GetAllKnownPackages();
                App.Current.Dispatcher.Invoke(() =>
                {
                    _allPackagesList.Clear();
                    foreach (var pkg in knownPackages)
                    {
                        pkg.State = "Unknown (No Device)";
                        _allPackagesList.Add(pkg);
                    }
                    ApplyFilters();
                });
            }
        }

        private void ApplyFilters()
        {
            var filtered = _allPackagesList.Where(p => 
                (string.IsNullOrEmpty(SearchQuery) || 
                 p.Name.Contains(SearchQuery, System.StringComparison.OrdinalIgnoreCase) || 
                 p.PackageName.Contains(SearchQuery, System.StringComparison.OrdinalIgnoreCase)) &&
                (SelectedOemFilter == "All" || p.Oem == SelectedOemFilter) &&
                (SelectedRiskFilter == "All" || p.RiskLevel == SelectedRiskFilter)
            ).ToList();

            App.Current.Dispatcher.Invoke(() =>
            {
                Packages.Clear();
                foreach (var p in filtered) Packages.Add(p);
            });
        }
        [RelayCommand]
        private async Task UninstallPackageAsync(AndroidPackage? package)
        {
            if (package == null) return;
            if (await ConfirmActionAsync(package, "Uninstall"))
            {
                ConsoleOutput += $"\nUninstalling {package.Name}...\n";
                string result = await _adbService.UninstallPackageAsync(package.PackageName);
                if (result.Contains("Success", System.StringComparison.OrdinalIgnoreCase))
                {
                    ConsoleOutput += $"[SUCCESS] Uninstalled {package.Name}.\n";
                    package.State = "Uninstalled";
                }
                else
                {
                    ConsoleOutput += $"Result: {result}\n";
                }
            }
        }

        [RelayCommand]
        private async Task DisablePackageAsync(AndroidPackage? package)
        {
            if (package == null) return;
            if (await ConfirmActionAsync(package, "Disable"))
            {
                ConsoleOutput += $"\nDisabling {package.Name}...\n";
                string result = await _adbService.DisablePackageAsync(package.PackageName);
                if (result.Contains("new state: disabled", System.StringComparison.OrdinalIgnoreCase) || result.Contains("Success", System.StringComparison.OrdinalIgnoreCase))
                {
                    ConsoleOutput += $"[SUCCESS] Disabled {package.Name}.\n";
                    package.State = "Disabled";
                }
                else
                {
                    ConsoleOutput += $"Result: {result}\n";
                }
            }
        }

        [RelayCommand]
        private async Task RestorePackageAsync(AndroidPackage? package)
        {
            if (package == null) return;
            
            ConsoleOutput += $"\nRestoring {package.Name}...\n";
            string result = await _adbService.RestorePackageAsync(package.PackageName);
            if (!result.Contains("Error", System.StringComparison.OrdinalIgnoreCase))
            {
                ConsoleOutput += $"[SUCCESS] Restored {package.Name}.\n";
                package.State = "Installed";
            }
            else
            {
                ConsoleOutput += $"Result: {result}\n";
            }
        }

        private Task<bool> ConfirmActionAsync(AndroidPackage package, string action)
        {
            if (package.RiskLevel == "Unsafe" || package.RiskLevel == "Expert")
            {
                var result = System.Windows.MessageBox.Show(
                    $"WARNING: {package.Name} is categorized as {package.RiskLevel.ToUpper()} to {action.ToLower()}.\n\n" +
                    $"Removing or disabling this package might cause instability, bootloops, or severely break your device.\n\n" +
                    $"Are you absolutely sure you want to proceed?",
                    "Critical Safety Warning",
                    System.Windows.MessageBoxButton.YesNo,
                    System.Windows.MessageBoxImage.Warning);

                return Task.FromResult(result == System.Windows.MessageBoxResult.Yes);
            }
            return Task.FromResult(true);
        }
    }
}
