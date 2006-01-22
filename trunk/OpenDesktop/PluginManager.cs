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
using System.IO;
using System.Reflection;
using System.Collections;

using OpenDesktop.Plugin;

namespace OpenDesktop
{
    /// <summary>
    /// Responsible for Managing Plugins
    /// </summary>
    class PluginManager
    {
        #region Member Variables
        static PluginManager _instance = null;
        static object m_lock = new object();

        NameObjectCollection m_objCollection;
        Hashtable m_htTagMap;
        #endregion

        #region Constructor/Destructor
        private PluginManager()
        {
            Logger.Instance.LogDebug("Loading plugins...");
            LoadPlugins();
            Logger.Instance.LogDebug("Plugins loaded.");
        }

        ~PluginManager()
        {
            m_htTagMap.Clear();
            m_htTagMap = null;
        }
        #endregion

        public static PluginManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (m_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new PluginManager();
                        }
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Responsible for loading plugins and extracting RegisteredFileExtension and RegisteredHTMLTags
        /// </summary>
        private void LoadPlugins()
        {
            m_objCollection = new NameObjectCollection();
            m_htTagMap = new Hashtable(10);
            string strLocation = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "Plugins");
            string[] strFileList = Directory.GetFiles(strLocation, "*.dll");

            foreach (string file in strFileList)
            {
                Assembly assembly;
                try
                {
                    Logger.Instance.LogDebug("Loaded plugin: " + file);
                    assembly = Assembly.LoadFrom(file);
                }
                catch
                {
                    Logger.Instance.LogError("Unable to load plugin: " + file);
                    continue;
                }

                // TODO: Re-look this loop. Make it more efficient
                foreach (Type t in assembly.GetTypes())
                {
                    foreach (Type iface in t.GetInterfaces())
                    {
                        if (iface.Equals(typeof(IPlugin)))
                        {
                            IPlugin iPlugin;
                            try
                            {
                                iPlugin = (IPlugin)Activator.CreateInstance(t);
                            }
                            catch
                            {
                                Logger.Instance.LogError("Unable to load instance of plugin: " + file);
                                continue;
                            }
                            foreach(string fileExtension in iPlugin.RegisteredFileExtentions)
                            {
                                m_objCollection.Add(fileExtension, iPlugin);
                            }
                            foreach(string tag in iPlugin.RegisteredHTMLTags)
                            {
                                if(!m_htTagMap.ContainsKey(tag))
                                {
                                    m_htTagMap.Add(tag, iPlugin);
                                }
                                else
                                {
                                    Logger.Instance.LogError(iPlugin.PluginName + " trying to register " + 
                                        tag + "again. Previous Owner: " + (string) m_htTagMap[tag]);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns registered file extensions by all plugins
        /// </summary>
        public NameObjectCollection RegisteredFileExtensions
        {
            get { return m_objCollection; }
        }

        /// <summary>
        /// Returns registered HTML tags by all plugins
        /// </summary>
        public Hashtable RegisteredHTMLTags
        {
            get { return m_htTagMap; }
        }
    }
}
