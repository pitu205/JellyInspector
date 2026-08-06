using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Threading;
using System.Windows.Forms;

internal static class Program
{
    private const string Version = "0.2.0.0";
    private const string PluginFolderName = "JellyInspector_0.2.0.0";
    private const string PluginDllName = "Jellyfin.Plugin.JellyInspector.dll";

    [STAThread]
    private static void Main()
    {
        Application.EnableVisualStyles();

        try
        {
            Install();
            MessageBox.Show(
                $"JellyInspector {Version} se ha instalado correctamente.",
                "JellyInspector",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.Message,
                "Error instalando JellyInspector",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);

            Environment.ExitCode = 1;
        }
    }

    private static void Install()
    {
        string? jellyfinExe = new[]
        {
            @"C:\Program Files\Jellyfin\Server\jellyfin.exe",
            @"C:\Program Files (x86)\Jellyfin\Server\jellyfin.exe"
        }.FirstOrDefault(File.Exists);

        if (jellyfinExe is null)
        {
            throw new InvalidOperationException("No se ha encontrado Jellyfin.");
        }

        string jellyfinDirectory = Path.GetDirectoryName(jellyfinExe)!;
        string pluginsDirectory = @"C:\ProgramData\Jellyfin\Server\plugins";
        string destinationDirectory = Path.Combine(pluginsDirectory, PluginFolderName);
        string destinationDll = Path.Combine(destinationDirectory, PluginDllName);
        string stagingDirectory = Path.Combine(pluginsDirectory, PluginFolderName + ".installing");
        string? backupDirectory = null;

        string? trayExe = new[]
        {
            Path.Combine(jellyfinDirectory, "jellyfin-windows-tray", "Jellyfin.Windows.Tray.exe"),
            Path.Combine(jellyfinDirectory, "Jellyfin.Windows.Tray.exe")
        }.FirstOrDefault(File.Exists);

        bool wasRunning =
            Process.GetProcessesByName("jellyfin").Length > 0 ||
            Process.GetProcessesByName("Jellyfin.Windows.Tray").Length > 0;

        Directory.CreateDirectory(pluginsDirectory);

        byte[] pluginBytes = ReadEmbeddedPlugin();
        string sourceHash = Sha256(pluginBytes);

        if (Directory.Exists(stagingDirectory))
        {
            Directory.Delete(stagingDirectory, true);
        }

        Directory.CreateDirectory(stagingDirectory);
        string stagingDll = Path.Combine(stagingDirectory, PluginDllName);
        File.WriteAllBytes(stagingDll, pluginBytes);

        if (!string.Equals(
                Sha256(File.ReadAllBytes(stagingDll)),
                sourceHash,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "La copia temporal no coincide con el plugin integrado.");
        }

        StopProcess("Jellyfin.Windows.Tray");
        StopProcess("jellyfin");
        Thread.Sleep(2000);

        try
        {
            if (Directory.Exists(destinationDirectory))
            {
                backupDirectory =
                    destinationDirectory +
                    ".backup-" +
                    DateTime.Now.ToString("yyyyMMdd-HHmmss");

                Directory.Move(destinationDirectory, backupDirectory);
            }

            Directory.Move(stagingDirectory, destinationDirectory);

            GrantPermissions(destinationDirectory);

            string testFile = Path.Combine(
                destinationDirectory,
                "ji-write-test.tmp");

            File.WriteAllText(testFile, "OK");
            File.Delete(testFile);

            if (!File.Exists(destinationDll))
            {
                throw new InvalidOperationException(
                    "No se encuentra la DLL instalada.");
            }

            string destinationHash =
                Sha256(File.ReadAllBytes(destinationDll));

            if (!string.Equals(
                    destinationHash,
                    sourceHash,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "La DLL instalada no coincide con la integrada.");
            }

            if (trayExe is not null)
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = trayExe,
                    WorkingDirectory = Path.GetDirectoryName(trayExe)!,
                    UseShellExecute = true
                });
            }
        }
        catch
        {
            if (Directory.Exists(destinationDirectory))
            {
                Directory.Delete(destinationDirectory, true);
            }

            if (backupDirectory is not null &&
                Directory.Exists(backupDirectory))
            {
                Directory.Move(
                    backupDirectory,
                    destinationDirectory);
            }

            if (Directory.Exists(stagingDirectory))
            {
                Directory.Delete(stagingDirectory, true);
            }

            if (wasRunning && trayExe is not null)
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = trayExe,
                    WorkingDirectory = Path.GetDirectoryName(trayExe)!,
                    UseShellExecute = true
                });
            }

            throw;
        }
    }

    private static byte[] ReadEmbeddedPlugin()
    {
        Assembly assembly = Assembly.GetExecutingAssembly();

        using Stream? stream =
            assembly.GetManifestResourceStream(
                "JellyInspector.Plugin.dll");

        if (stream is null)
        {
            throw new InvalidOperationException(
                "No se encuentra el plugin integrado.");
        }

        using var memory = new MemoryStream();
        stream.CopyTo(memory);
        return memory.ToArray();
    }

    private static void StopProcess(string name)
    {
        foreach (Process process in Process.GetProcessesByName(name))
        {
            try
            {
                process.Kill(true);
                process.WaitForExit(5000);
            }
            catch
            {
                // Continuar con los demas procesos.
            }
        }
    }

    private static void GrantPermissions(string directory)
    {
        var directoryInfo = new DirectoryInfo(directory);
        DirectorySecurity security =
            directoryInfo.GetAccessControl();

        SecurityIdentifier users =
            new(WellKnownSidType.BuiltinUsersSid, null);

        SecurityIdentifier administrators =
            new(WellKnownSidType.BuiltinAdministratorsSid, null);

        SecurityIdentifier system =
            new(WellKnownSidType.LocalSystemSid, null);

        InheritanceFlags inheritance =
            InheritanceFlags.ContainerInherit |
            InheritanceFlags.ObjectInherit;

        security.SetAccessRule(
            new FileSystemAccessRule(
                users,
                FileSystemRights.Modify,
                inheritance,
                PropagationFlags.None,
                AccessControlType.Allow));

        security.SetAccessRule(
            new FileSystemAccessRule(
                administrators,
                FileSystemRights.FullControl,
                inheritance,
                PropagationFlags.None,
                AccessControlType.Allow));

        security.SetAccessRule(
            new FileSystemAccessRule(
                system,
                FileSystemRights.FullControl,
                inheritance,
                PropagationFlags.None,
                AccessControlType.Allow));

        directoryInfo.SetAccessControl(security);
    }

    private static string Sha256(byte[] data)
    {
        return Convert.ToHexString(
            SHA256.HashData(data));
    }
}
