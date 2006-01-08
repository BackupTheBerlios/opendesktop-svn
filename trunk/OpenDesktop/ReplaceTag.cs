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
                return DoSearchResult();
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

            // Get the position of the first term
            foreach (string term in searchTerms)
            {
                int position = text.IndexOf(term, StringComparison.InvariantCultureIgnoreCase);
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

                    break; // if atleast one term is present
                }
            }
            string strExcerpt = text.Substring(iStart, iEnd - iStart);
            MatchEvaluator matchBoldify = new MatchEvaluator(Boldify);
            // Make all search terms bold
            foreach (string term in searchTerms)
            {
                strExcerpt = Regex.Replace(strExcerpt, term, matchBoldify, RegexOptions.IgnoreCase);
            }

            return strExcerpt;
        }

        /// <summary>
        /// Highlights the search terms in the search results page by making them bold
        /// </summary>
        /// <param name="m">RegEx Match</param>
        /// <returns>string with the search terms in bold</returns>
        private string Boldify(Match m)
        {
            return "<b>" + m.Value + "</b>";
        }

        private string DoSearchResult()
        {
            string strSearchTerm = m_queryString["search"];
            string strStartAt = m_queryString["page"];
            int iStartAt = 0;
            if (strStartAt != null)
            {
                try
                {
                    iStartAt = Int32.Parse(strStartAt) * 10;
                }
                catch
                {
                }
            }

            // Lock the index before starting search. This is to prevent FileExplorer from
            // moving/deleting index while a search operation is in progress
            Synchronizer.Instance.LockIndex(this);
            Indexer m_indexer = new Indexer(OpenDesktop.Properties.Settings.Default.IndexPath, IndexMode.SEARCH);

            string strReturnVal = string.Empty;

            SearchInfo[] sinfo = m_indexer.Search(strSearchTerm);
            if (sinfo.Length > 0)
            {
                StringBuilder sb = new StringBuilder();
                int iEndAt = iStartAt + 10;
                if (iEndAt > sinfo.Length)
                {
                    iEndAt = sinfo.Length;
                }

                // Display: Results 0 - 10 of about 44 for killing+work
                sb.Append("<div class=\"search-nav\">" +
                    string.Format(Properties.Resources.SearchResultsHeader,
                    (iStartAt + 1).ToString(), iEndAt.ToString(),
                    sinfo.Length.ToString(), strSearchTerm) + "</div>");
                sb.Append("<ol class=\"search-results\">");

                for (int i = iStartAt; i < iEndAt; i++)
                {
                    string strDisplayText = GetExcerpt(sinfo[i].Text, strSearchTerm);
                    sb.Append("<li value=\"" + (i+1).ToString() + "\"><a href=\"" + sinfo[i].Launcher + 
                        "\">" + sinfo[i].Title + "</a><br />" + strDisplayText + "</li>");
                }
                sb.Append("</ol>");
                int iCurPageNum = (iStartAt/10);
                string strPrevUrl = string.Empty;
                string strNextUrl = string.Empty;
                if (iCurPageNum > 0)
                {
                    strPrevUrl = string.Format("<a href=\"search.html?search={0}&page={1}\">",
                        m_queryString["search"], (iCurPageNum - 1).ToString()) +
                        Properties.Resources.SearchResultsPrev + "</a>";
                }
                if (iEndAt != sinfo.Length)
                {
                    strNextUrl = string.Format("<a href=\"search.html?search={0}&page={1}\">",
                        m_queryString["search"], (iCurPageNum + 1).ToString()) +
                        Properties.Resources.SearchResultsNext + "</a>";
                }
                sb.Append("<div class=\"search-nav\">" + strPrevUrl + " | " + strNextUrl + "</div>");
                strReturnVal = sb.ToString();
            }
            else
            {
                strReturnVal = "<div class=\"no-search-results\">" + Properties.Resources.NoSearchResults + "</div>";
            }
            m_indexer.Close();
            Synchronizer.Instance.ReleaseIndex(this);

            return strReturnVal;
        }
    }
}
