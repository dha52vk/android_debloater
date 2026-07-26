using System.Configuration;
using System.Data;
using System.Windows;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System;

namespace AndroidDebloaterStudio;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AndroidDebloaterStudio");
        string assetsPath = Path.Combine(appDataPath, "Assets");

        if (!Directory.Exists(assetsPath))
        {
            Directory.CreateDirectory(appDataPath);
            var assembly = Assembly.GetExecutingAssembly();
            string resourceName = "AndroidDebloaterStudio.Assets.zip";

            using (Stream? stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream != null)
                {
                    using (ZipArchive archive = new ZipArchive(stream))
                    {
                        archive.ExtractToDirectory(appDataPath, overwriteFiles: true);
                    }
                }
                else
                {
                    MessageBox.Show("Could not find embedded Assets.zip resource.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
