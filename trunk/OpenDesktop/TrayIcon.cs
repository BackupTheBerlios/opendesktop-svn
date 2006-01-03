// OpenDesktop - A search tool for the Windows desktop
// Copyright (C) 2005-2006, Pravin Paratey (pravinp at gmail dot com)
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
using System.Windows.Forms;

using OpenDesktop.Properties;

namespace OpenDesktop
{
    /// <summary>
    /// Responsible for drawing the TrayIcon
    /// </summary>
    class TrayIcon
    {
        #region Private Members
        NotifyIcon m_trayIcon;
        ContextMenu m_contextMenu;
        #endregion

        public TrayIcon()
        {
            // Create Context Menu
            m_contextMenu = new ContextMenu();
            m_contextMenu.MenuItems.Add(Resources.TrayMenuOpen, OnDoubleClick);
            m_contextMenu.MenuItems.Add(Resources.TrayMenuSettings, OnSettingsClick);
            m_contextMenu.MenuItems.Add("-");
            m_contextMenu.MenuItems.Add(Resources.TrayMenuExit, OnExitClick);

            // Create Tray Icon
            m_trayIcon = new NotifyIcon();
            m_trayIcon.Text = "Open Desktop Search";
            m_trayIcon.Visible = true;
            m_trayIcon.Icon = Resources.MainIcon;
            m_trayIcon.ContextMenu = m_contextMenu;
            m_trayIcon.MouseDoubleClick += new MouseEventHandler(OnDoubleClick);
        }

        /// <summary>
        /// Sets the tooltip of the tray icon
        /// </summary>
        /// <param name="status"></param>
        public void SetStatus(string status)
        {
            m_trayIcon.Text = status;
        }

        private void OnDoubleClick(object sender, EventArgs e)
        {
            if (WebServer.Instance.IsRunning)
            {
                System.Diagnostics.Process.Start(WebServer.Instance.LocalAddress);
            }
        }

        private void OnSettingsClick(object sender, EventArgs ea)
        {
            if (WebServer.Instance.IsRunning)
            {
                string strExec = string.Format("{0}Settings.html", WebServer.Instance.LocalAddress);
                System.Diagnostics.Process.Start(strExec);
            }
        }

        private void OnExitClick(object sender, EventArgs ea)
        {
            m_trayIcon.Visible = false;
            Application.Exit();
        }
    }
}
