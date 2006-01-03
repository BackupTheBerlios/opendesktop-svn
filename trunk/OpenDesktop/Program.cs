// OpenDesktop - A search tool for the Windows desktop
// Copyright (C) 2005, Pravin Paratey (pravinp at gmail dot com)
// http://opendesktop.berlios.de
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
using System;
using System.Collections;
using System.Windows.Forms;

namespace OpenDesktop
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Logger LOG = Logger.Instance;
            LOG.LogDebug("Starting OpenDesktop");
            if (AppIsRunning())
            {
                return;
            }
            LOG.LogDebug("Bringing up Webserver");
            WebServer.Instance.Start();
            LOG.LogDebug("Displaying NotifyIcon");
            TrayIcon trayIcon = new TrayIcon();
            MessageBox.Show(WebServer.Instance.LocalAddress);
            FileExplorer fileExplorer = new FileExplorer(PluginManager.Instance.RegisteredFileExtensions);
            fileExplorer.Run();
            
            Application.Run();

            fileExplorer.Stop();
            WebServer.Instance.Stop();
            LOG.LogDebug("Exiting OpenDesktop");
            Logger.Instance.Dispose();
        }

        /// <summary>
        /// Creates a named mutex and tries to gain ownership. If successful,
        /// it means it is the only app running. Else another app is running.
        /// </summary>
        /// <returns>True if another instance is running</returns>
        static bool AppIsRunning()
        {
            bool bCreatedNew;
            System.Threading.Mutex m = new System.Threading.Mutex(true, "OpenDesktopMutex", out bCreatedNew);            
            return !bCreatedNew;
        }
    }
}