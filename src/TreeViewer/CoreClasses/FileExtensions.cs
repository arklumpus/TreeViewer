﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls.ApplicationLifetimes;
using Microsoft.Win32;

namespace TreeViewer
{
    internal class FileExtensions
    {
        public static void AssociateExtensions(IEnumerable<(string, string)> extensions)
        {
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
            {
                foreach ((string extension, string formatDescription) in extensions)
                {
                    AssociateExtensionWindows(extension, formatDescription);
                }
            }
            else if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux))
            {
                AssociateExtensionsLinux(extensions);
            }
        }

        private static void AssociateExtensionsLinux(IEnumerable<(string, string)> extensions)
        {
            string xml = "<?xml version=\"1.0\"?>\n<mime-info xmlns='http://www.freedesktop.org/standards/shared-mime-info'>\n";

            foreach ((string extension, string comment) in extensions)
            {
                string type = "application";
                string mimetype = type + "/vnd.treeviewer." + comment.Replace("file", "").Replace(" ", "").ToLower() + "." + extension;

                xml += "\t<mime-type type=\"" + mimetype + "\">\n\t\t<comment>" + comment + "</comment>\n\t\t<glob pattern=\"*." + extension + "\"/>\n\t</mime-type>\n";
            }

            xml += "</mime-info>\n";

            string temp = Path.Combine(Path.GetTempPath(), "TreeViewer.xml");

            File.WriteAllText(temp, xml);

            foreach ((string extension, string comment) in extensions)
            {
                string iconPath = Path.Combine(Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName), "Icons");
                string iconFile;

                if (File.Exists(Path.Combine(iconPath, extension + "-16.png")))
                {
                    iconFile = Path.Combine(iconPath, extension);
                }
                else
                {
                    iconFile = Path.Combine(iconPath, "tree");
                }

                string[] sizes = new string[] { "16", "32", "48", "64", "256", "512" };

                string type = "application";
                string mimetype = type + "/vnd.treeviewer." + comment.Replace("file", "").Replace(" ", "").ToLower() + "." + extension;

                for (int i = 0; i < sizes.Length; i++)
                {
                    string currIcon = iconFile + "-" + sizes[i] + ".png";

                    System.Diagnostics.ProcessStartInfo info = new System.Diagnostics.ProcessStartInfo("xdg-icon-resource");
                    info.ArgumentList.Add("install");
                    info.ArgumentList.Add("--novendor");
                    info.ArgumentList.Add("--context");
                    info.ArgumentList.Add("mimetypes");
                    info.ArgumentList.Add("--size");
                    info.ArgumentList.Add(sizes[i]);
                    info.ArgumentList.Add(currIcon);
                    info.ArgumentList.Add(mimetype.Replace("/", "-"));

                    System.Diagnostics.Process proc = System.Diagnostics.Process.Start(info);

                    proc.WaitForExit();
                }
            }

            System.Diagnostics.ProcessStartInfo mimeInstall = new System.Diagnostics.ProcessStartInfo("xdg-mime");
            mimeInstall.ArgumentList.Add("install");
            mimeInstall.ArgumentList.Add("--novendor");
            mimeInstall.ArgumentList.Add(temp);

            System.Diagnostics.Process mimeInstallProc = System.Diagnostics.Process.Start(mimeInstall);

            mimeInstallProc.WaitForExit();

            System.Diagnostics.ProcessStartInfo mimeDefault = new System.Diagnostics.ProcessStartInfo("xdg-mime");
            mimeDefault.ArgumentList.Add("default");
            mimeDefault.ArgumentList.Add("io.github.arklumpus.TreeViewer.desktop");

            foreach ((string extension, string comment) in extensions)
            {
                string type = "application";
                string mimetype = type + "/vnd.treeviewer." + comment.Replace("file", "").Replace(" ", "").ToLower() + "." + extension;
                mimeDefault.ArgumentList.Add(mimetype);
            }

            System.Diagnostics.Process mimeDefaultProc = System.Diagnostics.Process.Start(mimeDefault);

            mimeDefaultProc.WaitForExit();

            File.Delete(temp);
        }

        private static void AssociateExtensionWindows(string extension, string formatDescription)
        {
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
            {
                string iconPath = Path.Combine(Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName), "Icons");
                string iconFile;

                if (File.Exists(Path.Combine(iconPath, extension + ".ico")))
                {
                    iconFile = Path.Combine(iconPath, extension + ".ico");
                }
                else
                {
                    iconFile = Path.Combine(iconPath, "tree.ico");
                }

                RegistryKey extensionKey = Registry.ClassesRoot.OpenSubKey("." + extension, true);

                if (extensionKey != null)
                {
                    Registry.ClassesRoot.DeleteSubKeyTree("." + extension);
                }

                extensionKey = Registry.ClassesRoot.CreateSubKey("." + extension);
                extensionKey.SetValue(null, "TreeViewer." + extension, RegistryValueKind.String);

                RegistryKey commandKey = Registry.ClassesRoot.OpenSubKey("TreeViewer." + extension, true);

                if (commandKey != null)
                {
                    Registry.ClassesRoot.DeleteSubKeyTree("TreeViewer." + extension);
                }

                commandKey = Registry.ClassesRoot.CreateSubKey("TreeViewer." + extension);
                commandKey.SetValue(null, formatDescription, RegistryValueKind.String);

                commandKey.CreateSubKey("DefaultIcon").SetValue(null, "\"" + iconFile + "\"");

                RegistryKey shellKey = commandKey.CreateSubKey("shell");
                shellKey.SetValue(null, "Open", RegistryValueKind.String);

                shellKey.CreateSubKey("Open").CreateSubKey("command").SetValue(null, "\"" + System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName + "\" \"%1\"", RegistryValueKind.String);
            }
        }
    }

    internal static class Privileges
    {
        public static void Elevate(List<string> argsForElevator, List<string> argsForReboot)
        {
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
            {
                string executablePath = Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);

                string exePath;

#if DEBUG
                exePath = Path.Combine(executablePath, "..", "..", "..", "..", "Stairs", "bin", "Debug", "net5.0", "Stairs.exe");
#else
                exePath = Path.Combine(executablePath, "Stairs.exe");
#endif
                System.Diagnostics.ProcessStartInfo info = new System.Diagnostics.ProcessStartInfo(exePath);

                info.ArgumentList.Add(argsForElevator.Count.ToString());
                info.ArgumentList.Add(argsForReboot.Count.ToString());

                foreach (string arg in argsForElevator)
                {
                    info.ArgumentList.Add(arg);
                }

                foreach (string arg in argsForReboot)
                {
                    info.ArgumentList.Add(arg);
                }

                System.Diagnostics.Process.Start(info);

                ((IControlledApplicationLifetime)Avalonia.Application.Current.ApplicationLifetime).Shutdown();
            }
        }
    }
}
