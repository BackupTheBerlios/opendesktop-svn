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
using System.IO;
using System.Text;
using System.Threading;
using System.Collections;

using ODIPlugin;

namespace OpenDesktop
{
    class FileExplorer : IDisposable
    {
        #region Private Vars
        private NameObjectCollection m_fileFunctionMap;
        private Thread m_objThread;
        private bool _quit;
        private Hashtable m_htDNV; // Donot visit hashmap
        Indexer m_indexer;
        #endregion

        #region Constructor
        public FileExplorer(NameObjectCollection fileFunctionMap)
        {
            m_fileFunctionMap = fileFunctionMap;
            _quit = false;
            //TODO: m_htDNV = 
            m_indexer = new Indexer(OpenDesktop.Properties.Settings.Default.IndexPath, IndexMode.CREATE);
        }

        public void Dispose()
        {
            if (m_objThread != null)
            {
                m_objThread.Abort();
            }
            m_indexer.Close();
        }
        #endregion

        #region Run/Stop/Pause/Resume
        public void Run()
        {
            m_objThread = new Thread(new ThreadStart(Explore));
            m_objThread.Priority = ThreadPriority.Lowest;
            m_objThread.Name = "FileExplorer";
            try
            {
                m_objThread.Start();
            }
            catch (Exception e)
            {
                // TODO: log exception (can be out of stack exception)
            }
        }

        public void Pause()
        {
            if (m_objThread.ThreadState == ThreadState.Running)
            {
                m_objThread.Suspend();
            }
        }

        public void Resume()
        {
            if (m_objThread.ThreadState == ThreadState.Suspended)
            {
                m_objThread.Resume();
            }
        }

        public void Stop()
        {
            _quit = true;
        }
        #endregion

        private void Explore()
        {
            string [] strDriveList = Environment.GetLogicalDrives();
            foreach (string strDrive in strDriveList)
            {
                DriveInfo info = new DriveInfo(strDrive);
                if (info.IsReady)
                {
                    Explore(strDrive);
                }
            }
        }

        private void Explore(string directory)
        {
            if (_quit)
                return;

            string[] strDirList;
            string[] strFileList;
            
            try
            {
                strDirList = Directory.GetDirectories(directory);
                strFileList = Directory.GetFiles(directory);
            }
            catch
            {
                return;
            }

            foreach (string strFile in strFileList)
            {
                if (_quit)
                    return;

                string strExtention = Path.GetExtension(strFile).ToLower();
                if (strExtention.Length == 0)
                    continue;
                ArrayList arPluginList = m_fileFunctionMap.Get(strExtention);
                if (arPluginList == null)
                    continue;

                foreach (IPlugin plugin in arPluginList)
                {
                    try
                    {
                        SearchInfo sInfo = plugin.ProcessFile(strFile);
                        if (sInfo != null)
                        {
                            m_indexer.AddDocument(sInfo);
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Instance.LogException(e);
                    }
                }
            }
            foreach (string strDir in strDirList)
            {
                if (_quit)
                    return;
                try
                {
                    Explore(strDir);
                }
                catch (Exception e) // Out of stack exception?
                {
                    // TODO: log exception
                }
            }
        }
    }
}
