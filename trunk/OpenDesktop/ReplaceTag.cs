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
using System.Collections.Specialized;
using System.Text;
using System.Text.RegularExpressions;

using ODIPlugin;

namespace OpenDesktop
{
    /// <summary>
    /// This class is responsible for replacing the 
    /// </summary>
    public sealed class ReplaceTag
    {
        #region Member Variables
        NameValueCollection m_queryString;
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        public ReplaceTag(NameValueCollection query)
        {
            m_queryString = query;
        }

        /// <summary>
        /// Replaces Tags with appropriate string(s) got from the plugin
        /// </summary>
        /// <param name="m">Match m</param>
        /// <returns>String to replace the match with</returns>
        public string Replace(Match m)
        {
            string strTag = m.Groups[1].Value;
            if (strTag.Equals("SearchResult", StringComparison.InvariantCultureIgnoreCase))
            {
                StringBuilder sb = new StringBuilder();
                string strSearchTerm = m_queryString["search"];
                
                // If index is being moved by FileExplorer, wait till the move operation is complete
                while (Synchronizer.Instance.IndexIsLocked)
                {
                    System.Threading.Thread.Sleep(500);
                }
                
                Indexer m_indexer = new Indexer(OpenDesktop.Properties.Settings.Default.IndexPath, IndexMode.SEARCH);
                SearchInfo [] sinfo = m_indexer.Search(strSearchTerm);
                for (int i = 0; i < sinfo.Length; i++)
                {
                    string strDisplayText = GetExcerpt(sinfo[i].Text, strSearchTerm);
                    sb.Append("<li><a href=\"" + sinfo[i].Launcher + "\">" + sinfo[i].Title +
                        "</a><br />" + strDisplayText + "</li>");
                }
                m_indexer.Close();
                return sb.ToString();
            }
            else if (PluginManager.Instance.RegisteredHTMLTags.ContainsKey(strTag))
            {
                ODIPlugin.IPlugin plugin = PluginManager.Instance.RegisteredHTMLTags[strTag] as ODIPlugin.IPlugin;
                if (plugin != null)
                {
                    return plugin.ProcessTag(strTag, m_queryString);
                }
            }
            return m.Value;
        }

        /// <summary>
        /// Gets a small paragraph of the text to display on the search results page
        /// </summary>
        /// <param name="text">The text gotten from the index</param>
        /// <param name="searchTerm"></param>
        /// <returns>small paragraph of text</returns>
        string GetExcerpt(string text, string searchTerm)
        {
            const int textWindow = 100;
            string[] searchTerms = searchTerm.Split('+');
            int iStart = 0;
            int iEnd = text.Length;
            foreach (string term in searchTerms)
            {
                int position = text.IndexOf(term);
                if (position >= 0)
                {
                    if (position > textWindow)
                    {
                        iStart = position - textWindow;
                    }

                    if (position + textWindow < text.Length)
                    {
                        iEnd = position + textWindow;
                    }

                    break;
                }
            }
            return text.Substring(iStart, iEnd - iStart);
        }
    }
}
