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
using System.Collections;
using System.Collections.Specialized;

namespace OpenDesktop.Plugin
{
    #region class SearchInfo
    /// <summary>
    /// Contains info about the file to facilitate Indexing
    /// </summary>
    public class SearchInfo
    {
        #region Member Variables
        private string m_strTitle;
        private string m_strText;
        private string m_strLauncher;
        #endregion

        #region Constructor
        public SearchInfo(string title, string text)
        {
            m_strTitle = title;
            m_strText = text;
            m_strLauncher = null;
        }
        public SearchInfo(string title, string text, string launcher)
        {
            m_strTitle = title;
            m_strText = text;
            m_strLauncher = launcher;
        }
        #endregion

        #region Properties
        public string Title
        {
            get { return m_strTitle; }
        }
        public string Text
        {
            get { return m_strText; }
        }
        public string Launcher
        {
            get { return m_strLauncher; }
        }
        #endregion
    }
    #endregion

    #region IPlugin Interface
    public interface IPlugin
    {
        /// <summary>
        /// Enter tags that you want to register here. If a tag is registered by 
        /// another plugin and you try to register it again, your registration 
        /// will fail. While not a rule, do use tags of the form: PluginName.TagName
        /// </summary>
        string[] RegisteredHTMLTags { get;}
        /// <summary>
        /// Name of plugin. Shows up on the plugins config page
        /// </summary>
        string PluginName { get;}
        /// <summary>
        /// File extensions on which Yammy will call this plugin
        /// </summary>
        string[] RegisteredFileExtentions { get;}

        /// <summary>
        /// Extract indexable content from filePath
        /// </summary>
        /// <param name="filePath">file to parse</param>
        /// <returns>IndexVars object</returns>
        SearchInfo ProcessFile(string filePath);

        /// <summary>
        /// Process the Registered tag
        /// </summary>
        /// <param name="tag">Tag that this plugin has registered</param>
        /// <param name="queryString">QueryString of the request. 
        /// Use this as arguments to your function.</param>
        /// <returns>Text to replace this tag with</returns>
        string ProcessTag(string tag, NameValueCollection queryString);
    }
    #endregion
}
