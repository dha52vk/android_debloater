using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using AndroidDebloaterStudio.Models;

namespace AndroidDebloaterStudio.Services
{
    public class PackageDatabaseService
    {
        private const string DbFilePath = @"Assets\uad_lists.json";
        private Dictionary<string, AndroidPackage> _packageDb = new();

        public class UadPackageDto
        {
            public string id { get; set; } = "";
            public string list { get; set; } = "";
            public string description { get; set; } = "";
            public List<string> dependencies { get; set; } = new();
            public List<string> neededBy { get; set; } = new();
            public List<string> labels { get; set; } = new();
            public string removal { get; set; } = "";
        }

        public async Task<string?> LoadDatabaseAsync()
        {
            if (!File.Exists(DbFilePath))
            {
                throw new FileNotFoundException($"Database file not found at {DbFilePath}");
            }

            try
            {
                string json = await File.ReadAllTextAsync(DbFilePath);
                var packages = JsonSerializer.Deserialize<List<UadPackageDto>>(json);
                
                if (packages != null)
                {
                    _packageDb.Clear();
                    foreach (var dto in packages)
                    {
                        var pkg = new AndroidPackage
                        {
                            PackageName = dto.id ?? "Unknown",
                            Name = dto.id ?? "Unknown",
                            Oem = dto.list ?? "Unknown",
                            RiskLevel = dto.removal ?? "Unknown",
                            Description = dto.description ?? "",
                            Dependencies = dto.dependencies ?? new List<string>(),
                            NeededBy = dto.neededBy ?? new List<string>(),
                            Labels = dto.labels ?? new List<string>(),
                            State = "Unknown"
                        };
                        _packageDb[pkg.PackageName] = pkg;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                // Return error message to be logged by the ViewModel
                return $"Error loading package database: {ex.Message}";
            }
        }

        public AndroidPackage GetPackageInfo(string packageName)
        {
            if (_packageDb.TryGetValue(packageName, out var pkg))
            {
                return pkg;
            }
            
            // If not found in DB, return a generic unlisted package
            return new AndroidPackage
            {
                PackageName = packageName,
                Name = packageName,
                RiskLevel = "Unknown",
                Oem = "Unknown",
                State = "Unknown",
                Description = "Not listed in the database."
            };
        }

        public IEnumerable<AndroidPackage> GetAllKnownPackages()
        {
            return _packageDb.Values;
        }
    }
}
