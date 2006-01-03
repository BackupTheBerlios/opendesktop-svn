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
using System.Collections;
using System.Collections.Specialized;

using Microsoft.Win32;

using ODIPlugin;
using YammyPlugin.Properties;

namespace YammyPlugin
{
    public class YMArchiveDecoder : IPlugin
    {
        #region Private Variables
        private string[] m_strRegisteredExtensions = {".dat"};
        private string[] m_strRegisteredHTMLTags = { "YammyDecode", "YammyVersion" };
        private string m_profilesPath;
        #endregion

        public YMArchiveDecoder()
        {
            RegistryKey keyYahooPagerLocation = Registry.ClassesRoot.OpenSubKey(@"ymsgr\shell\open\command");
            if (keyYahooPagerLocation != null)
            {
                m_profilesPath = keyYahooPagerLocation.GetValue(String.Empty) as string; // Get default string
            }
            if (m_profilesPath != null && m_profilesPath.Length > 0)
            {
                m_profilesPath = m_profilesPath.Substring(m_profilesPath.IndexOf("\"")+1, m_profilesPath.LastIndexOf('\\'));
            }
        }

        public string[] RegisteredHTMLTags
        {
            get
            {
                return m_strRegisteredHTMLTags;
            }
        }

        public string PluginName 
        {
            get
            {
                return "Yammy Decoder 0.8";
            }
        }

        public string[] RegisteredFileExtentions 
        {
            get
            {
                return m_strRegisteredExtensions;
            }
        }

        /// <summary>
        /// Decodes the file and returns a SearchInfo object for indexing
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public SearchInfo ProcessFile(string filePath)
        {
            SearchInfo indexvars = null;
            if (filePath.StartsWith(m_profilesPath, StringComparison.OrdinalIgnoreCase))
            {
                if (filePath.Contains("Profiles") && filePath.Contains("Messages"))
                {
                    Decoder d = new Decoder(filePath);
                    string strText = d.Decode(true);
                    string strTitle = string.Format(Resources.IndexTitle, d.LocalID, d.RemoteID);
                    indexvars = new SearchInfo(strTitle, strText, "display.html?convo=" + filePath);
                }
            }
            
            return indexvars;
        }

        public string ProcessTag(string tag, NameValueCollection queryString)
        {
            string strText = string.Empty;
            if (tag.Equals("YammyDecode", StringComparison.InvariantCultureIgnoreCase))
            {
                if (queryString != null)
                {
                    string strPath = queryString["convo"];
                    if (strPath != null)
                    {
                        string strFilePath = Uri.UnescapeDataString(strPath);
                        Decoder d = new Decoder(strFilePath);
                        strText = d.Decode(false);
                    }
                }
            }
            return strText;
        }
    }
}
