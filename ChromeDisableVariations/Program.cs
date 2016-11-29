using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using Microsoft.Win32;

static class Program {
    public const string ProgramName = "Chrome Disable Variations";

    static string PatchTarget = "chrome.dll";
    static string Install = "InstallLocation";
    static string Version = "Version";
    static string LocalState = $@"{Environment.GetEnvironmentVariable("localappdata")}\Google\Chrome\User Data\Local State";

    static string SeedURL = "chrome-variations/seed";
    static byte Replacer = 0x20;

    static bool RunningAsAdmin = IsAdmin();
    
    static string[] REG = {
        @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Uninstall\Google Chrome",
        @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Google Chrome",
        @"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Google Chrome"
    };

    static void Main() {
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine($"{Console.Title = $"{ProgramName} v{Assembly.GetExecutingAssembly().GetName().Version}"}\n");

        if(File.Exists(PatchTarget))
            Patch(PatchTarget);
        else
            FindInstallation();

        Console.ResetColor();
        Console.ReadKey(true);
    }

    static void FindInstallation() {
        var found = false;
        string path, ver;
        for(int i = 0; i < REG.Length && !found; i++)
            if(found = (
            (path = Registry.GetValue(REG[i], Install, null) as string) != null &&
            (ver = Registry.GetValue(REG[i], Version, null) as string) != null &&
            File.Exists(path += "\\" + ver + "\\" + PatchTarget)))
                Patch(path);

        if(!found) {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(Locale.GetString("InstallationNotFound"));

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"{Locale.GetString("IsPortable")} {PatchTarget}");
        }
    }

    static void Patch(string path) {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"{Locale.GetString("PatchTarget")}: {path}\n");
        try {
            var tmp = new FileStream(path, FileMode.Open, FileAccess.Write);
            tmp.Close();
            tmp.Dispose();
        } catch {
            if(RunningAsAdmin) {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(Locale.GetString("IsRunning"));
            } else {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(Locale.GetString("NeedAdmin"));
                RestartAsAdmin();
            }
            return;
        }

        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine($"{Locale.GetString("Patching")}\n");

        var file = File.ReadAllBytes(path);
        var max = file.Length - SeedURL.Length;
        var src = Encoding.Default.GetBytes(SeedURL);
        
        var first = src[0];
        int i, j;
        for(i = 0; i < max; i++) {
            if(file[i] == first || file[i] == Replacer) {
                for(j = 1; j < src.Length && file[i + j] == src[j]; j++);
                if(j == src.Length) {
                    if(file[i] == Replacer) {
                        file[i] = first;
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write($"{Locale.GetString("AlreadyPatched")} (Y/N) ");
                        if(ConsoleSelectYN() && WriteFile(path, file)) {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine(Locale.GetString("Reverted"));
                        }
                        return;
                    } else {
                        file[i] = Replacer;
                        if(WriteFile(path, file)) {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine(Locale.GetString("Done"));
                            RemoveVersions();
                        }
                        return;
                    }
                }
            }
        }
        
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(Locale.GetString("Failed"));
        
    }

    static bool WriteFile(string path, byte[] data) {
        try {
            File.WriteAllBytes(path, data);
        } catch {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(Locale.GetString("UnknownError"));
            return false;
        }
        return true;
    }

    static void RemoveVersions() {
        try {
            if(!File.Exists(LocalState)) return;
            string str = File.ReadAllText(LocalState), rep = "\"variations_compressed_seed\"";
            int i0 = str.IndexOf(rep); 
            if(i0 > -1) {
                int i1 = str.IndexOf('"', i0 + rep.Length + 1);
                if(i1 > -1) {
                    int i2 = str.IndexOf('"', i1 + 1);
                    if((i2 > -1) && (i2 - i1 > 1)) {
                        str = str.Substring(0, i1 + 1) + str.Substring(i2);
                        File.WriteAllText(LocalState, str);
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine(Locale.GetString("VariationsCleared"));
                    }
                }
            }
        } catch { }
    }

    static void RestartAsAdmin() {
        var info = new ProcessStartInfo();
        info.FileName = Assembly.GetEntryAssembly().Location;
        info.WorkingDirectory = Environment.CurrentDirectory;
        info.Verb = "runas";
        info.UseShellExecute = true;

        try {
            Process.Start(info);
            Environment.Exit(0);
        } catch {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(Locale.GetString("ConfirmUAC"));
        }
    }
    
    static bool IsAdmin() {
        System.Security.Principal.WindowsIdentity id = System.Security.Principal.WindowsIdentity.GetCurrent();
        System.Security.Principal.WindowsPrincipal p = new System.Security.Principal.WindowsPrincipal(id);

        return p.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
    }

    static bool ConsoleSelectYN() {
        ConsoleKeyInfo key;
        while(true) {
            key = Console.ReadKey(true);
            if(key.KeyChar == 'y' || key.KeyChar == 'Y') {
                Console.WriteLine(key.KeyChar);
                return true;
            } else if(key.KeyChar == 'n' || key.KeyChar == 'N') {
                Console.WriteLine(key.KeyChar);
                return false;
            } else Console.Beep();
        }
    }
}
