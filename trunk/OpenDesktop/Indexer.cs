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
using System.Text;

using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.QueryParsers;
using Lucene.Net.Analysis.Standard;

using OpenDesktop.Plugin;

namespace OpenDesktop
{
    enum IndexMode : int
    {
        CREATE,
		APPEND,
        SEARCH
    }

    class Indexer
    {
        #region Member Vars
        IndexWriter m_indexWriter;
        IndexSearcher m_indexSearcher;
        IndexMode m_indexMode;
        Analyzer m_analyzer;
        bool m_bSucess;
        #endregion

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="indexPath">Path where you wish to create the index</param>
		/// <param name="mode">One of Create, Append or Search</param>
        public Indexer(string indexPath, IndexMode mode)
        {
            m_indexMode = mode;
            m_bSucess = false;
            m_analyzer = new StandardAnalyzer();

            if (mode == IndexMode.CREATE)
            {
                try
                {
                    m_indexWriter = new IndexWriter(indexPath, m_analyzer, true);
                    m_indexWriter.SetUseCompoundFile(true);
                    m_bSucess = true;
                }
                catch (Exception e)
                {
                    Logger.Instance.LogException(e);
                    m_bSucess = false;
                }
            }
			else if (mode == IndexMode.APPEND)
			{
				try
				{
					m_indexWriter = new IndexWriter(indexPath, m_analyzer, false);
					m_indexWriter.SetUseCompoundFile(true);
					m_bSucess = true;
				}
				catch (Exception e)
				{
					Logger.Instance.LogException(e);
					m_bSucess = false;
				}
			}
			else if (mode == IndexMode.SEARCH)
			{
				try
				{
					m_indexSearcher = new IndexSearcher(indexPath);
					m_bSucess = true;
				}
				catch (Exception e)
				{
					Logger.Instance.LogException(e);
					m_bSucess = false;
				}
			}
        }

		/// <summary>
		/// Close the index
		/// </summary>
        public void Close()
        {
            if ((m_indexMode == IndexMode.CREATE || m_indexMode == IndexMode.APPEND)
				&& m_indexWriter != null)
            {
                m_indexWriter.Close();
                m_indexWriter = null;
            }
            else if (m_indexMode == IndexMode.SEARCH && m_indexSearcher != null)
            {
                m_indexSearcher.Close();
                m_indexSearcher = null;
            }
        }

		/// <summary>
		/// Adds a document to the index
		/// </summary>
		/// <param name="info">Document to add</param>
		/// <returns>true if successful, false otherwise</returns>
        public bool AddDocument(SearchInfo info)
        {
			bool retVal = false;
			if (m_bSucess && (m_indexMode == IndexMode.CREATE || m_indexMode == IndexMode.APPEND))
			{
				Document doc = new Document();
				doc.Add(Field.UnIndexed("title", info.Title));
				doc.Add(Field.Text("content", info.Text));
				doc.Add(Field.UnIndexed("launcher", info.Launcher));
				m_indexWriter.AddDocument(doc);
				retVal = true;
			}
			return retVal;
        }

		/// <summary>
		/// Searches an index opened in search mode for the search term(s)
		/// </summary>
		/// <param name="searchTerm">Term to search for. Multiple terms are separated by + </param>
		/// <returns>An array of search results</returns>
        public SearchInfo[] Search(string searchTerm)
        {
			SearchInfo[] retVal = null;

			if (m_bSucess && m_indexMode == IndexMode.SEARCH && searchTerm != null)
			{
				// TODO: Needs more work. haha
				searchTerm = searchTerm.Replace("+", " + ");

				Query query = QueryParser.Parse(searchTerm, "content", m_analyzer);
				Hits hits = m_indexSearcher.Search(query);
				int len = hits.Length();
				if (len > 0)
				{
					retVal = new SearchInfo[len];
					for (int i = 0; i < len; i++)
					{
						retVal[i] = new SearchInfo(hits.Doc(i).Get("title"), hits.Doc(i).Get("content"), hits.Doc(i).Get("launcher"));
					}
				}
			}
			return retVal;
		}
	}
}
