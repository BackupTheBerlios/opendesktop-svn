/*
 * Created by SharpDevelop.
 * User: pravin
 * Date: 1/22/2006
 * Time: 8:27 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.IO;
using System.Collections.Specialized;

using OpenDesktop.Plugin;

namespace OpenDesktop.CommonIndexables
{
	/// <summary>
	/// Responsible for indexing Text file
	/// </summary>
	public class TextIndexer : IPlugin
	{
		#region Member Vars
		private string[] m_strRegisteredExtensions = { ".txt" };
        private string[] m_strRegisteredHTMLTags = { "DisplayText" };
        #endregion
        
		public TextIndexer()
		{
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
                return "Text Indexer";
            }
        }

        public string[] RegisteredFileExtentions 
        {
            get
            {
                return m_strRegisteredExtensions;
            }
        }
        
        public string ProcessTag(string tag, NameValueCollection queryString)
        {
        	return string.Empty;
        }
        
        public SearchInfo ProcessFile(string filePath)
        {
        	FileInfo finfo = null;
        	SearchInfo sinfo = null;
        	try
        	{
        		finfo = new FileInfo(filePath);
        	}
        	catch 
        	{
        		return null;
        	}

        	if(finfo.Length > 1 * 1024 * 1024)
    		{
    			return null;
    		}
        	
        	string strContent = null;
        	try
        	{
	        	StreamReader reader = new StreamReader(filePath);
	        	strContent = reader.ReadToEnd();
	        	reader.Close();
        	}
        	catch
        	{
        		return null;
        	}
        	
        	string strTitle = finfo.Name;
        	string strLauncher = "file:///" + filePath;
        	
        	sinfo = new SearchInfo(strTitle, strContent, strLauncher);
        	return sinfo;
        }
	}
}
